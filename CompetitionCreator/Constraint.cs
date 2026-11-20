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
        public abstract string[] GetTextDescription(Match match);
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
                var conflict = new Conflict(this, cost);
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
        protected void AddConflictMatchWithCost(VisitorHomeBoth who, Match match, int penalty)
        {
            if (match == null || penalty <= 0)
                return;

            conflict_cost += penalty;
            var existingConflict = match.GetConflictList().FirstOrDefault(c => c.con == this);
            if (existingConflict != null)
            {
                existingConflict.Cost += penalty;
                if (who != VisitorHomeBoth.VisitorOnly && match.homeTeam.RealTeam() &&
                    !match.homeTeam.GetConflictList().Contains(existingConflict))
                    match.homeTeam.AddConflict(existingConflict);
                if (who != VisitorHomeBoth.HomeOnly && match.visitorTeam.RealTeam() &&
                    !match.visitorTeam.GetConflictList().Contains(existingConflict))
                    match.visitorTeam.AddConflict(existingConflict);

                if (club == null)
                {
                    if (who != VisitorHomeBoth.VisitorOnly &&
                        !match.homeTeam.club.GetConflictList().Contains(existingConflict))
                        match.homeTeam.club.AddConflict(existingConflict);
                    if (who != VisitorHomeBoth.HomeOnly &&
                        !match.visitorTeam.club.GetConflictList().Contains(existingConflict))
                        match.visitorTeam.club.AddConflict(existingConflict);
                }
                return;
            }

            var conflict = new Conflict(this, penalty);
            conflictMatches.Add(match);
            match.AddConflict(conflict);
            match.Week.AddConflict(conflict);
            match.poule.AddConflict(conflict);
            match.poule.serie.AddConflict(conflict);
            if (who != VisitorHomeBoth.VisitorOnly && match.homeTeam.RealTeam())
                match.homeTeam.AddConflict(conflict);
            if (who != VisitorHomeBoth.HomeOnly && match.visitorTeam.RealTeam())
                match.visitorTeam.AddConflict(conflict);

            if (club != null)
            {
                club.AddConflict(conflict);
            }
            else if (who == VisitorHomeBoth.Both)
            {
                match.homeTeam.club.AddConflict(conflict);
                if (match.homeTeam.club != match.visitorTeam.club)
                    match.visitorTeam.club.AddConflict(conflict);
            }
            else
            {
                if (who != VisitorHomeBoth.VisitorOnly)
                    match.homeTeam.club.AddConflict(conflict);
                if (who != VisitorHomeBoth.HomeOnly)
                    match.visitorTeam.club.AddConflict(conflict);
            }
        }
        public void Sort()
        {
            conflictMatches.Sort(delegate (Match m1, Match m2) { return m1.datetime.CompareTo(m2.datetime); });
        }
    }

    public abstract class BlockingConstraint
    {
        private readonly List<Poule> relatedPoules = new List<Poule>();

        protected void AddRelated(Poule poule)
        {
            if (!relatedPoules.Contains(poule))
                relatedPoules.Add(poule);
        }

        public bool IsRelated(Poule poule)
        {
            return relatedPoules.Count == 0 || relatedPoules.Contains(poule);
        }

        public abstract bool IsSatisfied();
    }

    class ConstraintTeamGroupConsistency : Constraint
    {
        public ConstraintTeamGroupConsistency()
        {
            name = "Team constraint/group consistency";
            cost = 0;
        }

        public override void Evaluate(Model model)
        {
            // This diagnostic depends on mostly static input data, not on schedule evaluations.
        }

        public void Refresh(Model model)
        {
            ClearErrors();

            foreach (TeamConstraint constraint in model.teamConstraints)
            {
                Team team1 = constraint.team1;
                Team team2 = constraint.team2;
                if (team1 == null || team2 == null || team1.club != team2.club)
                    continue;

                bool shouldShareGroup;
                switch (constraint.what)
                {
                    case TeamConstraint.What.HomeNotOnSameDay:
                    case TeamConstraint.What.HomeNotInSameWeekend:
                        shouldShareGroup = false;
                        break;
                    case TeamConstraint.What.HomeOnSameDay:
                    case TeamConstraint.What.HomeInSameWeekend:
                        shouldShareGroup = true;
                        break;
                    default:
                        continue;
                }

                bool teamsShareGroup = team1.group == team2.group;
                if (teamsShareGroup == shouldShareGroup)
                    continue;

                string relation = constraint.what == TeamConstraint.What.HomeOnSameDay
                    ? "on the same day"
                    : constraint.what == TeamConstraint.What.HomeNotOnSameDay
                        ? "not on the same day"
                        : constraint.what == TeamConstraint.What.HomeInSameWeekend
                            ? "in the same weekend"
                            : "not in the same weekend";
                string groupState = teamsShareGroup ? "the same group" : "different groups";
                AddError(string.Format(
                    "Inconsistent data: Teams '{0}' ({1}) and '{2}' ({3}) should play {4}, but are in {5}.",
                    team1.name,
                    team1.serie.name,
                    team2.name,
                    team2.serie.name,
                    relation,
                    groupState));
            }
        }

        public override string[] GetTextDescription(Match match)
        {
            return ErrorInfo();
        }
    }

    class ConstraintSporthallNotAvailable : Constraint
    {
        public ConstraintSporthallNotAvailable(Team team)
        {
            name = "Sporthall not available";
            this.team = team;
            VisitorAlso = false;
            cost = MySettings.Settings.SporthalNotAvailableCost;
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
                        DateTime dt = match.datetime;
                        if (match.homeTeam == team &&
                            match.RealMatch() &&
                            team.sporthal.NotAvailable.Any(period => period.InPeriod((dt))))
                        {
                            this.AddConflictMatch(VisitorHomeBoth.HomeOnly, match);
                        }
                    }
                }
            }
        }
        public override string[] GetTextDescription(Match match)
        {
            List<string> result = new List<string>();

            string r = team.sporthal.name + $"is niet beschikbaar op: {match.dateday.Date.Date.ToString("yyyy-MM-dd ")}";
            foreach (Period period in team.sporthal.NotAvailable)
            {
                if (period.From.Date == match.dateday.Date)
                    r += string.Format("van {0} - {1}", period.From.ToString("H:mm"), period.Until.ToString("H:mm"));
            }
            result.Add(r);
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
                            break;
                        }
                    }
                }
            }
        }
        public override string[] GetTextDescription(Match match)
        {
            List<string> result = new List<string>();
            return result.ToArray();
        }
    }

    class ConstraintIncreasedUnbalance : BlockingConstraint
    {
        private readonly int baselineImbalance;
        private readonly Poule poule;

        public ConstraintIncreasedUnbalance(Poule poule)
        {
            this.poule = poule;
            AddRelated(poule);
            baselineImbalance = CalculateTotalImbalance();
        }

        public override bool IsSatisfied()
        {
            return !poule.evaluated || CalculateTotalImbalance() <= baselineImbalance;
        }

        private int CalculateTotalImbalance()
        {
            int totalImbalance = 0;
            foreach (Team team in poule.teams.Where(t => t.RealTeam()))
            {
                int homeCount = 0;
                int visitorCount = 0;
                foreach (Match match in poule.matches)
                {
                    if (!match.RealMatch())
                        continue;
                    if (match.homeTeam == team) homeCount++;
                    if (match.visitorTeam == team) visitorCount++;
                }
                totalImbalance += Math.Abs(homeCount - visitorCount);
            }
            return totalImbalance;
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
                    List<Match> homeFirstHalf = new List<Match>();
                    List<Match> homeSecondHalf = new List<Match>();
                    int matchNr = 0;
                    bool? currentStreakIsHome = null;
                    List<Match> currentStreak = new List<Match>();

                    foreach (Match match in sortedMatches)
                    {
                        bool isHomeMatch;
                        if (match.homeTeam == team)
                        {
                            isHomeMatch = true;
                            if (matchNr < poule.TeamCount) homeFirstHalf.Add(match);
                            else homeSecondHalf.Add(match);
                        }
                        else if (match.visitorTeam == team)
                        {
                            isHomeMatch = false;
                        }
                        else
                        {
                            continue;
                        }

                        if (currentStreakIsHome.HasValue && currentStreakIsHome.Value != isHomeMatch)
                        {
                            AddStreakPenalty(currentStreakIsHome.Value, currentStreak);
                            currentStreak.Clear();
                        }

                        currentStreakIsHome = isHomeMatch;
                        currentStreak.Add(match);
                        matchNr++;
                    }

                    // The last run also counts; it may not be followed by an opposite-side match.
                    if (currentStreakIsHome.HasValue)
                        AddStreakPenalty(currentStreakIsHome.Value, currentStreak);

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
        private void AddStreakPenalty(bool isHome, List<Match> streak)
        {
            int limit = MySettings.Settings.MatchTooManyAfterEachOther;
            int baseCost = MySettings.Settings.MatchTooManyAfterEachOtherCostLow;
            for (int i = limit - 1; i < streak.Count; i++)
            {
                int penalty = baseCost * (i - limit + 2);
                AddConflictMatchWithCost(
                    isHome ? VisitorHomeBoth.HomeOnly : VisitorHomeBoth.VisitorOnly,
                    streak[i],
                    penalty);
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
        public override string[] GetTextDescription(Match match)
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
            AddRelated(poule);
        }
        public override void Evaluate(Model model)
        {
            Number++;
            if (Number != 1 && Number % 10000 != 0)
                return;

            ClearErrors();
            if (poule.evaluated && poule.imported == false)
            {
                foreach (Team team in poule.teams)
                {
                    if (team.poule != poule)
                    {
                        AddError(string.Format("Poule consistency: Team '{0}' does not reference its containing poule '{1}'", team.name, poule.fullName),
                                 "The cause can be that an invalid xml is used as input, or that indicates a bug in the program. In both cases save your project and contact the author.");
                    }
                    if (team.serie != poule.serie)
                    {
                        AddError(string.Format("Poule consistency: Team '{0}' belongs to a different serie than poule '{1}'", team.name, poule.fullName),
                                 "The cause can be that an invalid xml is used as input, or that indicates a bug in the program. In both cases save your project and contact the author.");
                    }
                }

                if (poule.serie == null || !poule.serie.poules.Contains(poule))
                {
                    AddError(string.Format("Poule consistency: Serie does not contain poule '{0}'", poule.fullName),
                             "The cause can be that an invalid xml is used as input, or that indicates a bug in the program. In both cases save your project and contact the author.");
                }

                UInt64[] days = new UInt64[poule.weeks.Count];
                foreach (Match match in poule.matches)
                {
                    if (!match.RealMatch())
                        continue;

                    int index = match.weekIndex;
                    UInt32 bitMask = (1u << match.homeTeamIndex) | (1u << match.visitorTeamIndex);
                    if ((days[index] & bitMask) != 0)
                    {
                        AddError("Poule consistency: One teams plays more than one match on a single day",
                                 "The cause can be that an invalid xml is used as input, or that indicates a bug in the program. In both cases save your project and contact the author.");
                    }
                    days[index] |= bitMask;
                }

                foreach (Team team in poule.teams)
                {
                    if (team.RealTeam() && team.poule.matches.Count(m =>
                        m.RealMatch() && (m.homeTeam == team || m.visitorTeam == team)) == 0)
                    {
                        AddError(string.Format("Internal test - poule consistency: No matches for team '{0}'", team.name),
                                 "The cause can be that an invalid xml is used as input, or that indicates a bug in the program. In both cases save your project and contact the author.");
                    }
                }
            }
        }
        public override string[] GetTextDescription(Match match)
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
        public override string[] GetTextDescription(Match match)
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
            name = "Teams of one club in one poule should play in the first week of a round";
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
                for (int t1Index = 0; t1Index < poule.teams.Count; t1Index++)
                {
                    Team t1 = poule.teams[t1Index];
                    for (int t2Index = t1Index + 1; t2Index < poule.teams.Count; t2Index++)
                    {
                        Team t2 = poule.teams[t2Index];
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
                                    (m.homeTeam == t2 && m.visitorTeam == t1))
                                {
                                    firstPlayed[m.Week.round] = m;
                                }
                            }

                            for (int i = 0; i < 4; i++)
                            {
                                if (firstPlayed[i] != null)
                                {
                                    DateTime firstWeek = new MatchWeek(firstWeekend[i]).Monday.Date;
                                    int weeksLate = (int)((firstPlayed[i].Week.Monday.Date - firstWeek).TotalDays / 7);
                                    if (weeksLate > 0)
                                    {
                                        AddConflictMatch(VisitorHomeBoth.HomeOnly, firstPlayed[i]);
                                        // AddConflictMatch charges the first delayed week; each further week adds another penalty.
                                        int additionalCost = (weeksLate - 1) * cost;
                                        conflict_cost += additionalCost;
                                        conflicts[conflicts.Count - 1].Cost += additionalCost;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public override string[] GetTextDescription(Match match)
        {
            List<string> result = new List<string>();
            if (match != null && conflictMatches.Contains(match))
            {
                DateTime firstMatchDate = poule.matches
                    .Where(m => m.Week.round == match.Week.round)
                    .Min(m => m.datetime);
                DateTime firstWeek = new MatchWeek(firstMatchDate).Monday.Date;
                int weeksAfterFirst = (int)((match.Week.Monday.Date - firstWeek).TotalDays / 7);
                result.Add(string.Format(
                    "Teams {0} and {1} from club {2} play each other {3} week(s) after the first scheduled week of round {4}. Penalty: {5} points ({6} per week).",
                    match.homeTeam.name,
                    match.visitorTeam.name,
                    match.homeTeam.club.name,
                    weeksAfterFirst,
                    match.Week.round + 1,
                    weeksAfterFirst * cost,
                    cost));
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
                        // ToDo: reistijd afhankelijk van afstand
                        if (m1.homeTeam.club != m2.homeTeam.club) delta += MySettings.Settings.TravelingTime; // extra reistijd
                        DateTime st1 = m1.datetime;
                        DateTime en1 = st1.AddHours(delta);
                        st1 = st1.AddHours(-m1.serie.extraTimeBefore); // reserve wedstrijd
                        DateTime st2 = m2.datetime;
                        DateTime en2 = st2.AddHours(delta);
                        st2 = st2.AddHours(-m2.serie.extraTimeBefore); // reserve wedstrijd

                        if (st1 <= en2 && en1 >= st2) // if (!(st2 > en1 || st1 > en2))
                        {
                            this.AddConflictMatch(VisitorHomeBoth.Both, m1, m2);
                        }
                    }
                    i1++;
                    i2++;
                }
            }
        }
        public override string[] GetTextDescription(Match match)
        {
            List<string> result = new List<string>();
            result.Add("Team: " + team.name + " - " + team.defaultTime);
            result.Add("Team: " + team2.name + " - " + team2.defaultTime);
            result.Add("Conflict matches:");
            foreach (var m in conflictMatches)
            {
                result.Add($"{m.datetime}: {m.homeTeam.name} - {m.visitorTeam.name}");
            }

            return result.ToArray();
        }
    }



    class ConstraintPlayOnSameDay: Constraint
    {
        Team team2;
        TeamConstraint.What what;
        public ConstraintPlayOnSameDay(Team cl, Team cl2, TeamConstraint.What _what)
        {
            team2 = cl2;
            team = cl;
            what = _what;
            VisitorAlso = false;
            if (what == TeamConstraint.What.HomeNotOnSameDay)
                name = "Teams on the same day";
            else if (what == TeamConstraint.What.HomeOnSameDay)
                name = "Teams not on the same day";
            else if (what == TeamConstraint.What.HomeInSameWeekend)
                name = "Teams not in the same week";
            else 
                name = "Teams in the same week";

            cost = MySettings.Settings.PlayAtSameTime;
            AddRelated(cl.poule);
            AddRelated(cl2.poule);
        }
        public override void Evaluate(Model model)
        {
            ClearConflicts();

            conflict_cost = 0;
            conflictMatches.Clear();
            List<Match> team1Matches = team.poule.matches.FindAll(m => m.RealMatch() && m.homeTeam == team);
            team1Matches.Sort(delegate (Match m1, Match m2) { return m1.datetime.CompareTo(m2.datetime); });
            List<Match> team2Matches = team2.poule.matches.FindAll(m => m.RealMatch() && m.homeTeam == team2);
            team2Matches.Sort(delegate (Match m1, Match m2) { return m1.datetime.CompareTo(m2.datetime); });
            Poule poule2 = team2.poule;
            int i1 = 0;
            int i2 = 0;
            bool isWeekConstraint = what == TeamConstraint.What.HomeInSameWeekend ||
                what == TeamConstraint.What.HomeNotInSameWeekend;
            while (i1 < team1Matches.Count && i2 < team2Matches.Count)
            {
                Match m1 = team1Matches[i1];
                Match m2 = team2Matches[i2];
                DateTime period1 = isWeekConstraint ? m1.Week.Monday.Date : m1.datetime.Date;
                DateTime period2 = isWeekConstraint ? m2.Week.Monday.Date : m2.datetime.Date;
                if (period1 < period2)
                {
                    i1++;
                    if (what == TeamConstraint.What.HomeOnSameDay || what == TeamConstraint.What.HomeInSameWeekend)
                        this.AddConflictMatch(VisitorHomeBoth.HomeOnly, m1);
                }
                else if (period2 < period1)
                {
                    i2++;
                    if (what == TeamConstraint.What.HomeOnSameDay || what == TeamConstraint.What.HomeInSameWeekend)
                        this.AddConflictMatch(VisitorHomeBoth.HomeOnly, m2);
                }
                else
                {
                    if (what == TeamConstraint.What.HomeNotOnSameDay || what == TeamConstraint.What.HomeNotInSameWeekend)
                        this.AddConflictMatch(VisitorHomeBoth.Both, m1, m2);
                    i1++;
                    i2++;
                }
            }
        }
        public override string[] GetTextDescription(Match match)
        {
            List<string> result = new List<string>();
            bool isWeekConstraint = what == TeamConstraint.What.HomeInSameWeekend ||
                what == TeamConstraint.What.HomeNotInSameWeekend;
            bool shouldNotSharePeriod = what == TeamConstraint.What.HomeNotOnSameDay ||
                what == TeamConstraint.What.HomeNotInSameWeekend;
            string period = isWeekConstraint ? "week" : "day";
            string s = "Team '" + team.name + " (" + team.serie.name + ")' should ";
            if (shouldNotSharePeriod)
                s += "not ";
            s += "play in the same " + period + " as '" + team2.name + " (" + team2.serie.name + ").";
            result.Add(s);
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
        public override string[] GetTextDescription(Match match)
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
        public override string[] GetTextDescription(Match match)
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

    // ToDo: TeamConstraint
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
                    int penalty = MySettings.Settings.FixedIndexConstraintCost;
                    var conflict = new Conflict(this, penalty);
                    team.AddConflict(conflict);
                    team.poule.AddConflict(conflict);
                    team.club.AddConflict(conflict);
                    conflict_cost += penalty;
                }
            }
        }
        public override string[] GetTextDescription(Match match)
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
    // ToDo: TeamConstraint
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
        public override string[] GetTextDescription(Match match)
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
            // cost = MySettings.Settings.DefaultCustomTeamConstraintCost;
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
        public override string[] GetTextDescription(Match match)
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
                name = "X (or Y): Too many weekends used";
            else
                name = "XY: Conflicts";

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
            cost =  MySettings.Settings.DifferentGroupsOnSameDayCostHigh;

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
        public override string[] GetTextDescription(Match match)
        {
            List<string> result = new List<string>();
            result.Add($"Op: {match.dateday.Date.Date.ToString("yyyy-MM-dd")} is er een conflict vanwege de volgende teams:");
            foreach (var t in ABGroup.A)
            {
                foreach (var m in t.poule.matches.Where(m => m.homeTeam == t))
                {
                    if (m.Week.Monday == match.Week.Monday)
                    {
                        result.Add($"{t.GroupName}: {m.homeTeam.name} - {m.serie.name} {m.poule.name}");
                    }
                }
            }
            foreach (var t in ABGroup.B)
            {
                foreach (var m in t.poule.matches.Where(m => m.homeTeam == t))
                {
                    if (m.Week.Monday == match.Week.Monday)
                    {
                        result.Add($"{t.GroupName}: {m.homeTeam.name} - {m.serie.name} {m.poule.name}");
                    }
                }
            }

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
