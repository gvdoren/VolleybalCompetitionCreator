using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeifenLuo.WinFormsUI.Docking;

namespace CompetitionCreator
{
    public enum TeamGroups {GroupX, GroupY, NoGroup};
    public static class Extensions
    {
        public static string ToStringCustom(this TeamGroups group)
        {
            if (group == TeamGroups.GroupX) return "X";
            else if (group == TeamGroups.GroupY) return "Y";
            else return "-";
        }
    }

    public class ConstraintAdmin
    {
        public List<Constraint> conflictConstraints = new List<Constraint>();
        public int conflict { get; set; }
        public int conflict_cost { get; set; }
        public void ClearConflicts()
        {
            conflictConstraints.Clear();
            conflict = 0;
            conflict_cost = 0;
        }
        public virtual void AddConflict(Constraint constraint)
        {
            if (conflictConstraints.Contains(constraint) == false)
            {
                conflictConstraints.Add(constraint);
            }
            conflict ++;
            conflict_cost += constraint.cost;
        }
    }

    public abstract class Constraint
    {
        public bool VisitorAlso = true;
        public bool error = false;
        List<String> errorList = new List<string>();
        protected void ClearErrors()
        {
            error = false;
            errorList.Clear();
        }
        protected void AddError(string error_str)
        {
            error = true;
            errorList.Add(error_str);
        }
        protected string[] ErrorInfo()
        {
            return errorList.ToArray();
        }
        public Club club { get; set; }
        public Poule poule { get; set; }
        public Team team { get; set; }
        public virtual bool RelatedTo(List<Club> clubs) { return clubs.Contains(this.club); }
        public virtual bool RelatedTo(List<Poule> poules) { return poules.Contains(this.poule); }
        public virtual bool RelatedTo(List<Team> teams) { return teams.Contains(this.team); }
        public virtual List<Club> RelatedClubs()
        {
            List<Club> clubs = new List<Club>();
            if(club != null) clubs.Add(club);
            return clubs;
        }
        public virtual List<Poule> RelatedPoules()
        {
            List<Poule> poules = new List<Poule>();
            if (poule != null) poules.Add(poule);
            return poules;
        }
        public virtual List<Team> RelatedTeams()
        {
            List<Team> teams = new List<Team>();
            if (team != null) teams.Add(team);
            return teams;
        }
        public virtual string Context
        {
            get
            {
                if (poule != null) return poule.fullName;
                if (club != null) return club.name;
                return "----";
            }
        }
        public string name { get; set; }
        public int cost = 1;
        public int conflict_cost = 0;
        public abstract string[] GetTextDescription();
        public abstract void Evaluate(Model model);
        public virtual string Title 
        { 
            get 
            {
                if (club != null) return name + " - " + club.name;
                else return name;
            } 
        }
        public List<Match> conflictMatches = new List<Match>();
        protected enum VisitorHomeBoth { VisitorOnly, HomeOnly, Both };
        protected void AddConflictMatch(VisitorHomeBoth who,params Match[] matches) 
        {
            for (int i = 0; i < matches.Length; i++)
            {
                conflict_cost += cost;
                if (conflictMatches.Contains(matches[i]) == false)
                {
                    conflictMatches.Add(matches[i]);
                    matches[i].AddConflict(this);
                    matches[i].Week.AddConflict(this);
                    matches[i].poule.AddConflict(this);
                    matches[i].poule.serie.AddConflict(this);
                    if (who != VisitorHomeBoth.VisitorOnly) matches[i].homeTeam.AddConflict(this);
                    if (who != VisitorHomeBoth.HomeOnly) matches[i].visitorTeam.AddConflict(this);
                    if (club != null)
                    {
                        club.AddConflict(this);
                    }
                    else
                    {
                        if (who == VisitorHomeBoth.Both)
                        {
                            matches[i].homeTeam.club.AddConflict(this);
                            if (matches[i].homeTeam.club != matches[i].visitorTeam.club) matches[i].visitorTeam.club.AddConflict(this);
                        }
                        else
                        {
                            if (who != VisitorHomeBoth.VisitorOnly) matches[i].homeTeam.club.AddConflict(this);
                            if (who != VisitorHomeBoth.HomeOnly) matches[i].visitorTeam.club.AddConflict(this);
                        }
                        
                    }
                }

            }
        }
        public void Sort()
        {
            conflictMatches.Sort(delegate(Match m1, Match m2) { return m1.datetime.CompareTo(m2.datetime); });
        }
    }
    class ConstraintSporthallNotAvailable : Constraint
    {
        SporthallClub sporthal;
        public ConstraintSporthallNotAvailable(Team team)
        {
            name = "Sporthal niet beschikbaar";
            this.team = team;
            VisitorAlso = false;
            cost = MySettings.Settings.SporthalNotAvailableCostLow;
        }
        public override void Evaluate(Model model)
        {
            conflict_cost = 0;
            conflictMatches.Clear();
            bool optimize = false;
            if (team.sporthal != null && team.sporthal.NotAvailable.Count > 0) optimize = true;
            if (optimize)
            {
                Poule poule = team.poule;
                if (poule != null && poule.evaluated)
                {
                    foreach (Match match in poule.matches)
                    {
                        DateTime dt = match.datetime.Date;
                        if (match.homeTeam == team &&
                            match.RealMatch() &&
                            team.sporthal.NotAvailable.Contains(dt) == true)
                        {
                            if (match.poule.serie.importance == Serie.ImportanceLevels.Low) cost = MySettings.Settings.SporthalNotAvailableCostLow;
                            if (match.poule.serie.importance == Serie.ImportanceLevels.Medium) cost = MySettings.Settings.SporthalNotAvailableCostMedium;
                            if (match.poule.serie.importance == Serie.ImportanceLevels.High) cost = MySettings.Settings.SporthalNotAvailableCostHigh;
                            this.AddConflictMatch(VisitorHomeBoth.HomeOnly, match);
                        }
                    }
                }
            }
        }
        public override string[] GetTextDescription()
        {
            List<string> result = new List<string>();
            result.Add(sporthal.name + "is niet beschikbaar op: ");
            foreach (DateTime dt in sporthal.NotAvailable)
            {
                result.Add(string.Format("{0} - {1}",dt.DayOfWeek, dt.Date));
            }
            return result.ToArray();
        }
    }
    class ConstraintSchemaTooClose : Constraint
    {
        public override string Title
        {
            get
            {
                return name + " - " + poule.serie.name + poule.name;
            }
        }
        public ConstraintSchemaTooClose(Poule poule)
        {
            name = "Heen en Terug te dicht bij elkaar";
            this.poule = poule;
        }
        public override void Evaluate(Model model)
        {
            conflict_cost = 0;
            conflictMatches.Clear();
            if (poule.evaluated)
            {
                for (int m1 = 0; m1 < poule.matches.Count; m1++)
                {
                    Match match1 = poule.matches[m1];
                    for (int m2 = m1 + 1; m2 < poule.matches.Count; m2++)
                    {
                        Match match2 = poule.matches[m2];
                        if (match1.homeTeam == match2.visitorTeam &&
                            match1.visitorTeam == match2.homeTeam &&
                            match1.RealMatch())
                        {
                            double days = Math.Abs((match2.datetime - match1.datetime).TotalDays);
                            // 12 - teams: 22 weeks -> 5.5 weeks -> 6 weeks
                            // 6 teams: 11 weeks -> 2.7 weeks -> 3 weeks
                            // Of (delen door 6)
                            // 12 - teams: 22 weeks -> 3.xx weeks -> 4 weeks
                            // 6 teams: 11 weeks -> 1.9 weeks -> 2 weeks
                            double mindays = (((poule.weeksFirst.Count + poule.weeksSecond.Count) * 7) / 6);
                            days = mindays-days;
                            if (days>0)
                            {
                                cost = MySettings.Settings.MatchTooCloseCost;
                                AddConflictMatch(VisitorHomeBoth.Both, match1, match2);
                            }
                        }
                    }
                }
            }
        }
        public override string[] GetTextDescription()
        {
            List<string> result = new List<string>();
            return result.ToArray();
        }
    }
    class ConstraintSchemaTooManyHomeAfterEachOther : Constraint
    {
        public override string Title
        {
            get
            {
                return name + " - " + poule.serie.name + poule.name;
            }
        }
        public ConstraintSchemaTooManyHomeAfterEachOther(Poule poule)
        {
            name = "Teveel thuis of uit wedstrijden na elkaar";
            this.poule = poule;
            this.cost = MySettings.Settings.MatchTooManyAfterEachOtherCostLow;
        }
        public override void Evaluate(Model model)
        {
            conflict_cost = 0;
            conflictMatches.Clear();
            if (poule.evaluated)
            {
                List<Match> sortedMatches = new List<Match>(poule.matches);
                sortedMatches.Sort(delegate(Match m1, Match m2) { return m1.datetime.CompareTo(m2.datetime); });
                foreach (Team team in poule.teams)
                {
                    List<Match> maxMatches = new List<Match>();
                    List<Match> homeMatches = new List<Match>();
                    List<Match> visitorMatches = new List<Match>();
                    List<Match> homeFirstHalf = new List<Match>();
                    List<Match> homeSecondHalf = new List<Match>();

                    bool home = false;
                    int matchNr = 0;
                    foreach (Match match in sortedMatches)
                    {
                        if (match.homeTeam == team)
                        {
                            if (match.RealMatch())
                            {
                                if (matchNr < poule.maxTeams) homeFirstHalf.Add(match);
                                else homeSecondHalf.Add(match);
                            }
                            visitorMatches = new List<Match>();
                            homeMatches.Add(match);
                            matchNr++;
                        }
                        if (match.visitorTeam == team)
                        {
                            homeMatches = new List<Match>();
                            visitorMatches.Add(match);
                            matchNr++;
                        }
                        if (visitorMatches.Count > maxMatches.Count)
                        {
                            maxMatches = visitorMatches;
                            home = false;
                        }
                        if (homeMatches.Count > maxMatches.Count)
                        {
                            maxMatches = homeMatches;
                            home = true;
                        }
                    }
                    if (maxMatches.Count >= MySettings.Settings.MatchTooManyAfterEachOther)
                    {
                        foreach (Match match in maxMatches)
                        {
                            if (home) AddConflictMatch(VisitorHomeBoth.HomeOnly, match);
                            else AddConflictMatch(VisitorHomeBoth.VisitorOnly, match);
                        }
                    }
                    // Ideal to spread home/visit evenly. Also for further optimisation
                    conflict_cost += Math.Max(0, homeFirstHalf.Count - ((int)((poule.TeamCount + 1) / 2)));
                    conflict_cost += Math.Max(0, homeSecondHalf.Count - ((int)((poule.TeamCount + 1) / 2)));
                    // Extreme will be punished
                    if (homeFirstHalf.Count > maxNumberHomeMatches(poule.TeamCount))
                    {
                        foreach (Match match in homeFirstHalf)
                        {
                            if (poule.serie.importance == Serie.ImportanceLevels.Low) conflict_cost += MySettings.Settings.MatchTooManyAfterEachOtherCostLow;
                            if (poule.serie.importance == Serie.ImportanceLevels.Medium) conflict_cost += MySettings.Settings.MatchTooManyAfterEachOtherCostMedium;
                            if (poule.serie.importance == Serie.ImportanceLevels.High) conflict_cost += MySettings.Settings.MatchTooManyAfterEachOtherCostHigh;
                        }
                    }
                    if (homeSecondHalf.Count > maxNumberHomeMatches(poule.TeamCount))
                    {
                        foreach (Match match in homeSecondHalf)
                        {
                            conflict_cost += cost;
                            //AddConflictMatch(VisitorHomeBoth.HomeOnly, match);
                        }
                    }
                }
            }
       }
        private int maxNumberHomeMatches(int teamCount)
        {
            switch (teamCount)
            {
                case 4: // 3 matches are played
                    return 2;
                case 5: // 4 matches are played
                    return 3;
                case 6:
                case 7:
                    return 4;
                case 8:
                case 9:
                    return 5;
                case 10:
                case 11:
                    return 6;
                case 12:
                case 13:
                    return 7;
                default:
                    return 8;

            }
        }
        public override string[] GetTextDescription()
        {
            List<string> result = new List<string>();
            return result.ToArray();
        }
    }
    class ConstraintPouleInconsistent : Constraint
    {
        public override string Title
        {
            get
            {
                return name + " - " + poule.serie.name + poule.name;
            }
        }
        int Number = 0;
        public ConstraintPouleInconsistent(Poule poule)
        {
            name = "Internal test - poule consistency";
            this.poule = poule;
            cost = MySettings.Settings.InconsistentCost;
        }
        public override void Evaluate(Model model)
        {
            if (Number % 1000 == 0 || error == true) // Deze test niet elke keer uitvoeren.
            {
                conflict_cost = 0;
                conflictMatches.Clear();
                ClearErrors();
                if (poule.evaluated)
                {
                    SortedList<int, int> HomeMatches = new SortedList<int, int>();
                    SortedList<int, int> VisitorMatches = new SortedList<int, int>();
                    foreach (Team t in poule.teams)
                    {
                        HomeMatches.Add(t.Index, 0);
                        VisitorMatches.Add(t.Index, 0);
                        if (t.poule.teams.Contains(t) == false)
                        {
                            conflict_cost += cost;
                            AddError("Internal test - poule consistency: Poule does not contain team");
                        }
                        if (t.serie.poules.Contains(poule) == false)
                        {
                            conflict_cost += cost;
                            AddError("Internal test - poule consistency: Serie does not contain poule");
                        }
                    }
                    for (int m1 = 0; m1 < poule.matches.Count; m1++)
                    {
                        Match match1 = poule.matches[m1];
                        if (match1.RealMatch())
                        {
                            HomeMatches[match1.homeTeam.Index]++;
                            VisitorMatches[match1.visitorTeam.Index]++;
                        }
                    }
                    foreach (Team t in poule.teams)
                    {
                        if (t.RealTeam())
                        {
                            if (HomeMatches[t.Index] != poule.TeamCount - 1)
                            {
                                foreach (Match match in poule.matches)
                                {
                                    if (match.homeTeam == t)
                                    {
                                        AddError(string.Format("Internal test - poule consistency: Aantal thuis matches klopt niet (poule: {0}, team: {1})",poule.fullName,t.name));
                                    }
                                }
                            }
                            if (VisitorMatches[t.Index] != poule.TeamCount - 1)
                            {
                                foreach (Match match in poule.matches)
                                {
                                    if (match.visitorTeam == t)
                                    {
                                        AddError(string.Format("Internal test - poule consistency: Aantal uit matches klopt niet (poule: {0}, team: {1})", poule.fullName, t.name));
                                    }
                                }
                            }
                        }
                    }
                    foreach (Team team in poule.teams)
                    {
                        if (team.RealTeam() && team.poule.matches.Count(m => m.homeTeam == team) == 0)
                        {
                            conflict_cost += cost;
                            AddError(string.Format("Internal test - poule consistency: No matches for team '{0}'",team.name));
                        }
                    }
                }
            }
            Number++;
        }
        public override string[] GetTextDescription()
        {
            return ErrorInfo();
        }
    }
    class ConstraintNoPouleAssigned : Constraint
    {
        public override string Title
        {
            get
            {
                return name + " - " + club.name;
            }
        }
        public ConstraintNoPouleAssigned(Club club)
        {
            name = "No poule assigned to team";
            this.club = club;
            cost = MySettings.Settings.NoPouleAssignedCost;
        }
        public override void Evaluate(Model model)
        {
            conflict_cost = 0;
            conflictMatches.Clear();
            ClearErrors();
            foreach (Team team in club.teams)
            {
                if (team.deleted == false && team.poule == null)
                {
                    conflict_cost += cost;
                    AddError(string.Format("Team {0} (serie:{1}) has no poule assigned", team.name, team.serie.name));
                }
            }
        }
        public override string[] GetTextDescription()
        {
            return ErrorInfo();
        }
    }
    class ConstraintPouleTwoTeamsOfSameClub : Constraint
    {
        public override string Title
        {
            get
            {
                return name + " - " + poule.serie.name + poule.name;
            }
        }
        public ConstraintPouleTwoTeamsOfSameClub(Poule poule)
        {
            name = "Teams van een club in één poule moeten op de eerste dag spelen";
            this.poule = poule;
            this.VisitorAlso = false;
            this.cost = MySettings.Settings.TwoPoulesOfSameClubInPouleCost;
        }
        public override void Evaluate(Model model)
        {
            conflict_cost = 0;
            conflictMatches.Clear();
            if (poule.evaluated)
            {
                foreach(Team t1 in poule.teams)
                {
                    foreach (Team t2 in poule.teams)
                    {
                        if (t1 != t2)
                        {
                            if (t1.club == t2.club)
                            {
                                foreach (Match m in poule.matches)
                                {
                                    if((m.homeTeam == t1 && m.visitorTeam == t2) ||
                                        (m.homeTeam == t2 && m.visitorTeam == t1)
                                        )
                                    {
                                        int weekIndexFirst = poule.weeksFirst.FindIndex(d => d == m.Week);
                                        int weekIndexSecond = poule.weeksSecond.FindIndex(d => d == m.Week);
                                        if (weekIndexFirst != 0 && weekIndexSecond != 0)
                                        {
                                            AddConflictMatch(VisitorHomeBoth.HomeOnly,m);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public override string[] GetTextDescription()
        {
            List<string> result = new List<string>();
            return result.ToArray();
        }
    }
    class ConstraintDifferentGroupsOnSameDay : Constraint
    {
        public ConstraintDifferentGroupsOnSameDay(Club cl)
        {
            club = cl;
            VisitorAlso = false;
            name = "Teams in verschillende groupen spelen op dezelfde dag";
        }
        public override List<Club> RelatedClubs()
        {
            List<Club> clubs = new List<Club>();
            clubs.Add(club);
            if (club.groupingWithClub != null) clubs.Add(club);
            return base.RelatedClubs();
        }
        public override void Evaluate(Model model)
        {
            conflict_cost = 0;
            conflictMatches.Clear();
            List<Match> all_matches = new List<Match>();
            List<Team> listTeams = new List<Team>(club.teams);
            if (club.groupingWithClub != null)
                listTeams.AddRange(club.groupingWithClub.teams);
            for (int i = 0; i < listTeams.Count; i++)
            {
                Team team1 = listTeams[i];
                Poule poule1 = team1.poule;
                if (poule1 != null)
                {
                    if (poule == null || poule == poule1)
                    {
                        //if (poule1.evaluated)
                        {
                            all_matches.AddRange(team1.poule.matches.FindAll(m => m.homeTeam == team1 && m.RealMatch() && m.homeTeam.group!= TeamGroups.NoGroup));
                        }
                    }
                }
            }
            all_matches.Sort(delegate(Match m1, Match m2) { return m1.datetime.Date.CompareTo(m2.datetime.Date); });
            int s = 0;
            while(s<all_matches.Count)
            {
                DateTime date = all_matches[s].datetime.Date;
                int e=s;
                while(e<all_matches.Count && all_matches[e].datetime.Date == date) e++;
                int cX = 0;
                int cY = 0;
                bool xUnchangeable = false;
                bool yUnchangeable = false;
                for(int j=s;j<e;j++)
                {
                    Match m = all_matches[j];
                    if (m.homeTeam.poule.optimizable == false)
                    {
                        if (m.homeTeam.group == TeamGroups.GroupX) xUnchangeable = true;
                        if (m.homeTeam.group == TeamGroups.GroupY) yUnchangeable = true;
                    }
                    if(m.homeTeam.group == TeamGroups.GroupX && (m.visitorTeam.club != club || m.visitorTeam.group == TeamGroups.GroupX)) cX++;
                    if(m.homeTeam.group == TeamGroups.GroupY && (m.visitorTeam.club != club || m.visitorTeam.group == TeamGroups.GroupY)) cY++;
                }
                if(cX>0 && cY>0)
                {
                    // Welke group gaan we als 'verkeerd' bestempelen.
                    bool x = false;
                    if (cX < cY) x = true;
                    // Indien of X of Y niet veranderd kan worden, dan is automatisch de andere groep 'verkeerd'
                    if (yUnchangeable && xUnchangeable == false) x = true;
                    if (xUnchangeable && yUnchangeable == false) x = false;
                    for(int j=s;j<e;j++)
                    {
                        Match m = all_matches[j];
                        if (m.poule.serie.importance == Serie.ImportanceLevels.High) cost = MySettings.Settings.DifferentGroupsOnSameDayCostHigh;
                        if (m.poule.serie.importance == Serie.ImportanceLevels.Low) cost = MySettings.Settings.DifferentGroupsOnSameDayCostLow;
                        if (m.poule.serie.importance == Serie.ImportanceLevels.Medium) cost = MySettings.Settings.DifferentGroupsOnSameDayCostMedium;
                        if (x && m.homeTeam.group == TeamGroups.GroupX && m.homeTeam.club == club)
                        {
                            
                            for (int k = s; k < e; k++)
                            {
                                Match m1 = all_matches[k];
                                if (m1.homeTeam.group == TeamGroups.GroupY && m.Overlapp(m1))
                                {
                                    cost += MySettings.Settings.DifferentGroupsOnSameDayOverlappingExtraCost; // indien de tijden overlappend zijn extra kosten
                                }
                            }
                            AddConflictMatch(VisitorHomeBoth.HomeOnly, m);
                        }
                        if (x == false && m.homeTeam.group == TeamGroups.GroupY && m.homeTeam.club == club)
                        {
                            
                            for (int k = s; k < e; k++)
                            {
                                Match m1 = all_matches[k];
                                if (m1.homeTeam.group == TeamGroups.GroupX && m.Overlapp(m1))
                                {
                                    cost += MySettings.Settings.DifferentGroupsOnSameDayOverlappingExtraCost; // indien de tijden overlappend zijn extra kosten
                                }
                            }
                            AddConflictMatch(VisitorHomeBoth.HomeOnly, m);
                        }
                    }
                        
                }
                s = e;
            }
        }
        public override string[] GetTextDescription()
        {
            List<string> result = new List<string>();
            return result.ToArray();
        }
    }
    class ConstraintNotAllInSameHomeDay : Constraint
    {
        public ConstraintNotAllInSameHomeDay(Club club)
        {
            this.club = club;
            name = "Niet iedereen in zelfde thuis-weekend";
            VisitorAlso = false;
            cost = MySettings.Settings.NotAllInSameWeekCost;
        }
        public override void Evaluate(Model model)
        {
            // TODO: skip if poule is not related to club.
            conflict_cost = 0;
            conflictMatches.Clear();
            EvaluateForDay(model, DayOfWeek.Saturday);
            EvaluateForDay(model, DayOfWeek.Sunday);
        }
        public void EvaluateForDay(Model model, DayOfWeek day)
        {
            List<Team> listTeams = new List<Team>();
            listTeams.AddRange(club.teams.FindAll(t =>t.defaultDay == day));
            if (club.groupingWithClub != null) listTeams.AddRange(club.groupingWithClub.teams.FindAll(t => t.defaultDay == day));

            bool X = false;
            bool Y = false;
            TeamGroups targetGroup = TeamGroups.NoGroup;
            bool optimize = (poule == null);
            foreach (Team team in listTeams)
            {
                if (team.group == TeamGroups.GroupX) X = true;
                if (team.group == TeamGroups.GroupY) Y = true;
                if (team.poule == poule) optimize = true;
            }
            if (optimize)
            {
                if (X == true && Y == false)
                {
                    targetGroup = TeamGroups.GroupX;
                }
                if (X == false && Y == true)
                {
                    targetGroup = TeamGroups.GroupY;
                }
                if (targetGroup != TeamGroups.NoGroup)
                {
                    SortedList<Week, List<Match>> CountPerWeek = new SortedList<Week, List<Match>>();
                    foreach (Team team in listTeams)
                    {
                        if (team.poule != null)
                        {
                            if (team.group == targetGroup)
                            {
                                Poule po = team.poule;
                                foreach (Match match in po.matches)
                                {
                                    if (match.RealMatch())
                                    {
                                        if (CountPerWeek.ContainsKey(match.Week) == false) CountPerWeek.Add(match.Week, new List<Match>());
                                        if (match.homeTeam.club == club) CountPerWeek[match.Week].Add(match);
                                    }
                                }
                            }
                        }
                    }
                    List<List<Match>> sortedCounts = CountPerWeek.Values.ToList();
                    sortedCounts.Sort(delegate(List<Match> l1, List<Match> l2)
                    {
                        // ensure that the series that cannot be optimized weight more in selecting the weekends
                        int l1count = l1.Count;
                        foreach (Match match in l1)
                        {
                            if (match.poule.optimizable == false) l1count += 10;
                        }
                        int l2count = l2.Count;
                        foreach (Match match in l2)
                        {
                            if (match.poule.optimizable == false) l2count += 10;
                        }
                        return l1count.CompareTo(l2count);
                    });
                    int week_cost = cost;
                    for (int i = 0; i < sortedCounts.Count / 2; i++)
                    {
                        foreach (Match match in sortedCounts[i])
                        {
                            this.AddConflictMatch(VisitorHomeBoth.HomeOnly, match);
                        }
                        if (sortedCounts[i].Count > 0)
                        {
                            this.conflict_cost += week_cost;
                            week_cost *= 2;
                        }

                    }
                }
            }
        }
        public override string[] GetTextDescription()
        {
            List<string> result = new List<string>();
            return result.ToArray();
        }
    }


    class ConstraintPlayAtSameTime : Constraint
    {
        public ConstraintPlayAtSameTime(Club cl)
        {
            club = cl;
            VisitorAlso = false;
            name = "Teams spelen op hetzelfde uur";
            cost = MySettings.Settings.PlayAtSameTime;
        }
        public override void Evaluate(Model model)
        {
            conflict_cost = 0;
            conflictMatches.Clear();
            foreach (Team team1 in club.teams)
            {
                Poule poule1 = team1.poule;
                if (poule1 != null && team1.NotAtSameTime != null)
                {
                    if (poule == null || poule == poule1)
                    {
                        if (poule1.evaluated)
                        {
                            List<Match> team1Matches = team1.poule.matches.FindAll(m => m.homeTeam == team1 || m.visitorTeam == team1);
                            team1Matches.Sort(delegate(Match m1, Match m2) { return m1.datetime.CompareTo(m2.datetime); });
                            Team team2 = team1.NotAtSameTime;
                            if (team2.poule != null)
                            {
                                if (team2.poule.evaluated)
                                {
                                    List<Match> team2Matches = team2.poule.matches.FindAll(m => m.homeTeam == team2 || m.visitorTeam == team2);
                                    team2Matches.Sort(delegate(Match m1, Match m2) { return m1.datetime.CompareTo(m2.datetime); });

                                    Poule poule2 = team2.poule;
                                    int i1 = 0;
                                    int i2 = 0;


                                    while (i1 < team1Matches.Count && i2 < team2Matches.Count)
                                    {
                                        Match m1 = team1Matches[i1];
                                        Match m2 = team2Matches[i2];
                                        if (m1.datetime.Date < m2.datetime.Date || m1.visitorTeam.club == m1.homeTeam.club)
                                        {
                                            i1++;
                                        }
                                        else if (m2.datetime.Date < m1.datetime.Date || m2.visitorTeam.club == m2.homeTeam.club)
                                        {
                                            i2++;
                                        }
                                        else
                                        {
                                            if (m1.RealMatch() && m2.RealMatch())
                                            {
                                                double delta = MySettings.Settings.NormalLengthMatch -0.01; // normale lengte wedstrijd
                                                if (m1.homeTeam.club != m2.homeTeam.club) delta += MySettings.Settings.TravelingTime; // extra reistijd
                                                DateTime st1 = m1.datetime;
                                                DateTime en1 = st1.AddHours(delta);
                                                st1 = st1.AddHours(-m1.serie.extraTimeBefore); // reserve wedstrijd
                                                DateTime st2 = m2.datetime;
                                                DateTime en2 = st2.AddHours(delta);
                                                st2 = st2.AddHours(-m2.serie.extraTimeBefore); // reserve wedstrijd
                                                if (st1 <= en2 && en1 >= st2)
                                                {
                                                    this.AddConflictMatch(VisitorHomeBoth.HomeOnly, m1, m2);
                                                }
                                            }
                                            i1++;
                                            i2++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public override string[] GetTextDescription()
        {
            List<string> result = new List<string>();
            return result.ToArray();
        }
    }
    class ConstraintClubTooManyConflicts : Constraint
    {
        public ConstraintClubTooManyConflicts(Club cl)
        {
            club = cl;
            VisitorAlso = false;
            name = "Relatief veel conflict tov teams";
        }
        public override void Evaluate(Model model)
        {
            conflict_cost = 0;
            conflictMatches.Clear();
            if (club.conflict > 0)
            {
                conflict_cost += Math.Max(0, club.conflict - (club.teams.Count - 1)) * MySettings.Settings.TooManyConflictsPerClubCosts;
            }
        }
        public override string[] GetTextDescription()
        {
            List<string> result = new List<string>();
            return result.ToArray();
        }
    }

    class ConstraintTeamTooManyConflicts : Constraint
    {
        public ConstraintTeamTooManyConflicts(Team team)
        {
            this.team = team;
            poule = team.poule;
            VisitorAlso = false;
            name = "Relatief veel conflict tov # thuismatchen";
        }
        public override void Evaluate(Model model)
        {
            conflict_cost = 0;
            conflictMatches.Clear();
            int maxConflicts = 0;
            if (team.poule != null)
            {
                maxConflicts = ((team.poule.teams.Count - 1) / 3);
            }
            conflict_cost += Math.Max(0, team.conflict - maxConflicts) * MySettings.Settings.TooManyConflictsPerTeamCosts;
        }
        public override string[] GetTextDescription()
        {
            List<string> result = new List<string>();
            return result.ToArray();
        }
        public override string Context
        {
            get
            {
                return team.name;
            }
        } 

    }
    class ConstraintTeamFixedNumber : Constraint
    {
        public ConstraintTeamFixedNumber(Team team)
        {
            this.team = team;
            poule = team.poule;
            VisitorAlso = false;
            name = "Team staat niet op zijn vaste index in het schema";
            cost = 0;
        }
        public override void Evaluate(Model model)
        {
            conflict_cost = 0;
            conflictMatches.Clear();
            if (team.poule != null)
            {
                if (team.fixedNumber > team.poule.teams.Count || team.fixedNumber <= 0 || team.poule.teams[team.fixedNumber - 1] != team)
                {
                    team.AddConflict(this);
                    team.poule.AddConflict(this);
                    team.club.AddConflict(this);
                    conflict_cost += MySettings.Settings.FixedIndexConstraintCost;
                 }
            }
        }
        public override string[] GetTextDescription()
        {
            List<string> result = new List<string>();
            return result.ToArray();
        }
        public override string Context
        {
            get
            {
                return team.name;
            }
        }

    }
    public abstract class SpecificConstraint : Constraint
    { }
    public class DateConstraint : SpecificConstraint
    {
        public Serie serie { get { return team.serie; } }
        public DateTime date;
        public enum HomeVisitNone { Home, Visit, Free, NotHome, NotVisit };
        public HomeVisitNone homeVisitNone;
        public DateConstraint(Team team)
        {
            this.team = team;
            this.club = team.club;
            VisitorAlso = false;
            cost = MySettings.Settings.DefaultCustomTeamConstraintCost;
            name = "Team conflict";
        }
        public override void Evaluate(Model model)
        {
            conflict_cost = 0;
            conflictMatches.Clear();
            if (team.poule != null)
            {
                Match match = team.poule.matches.Find(m => m.datetime.Date == date.Date && m.RealMatch() && (m.homeTeam == team || m.visitorTeam == team));
                switch (homeVisitNone)
                {
                    case DateConstraint.HomeVisitNone.Free:
                        if (match != null)
                        {
                            if (match.homeTeam == team) AddConflictMatch(VisitorHomeBoth.HomeOnly, match);
                            else AddConflictMatch(VisitorHomeBoth.VisitorOnly, match);
                        }
                        break;
                    case DateConstraint.HomeVisitNone.NotHome:
                        if (match != null && match.homeTeam == team) AddConflictMatch(VisitorHomeBoth.HomeOnly, match);
                        break;
                    case DateConstraint.HomeVisitNone.NotVisit:
                        if (match != null && match.visitorTeam == team) AddConflictMatch(VisitorHomeBoth.VisitorOnly, match);
                        break;
                    case DateConstraint.HomeVisitNone.Home:
                        if (match == null)
                        {
                            conflict_cost += cost;
                            team.AddConflict(this);
                            team.club.AddConflict(this);
                        }
                        else if (match.homeTeam != team)
                        {
                            AddConflictMatch(VisitorHomeBoth.VisitorOnly, match);
                        }
                        break;
                    case DateConstraint.HomeVisitNone.Visit:
                        if (match == null)
                        {
                            conflict_cost += cost;
                            team.AddConflict(this);
                            team.club.AddConflict(this);
                        }
                        else if (match.visitorTeam != team)
                        {
                            AddConflictMatch(VisitorHomeBoth.HomeOnly, match);
                        }
                        break;
                }
            }
        }
        public override string[] GetTextDescription()
        {
            List<string> result = new List<string>();
            result.Add("Team '" + team.name + "' (" + team.seriePouleName + ") van club " + team.club.name + " heeft als wens ");
            switch (homeVisitNone)
            {
                case HomeVisitNone.Free:
                    result.Add("'vrij te zijn'");
                    break;
                case HomeVisitNone.Home:
                    result.Add("'thuis te spelen'");
                    break;
                case HomeVisitNone.NotHome:
                    result.Add("'niet thuis te spelen'");
                    break;
                case HomeVisitNone.NotVisit:
                    result.Add("'niet uit te spelen'");
                    break;
                case HomeVisitNone.Visit:
                    result.Add("'uit te spelen'");
                    break;
            }
            result.Add(" op " + date.ToShortDateString());

            return result.ToArray();
        }
    }
    public class TeamConstraint : SpecificConstraint
    {
        public Serie serie { get { return team.serie; } }
        public DateTime date;
        public enum What { HomeInSameWeekend, HomeOnSameDay, HomeNotInSameWeekend, HomeNotOnSameDay };
        public What what;
        public Team team2;
        public TeamConstraint(Team team1, Team team2, What what)
        {
            this.team = team1;
            this.team2 = team2;
            VisitorAlso = true;
            cost = MySettings.Settings.DefaultCustomTeamConstraintCost;
            name = ToText(what);
        }
        public override bool RelatedTo(List<Team> teams)
        {
            return teams.Contains(this.team) || teams.Contains(this.team2);
        }
        public override List<Club> RelatedClubs()
        {
            List<Club> clubs = new List<Club>();
            if (team != null) clubs.Add(team.club);
            if (team2 != null) clubs.Add(team2.club);
            return clubs;
        }
        public override List<Poule> RelatedPoules()
        {
            List<Poule> poules = new List<Poule>();
            if (team.poule != null) poules.Add(team.poule);
            if (team2.poule != null) poules.Add(team2.poule);
            return poules;
        }
        public override List<Team> RelatedTeams()
        {
            List<Team> teams = new List<Team>();
            if (team != null) teams.Add(team);
            if (team2 != null) teams.Add(team2);
            return teams;
        }
        public override void Evaluate(Model model)
        {
            conflict_cost = 0;
            conflictMatches.Clear();
            if (team.poule != null && team2.poule != null)
            {
                List<Match> matches = team.poule.matches.FindAll(m => m.homeTeam == team);
                matches.Sort((m1, m2) => { return m1.datetime.CompareTo(m2.datetime); });
                List<Match> matches2 = team2.poule.matches.FindAll(m => m.homeTeam == team2);
                matches2.Sort((m1, m2) => { return m1.datetime.CompareTo(m2.datetime); });
                List<Match>.Enumerator e = matches.GetEnumerator();
                List<Match>.Enumerator e2 = matches2.GetEnumerator();
                bool valid = e.MoveNext();
                bool valid2 = e2.MoveNext();
                do
                {
                    while (valid && valid2 && e.Current.datetime.Date == e2.Current.datetime.Date)
                    {
                        valid = e.MoveNext();
                        valid2 = e2.MoveNext();
                    }
                    if (valid && (valid2 == false || e.Current.datetime.Date <= e2.Current.datetime.Date)) valid = e.MoveNext();
                    else if (valid2 && (valid == false || e2.Current.datetime.Date <= e.Current.datetime.Date)) valid2 = e2.MoveNext();
                    if (valid && valid2 )
                    {
                        if (e.Current.RealMatch() && e2.Current.RealMatch())
                        {
                            switch (what)
                            {
                                case What.HomeInSameWeekend:
                                    if (e.Current.Week.Saturday.Date < e2.Current.Week.Saturday.Date) AddConflictMatch(VisitorHomeBoth.HomeOnly, e.Current);
                                    if (e.Current.Week.Saturday.Date > e2.Current.Week.Saturday.Date) AddConflictMatch(VisitorHomeBoth.HomeOnly, e2.Current);
                                    break;
                                case What.HomeOnSameDay:
                                    if (e.Current.datetime.Date < e2.Current.datetime.Date) AddConflictMatch(VisitorHomeBoth.HomeOnly, e.Current);
                                    if (e.Current.datetime.Date > e2.Current.datetime.Date) AddConflictMatch(VisitorHomeBoth.HomeOnly, e2.Current);
                                    break;
                                case What.HomeNotInSameWeekend:
                                    if (e.Current.Week.Saturday.Date == e2.Current.Week.Saturday.Date) AddConflictMatch(VisitorHomeBoth.HomeOnly, e.Current, e2.Current);
                                    break;
                                case What.HomeNotOnSameDay:
                                    if (e.Current.datetime.Date == e2.Current.datetime.Date) AddConflictMatch(VisitorHomeBoth.HomeOnly, e.Current, e2.Current);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        if (valid)
                        {
                            if (e.Current.RealMatch())
                            {
                                switch (what)
                                {
                                    case What.HomeInSameWeekend:
                                    case What.HomeOnSameDay:
                                        AddConflictMatch(VisitorHomeBoth.HomeOnly, e.Current);
                                        break;
                                    case What.HomeNotInSameWeekend:
                                    case What.HomeNotOnSameDay:
                                        break;
                                }
                            }
                        }
                        if (valid2)
                        {
                            if (e2.Current.RealMatch())
                            {
                                switch (what)
                                {
                                    case What.HomeInSameWeekend:
                                    case What.HomeOnSameDay:
                                        AddConflictMatch(VisitorHomeBoth.HomeOnly, e2.Current);
                                        break;
                                    case What.HomeNotInSameWeekend:
                                    case What.HomeNotOnSameDay:
                                        break;
                                }
                            }

                        }
                    }
                } while (valid || valid2);
            }
        }
        private string ToText(What what)
        {
            switch (what)
            {
                case What.HomeInSameWeekend:
                    return "In hetzelfde weekend thuis spelen";
                case What.HomeNotInSameWeekend:
                    return "Niet in hetzelfde weekend thuis spelen";
                case What.HomeNotOnSameDay:
                    return "Niet op dezelfde dag thuis te spelen";
                case What.HomeOnSameDay:
                    return "Op dezelfde dag thuis spelen";
            }
            return "";
        }
        public override string[] GetTextDescription()
        {
            List<string> result = new List<string>();
            result.Add("Team '" + team.name + "' (" + team.seriePouleName + ") van club " + team.club.name + " en ");
            result.Add("Team '" + team2.name + "' (" + team2.seriePouleName + ") van club " + team2.club.name + " willen ");
            result.Add(ToText(what));
            return result.ToArray();
        }
    }



}
