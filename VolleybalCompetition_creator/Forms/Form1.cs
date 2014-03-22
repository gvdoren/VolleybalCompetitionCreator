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

        private void constraintsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClubConstraints clubConstraints = null;
            foreach (DockContent content in this.dockPanel.Contents)
            {
                clubConstraints = content as ClubConstraints;
                if (clubConstraints != null)
                {
                    clubConstraints.Activate();
                    return;
                }
            }
            clubConstraints = new ClubConstraints(klvv, state);
            clubConstraints.Show(this.dockPanel);

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