using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using BrightIdeasSoftware;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace VolleybalCompetition_creator
{
    public partial class Form1 : Form
    {
        Klvv klvv = null;
        GlobalState state = new GlobalState();
        public Form1()
        {
            //Weekend.Show();
            //Weekend.Test();
            InitializeComponent();
            // reading club-constraints
            klvv = new Klvv(DateTime.Now.Year);
            Text = string.Format("Volleyball competition creation tool ({0})", klvv.year);

            this.WindowState = FormWindowState.Maximized;
            ClubListView clubview = new ClubListView(klvv,state);
            clubview.ShowHint = DockState.DockLeft;
            clubview.Show(dockPanel);

            //SerieView serieview = new SerieView(klvv, state);
            //serieview.ShowHint = DockState.DockRight;
            //serieview.Show(dockPanel);
            //serieview.Show(clubview.Pane, DockAlignment.Right, 0.35);
            //SerieTreeView serietreeview = new SerieTreeView(klvv, state);
            //serietreeview.ShowHint = DockState.DockRight;
            //serietreeview.Show(dockPanel);
            
            PouleListView pouleListview = new PouleListView(klvv, state);
            pouleListview.ShowHint = DockState.DockLeft;
            pouleListview.Show(clubview.Pane, DockAlignment.Right, 0.5);
            //pouleListview.Show(dockPanel);
            ConstraintListView constraintListview = new ConstraintListView(klvv, state);
            constraintListview.ShowHint = DockState.DockRight;
            constraintListview.Show(dockPanel);
            dockPanel.DockRightPortion = 500;
            List<string> ListTeam = new List<string>();
            for (int i = 0; i < 11; i++)
            {
                ListTeam.Add(string.Format("Team {0}",i+1));
            }

        }


        private void clubsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClubListView clubview = null;
            foreach (DockContent content in this.dockPanel.Contents)
            {
                clubview = content as ClubListView;
                if (clubview != null)
                {
                    clubview.Activate();
                    return;
                }
            }
            clubview = new ClubListView(klvv, state);
            clubview.ShowHint = DockState.DockLeft;
            
            clubview.Show(this.dockPanel);

        }

        private void seriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PouleListView pouleListView = null;
            foreach (DockContent content in this.dockPanel.Contents)
            {
                pouleListView = content as PouleListView;
                if (pouleListView != null)
                {
                    pouleListView.Activate();
                    return;
                }
            }
            pouleListView = new PouleListView(klvv, state);
            pouleListView.ShowHint = DockState.DockLeft;
            pouleListView.Show(this.dockPanel);

        }

        private void lijstToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClubListView clubview = null;
            foreach (DockContent content in this.dockPanel.Contents)
            {
                clubview = content as ClubListView;
                if (clubview != null)
                {
                    clubview.Activate();
                    return;
                }
            }
            clubview = new ClubListView(klvv, state);
            clubview.ShowHint = DockState.DockLeft;

            clubview.Show(this.dockPanel);
        }

        private void optimizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptimizeForm optimizeForm = null;
            foreach (DockContent content in this.dockPanel.Contents)
            {
                optimizeForm = content as OptimizeForm;
                if (optimizeForm != null)
                {
                    optimizeForm.Activate();
                    return;
                }
            }
            optimizeForm = new OptimizeForm(klvv, state);
            optimizeForm.Show(this.dockPanel);

        }

        private void anoramaToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 about = new AboutBox1();
            about.Show();
        }

        private void inschrijvingenToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void seriesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
        }

        private void constraintsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConstraintListView constraintView = null;
            foreach (DockContent content in this.dockPanel.Contents)
            {
                constraintView = content as ConstraintListView;
                if (constraintView != null)
                {
                    constraintView.Activate();
                    return;
                }
            }
            constraintView = new ConstraintListView(klvv, state);
            constraintView.ShowHint = DockState.DockRight;
            constraintView.Show(this.dockPanel);
        }

        private void externalCompetitionToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void kLVVCompetitionToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void anoramaToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AnoramaView anorama = null;
            foreach (DockContent content in this.dockPanel.Contents)
            {
                anorama = content as AnoramaView;
                if (anorama != null)
                {
                    anorama.Activate();
                    return;
                }
            }
            anorama = new AnoramaView(klvv, state);
            anorama.Show(this.dockPanel);


        }

        private void poulesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SerieView serieView = null;
            foreach (DockContent content in this.dockPanel.Contents)
            {
                serieView = content as SerieView;
                if (serieView != null)
                {
                    serieView.Activate();
                    return;
                }
            }
            serieView = new SerieView(klvv, state);
            serieView.Show(this.dockPanel);

        }

        private void subscriptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InschrijvingenView clubConstraints = null;
            foreach (DockContent content in this.dockPanel.Contents)
            {
                clubConstraints = content as InschrijvingenView;
                if (clubConstraints != null)
                {
                    clubConstraints.Activate();
                    return;
                }
            }
            clubConstraints = new InschrijvingenView(klvv, state);
            clubConstraints.Show(this.dockPanel);
        }

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void reportToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void perTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "ConflictReportPerType.csv";
            saveFileDialog1.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
            saveFileDialog1.FileOk += new CancelEventHandler(saveFileDialog1_FileOk);
            saveFileDialog1.ShowDialog();
        }
        public void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            StreamWriter writer = new StreamWriter(saveFileDialog1.FileName);
            string name = "";
            string title = "";
            klvv.constraints.Sort(delegate(Constraint c1, Constraint c2) { return c1.Title.CompareTo(c2.Title); });
            foreach (Constraint constraint in klvv.constraints)
            {
                if (constraint.conflict_cost > 0)
                {
                    if (constraint.name != name)
                    {
                        name = constraint.name;
                        writer.WriteLine("{0}", name);
                    }
                    if (constraint.Title != title)
                    {
                        title = constraint.Title;
                        writer.WriteLine("{0}", title);
                    }
                    constraint.Sort();
                    foreach (Match match in constraint.conflictMatches)
                    {
                        writer.WriteLine("{0},{1},{2},{3},{4}", match.datetime, match.poule.fullName,match.homeTeam.name, match.visitorTeam.name, match.homeTeam.group.ToString());
                    }
                }
            }
            writer.Close();
        }

        private void perClubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "ConflictReportPerClub.txt";
            saveFileDialog1.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
            saveFileDialog1.FileOk += new CancelEventHandler(saveFileDialog1_FileOk1);
            saveFileDialog1.ShowDialog();
        }
        public void saveFileDialog1_FileOk1(object sender, CancelEventArgs e)
        {
            StreamWriter writer = new StreamWriter(saveFileDialog1.FileName);
            string name = "";
            string title = "";
            foreach (Club club in klvv.clubs)
            {
                if (club.conflict_cost > 0)
                {
                    writer.WriteLine("{0}", club.name);
                    club.conflictConstraints.Sort(delegate(Constraint c1, Constraint c2) { return c1.Title.CompareTo(c2.Title); });
                    foreach (Constraint constraint in club.conflictConstraints)
                    {
                        if (constraint.conflict_cost > 0)
                        {
                            if (constraint.name != name)
                            {
                                name = constraint.name;
                                writer.WriteLine(" {0}", name);
                            }
                            if (constraint.Title != title)
                            {
                                title = constraint.Title;
                                writer.WriteLine("  - {0}", title);
                            }
                            constraint.Sort();
                            foreach (Match match in constraint.conflictMatches)
                            {
                                writer.WriteLine("{0},{1},{2},{3},{4}", match.datetime, match.poule.fullName, match.homeTeam.name, match.visitorTeam.name, match.homeTeam.group.ToString());
                            }
                        }
                    }
                }
            }
            writer.Close();

        }

        private void perClubToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            saveFileDialog1.FileName = string.Format("CompetitionPerClub{0:00}{1:00}{2:00}_{3:00}{4:00}.csv",now.Year,now.Month,now.Day,now.Hour,now.Minute);
            saveFileDialog1.Filter = "Comma-separated (*.csv)|*.csv";
            saveFileDialog1.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
            saveFileDialog1.FileOk += new CancelEventHandler(saveFileDialog1_FileOk2);
            saveFileDialog1.ShowDialog();
        }
        public void saveFileDialog1_FileOk2(object sender, CancelEventArgs e)
        {
            StreamWriter writer = new StreamWriter(saveFileDialog1.FileName);
            foreach (Club club in klvv.clubs)
            {
                List<Match> matches = new List<Match>();
                foreach (Team team in club.teams)
                {
                    if (team.poule != null)
                    {
                        foreach (Match match in team.poule.matches)
                        {
                            if (match.homeTeam.club == club)
                            {
                                if (matches.Contains(match) == false) matches.Add(match);
                            }
                        }
                    }
                }

                writer.WriteLine("{0}", club.name);
                matches.Sort(delegate(Match m1, Match m2) { return m1.datetime.CompareTo(m2.datetime); });
                foreach (Match match in matches)
                {
                    if (match.RealMatch())
                    {
                        string constraint = " - ";
                        if (match.conflictConstraints.Count > 0) constraint = " * ";
                        string conflicts = "";
                        foreach (Constraint constr in match.conflictConstraints)
                        {
                            conflicts += constr.name + ",";
                        }
                        writer.WriteLine("{5},{0},{1},{2},{3},{4},{7},{6}", match.datetime, match.poule.fullName, match.homeTeam.name, match.visitorTeam.name, match.homeTeam.group.ToString(), constraint,conflicts, match.homeTeam.sporthal.name.Replace(","," "));
                    }
                }
            }
            writer.Close();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            saveFileDialog1.FileName = string.Format("FullCompetition{0:00}{1:00}{2:00}_{3:00}{4:00}.xml", now.Year, now.Month, now.Day, now.Hour, now.Minute);
            saveFileDialog1.Filter = "Xml (*.xml)|*.xml";
            saveFileDialog1.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
            saveFileDialog1.FileOk += new CancelEventHandler(saveFileDialog1_FileOk3);
            saveFileDialog1.ShowDialog();
        }
        public void saveFileDialog1_FileOk3(object sender, CancelEventArgs e)
        {
            WriteProject(saveFileDialog1.FileName);
        }

        private void WriteProject(string fileName)
        {
            XmlWriter writer = XmlWriter.Create(fileName);
            writer.WriteStartDocument();
            writer.WriteStartElement("Competition");
            writer.WriteStartElement("Settings");
            writer.WriteElementString("Year", klvv.year.ToString());
            writer.WriteEndElement();
            klvv.WriteClubConstraints(writer);
            writer.WriteStartElement("Poules");
            foreach (Poule poule in klvv.poules)
            {
                writer.WriteStartElement("Poule");
                writer.WriteAttributeString("Name", poule.name);
                writer.WriteAttributeString("SerieSortId", poule.serie.id.ToString());
                writer.WriteAttributeString("SerieName", poule.serie.name);
                writer.WriteAttributeString("MaxTeams", poule.maxTeams.ToString());
                writer.WriteStartElement("Teams");
                foreach (Team team in poule.teams)
                {
                    writer.WriteStartElement("Team");
                    writer.WriteAttributeString("TeamName", team.name);
                    writer.WriteAttributeString("Id", team.Id.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteStartElement("Weekends");
                foreach (Weekend weekend in poule.weekends)
                {
                    writer.WriteStartElement("Weekend");
                    writer.WriteAttributeString("Date", weekend.Saturday.Date.ToShortDateString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteStartElement("Matches");
                foreach (Match match in poule.matches)
                {
                    writer.WriteStartElement("Match");
                    writer.WriteAttributeString("homeTeam", match.homeTeamIndex.ToString());
                    writer.WriteAttributeString("visitorTeam", match.visitorTeamIndex.ToString());
                    writer.WriteAttributeString("weekend", match.weekIndex.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteStartElement("TeamConstraints");
            foreach (TeamConstraint con in klvv.teamConstraints)
            {
                writer.WriteStartElement("Constraint");
                writer.WriteAttributeString("TeamId", con.team.Id.ToString());
                writer.WriteAttributeString("Date",con.date.ToShortDateString());
                writer.WriteAttributeString("What", con.homeVisitNone.ToString());
                writer.WriteAttributeString("Cost", con.cost.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "FullCompetition.xml";
            openFileDialog1.Filter = "Xml (*.xml)|*.xml";
            openFileDialog1.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
            openFileDialog1.FileOk += new CancelEventHandler(openFileDialog1_FileOk1);
            openFileDialog1.ShowDialog();

        }
        public void openFileDialog1_FileOk1(object sender, CancelEventArgs e)
        {
            XDocument doc = XDocument.Load(openFileDialog1.FileName);
            XElement competition = doc.Element("Competition");
            XElement settings = competition.Element("Settings");
            int year = int.Parse(settings.Element("Year").Value);
            Klvv klvvnew = new Klvv(year);
            klvvnew.ImportTeamSubscriptions(competition.Element("Clubs"));
            foreach (XElement poule in competition.Element("Poules").Elements("Poule"))
            {
                string PouleName = poule.Attribute("Name").Value;
                string SerieName = poule.Attribute("SerieName").Value;
                int SerieId = int.Parse(poule.Attribute("SerieSortId").Value);
                Serie serie = klvvnew.series.Find(s => s.id == SerieId && s.name == SerieName);
                Poule po = klvvnew.poules.Find(p => p.serie == serie && p.name == PouleName);
                if (po == null)
                {
                    po = new Poule(PouleName);
                    po.serie = serie;
                    serie.poules.Add(po.name, po);
                    klvvnew.poules.Add(po);
                }
                XAttribute attr = poule.Attribute("MaxTeams");
                if (attr != null)
                {
                    po.maxTeams = int.Parse(attr.Value);
                }
                foreach (XElement team in poule.Element("Teams").Elements("Team"))
                {
                    int teamId = int.Parse(team.Attribute("Id").Value);
                    string teamName = team.Attribute("TeamName").Value;
                    Team te = klvvnew.teams.Find(t => t.Id == teamId && t.name == teamName);
                    te.poule = po;
                    po.teams.Add(te);
                }
                foreach (XElement weekend in poule.Element("Weekends").Elements("Weekend"))
                {
                    DateTime date = DateTime.Parse(weekend.Attribute("Date").Value);
                    po.weekends.Add(new Weekend(date));
                }
                foreach (XElement match in poule.Element("Matches").Elements("Match"))
                {
                    int weekIndex = int.Parse(match.Attribute("weekend").Value);
                    int homeTeam = int.Parse(match.Attribute("homeTeam").Value);
                    int visitorTeam = int.Parse(match.Attribute("visitorTeam").Value);
                    po.matches.Add(new Match(weekIndex, homeTeam, visitorTeam, serie, po));
                }
            }
            XElement teamConstraints = competition.Element("TeamConstraints");
            if (teamConstraints != null)
            {
                foreach (XElement con in teamConstraints.Elements("Constraint"))
                {
                    int teamId = int.Parse(con.Attribute("TeamId").Value);
                    DateTime date = DateTime.Parse(con.Attribute("Date").Value);
                    TeamConstraint.HomeVisitNone what = (TeamConstraint.HomeVisitNone)Enum.Parse(typeof(TeamConstraint.HomeVisitNone), con.Attribute("What").Value);
                    Team team = klvvnew.teams.Find(t => t.Id == teamId);
                    TeamConstraint tc = new TeamConstraint(team);
                    tc.homeVisitNone = what;
                    tc.date = date;
                    tc.cost = int.Parse(con.Attribute("Cost").Value);
                    klvvnew.teamConstraints.Add(tc);
                }
            }
            klvvnew.fileName = openFileDialog1.FileName;
            klvvnew.RenewConstraints();
            klvv.Changed(klvvnew);
            klvv = klvvnew;
            klvv.Evaluate(null);
            klvv.Changed();
            state.Clear();
            Text = string.Format("Volleyball competition creation tool ({0})", klvv.year);
        }

        private void vVBCompetitionToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            klvv.ImportVVBCompetition1();
            klvv.RenewConstraints();
            klvv.Evaluate(null);
            klvv.Changed();

        }

        private void subscriptionsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string MyDocuments = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            klvv.ImportTeamSubscriptions(XDocument.Load(MyDocuments + "\\ClubRegistrations.xml").Root);
            klvv.RenewConstraints();
            klvv.Evaluate(null);
            klvv.Changed();
        }

        private void kLVVCompetitionToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            klvv.ImportKLVVCompetition();
            klvv.RenewConstraints();
            klvv.Evaluate(null);
            klvv.Changed();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InputForm form = new InputForm("Creeer een nieuw seizoen", "Geef een jaartal");
            form.ShowDialog();
            if (form.Result)
            {
                int year;
                bool ok = int.TryParse(form.GetInputString(), out year);
                if (ok)
                {

                    Klvv newKlvv = new Klvv(year);
                    klvv.Changed(newKlvv);
                    klvv = newKlvv;
                    klvv.Evaluate(null);
                    klvv.Changed();
                    state.Clear();
                    Text = string.Format("Volleyball competition creation tool ({0})", klvv.year);

                }
            }

        }

        private void saveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            WriteProject(klvv.fileName);
        }

        private void kLVVTeamSubscriptionsklvvbeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string MyDocuments = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //klvv.ImportTeamSubscriptions(XDocument.Load(MyDocuments + "\\registrationsXML.php").Root);
            klvv.ImportTeamSubscriptions(XDocument.Load("http://klvv.be/server/restricted/registrations/registrationsXML.php").Root);
            klvv.RenewConstraints();
            klvv.Evaluate(null);
            klvv.Changed();
        }

        private void clubRegistrationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string MyDocuments = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            XmlWriter writer = XmlWriter.Create(MyDocuments+"\\ClubRegistrations.xml");
            writer.WriteStartDocument();
            klvv.WriteClubConstraints(writer);
            writer.WriteEndDocument();
            writer.Close();
        }

        private void competitionxmlForKlvvsiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            saveFileDialog1.FileName = string.Format("ExportToKlvv{0:00}{1:00}{2:00}_{3:00}{4:00}.xml", now.Year, now.Month, now.Day, now.Hour, now.Minute);
            saveFileDialog1.Filter = "Xml (*.xml)|*.xml";
            saveFileDialog1.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
            saveFileDialog1.FileOk += new CancelEventHandler(saveFileDialog1_FileOk4);
            saveFileDialog1.ShowDialog();
        }
        public void saveFileDialog1_FileOk4(object sender, CancelEventArgs e)
        {
            string MyDocuments = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            XmlWriter writer = XmlWriter.Create(saveFileDialog1.FileName);
            writer.WriteStartDocument();
            writer.WriteStartElement("Competition");
            klvv.WritePoules(writer);
            klvv.WriteTeams(writer);
            klvv.WriteMatches(writer);
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }

        private void poulesSeriescsvToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            saveFileDialog1.FileName = string.Format("PouleSeries{0:00}{1:00}{2:00}_{3:00}{4:00}.csv",now.Year,now.Month,now.Day,now.Hour,now.Minute);
            saveFileDialog1.Filter = "Comma-separated (*.csv)|*.csv";
            saveFileDialog1.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
            saveFileDialog1.FileOk += new CancelEventHandler(saveFileDialog1_FileOk5);
            saveFileDialog1.ShowDialog();
        }
        public void saveFileDialog1_FileOk5(object sender, CancelEventArgs e)
        {
            StreamWriter writer = new StreamWriter(saveFileDialog1.FileName);
            writer.WriteLine("Serie,Poule,Poule-team-number,Team,Club,Speeldag,Speeltijd");
            foreach (Team team in klvv.teams)
            {
                if (team.serie.id >= 0)
                {
                    string pouleName = "-";
                    if (team.poule != null)
                    {
                        pouleName = team.poule.name;
                    }

                    writer.WriteLine("{0},{1},{2},{3},{4},{5},{6}", team.serie.name, pouleName, team.Index, team.name, team.club.name, team.defaultDay.ToString(), team.defaultTime.ToString());
                }
            }
            writer.Close();
        }

        private void matchescsvToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            saveFileDialog1.FileName = string.Format("Matches{0:00}{1:00}{2:00}_{3:00}{4:00}.csv", now.Year, now.Month, now.Day, now.Hour, now.Minute);
            saveFileDialog1.Filter = "Comma-separated (*.csv)|*.csv";
            saveFileDialog1.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
            saveFileDialog1.FileOk += new CancelEventHandler(saveFileDialog1_FileOk6);
            saveFileDialog1.ShowDialog();
        }
        public void saveFileDialog1_FileOk6(object sender, CancelEventArgs e)
        {
            StreamWriter writer = new StreamWriter(saveFileDialog1.FileName);
            writer.WriteLine("Serie,Poule,Speeldag,datum,Speeltijd,homeClub,homeTeam,visitorClub,visitorTeam");
            foreach (Poule poule in klvv.poules)
            {
                if(poule.serie.id>=0)
                {
                    foreach(Match match in poule.matches)
                    {
                        if (match.RealMatch())
                        {
                            writer.Write("{0},{1},{2},{3},{4},{5},{6},{7},{8}", poule.serie.name, poule.fullName, match.datetime.ToShortDateString(), match.DayString, match.Time.ToString(), match.homeTeam.club.name, match.homeTeam.name, match.visitorTeam.club.name, match.visitorTeam.name);

                            foreach (Constraint con in match.conflictConstraints)
                            {
                                writer.Write("," + con.name);
                            }
                            writer.WriteLine();
                        }
                    }
                }
            }
            writer.Close();
        }


    }
}

/*
            dockPanel.SuspendLayout(true);

            CloseAllDocuments();

            CreateStandardControls();

            m_solutionExplorer.Show(dockPanel, DockState.DockRight);
            m_propertyWindow.Show(m_solutionExplorer.Pane, m_solutionExplorer);
            m_toolbox.Show(dockPanel, new Rectangle(98, 133, 200, 383));
            m_outputWindow.Show(m_solutionExplorer.Pane, DockAlignment.Bottom, 0.35);
            m_taskList.Show(m_toolbox.Pane, DockAlignment.Left, 0.4);

            DummyDoc doc1 = CreateNewDocument("Document1");
            DummyDoc doc2 = CreateNewDocument("Document2");
            DummyDoc doc3 = CreateNewDocument("Document3");
            DummyDoc doc4 = CreateNewDocument("Document4");
            doc1.Show(dockPanel, DockState.Document);
            doc2.Show(doc1.Pane, null);
            doc3.Show(doc1.Pane, DockAlignment.Bottom, 0.5);
            doc4.Show(doc3.Pane, DockAlignment.Right, 0.5);

            dockPanel.ResumeLayout(true, true);
*/