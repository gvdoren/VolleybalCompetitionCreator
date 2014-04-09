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

namespace VolleybalCompetition_creator
{
    public partial class Form1 : Form
    {
        Klvv klvv = new Klvv();
        GlobalState state = new GlobalState();
        public Form1()
        {
            //Weekend.Test();
            InitializeComponent();
            // reading club-constraints


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
            klvv.ImportTeamSubscriptions();
            klvv.RenewConstraints();
            klvv.Evaluate(null);
            klvv.Changed();
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

        private void vVBCompetitionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            klvv.ImportVVBCompetition();
            klvv.RenewConstraints();
            klvv.Evaluate(null);
            klvv.Changed();
        }

        private void kLVVCompetitionToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            klvv.ImportKLVVCompetition();
            klvv.RenewConstraints();
            klvv.Evaluate(null);
            klvv.Changed();
        }

        private void reportToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void perTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "ConflictReportPerType.txt";
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
                if (constraint.conflict > 0)
                {
                    if (constraint.name != name)
                    {
                        name = constraint.name;
                        writer.WriteLine("{0}", name);
                    }
                    if (constraint.Title != title)
                    {
                        title = constraint.Title;
                        writer.WriteLine(" - {0}", title);
                    }
                    foreach (Match match in constraint.conflictMatches)
                    {
                        writer.WriteLine("   * {0} {1} - {2}", match.datetime, match.homeTeam.name, match.visitorTeam.name);
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
                if (club.conflict > 0)
                {
                    writer.WriteLine("{0}", club.name);
                    club.conflictConstraints.Sort(delegate(Constraint c1, Constraint c2) { return c1.Title.CompareTo(c2.Title); });
                    foreach (Constraint constraint in club.conflictConstraints)
                    {
                        if (constraint.conflict > 0)
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
                            foreach (Match match in constraint.conflictMatches)
                            {
                                writer.WriteLine("    * {0} {1} - {2}", match.datetime, match.homeTeam.name, match.visitorTeam.name);
                            }
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