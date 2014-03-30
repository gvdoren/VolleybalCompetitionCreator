using System;
using System.Collections.Generic;
using System.Collections;
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
    public partial class SerieView : DockContent
    {
        Klvv klvv = null;
        GlobalState state;
        Serie serie = null;
        Poule poule = null;
        public SerieView(Klvv klvv, GlobalState state)
        {
            this.klvv = klvv;
            this.state = state;
            InitializeComponent();
            objectListView1.SetObjects(klvv.series.Values);

        }
        void UpdateSerieList()
        {
            objectListView1.BuildList(true);
        }
        void UpdatePouleList()
        {
            objectListView2.SetObjects(serie.poules.Values);
            objectListView2.BuildList(true);
        }
        private void objectListView1_SelectionChanged(object sender, EventArgs e)
        {
            serie = (Serie)objectListView1.SelectedObject;
            UpdatePouleList();
        }

        private void objectListView2_SelectionChanged(object sender, EventArgs e)
        {
            button2.Enabled = (objectListView2.SelectedObjects.Count == 1);
            poule = (Poule)objectListView2.SelectedObject;
        }

        private void button2_Click(object sender, EventArgs e)
        {// delete poule
            foreach (Team team in poule.teams)
            {
                team.poule = null;
            }
            int i = 0;
            while(i<klvv.constraints.Count)
            {
                ConstraintSchemaTooClose c = klvv.constraints[i] as ConstraintSchemaTooClose;
                if (c != null && c.poule == poule)
                {
                    klvv.constraints.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
            serie.poules.Remove(poule.name);
            klvv.poules.Remove(poule);
            foreach (DockContent content in this.DockPanel.Contents)
            {
                PouleView pouleview = content as PouleView;
                if (pouleview != null)
                {
                    if (poule == pouleview.poule)
                    {
                        pouleview.Close();
                        break;
                    }
                }
            }

            UpdateSerieList();
            UpdatePouleList();
            klvv.Changed();
        }
    
    }
}
