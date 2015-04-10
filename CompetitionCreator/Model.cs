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
        public Annorama annorama = new Annorama(DateTime.Now.Year);
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
        public void Evaluate(Poule p)
        {
            lock (this)
            {
                foreach (Team team in teams) team.ClearConflicts();
                foreach (Poule poule in poules)
                {
                    poule.ClearConflicts();
                    foreach (Match match in poule.matches)
                    {
                        match.ClearConflicts();
                    }
                    foreach (Week week in poule.weeksFirst)
                    {
                        week.ClearConflicts();
                    }
                    foreach (Week week in poule.weeksSecond)
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
                foreach (Constraint constraint in constraints)
                {
                    constraint.Evaluate(this);
                }
            }
        }
        public void EvaluateRelatedConstraints(Poule p)
        {
            lock (this)
            {
                foreach (Club cl in clubs)
                {
                    cl.ClearConflicts();
                }
                foreach (Team te in teams)
                {
                    te.ClearConflicts();
                }
                foreach (Week we in p.weeksFirst)
                {
                    we.ClearConflicts();
                }
                foreach (Week we in p.weeksSecond)
                {
                    we.ClearConflicts();
                }
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
        public Model(int year)
        {

            string Key = CompetitionCreator.Properties.Settings.Default.LicenseKey;
            licenseKey = new Security.LicenseKey(Key);
            this.year = year;
            this.annorama = new Annorama(year);
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
                    constraints.Add(new ConstraintSchemaTooClose(poule));
                    constraints.Add(new ConstraintPouleInconsistent(poule));
                    constraints.Add(new ConstraintPouleTwoTeamsOfSameClub(poule));
                    //constraints.Add(new ConstraintPouleFixedNumber(poule));
                    //constraints.Add(new ConstraintPouleOddEvenWeek(poule));
                    //constraints.Add(new ConstraintPouleFullLastTwoWeeks(poule));
                    constraints.Add(new ConstraintSchemaTooManyHomeAfterEachOther(poule));
                }

                foreach (Club club in clubs)
                {
                    constraints.Add(new ConstraintNotAllInSameHomeDay(club));
                    constraints.Add(new ConstraintDifferentGroupsOnSameDay(club));
                    constraints.Add(new ConstraintPlayAtSameTime(club));
                    constraints.Add(new ConstraintNoPouleAssigned(club));
                }
                // Deze moet als laatste
                foreach (Club club in clubs)
                {
                    constraints.Add(new ConstraintClubTooManyConflicts(club));
                }
                foreach (Team team in teams)
                {
                    constraints.Add(new ConstraintTeamTooManyConflicts(team));
                    constraints.Add(new ConstraintSporthallNotAvailable(team));
                    if (team.fixedNumber > 0) constraints.Add(new ConstraintTeamFixedNumber(team));
                }
                foreach (Poule poule in poules)
                {
                    poule.CalculateRelatedConstraints(this);
                }
            }
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
 /*     XML-based variant was not available. Was it due to website changes?
  *     public void ConvertVVBCompetitionToCSV(string filename)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(@"\vvb2014.txt");
            StreamWriter writer = new StreamWriter(filename);

            foreach (HtmlNode table in doc.DocumentNode.SelectNodes("//table"))
            {
                HtmlNode tr = table.SelectSingleNode(".//tr");
                HtmlNode td = tr.SelectSingleNode(".//td");
                string poule = td.InnerText;
                poule = poule.Substring(12);
                HtmlNodeCollection lines = table.SelectNodes(".//tr");
                for (int i = 2; i < lines.Count; i++)
                {
                    HtmlNode line = lines[i];
                    HtmlNodeCollection fields = line.SelectNodes(".//td");
                    string date = fields[2].InnerText;
                    DateTime dt = DateTime.Parse(date);
                    string aanvangsuur = fields[3].InnerText;
                    string thuisploeg = fields[4].InnerText;
                    int thuisploegId = -1;
                    string bezoekersploeg = fields[5].InnerText;
                    int bezoekersploegId = -1;
                    string sporthal = fields[6].InnerText;
                    sporthal = sporthal.Replace(",", " ");
                    int sporthalId = -1;
                    string thuisclubname = "Nationaal";
                    int thuisclubId = -1;
                    writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                        "Nationaal1", "-1", poule, "-1", thuisploeg, thuisploegId, bezoekersploeg, bezoekersploegId, sporthal, sporthalId, date, aanvangsuur, thuisclubname, thuisclubId);
                }
            }
            writer.Close();
        }
  * */
    }
}
