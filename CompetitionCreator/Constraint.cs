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
                if (team != null) return team.name;
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
            result.Add(team.sporthal.name + "is niet beschikbaar op: ");
            foreach (DateTime dt in team.sporthal.NotAvailable)
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
                return name + " - " + team.name;
            }
        }
        public ConstraintNoPouleAssigned(Team team)
        {
            name = "No poule assigned to team";
            this.team = team;
            cost = MySettings.Settings.NoPouleAssignedCost;
        }
        public override void Evaluate(Model model)
        {
            conflict_cost = 0;
            conflictMatches.Clear();
            ClearErrors();
            conflict_cost += cost;
            AddError(string.Format("Team {0} (serie:{1}) has no poule assigned", team.name, team.serie.name));
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
    class ConstraintNotAllInSameHomeDay : Constraint
    {
        List<Team> listTeams;
        public ConstraintNotAllInSameHomeDay(List<Team> teams)
        {
            this.listTeams = teams;
            name = "Teams in same group play on different days";
            VisitorAlso = false;
            cost = MySettings.Settings.NotAllInSameWeekCost;
        }
        public override bool RelatedTo(List<Team> teams)
        {
            return Team.Overlap(this.listTeams, teams);
        }
        public override List<Club> RelatedClubs()
        {
            List<Club> relatedClubs = new List<Club>();
            foreach (Team team in listTeams)
            {
                if (relatedClubs.Contains(team.club) == false) relatedClubs.Add(team.club);
            }
            return relatedClubs;
        }
        public override List<Team> RelatedTeams()
        {
            return listTeams;
        }

        public override void Evaluate(Model model)
        {
            // TODO: skip if poule is not related to club.
            conflict_cost = 0;
            conflictMatches.Clear();
            int maxTeams = 0;
            SortedList<MatchWeek, List<Match>> CountPerWeek = new SortedList<MatchWeek, List<Match>>();
            foreach (Team team in listTeams)
            {
                if (team.poule != null)
                {
                    if (team.poule.TeamCount > maxTeams) maxTeams = team.poule.TeamCount;
                    foreach (Match match in team.poule.matches)
                    {
                        if (match.RealMatch() && match.homeTeam == team)
                        {
                            if (CountPerWeek.ContainsKey(match.Week) == false) CountPerWeek.Add(match.Week, new List<Match>());
                            CountPerWeek[match.Week].Add(match);
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
            for (int i = 0; i < sortedCounts.Count - maxTeams; i++) // 12 teams, 11 thuiswedstrijden, dus hier is 1 extra wedstrijd toegestaan
            {
                foreach (Match match in sortedCounts[i])
                {
                    this.AddConflictMatch(VisitorHomeBoth.HomeOnly, match);
                }
                if (sortedCounts[i].Count > 0)
                {
                    this.conflict_cost += week_cost;
                    week_cost *= 2;
                    conflict_cost += sortedCounts[i].Count;
                }
            }
        }
        public override string[] GetTextDescription()
        {
            List<string> result = new List<string>();
            result.Add("Groep:");
            foreach(Team t in listTeams)
            {
                result.Add(" - " + t.serie.name.ToString() + " - " + t.name.ToString()+" ("+t.Id.ToString()+")");
            }
            return result.ToArray();
        }
    }


    class ConstraintPlayAtSameTime : Constraint
    {
        public ConstraintPlayAtSameTime(Team cl)
        {
            team = cl;
            VisitorAlso = false;
            name = "Teams spelen op hetzelfde uur";
            cost = MySettings.Settings.PlayAtSameTime;
        }
        public override void Evaluate(Model model)
        {
            conflict_cost = 0;
            conflictMatches.Clear();
            List<Match> team1Matches = team.poule.matches.FindAll(m => m.homeTeam == team || m.visitorTeam == team);
            team1Matches.Sort(delegate(Match m1, Match m2) { return m1.datetime.CompareTo(m2.datetime); });
            Team team2 = team.NotAtSameTime;
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

    class ConstraintGrouping : Constraint
    {
        public List<Team> GroupA = new List<Team>();
        public List<Team> GroupB = new List<Team>();
        public ConstraintGrouping()
        {
            VisitorAlso = false;
            name = "Teams in verschillende groupen!";
        }
        public override bool RelatedTo(List<Team> teams)
        {
            foreach(Team team in teams)
            {
                if(GroupA.Contains(team) || GroupB.Contains(team)) return true;
            }
            return false;
        }

        public override List<Club> RelatedClubs()
        {
            List<Club> relatedClubs = new List<Club>();
            foreach (Team team in GroupA)
            {
                if (relatedClubs.Contains(team.club) == false) relatedClubs.Add(team.club);
            }
            foreach (Team team in GroupB)
            {
                if (relatedClubs.Contains(team.club) == false) relatedClubs.Add(team.club);
            }
            return relatedClubs;
        }
        public override List<Team> RelatedTeams()
        {
            return new List<Team>(GroupA.Union(GroupB));
        }
        enum where { none, A, B };
        private bool AddTeamIfNeeded(TeamConstraint.What what, Team team1, Team team2, List<Team> GroupA, List<Team> GroupB)
        {
            bool result = false;
            if (team1.defaultDay == team2.defaultDay)
            {
                if (GroupA.Contains(team1))
                {
                    switch (what)
                    {
                        case TeamConstraint.What.HomeOnSameDay:
                            if (GroupA.Contains(team2) == false) GroupA.Add(team2);
                            result = true;
                            break;
                        case TeamConstraint.What.HomeNotOnSameDay:
                            if (GroupB.Contains(team2) == false) GroupB.Add(team2);
                            result = true; 
                            break;
                        case TeamConstraint.What.HomeInSameWeekend: // also take these into account
                            if (GroupA.Contains(team2) == false) GroupA.Add(team2);
                            result = true; 
                            break;
                        case TeamConstraint.What.HomeNotInSameWeekend: // also take these into account
                            if (GroupB.Contains(team2) == false) GroupB.Add(team2);
                            result = true; 
                            break;
                    }
                }
            }
            return result;
        }
        private bool AddTeamIfNeededWeekend(TeamConstraint.What what, Team team1, Team team2, List<Team> GroupA, List<Team> GroupB)
        {
            bool result = false;
            if (GroupA.Contains(team1))
            {
                switch (what)
                {
                    case TeamConstraint.What.HomeInSameWeekend:
                        if (GroupA.Contains(team2) == false) GroupA.Add(team2);
                        result = true; 
                        break;
                    case TeamConstraint.What.HomeNotInSameWeekend:
                        if (GroupB.Contains(team2) == false) GroupB.Add(team2);
                        result = true; 
                        break;
                }
            }
            return result;
        }
        public bool AddTeamConstraintDay(TeamConstraint tc)
        {
            bool result = false;
            result = result || AddTeamIfNeeded(tc.what, tc.team1, tc.team2, GroupA, GroupB);
            result = result || AddTeamIfNeeded(tc.what, tc.team1, tc.team2, GroupB, GroupA);
            result = result || AddTeamIfNeeded(tc.what, tc.team2, tc.team1, GroupA, GroupB);
            result = result || AddTeamIfNeeded(tc.what, tc.team2, tc.team1, GroupB, GroupA);
            // check overlap
            if (GroupA.Find(t => GroupB.Contains(t)) != null)
            {
                error = true;
            }
            return result;
        }
        public bool AddTeamConstraintWeekend(TeamConstraint tc)
        {
            bool result = false;
            result = result || AddTeamIfNeededWeekend(tc.what, tc.team1, tc.team2, GroupA, GroupB);
            result = result || AddTeamIfNeededWeekend(tc.what, tc.team1, tc.team2, GroupB, GroupA);
            result = result || AddTeamIfNeededWeekend(tc.what, tc.team2, tc.team1, GroupA, GroupB);
            result = result || AddTeamIfNeededWeekend(tc.what, tc.team2, tc.team1, GroupB, GroupA);
            // check overlap
            if (GroupA.Find(t => GroupB.Contains(t)) != null)
            {
                error = true;
            }
            return result;
        }
        private int calculateCost(List<Match> overlap1, List<Match> overlap2)
        {
            int costOverlap = 0;
            foreach (Match m in overlap1)
            {
                if (m.serie.importance == Serie.ImportanceLevels.High) costOverlap += MySettings.Settings.DifferentGroupsOnSameDayCostHigh;
                if (m.serie.importance == Serie.ImportanceLevels.Low) costOverlap += MySettings.Settings.DifferentGroupsOnSameDayCostLow;
                if (m.serie.importance == Serie.ImportanceLevels.Medium) costOverlap += MySettings.Settings.DifferentGroupsOnSameDayCostMedium;
                costOverlap += overlap2.Count(m1 => m1.Overlapp(m)) * MySettings.Settings.DifferentGroupsOnSameDayOverlappingExtraCost; // extra cost when there is overlap
                if (m.serie.imported) costOverlap += 100; // match cannot be changed.
            }
            return costOverlap;
        }
        public override void Evaluate(Model model)
        {
            conflictMatches.Clear();
            conflict_cost = 0;
            cost = 0;
            List<Match> AMatches = new List<Match>();
            List<Match> BMatches = new List<Match>();
            foreach (Team team in GroupA)
            {
                if(team.poule != null) AMatches.AddRange(team.poule.matches.FindAll(m => m.homeTeam == team && m.RealMatch()));
            }
            foreach (Team team in GroupB)
            {
                if (team.poule != null) BMatches.AddRange(team.poule.matches.FindAll(m => m.homeTeam == team && m.RealMatch()));
            }
            if(AMatches.Count >0 && BMatches.Count >0)
            {
                // Walk through the weeks of the AMatches. This can be done since we are looking for the overlap
                AMatches.Sort((x, y) => x.datetime.CompareTo(y.datetime));
                MatchWeek week = AMatches[0].Week;
                while(week != null)
                {
                    List<Match> overlapA = AMatches.FindAll(m => m.Week == week);
                    List<Match> overlapB = BMatches.FindAll(m => m.Week == week);
                    if(overlapA.Count >0 && overlapB.Count >0)
                    {
                        // calculate the cost of each overlap to determine which is highest.
                        int costA = calculateCost(overlapA, overlapB);
                        int costB = calculateCost(overlapB, overlapA);
                        if(costA > costB)
                        {   // show overlapB as conflicts
                            foreach(Match m in overlapB) AddConflictMatch(VisitorHomeBoth.HomeOnly,m);
                            conflict_cost += costB;
                        } else
                        {   // show overlapA as conflicts
                            foreach(Match m in overlapA) AddConflictMatch(VisitorHomeBoth.HomeOnly,m);
                            conflict_cost += costA;
                        }
                    }
                    // looking for the next week
                    MatchWeek nextWeek = null;
                    foreach(Match m in AMatches)
                    {
                        if(m.Week > week)
                        {
                            nextWeek = m.Week;
                            break;
                        }
                    }
                    week = nextWeek;
                } 
            }
            AMatches.Sort((m1, m2) => m1.datetime.CompareTo(m2.datetime));
            AMatches.Sort((m1, m2) => m1.datetime.CompareTo(m2.datetime));

        }
        private void CheckAtSameDay()
        {

        }
        public override string[] GetTextDescription()
        {
            List<string> result = new List<string>();
            result.Add("Groep A:");
            foreach(Team t in GroupA)
            {
                result.Add(" - " + t.serie.name.ToString() + " - " + t.name.ToString()+" ("+t.Id.ToString()+")");
            }
            result.Add("Groep B:");
            foreach (Team t in GroupB)
            {
                result.Add(" - " + t.serie.name.ToString() + " - " + t.name.ToString() + " (" + t.Id.ToString() + ")");
            }
            return result.ToArray();
        }
    }


}
