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
            // Remove links from other teams to this team
            foreach (Team t in teams)
            {
                if (t.NotAtSameTime == team) t.NotAtSameTime = null;
            }
            teams.Remove(team);
            MakeDirty();
        }
        public Annorama annorama = null; // = new Annorama(DateTime.Now.Year).ReadXML();
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
            this.annorama = new Annorama(year);
            this.annorama.ReadXML();
            clubs = new List<Club>();
        }
        public Model(int year)
        {
            string Key = CompetitionCreator.Properties.Settings.Default.LicenseKey;
            licenseKey = new Security.LicenseKey(Key);
            this.year = year;
            this.annorama = new Annorama(year);
            this.annorama.ReadXML();
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
                            if (te.NotAtSameTime != null && te.NotAtSameTime.evaluated && te.NotAtSameTime.poule != null)
                            {
                                constraints.Add(new ConstraintPlayAtSameTime(te));
                            }
                        } else
                        {
                            constraints.Add(new ConstraintNoPouleAssigned(te));
                        }
                
                    }
                }
                
                // All group related constraints
                ConstructGroupConstraintsDay(DayOfWeek.Saturday);
                ConstructGroupConstraintsDay(DayOfWeek.Sunday);
                ConstructGroupConstraintsWeekend();

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
                }

                foreach (Poule poule in poules)
                {
                    poule.CalculateRelatedConstraints(this);
                }
            }
        }

        public void ConstructGroupConstraintsDay(DayOfWeek day)
        {
            // Grouping of teams, and creating the constraints for it
            // first day constraints
            List<TeamConstraint> remaining = teamConstraints.FindAll(c => c.team1 != null && c.team1.defaultDay == day && 
                                                                      c.team2 != null && c.team2.defaultDay == day && 
                                                                      (c.what == TeamConstraint.What.HomeNotOnSameDay || c.what == TeamConstraint.What.HomeOnSameDay) &&
                                                                      c.team1.evaluated && c.team2.evaluated);
            List<Club> remainingClubs = clubs.FindAll(c => c.teams.Exists(t => t.defaultDay == day && t.group != TeamGroups.NoGroup && t.evaluated));
            // find the first constraint in remaining that is day related
            Team team = null;
            do
            {
                team = null;
                // selecteer team waar vanuit het maken van de groepen begint
                if (remaining.Count > 0) team = remaining[0].team1;
                else if (remainingClubs.Count > 0) team = remainingClubs[0].teams.Find(t => t.defaultDay == day && t.group != TeamGroups.NoGroup && t.evaluated);
                if (team != null)
                {
                    ConstraintGrouping groupCon = new ConstraintGrouping();
                    groupCon.name = "Teams in different groups play on same day";
                    groupCon.GroupA.Add(team);
                    bool added = false;
                    do
                    {
                        added = false;
                        List<TeamConstraint> newCons = remaining.FindAll(c => groupCon.AddTeamConstraintDay(c));
                        foreach (TeamConstraint tc in newCons)
                        {
                            remaining.Remove(tc);
                            added = true;
                        }
                        // check overlap met (X en A) of (Y en B) => X toevoegen aan A, Y toevoegen aan B
                        // check overlap met (X en B) of (Y en A) => X toevoegen aan B, Y toevoegen aan A
                        List<Club> tempRemainingClubs = new List<Club>(remainingClubs);
                        foreach (Club club in tempRemainingClubs)
                        {
                            List<Team> selectedX = club.GetGroupX().FindAll(t => t.defaultDay == team.defaultDay && t.evaluated);
                            List<Team> selectedY = club.GetGroupY().FindAll(t => t.defaultDay == team.defaultDay && t.evaluated);
                            if (Team.Overlap(selectedX, groupCon.GroupA))
                            {
                                Team.AddIfNeeded(groupCon.GroupA, selectedX);
                                Team.AddIfNeeded(groupCon.GroupB, selectedY);
                                added = true;
                                remainingClubs.Remove(club);
                            }
                            else if (Team.Overlap(selectedX, groupCon.GroupB))
                            {
                                Team.AddIfNeeded(groupCon.GroupA, selectedY);
                                Team.AddIfNeeded(groupCon.GroupB, selectedX);
                                added = true;
                                remainingClubs.Remove(club);
                            }
                            else if (Team.Overlap(selectedY, groupCon.GroupA))
                            {
                                Team.AddIfNeeded(groupCon.GroupA, selectedY);
                                Team.AddIfNeeded(groupCon.GroupB, selectedX);
                                added = true;
                                remainingClubs.Remove(club);
                            }
                            else if (Team.Overlap(selectedY, groupCon.GroupB))
                            {
                                Team.AddIfNeeded(groupCon.GroupA, selectedX);
                                Team.AddIfNeeded(groupCon.GroupB, selectedY);
                                added = true;
                                remainingClubs.Remove(club);
                            }
                        }

                    } while (added);
                    // Als ze allemaal in 1 group zitten, komt er een constraint dat ze in zo weinig mogelijk dagen moeten spelen
                    if (groupCon.GroupA.Count == 0) constraints.Add(new ConstraintNotAllInSameHomeDay(groupCon.GroupB));
                    else if (groupCon.GroupB.Count == 0) constraints.Add(new ConstraintNotAllInSameHomeDay(groupCon.GroupA));
                    else constraints.Add(groupCon);
                }
            } while (team != null);

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
                    constraints.Add(groupCon);
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
                }
            } while (team != null);

        }

        public int LastTotalConflicts = 0;
        public event MyEventHandler OnMyChange;
        public void Changed(Model p = null)
        {
            LastTotalConflicts = TotalConflicts();
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
        public enum What { HomeInSameWeekend, HomeOnSameDay, HomeNotInSameWeekend, HomeNotOnSameDay };
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
        public string serie1str { get { if (team1 != null) return team1.serie.name; else return "?"; } }
        public string serie2str { get { if (team2 != null) return team2.serie.name; else return "?"; } }
        public string team1str { get { if (team1 != null) return team1.name + " (" + team1Id + ")"; else return "? (" + team1Id + ")"; } }
        public string team2str { get { if (team2 != null) return team2.name + " (" + team2Id + ")"; else return "? (" + team2Id + ")"; } }
        public string description2
        {
            get { return what.ToString(); }
        }
    }

}
