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

namespace CompetitionCreator
{
    public partial class PouleListView : DockContent
    {
        Model model = null;
        SerieFilter serieFilter;
        GlobalState state;
        public PouleListView(Model model, GlobalState state)
        {
            this.model = model;
            this.state = state;
            this.serieFilter = new SerieFilter(model,state);
            InitializeComponent();
            objectListView1.SetObjects(model.poules);
            objectListView1.ModelFilter = serieFilter;
            //objectListView1.Activation = ItemActivation.TwoClick;
            objectListView1.UseFiltering = true;
            GlobalState.OnMyChange += new MyEventHandler(state_OnMyChange); 
            model.OnMyChange += state_OnMyChange;
            //if (model.licenseKey.Feature(Security.LicenseKey.FeatureType.Expert))
            //{
            //    evaluatedColumn.IsVisible = true;
            //    objectListView1.RebuildColumns();
            //}
            //else
            {
                evaluatedColumn.IsVisible = false;
                objectListView1.RebuildColumns();
            }
        }
        public void state_OnMyChange(object source, MyEventArgs e)
        {
            if (e.model != null)
            {
                model.OnMyChange -= state_OnMyChange;
                model = e.model;
                model.OnMyChange += state_OnMyChange;
            }
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => state_OnMyChange(source, e)));
                return;
            }
            /*if (model.poules.Count != objectListView1.Items.Count)
            {
                objectListView1.SetObjects(model.poules);
            }*/
            lock (model)
            {
                objectListView1.SetObjects(GlobalState.shownPoules);
                objectListView1.SelectedObjects = GlobalState.selectedPoules;
                
                //objectListView1.SetObjects(model.poules);
                //objectListView1.BuildList();
                //Refresh();
            }
        }
        public class SerieFilter: IModelFilter 
        {
            Model model = null;
            GlobalState state = null;
            public SerieFilter(Model model,GlobalState state)
            {
                this.model = model;
                this.state = state;
            }
            public bool Filter(object modelObject)
            {
                Poule poule = (Poule)modelObject;
                if (GlobalState.selectedClubs.Count == 0) return true;
                foreach (Team team in poule.teams)
                {
                    if (GlobalState.selectedClubs.Contains(team.club))
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
                    PouleView pouleView = new PouleView(model, state, poule);
                    pouleView.Show(this.DockPanel);
                }
            };
        }


        private void objectListView1_SelectionChanged(object sender, EventArgs e)
        {

        }

        private void objectListView1_CellClick(object sender, CellClickEventArgs e)
        {
            GlobalState.selectedPoules.Clear();
            if (objectListView1.SelectedObjects.Count > 0)
            {

                List<Constraint> constraints = new List<Constraint>();
                foreach (Object obj in objectListView1.SelectedObjects)
                {
                    Poule poule = (Poule)obj;
                    constraints.AddRange(poule.conflictConstraints);
                    GlobalState.selectedPoules.Add(poule);
                }
                GlobalState.selectedClubs.Clear();
                GlobalState.ShowConstraints(constraints);
            }
        }

        private void PouleListView_FormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalState.OnMyChange -= new MyEventHandler(state_OnMyChange);
            model.OnMyChange -= state_OnMyChange;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //GlobalState.selectedClubs.Clear();
            GlobalState.shownPoules = model.poules;
            GlobalState.Changed();
        }


    }
}
