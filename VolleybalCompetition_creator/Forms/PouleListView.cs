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
    public partial class PouleListView : DockContent
    {
        Klvv klvv = null;
        SerieFilter serieFilter;
        GlobalState state;
        public PouleListView(Klvv klvv, GlobalState state)
        {
            this.klvv = klvv;
            this.state = state;
            this.serieFilter = new SerieFilter(klvv,state);
            InitializeComponent();
            objectListView1.SetObjects(klvv.poules);
            objectListView1.ModelFilter = serieFilter;
            //objectListView1.Activation = ItemActivation.TwoClick;
            objectListView1.UseFiltering = true;
            state.OnMyChange += new MyEventHandler(state_OnMyChange); 
            klvv.OnMyChange += state_OnMyChange;
        }
        public void state_OnMyChange(object source, MyEventArgs e)
        {
            if (e.klvv != null)
            {
                klvv.OnMyChange -= state_OnMyChange;
                klvv = e.klvv;
                klvv.OnMyChange += state_OnMyChange;
            }
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => state_OnMyChange(source, e)));
                return;
            }
            /*if (klvv.poules.Count != objectListView1.Items.Count)
            {
                objectListView1.SetObjects(klvv.poules);
            }*/
            lock (klvv) ;
            objectListView1.SetObjects(klvv.poules);
            objectListView1.BuildList(true);
            Refresh();
        }
        public class SerieFilter: IModelFilter 
        {
            Klvv klvv = null;
            GlobalState state = null;
            public SerieFilter(Klvv klvv,GlobalState state)
            {
                this.klvv = klvv;
                this.state = state;
            }
            public bool Filter(object modelObject)
            {
                Poule poule = (Poule)modelObject;
                if (state.selectedClubs.Count == 0) return true;
                foreach (Team team in poule.teams)
                {
                    if (state.selectedClubs.Contains(team.club))
                    {
                        return true;
                    }
                }
                return false;
            } 
        }

        private void objectListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hit = objectListView1.HitTest(e.Location);
            if (hit.Item != null)
            {
                Poule poule = objectListView1.GetModelObject(hit.Item.Index) as Poule;
                if (poule != null)
                {
                    // check whether the PouleView is already existing
                    foreach (DockContent content in this.DockPanel.Contents)
                    {
                        PouleView pouleview = content as PouleView;
                        if (pouleview != null)
                        {
                            if (poule == pouleview.poule)
                            {
                                pouleview.Activate();
                                return;
                            }
                        }
                    }
                    PouleView pouleView = new PouleView(klvv, state, poule);
                    pouleView.Show(this.DockPanel);
                }
            };
        }


        private void objectListView1_SelectionChanged(object sender, EventArgs e)
        {

        }

        private void objectListView1_CellClick(object sender, CellClickEventArgs e)
        {
            if (objectListView1.SelectedObjects.Count > 0)
            {

                List<Constraint> constraints = new List<Constraint>();
                foreach (Object obj in objectListView1.SelectedObjects)
                {
                    Poule poule = (Poule)obj;
                    constraints.AddRange(poule.conflictConstraints);
                }
                state.ShowConstraints(constraints);
            }
        }

        private void PouleListView_FormClosed(object sender, FormClosedEventArgs e)
        {
            state.OnMyChange -= new MyEventHandler(state_OnMyChange);
            klvv.OnMyChange -= state_OnMyChange;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            state.selectedClubs.Clear();
            state.Changed();
        }


    }
}
