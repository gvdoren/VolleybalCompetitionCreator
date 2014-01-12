using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace VolleybalCompetition_creator
{
    public partial class Form1 : Form
    {
        Klvv klvv = new Klvv();
        GlobalState state = new GlobalState();
        public Form1()
        {
            Match.Test();
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            ClubView clubview = new ClubView(klvv,state);
            clubview.ShowHint = DockState.DockLeft;
            clubview.Show(dockPanel);

            SerieView serieview = new SerieView(klvv, state);
            serieview.ShowHint = DockState.DockRight;
            serieview.Show(dockPanel);

            //serieview.Show(clubview.Pane, DockAlignment.Right, 0.35);
            
            SerieTreeView serietreeview = new SerieTreeView(klvv, state);
            serietreeview.ShowHint = DockState.DockRight;
            serietreeview.Show(dockPanel);
            PouleListView pouleListview = new PouleListView(klvv, state);
            pouleListview.ShowHint = DockState.DockLeft;
            pouleListview.Show(clubview.Pane, DockAlignment.Right, 0.5);
            //pouleListview.Show(dockPanel);
            ConstraintListView constraintListview = new ConstraintListView(klvv, state);
            constraintListview.ShowHint = DockState.DockRight;
            constraintListview.Show(dockPanel);

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