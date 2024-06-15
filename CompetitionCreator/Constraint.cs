using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeifenLuo.WinFormsUI.Docking;

namespace CompetitionCreator
{
    public enum TeamGroups { GroupX, GroupY, NoGroup };
    public static class Extensions
    {
        public static string ToStringCustom(this TeamGroups group)
        {
            if (group == TeamGroups.GroupX) return "X";
            else if (group == TeamGroups.GroupY) return "Y";
            else return "-";
        }
    }

    public class Conflict
    {
        List<ConflictAdmin> admin = new List<ConflictAdmin>();
        public Constraint con;
        public int Cost;

        public Conflict(Constraint c, int cost)
        {
            con = c;
            c.conflicts.Add(this);
            Cost = cost;
        }
        public void Delete()
        {
            lock (this)
            {
                foreach (var adm in admin)
                    adm.RemoveConflict(this);
            }
        }
        public void AddAdm(ConflictAdmin adm)
        {
            lock (this)
            {
                admin.Add(adm);
            }
        }
        ~Conflict()
        {
            Delete();
        }
    }

    public class ConflictAdmin
    {
        protected List<Conflict> conflictList = new List<Conflict>();
        public List<Conflict> GetConflictList()
        {
            lock (this)
            {
                return new List<Conflict>(conflictList);
            }
        }

        public List<Constraint> constraintList { get { lock (this) { return conflictList.Select(con => con.con).ToList(); } } }
        public void RemoveConflict(Conflict con)
        {
            lock (this)
            {
                conflictList.Remove(con);
            }
        }
        public int conflict
        {
            get
            {
                lock (this)
                { return conflictList.Count(); }
            }
        }
        public int conflict_cost
        {
            get
            {
                lock (this)
                { return conflictList.Sum(c => c.Cost); }
            }
        }

        public void AddConflict(Conflict con)
        {
            lock (this)
            {
                if (con != null)
                    conflictList.Add(con);
                else
                    conflictList.Add(con);
                con.AddAdm(this);
            }
        }
    }

    public abstract class Constraint
    {
        private List<Poule> related = new List<Poule>();
        protected void AddRelated(Poule admin)
        {
            if (related.Contains(admin) == false)
                related.Add(admin);
        }

        public bool IsRelated(Poule poule)
        {
            if (related.Count == 0)
                return true;
            return related.Contains(poule);
        }

        public bool VisitorAlso = true;
        List<Error> errors = new List<Error>();
        public List<Error> GetErrors()
        {
            return errors;
        }
        protected void ClearErrors()
        {
            errors.Clear();
        }
        protected void AddError(Error error)
        {
            errors.Add(error);
        }
        protected void AddError(string str, string help = null)
        {
            Error error = new Error();
            error.text = str;
            if (help != null)
                error.AddHelpHtml(help);
            AddError(error);
        }
        protected string[] ErrorInfo()
        {
            List<string> strings = new List<string>();
            foreach (var error in errors)
            {
                strings.Add(error.text);
            }
            return strings.ToArray();
        }
        public Club club { get; set; }
        public Poule poule { get; set; }
        public Team team { get; set; }
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
        public List<Conflict> conflicts = new List<Conflict>();
        public void ClearConflicts()
        {
            foreach (var c in conflicts)
                c.Delete();
            conflicts.Clear();
        }
        protected void AddConflictMatch(VisitorHomeBoth who, params Match[] matches)
        {
            for (int i = 0; i < matches.Length; i++)
            {
                var conflict = new Conflict(this, conflict_cost);
                conflict_cost += cost;

                if (conflictMatches.Contains(matches[i]) == false)
                {
//                    if (matches[i].HasConflict() == false)
                    {
                        conflictMatches.Add(matches[i]);
                        matches[i].AddConflict(conflict);
                        matches[i].Week.AddConflict(conflict);
                        matches[i].poule.AddConflict(conflict);
                        matches[i].poule.serie.AddConflict(conflict);
                        if (who != VisitorHomeBoth.VisitorOnly && matches[i].homeTeam.RealTeam()) matches[i].homeTeam.AddConflict(conflict);
                        if (who != VisitorHomeBoth.HomeOnly && matches[i].visitorTeam.RealTeam()) matches[i].visitorTeam.AddConflict(conflict);
                        if (club != null)
                        {
                            club.AddConflict(conflict);
                        }
                        else
                        {
                            if (who == VisitorHomeBoth.Both)
                            {
                                matches[i].homeTeam.club.AddConflict(conflict);
                                if (matches[i].homeTeam.club != matches[i].visitorTeam.club) matches[i].visitorTeam.club.AddConflict(conflict);
                            }
                            else
                            {
                                if (who != VisitorHomeBoth.VisitorOnly) matches[i].homeTeam.club.AddConflict(conflict);
                                if (who != VisitorHomeBoth.HomeOnly) matches[i].visitorTeam.club.AddConflict(conflict);
                            }

                        }
                    }
                }

            }
        }
        public void Sort()
        {
            conflictMatches.Sort(delegate (Match m1, Match m2) { return m1.datetime.CompareTo(m2.datetime); });
        }
    }
    class ConstraintSporthallNotAvailable : Constraint
    {
        public ConstraintSporthallNotAvailable(Team team)
        {
            name = "Sporthall not available";
            this.team = team;
            VisitorAlso = false;
            cost = MySettings.Settings.SporthalNotAvailableCostLow;
            AddRelated(team.poule);
        }
        public override void Evaluate(Model model)
        {
            ClearConflicts();
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
                        DateTime dt = match.dateday.Date;
                        if (match.homeTeam == team &&
                            match.RealMatch() &&
                            team.sporthal.NotAvailable.Contains(dt) == true)
                        {
                            cost = MySettings.Settings.SporthalNotAvailableCostMedium;
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
                result.Add(string.Format("{0} - {1}", dt.DayOfWeek, dt.Date));
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
            AddRelated(poule);
        }
        public override void Evaluate(Model model)
        {
            ClearConflicts();

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
                            double mindays = (((poule.weeks.Count) * 7) / 6);
                            days = mindays - days;
                            if (days > 0)
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

    class ConstraintSchemaTooManyHome : Constraint
    {
        public override string Title
        {
            get
            {
                return name + " - " + poule.serie.name + poule.name;
            }
        }
        public ConstraintSchemaTooManyHome(Poule poule)
        {
            name = "Teveel thuiswedstrijden";
            this.poule = poule;
            AddRelated(poule);
        }
        public override void Evaluate(Model model)
        {
            ClearConflicts();

            conflict_cost = 0;
            conflictMatches.Clear();
            int[] counts = new int[poule.maxTeams];
            if (poule.evaluated)
            {
                foreach (Match m in poule.matches)
                {
                    counts[m.homeTeamIndex]++;
                }
                int upperLimit = (int)Math.Ceiling(((double)poule.matches.Count) / poule.TeamCount);
                for (int i = 0; i < poule.maxTeams; i++)
                {
                    if (counts[i] > upperLimit)
                    {
                        cost = MySettings.Settings.MatchTooManyHomeMatches;
                        Match match = poule.matches.First(m => m.homeTeamIndex == i);
                        AddConflictMatch(VisitorHomeBoth.HomeOnly, match);
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
            name = "Too many home or visit matches after each other";
            this.poule = poule;
            this.cost = MySettings.Settings.MatchTooManyAfterEachOtherCostLow;
            AddRelated(poule);
        }
        public override void Evaluate(Model model)
        {
            ClearConflicts();

            conflict_cost = 0;
            cost = MySettings.Settings.MatchTooManyAfterEachOtherCostLow;
            conflictMatches.Clear();
            if (poule.evaluated)
            {
                List<Match> sortedMatches = new List<Match>(poule.matches);
                sortedMatches.Sort(delegate (Match m1, Match m2) { return m1.datetime.CompareTo(m2.datetime); });
                var temp = sortedMatches.Where((Match m) => m.RealMatch());
                sortedMatches = temp.ToList();
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
                            if (matchNr < poule.TeamCount) homeFirstHalf.Add(match);
                            else homeSecondHalf.Add(match);

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
                    if (maxMatches.Count >= MySettings.Settings.MatchTooManyAfterEachOther - 1)
                        cost += 5;
                    // Ideal to spread home/visit evenly. Also for further optimisation
                    conflict_cost += Math.Max(0, homeFirstHalf.Count - ((int)((poule.TeamCount + 1) / 2)));
                    conflict_cost += Math.Max(0, homeSecondHalf.Count - ((int)((poule.TeamCount + 1) / 2)));
                    // Extreme will be punished
                    if (homeFirstHalf.Count > maxNumberHomeMatches(poule.TeamCount))
                    {
                        foreach (Match match in homeFirstHalf)
                        {
                            conflict_cost += MySettings.Settings.MatchTooManyAfterEachOtherCostMedium;
                        }
                    }
                    if (homeSecondHalf.Count > maxNumberHomeMatches(poule.TeamCount))
                    {
                        foreach (Match match in homeSecondHalf)
                        {
                            conflict_cost += MySettings.Settings.MatchTooManyAfterEachOtherCostMedium;
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
            AddRelated(poule);
        }
        public override void Evaluate(Model model)
        {
            ClearConflicts();

            //if (Number % 1000 == 0 || error == true) // Deze test niet elke keer uitvoeren.
            {
                conflict_cost = 0;
                conflictMatches.Clear();
                ClearErrors();
                if (poule.evaluated && poule.imported == false)
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
                            AddError("Poule consistency: Poule does not contain team",
                                     "The cause can be that an invalid xml is used as input, or that indicates a bug in the program. In both cases save your project and contact the author.");
                        }
                        if (t.serie.poules.Contains(poule) == false)
                        {
                            conflict_cost += cost;
                            AddError("Poule consistency: Serie does not contain poule",
                                     "The cause can be that an invalid xml is used as input, or that indicates a bug in the program. In both cases save your project and contact the author.");
                        }
                    }
                    UInt64[] days = new UInt64[poule.weeks.Count];
                    for (int i = 0; i < poule.weeks.Count; i++)
                    {
                        days[i] = 0;
                    }
                    foreach (var m in poule.matches)
                    {
                        if (m.RealMatch())
                        {
                            int index = m.weekIndex;
                            UInt32 bitMask = (1u << m.homeTeamIndex) | (1u << m.visitorTeamIndex);
                            if ((days[index] & bitMask) != 0)
                            {
                                conflict_cost += cost;
                                AddError("Poule consistency: One teams plays more than one match on a single day",
                                         "The cause can be that an invalid xml is used as input, or that indicates a bug in the program. In both cases save your project and contact the author.");
                            }
                            days[index] |= bitMask;
                        }
                    }
                    int rounds = 0;
                    for (int m1 = 0; m1 < poule.matches.Count; m1++)
                    {
                        Match match1 = poule.matches[m1];
                        if (match1.Week.round > rounds)
                            rounds = match1.Week.round;
                        if (match1.RealMatch())
                        {
                            HomeMatches[match1.homeTeam.Index]++;
                            VisitorMatches[match1.visitorTeam.Index]++;
                        }
                    }
                    double expectedMatches = ((double)((poule.TeamCount - 1) * (rounds + 1))) / 2;

                    foreach (Team t in poule.teams)
                    {
                        if (t.RealTeam())
                        {
                            if (HomeMatches[t.Index] > expectedMatches + 0.51 || HomeMatches[t.Index] < expectedMatches - 0.51)
                            {
                                AddError(string.Format("Poule consistency: Aantal thuis matches klopt niet (poule: {0}, team: {1})", poule.fullName, t.name),
                                                  "The cause can be that an invalid xml is used as input, or that indicates a bug in the program. In both cases save your project and contact the author.");
                            }
                            if (VisitorMatches[t.Index] > expectedMatches + 0.51 || VisitorMatches[t.Index] < expectedMatches - 0.51)
                            {
                                AddError(string.Format("Poule consistency: Aantal uit matches klopt niet (poule: {0}, team: {1})", poule.fullName, t.name),
                                                  "The cause can be that an invalid xml is used as input, or that indicates a bug in the program. In both cases save your project and contact the author.");
                            }
                        }
                    }
                    foreach (Team team in poule.teams)
                    {
                        if (team.RealTeam() && team.poule.matches.Count(m => m.homeTeam == team) == 0)
                        {
                            conflict_cost += cost;
                            AddError(string.Format("Internal test - poule consistency: No matches for team '{0}'", team.name),
                                     "The cause can be that an invalid xml is used as input, or that indicates a bug in the program. In both cases save your project and contact the author.");
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
            ClearConflicts();

            conflict_cost = 0;
            conflictMatches.Clear();
            ClearErrors();
            conflict_cost += cost;
            AddError(string.Format("Team {0} (serie:{1}) has no poule assigned", team.name, team.serie.name),
                     "Go to Edit/Poules and add this team to a poule, or go to Edit/Registrations and delete this team (it can always be undeleted). If it is intended that this team has no poule, optimization can run with this error.");
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
            name = "Teams of one club in one poule have to play on the first day";
            this.poule = poule;
            this.VisitorAlso = false;
            this.cost = MySettings.Settings.TwoPoulesOfSameClubInPouleCost;
            AddRelated(poule);
        }
        public override void Evaluate(Model model)
        {
            ClearConflicts();

            conflict_cost = 0;
            conflictMatches.Clear();
            if (poule.evaluated)
            {
                foreach (Team t1 in poule.teams)
                {
                    foreach (Team t2 in poule.teams)
                    {
                        if (t1 != t2)
                        {
                            if (t1.club == t2.club)
                            {
                                DateTime[] firstWeekend = new DateTime[4]; // max 4 rounds
                                for (int i = 0; i < 4; i++)
                                    firstWeekend[i] = new DateTime(9999, 1, 1);
                                Match[] firstPlayed = new Match[4];

                                foreach (Match m in poule.matches)
                                {
                                    if (m.datetime < firstWeekend[m.Week.round])
                                        firstWeekend[m.Week.round] = m.datetime;
                                    if ((m.homeTeam == t1 && m.visitorTeam == t2) ||
                                        (m.homeTeam == t2 && m.visitorTeam == t1)
                                        )
                                    {
                                        firstPlayed[m.Week.round] = m;
                                    }
                                }

                                for (int i = 0; i < 4; i++)
                                {
                                    if (firstPlayed[i] != null)
                                    {
                                        var dt = firstPlayed[i].datetime.AddDays(-3);
                                        if (dt > firstWeekend[i])
                                        {
                                            AddConflictMatch(VisitorHomeBoth.HomeOnly, firstPlayed[i]);
                                            // The earlier the better to have this game. Required for optimization
                                            conflict_cost += (int)dt.Subtract(firstWeekend[i]).TotalSeconds;
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
            // Related to
            foreach (var t in teams)
                if (t.poule != null)
                    AddRelated(t.poule);
        }
        public override void Evaluate(Model model)
        {
            ClearConflicts();

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
                        // Alleen als speeldatum toevoegen als de sporthal beschikbaar is. Anders wordt het een populaire datum.
                        if (match.RealMatch() && match.homeTeam == team && match.homeTeam.sporthal.NotAvailable.Contains(match.datetime.Date) == false && match.poule.Optimize(model))
                        {
                            if (CountPerWeek.ContainsKey(match.Week) == false) CountPerWeek.Add(match.Week, new List<Match>());
                            CountPerWeek[match.Week].Add(match);
                        }
                    }
                }
            }
            List<List<Match>> sortedCounts = CountPerWeek.Values.ToList();
            sortedCounts.Sort(delegate (List<Match> l1, List<Match> l2)
            {
                return l1.Count.CompareTo(l2.Count);
            });
            int week_cost = cost;
            //            for (int i = 0; i < sortedCounts.Count - maxTeams + 1; i++) // 11 thuiswedstrijden, precies genoeg. Oud: 12 teams, 11 thuiswedstrijden, dus hier is 1 extra wedstrijd toegestaan
            for (int i = sortedCounts.Count - maxTeams - 1; i >= 0; i--) // 11 thuiswedstrijden, precies genoeg. Oud: 12 teams, 11 thuiswedstrijden, dus hier is 1 extra wedstrijd toegestaan
            {
                foreach (Match match in sortedCounts[i])
                {
                    this.AddConflictMatch(VisitorHomeBoth.HomeOnly, match);
                }
                if (sortedCounts[i].Count > 0)
                {
                    this.conflict_cost += week_cost * sortedCounts[i].Count;
                    week_cost *= 2;
                }
            }
        }
        public override string[] GetTextDescription()
        {
            List<string> result = new List<string>();
            result.Add("Groep:");
            foreach (Team t in listTeams)
            {
                result.Add(" - " + t.serie.name.ToString() + " - " + t.name.ToString() + " (" + t.Id.ToString() + ")");
            }
            return result.ToArray();
        }
    }


    class ConstraintPlayAtSameTime : Constraint
    {
        Team team2;
        public ConstraintPlayAtSameTime(Team cl, Team cl2)
        {
            team2 = cl2;
            team = cl;
            VisitorAlso = false;
            name = "Teams on the same time";
            cost = MySettings.Settings.PlayAtSameTime;
            AddRelated(cl.poule);
            AddRelated(cl2.poule);
        }
        public override void Evaluate(Model model)
        {
            ClearConflicts();

            conflict_cost = 0;
            conflictMatches.Clear();
            List<Match> team1Matches = team.poule.matches.FindAll(m => m.homeTeam == team || m.visitorTeam == team);
            team1Matches.Sort(delegate (Match m1, Match m2) { return m1.datetime.CompareTo(m2.datetime); });
            List<Match> team2Matches = team2.poule.matches.FindAll(m => m.homeTeam == team2 || m.visitorTeam == team2);
            team2Matches.Sort(delegate (Match m1, Match m2) { return m1.datetime.CompareTo(m2.datetime); });
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
                        double delta = MySettings.Settings.NormalLengthMatch - 0.01; // normale lengte wedstrijd
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
            ClearConflicts();

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
            if (team.poule != null)
                AddRelated(poule);
        }
        public override void Evaluate(Model model)
        {
            ClearConflicts();

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
            AddRelated(poule);
        }
        public override void Evaluate(Model model)
        {
            ClearConflicts();

            conflict_cost = 0;
            conflictMatches.Clear();
            if (team.poule != null)
            {
                if (team.fixedNumber > team.poule.teams.Count || team.fixedNumber <= 0 || team.poule.teams[team.fixedNumber - 1] != team)
                {
                    var conflict = new Conflict(this, conflict_cost);
                    team.AddConflict(conflict);
                    team.poule.AddConflict(conflict);
                    team.club.AddConflict(conflict);
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
    class ConstraintTeamNaming : Constraint
    {
        public ConstraintTeamNaming(Club club)
        {
            this.club = club;
            VisitorAlso = false;
            name = "Names of the teams are not unique within the serie";
            cost = 0;
        }
        public override void Evaluate(Model model)
        {
            ClearConflicts();

            ClearErrors();
            bool found = false;
            List<Team> teams = new List<Team>();
            foreach (Team t in club.teams)
            {
                if (t.serie.imported == false)
                {
                    foreach (Team t1 in club.teams.FindAll(te => te.serie.id == t.serie.id))
                    {
                        if (t1.Id != t.Id && t1.name == t.name && t1.deleted == false && t.deleted == false)
                        {
                            teams.Add(t);
                            found = true;
                        }
                    }
                }
            }
            if (found)
            {
                string html = string.Format("Team names of club '{0}' are not unique within the serie", club.name);
                html += "<ul>";
                foreach (Team t in teams)
                {
                    html += "<li>" + t.name + " (" + t.seriePouleName + ") </li>";
                }
                html += "</ul>";
                AddError(html);
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
                return club.name;
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
            AddRelated(team.poule);
        }
        public string team1str { get { if (team != null) return team.name + " (" + team.seriePouleName + ")"; else return "? (Team removed)"; } }

        public override void Evaluate(Model model)
        {
            ClearConflicts();

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
                            var conflict = new Conflict(this, conflict_cost);
                            team.AddConflict(conflict);
                            team.club.AddConflict(conflict);
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
                            var conflict = new Conflict(this, conflict_cost);
                            team.AddConflict(conflict);
                            team.club.AddConflict(conflict);
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
            result.Add(" op " + date.ToString("yyyy-MM-dd"));

            return result.ToArray();
        }
    }

    class ConstraintGroupingAB : Constraint
    {
        Club.AB ABGroup;
        public ConstraintGroupingAB(Club cl, Club.AB AB)
        {
            VisitorAlso = false;
            club = cl;
            ABGroup = AB;
            if (AB.A.Count() == 0 || AB.B.Count() == 0)
                name = "AB: Too many weekends used";
            else
                name = "AB: Constraints";

            // calculated related entities
            foreach (Team t in ABGroup.A)
                AddRelated(t.poule);
            foreach (Team t in ABGroup.B)
                AddRelated(t.poule);
        }

        public override void Evaluate(Model model)
        {
            ClearConflicts();
            ABGroup.DetermineWeeks();

            conflictMatches.Clear();
            conflict_cost = 0;
            cost = 1000; // Must come from setting

            foreach (Team t in ABGroup.A)
                foreach (var m in t.poule.matches)
                {
                    if (m.homeTeam == t && m.homeTeam.club == club && m.RealMatch() && ABGroup.BWeeks[m.Week.WeekNr()])
                        AddConflictMatch(VisitorHomeBoth.HomeOnly, m);

                }
            foreach (Team t in ABGroup.B)
                foreach (var m in t.poule.matches)
                {
                    if (m.homeTeam == t && m.homeTeam.club == club && m.RealMatch() && ABGroup.AWeeks[m.Week.WeekNr()])
                        AddConflictMatch(VisitorHomeBoth.HomeOnly, m);

                }
        }
        public override string[] GetTextDescription()
        {
            List<string> result = new List<string>();
            result.Add("Group X:");
            foreach (Team t in ABGroup.A)
            {
                result.Add(" - " + t.serie.name.ToString() + " - " + t.name.ToString() + " (" + t.Id.ToString() + ")");
            }
            result.Add("Group Y:");
            foreach (Team t in ABGroup.B)
            {
                result.Add(" - " + t.serie.name.ToString() + " - " + t.name.ToString() + " (" + t.Id.ToString() + ")");
            }
            return result.ToArray();
        }
    }




}
