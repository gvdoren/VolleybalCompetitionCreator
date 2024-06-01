using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightIdeasSoftware;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CompetitionCreator
{
    public class Model
    {
        public Security.LicenseKey licenseKey;
        public bool stateNotSaved = false;
        public void MakeDirty() { stateNotSaved = true; }
        public string savedFileName = "Competition.xml";
        public int year;
        public bool OptimizeNumber;
        public bool OptimizeHomeVisit;
        public bool OptimizeSchema;
        public List<Club> clubs;
        public List<Serie> series = new List<Serie>();
        public List<Poule> poules = new List<Poule>();
        public List<Team> teams = new List<Team>();
        //public List<TeamConstraint> teamConstraints = new List<TeamConstraint>(); 
        public void AddTeam(Team team)
        {
            teams.Add(team);
            //team.serie.AddTeam(team);
            team.club.AddTeam(team);
        }
        public void RemoveTeam(Team team)
        {
            // Remove constraints related to the team
            List<Constraint> tobedeleted = new List<Constraint>();
            foreach (Constraint con in constraints)
            {
                if (con.team == team) tobedeleted.Add(con);
            }
            foreach (Constraint con in tobedeleted)
            {
                constraints.Remove(con);
            }
            teams.Remove(team);
            MakeDirty();
        }
        public YearPlans yearPlans = null; 
        public List<TeamConstraint> teamConstraints = new List<TeamConstraint>();
        public List<Constraint> constraints = new List<Constraint>();
        public List<Sporthal> sporthalls = new List<Sporthal>();
        public void Optimize()
        {
            poules.Sort(delegate(Poule p1, Poule p2) { return p1.conflict_cost.CompareTo(p2.conflict_cost); });
            foreach (Poule p in poules)
            {
                //p.OptimizeTeamAssignment(this);
                Changed();
            }
        }
        private void ClearAllConflicts()
        {
            foreach (Team team in teams) team.ClearConflicts();
            foreach (Poule poule in poules)
            {
                poule.ClearConflicts();
                foreach (Match match in poule.matches)
                {
                    match.ClearConflicts();
                }
                foreach (MatchWeek week in poule.weeks)
                {
                    week.ClearConflicts();
                }
            }
            foreach (Serie serie in series)
            {
                serie.ClearConflicts();
            }
            foreach (Club club in clubs)
            {
                club.ClearConflicts();
            }

        }

        public void ReInit()
        {
            lock (this)
            {
                foreach (Constraint constraint in constraints)
                {
                    constraint.ReInit();
                }
            }
        }

        public void Evaluate(Poule p)
        {
            lock (this)
            {
                ClearAllConflicts();
                foreach (Constraint constraint in constraints)
                {
                    constraint.Evaluate(this);
                }
            }
        }
        public void EvaluateRelatedConstraints(Poule p)
        {
            //Evaluate(p); return;
            
            lock (this)
            {
                ClearAllConflicts();
                foreach (Constraint constraint in p.relatedConstraints)
                //foreach (Constraint constraint in constraints)
                {
                    constraint.Evaluate(this);
                }
            }
        }
        public int TotalConflicts()
        {
            lock (this)
            {
                int conflicts = 0;
                foreach (Constraint constraint in constraints)
                {
                    conflicts += constraint.conflict_cost;
                    //Console.WriteLine("{0} - {1}", constraint.name, constraint.conflict_cost);
                }
                return conflicts;
            }
        }
        public int TotalRelatedConflicts(Poule p)
        {
            lock (this)
            {
                int conflicts = 0;
                foreach (Constraint constraint in p.relatedConstraints)
                {
                    conflicts += constraint.conflict_cost;
                }
                return conflicts;
            }
        }
        public string version
        {
            get
            {
                try
                {
                    if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
                    {
                        return String.Format("Version {0}", System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion);
                    }
                }
                catch { }
                return "1.0.0.83 (not published)";
            }
        }
        public Model()
        {
            string Key = CompetitionCreator.Properties.Settings.Default.LicenseKey;
            licenseKey = new Security.LicenseKey(Key);
            this.year = DateTime.Now.Year;
            this.yearPlans = new YearPlans(year);
            this.yearPlans.ReadXML();
            clubs = new List<Club>();
        }
        public Model(int year)
        {
            string Key = CompetitionCreator.Properties.Settings.Default.LicenseKey;
            licenseKey = new Security.LicenseKey(Key);
            this.year = year;
            this.yearPlans = new YearPlans(year);
            this.yearPlans.ReadXML();
            clubs = new List<Club>();
        }
        public void RenewConstraints()
        {
            lock (this)
            {
                //constraints.Clear();
                constraints.RemoveAll(c => (c as SpecificConstraint) == null); // alles behalve specialecontraints die zijn opgegeven.
                foreach (Poule poule in this.poules)
                {
                    if (poule.serie.evaluated)
                    {
                        constraints.Add(new ConstraintSchemaTooClose(poule));
                        constraints.Add(new ConstraintPouleInconsistent(poule));
                        constraints.Add(new ConstraintPouleTwoTeamsOfSameClub(poule));
                        constraints.Add(new ConstraintSchemaTooManyHome(poule));
                        //constraints.Add(new ConstraintPouleOddEvenWeek(poule));
                        //constraints.Add(new ConstraintPouleFullLastTwoWeeks(poule));
                        constraints.Add(new ConstraintSchemaTooManyHomeAfterEachOther(poule));
                    }
                }
                foreach (Team te in teams)
                {
                    if (te.evaluated)
                    {
                        if(te.poule != null)
                        {
                            constraints.Add(new ConstraintSporthallNotAvailable(te));
                            if (te.fixedNumber > 0) constraints.Add(new ConstraintTeamFixedNumber(te));
                        } else
                        {
                            constraints.Add(new ConstraintNoPouleAssigned(te));
                        }
                
                    }
                }
                foreach(TeamConstraint con in  teamConstraints.FindAll(c => c.what == TeamConstraint.What.NotOnSameTime))
                {
                    constraints.Add(new ConstraintPlayAtSameTime(con.team1, con.team2));
                }
                                
                // All group related constraints
                //foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                //{
                //    ConstructGroupConstraintsDay(day);
                //}
                ConstructGroupConstraintsDay();
                //ConstructGroupConstraintsWeekend();
                //ConstructGroupConstraintsWeekendForClub();

                // Deze moet als laatste
                foreach (Team te in teams)
                {
                    if (te.evaluated)
                    {
                        constraints.Add(new ConstraintTeamTooManyConflicts(te));
                    }
                }
                foreach (Club club in clubs)
                {
                    constraints.Add(new ConstraintClubTooManyConflicts(club));
                    constraints.Add(new ConstraintTeamNaming(club));
                }
                foreach (var con in constraints)
                    con.ReInit();
                foreach (Poule poule in poules)
                {
                    poule.CalculateRelatedConstraints(this);
                }
            }
        }

        bool EqualDay(Team team, DayOfWeek day2)
        {
            if (team.club.PerWeek) // eigenlijk per week
                return true;
            return team.defaultDay == day2;
        }

        class XY
        {
            public List<Team> X = new List<Team>();
            public List<Team> Y = new List<Team>();
        }

        void CreateGroupsPerClub(Club club, DayOfWeek day, ref XY xy)
        {
            foreach (var t in club.teams.Where(t => t.defaultDay == day && t.evaluated))
            {
                if (t.group == TeamGroups.GroupX)
                    xy.X.Add(t);
                if (t.group == TeamGroups.GroupY)
                    xy.Y.Add(t);

            }
        }

        XY GetGroup(ref List<XY> XY_groups, Team team, ref bool X)
        {
            foreach(var selectedXy in XY_groups)
            {
                if (selectedXy.X.Contains(team))
                {
                    X = true;
                    return selectedXy;
                }
                if (selectedXy.Y.Contains(team))
                {
                    X = false;
                    return selectedXy;
                }
            }
            XY xy = new XY();
            xy.X.Add(team);
            X = true;
            return xy;
        }

        bool SwitchGroup(XY xy)
        {
            foreach (var t in xy.X)
                if (t.group == TeamGroups.GroupX)
                    return false;
            foreach (var t in xy.Y)
                if (t.group == TeamGroups.GroupY)
                    return false;
            var temp = xy.X;
            xy.X = xy.Y;
            xy.Y = temp;
            return true;
        }

        XY MergeGroup(XY xy1, XY xy2, bool sw = false)
        {
            if (sw)
            {
                var ok = SwitchGroup(xy1);
                if (!ok) ok = SwitchGroup(xy2);
                if (!ok)
                    throw new Exception("X and Y are stronger than team constraints");
            }

            XY xy = new XY();
            xy.X = new List<Team>(xy1.X);
            xy.Y = new List<Team>(xy1.Y);
            xy.X.AddRange(xy2.X);
            xy.Y.AddRange(xy2.Y);
            return xy;
        }
        void IgnoredError(TeamConstraint c)
        {
            CompetitionCreator.Error.AddManualError("Conflicting team constraint: ",
                string.Format("Team constraint ignored:<br/>" +
                "Team1: {0}({1}) - {5}<br/>" +
                "Team2: {2}({3}) - {6}<br/>" +
                "What:  {4}<br/>" +
                "Explanation: Teams on same day/week should be in same group or teams on different day/week should be in different group", c.team1str, c.team1Id, c.team2str, c.team2Id, c.what.ToString(), c.team1.GroupName, c.team2.GroupName));

        }

        public void ConstructGroupConstraintsDay()
        {
            List<XY> XY_groups = new List<XY>();

            // Groups per club (that share a sporthall)
            List<Club> remaining = new List<Club>(clubs);
            while (remaining.Count > 0)
            {
                List<Club> sharedClubs = new List<Club>();
                var selectedClub = remaining.First();
                sharedClubs.Add(selectedClub);
                remaining.Remove(selectedClub);
                var perWeek = selectedClub.PerWeek;
                foreach (var sharedClub in selectedClub.SharingSporthal)
                {
                    remaining.Remove(sharedClub);
                    sharedClubs.Add(sharedClub);
                    if (sharedClub.PerWeek)
                        perWeek = true;
                }
                if (perWeek)
                {
                    XY xy = new XY();
                    foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                        foreach (var sharedClub in sharedClubs)
                            CreateGroupsPerClub(sharedClub, day, ref xy);
                    if (xy.X.Count + xy.Y.Count > 1)
                        XY_groups.Add(xy);
                }
                else
                {
                    foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                    {
                        XY xy = new XY();
                        foreach (var sharedClub in sharedClubs)
                            CreateGroupsPerClub(sharedClub, day, ref xy);
                        if (xy.X.Count + xy.Y.Count > 1)
                            XY_groups.Add(xy);
                    }
                }
            }

            // Merge groups based on day constraints
            foreach (var dayContraint in  teamConstraints.FindAll(c => c.team1 != null && c.team2 != null &&
                                                                      (c.what == TeamConstraint.What.HomeNotOnSameDay || c.what == TeamConstraint.What.HomeOnSameDay) &&
                                                                      c.team1.evaluated && c.team2.evaluated))
            {
                bool x1 = false;
                var group1 = GetGroup(ref XY_groups, dayContraint.team1, ref x1);
                bool x2 = false;
                var group2 = GetGroup(ref XY_groups, dayContraint.team2, ref x2);
                if (group1 == group2 && ((x1==x2) != (dayContraint.what == TeamConstraint.What.HomeOnSameDay)))
                    IgnoredError(dayContraint);
                else if (group1 == group2)
                {
                    // Nothing has to be changed, are already in same group in correct x en y
                }
                else
                {
                    bool sw = (x1 == x2) != (dayContraint.what == TeamConstraint.What.HomeOnSameDay); // switch needed
                    try
                    {
                        XY_groups.Add(MergeGroup(group1, group2, sw));
                        XY_groups.Remove(group1);
                        XY_groups.Remove(group2);
                    }
                    catch 
                    {
                        IgnoredError(dayContraint);
                    }
                }
            }

            // Merge groups based on week constraints
            foreach (var dayContraint in teamConstraints.FindAll(c => c.team1 != null && c.team2 != null &&
                                                                     (c.what == TeamConstraint.What.HomeInSameWeekend || c.what == TeamConstraint.What.HomeNotInSameWeekend) &&
                                                                     c.team1.evaluated && c.team2.evaluated))
            {
                bool x1 = false;
                var group1 = GetGroup(ref XY_groups, dayContraint.team1, ref x1);
                bool x2 = false;
                var group2 = GetGroup(ref XY_groups, dayContraint.team2, ref x2);
                if (group1 == group2 && ((x1 == x2) != (dayContraint.what == TeamConstraint.What.HomeInSameWeekend)))
                    IgnoredError(dayContraint);
                else if (group1 == group2)
                {
                    // Nothing has to be changed, are already in same group in correct x en y
                }
                else
                {
                    bool sw = (x1 == x2) != (dayContraint.what == TeamConstraint.What.HomeInSameWeekend); //switch needed
                    try
                    {
                        XY_groups.Add(MergeGroup(group1, group2, sw));
                        XY_groups.Remove(group1);
                        XY_groups.Remove(group2);
                    }
                    catch
                    {
                        IgnoredError(dayContraint);
                    }
                }
            }

            // Create constraints
            foreach (var xy in XY_groups)
            {
                ConstraintGrouping groupCon = new ConstraintGrouping();

                if (xy.X.Count == 0 || xy.Y.Count == 0)
                    groupCon.name = "Only 1 Group. Too many weekends are used.";
                else
                    groupCon.name = "Teams in different groups play in same week.";

                groupCon.GroupA.AddRange(xy.X);
                groupCon.GroupB.AddRange(xy.Y);
                constraints.Add(groupCon);
            }
        }
        
        public void ConstructGroupConstraintsWeekend()
        {
            // Grouping of teams, and creating the constraints for it
            // first weekend constraints
            List<TeamConstraint> remaining = teamConstraints.FindAll(c => (c.what == TeamConstraint.What.HomeNotInSameWeekend || c.what == TeamConstraint.What.HomeInSameWeekend) &&
                                                                           c.team1 != null && c.team2 != null && c.team1.evaluated && c.team2.evaluated); 
            // find the first constraint in remaining that is day related
            Team team = null;
            do
            {
                team = null;
                if (remaining.Count > 0) team = remaining[0].team1;
                if (team != null)
                {
                    ConstraintGrouping groupCon = new ConstraintGrouping();
                    groupCon.name = "Teams in different groups play on same weekends";
                    groupCon.GroupA.Add(team);
                    bool added = false;
                    do
                    {
                        added = false;
                        List<TeamConstraint> newCons = remaining.FindAll(c => groupCon.AddTeamConstraintWeekend(c));
                        foreach (TeamConstraint tc in newCons)
                        {
                            remaining.Remove(tc);
                            added = true;
                        }
                    } while (added);
                    Error error = groupCon.GetError();
                    constraints.Add(groupCon);
                }
            } while (team != null);

        }

        public void ConstructGroupConstraintsWeekendForClub()
        {
            foreach (var club in clubs)
            {
                bool groupX = false;
                bool groupY = false;
                foreach (var team in club.teams)
                {
                    if (team.group == TeamGroups.GroupX)
                        groupX = true;
                    if (team.group == TeamGroups.GroupY)
                        groupY = true;
                }
                TeamGroups group = TeamGroups.NoGroup;
                if (groupX && !groupY)
                    group = TeamGroups.GroupX;
                if (!groupX && groupY)
                    group = TeamGroups.GroupY;
                if (group != TeamGroups.NoGroup)
                {
                    // ToDo: Different type of constraint is required
                    ConstraintGrouping groupCon = new ConstraintGrouping();
                    groupCon.name = "Teams in different groups play on same weekends(Club)";
                    foreach (var team in club.teams)
                    {
                        groupCon.GroupA.Add(team);
                    }
                    constraints.Add(groupCon);
                }
            }
        }

        public int TotalConflictsSnapshot = 0;
        public event MyEventHandler OnMyChange;
        public void Changed(Model p = null)
        {
            TotalConflictsSnapshot = TotalConflicts();
            //call it then you need to update:
            if (OnMyChange != null)
            {
                MyEventArgs e = new MyEventArgs(p);
                //e.EventInfo = content;
                OnMyChange(this, e);
            }
        }
    }

    public class TeamConstraint //: SpecificConstraint
    {
        public enum What { HomeInSameWeekend, HomeOnSameDay, HomeNotInSameWeekend, HomeNotOnSameDay, NotOnSameTime };
        public What what;
        public int team1Id;
        public int team2Id;
        public Model model;
        public Team team2
        {
            
            get { return model.teams.Find(t => t.Id == team2Id); }
        }
        public Team team1
        {
            get { return model.teams.Find(t => t.Id == team1Id); }
        }
        public TeamConstraint(Model model, int team1Id, int team2Id, What what)
        {
            this.model = model;
            this.team1Id = team1Id;
            this.team2Id = team2Id;
            this.what = what;
            //VisitorAlso = true;
            //cost = MySettings.Settings.DefaultTeamsConstraintCost;
            //name = ToText(what);
        }
        public /*override*/ bool RelatedTo(List<Team> teams)
        {
            if (team1 == null || team2 == null) return false;
            return teams.Contains(this.team1) || teams.Contains(this.team2);
        }
        public /*override*/ List<Club> RelatedClubs()
        {
            List<Club> clubs = new List<Club>();
            if (team1 == null || team2 == null) return clubs;
            if (team1 != null) clubs.Add(team1.club);
            if (team2 != null) clubs.Add(team2.club);
            return clubs;
        }
        public /*override*/ List<Poule> RelatedPoules()
        {
            List<Poule> poules = new List<Poule>();
            if (team1 == null || team2 == null) return poules;
            if (team1 != null && team1.poule != null) poules.Add(team1.poule);
            if (team2 != null && team2.poule != null) poules.Add(team2.poule);
            return poules;
        }
        public /*override*/ List<Team> RelatedTeams()
        {
            List<Team> teams = new List<Team>();
            if (team1 == null || team2 == null) return teams;
            if (team1 != null) teams.Add(team1);
            if (team2 != null) teams.Add(team2);
            return teams;
        }
        public string team1str { get { if (team1 != null) return team1.name + " (" + team1.seriePouleName + ")"; else return "? (Team removed)"; } }
        public string team2str { get { if (team2 != null) return team2.name + " (" + team2.seriePouleName + ")"; else return "? (Team removed)"; } }
        public string description2
        {
            get { return what.ToString(); }
        }
    }

}
