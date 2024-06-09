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
    public partial class TeamListView : DockContent
    {
        Model model = null;
        GlobalState state;
        public TeamListView(Model model, GlobalState state)
        {
            this.model = model;
            this.state = state;
            InitializeComponent();
            objectListView1.SetObjects(model.teams);
            GlobalState.OnMyChange += new MyEventHandler(state_OnMyChange); 
            model.OnMyChange += state_OnMyChange;
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
            lock (model)
            {
                // Niet nodig? teams veranderen niet. Selected team is anders deselected
                // objectListView1.SetObjects(model.teams);
                objectListView1.BuildList(true);
                Refresh();
            }
        }

        private void objectListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hit = objectListView1.HitTest(e.Location);
            if (hit.Item != null)
            {
                Team team = objectListView1.GetModelObject(hit.Item.Index) as Team;
                if (team != null && team.poule != null)
                {
                    // check whether the PouleView is already existing
                    foreach (DockContent content in this.DockPanel.Contents)
                    {
                        PouleView pouleview = content as PouleView;
                        if (pouleview != null)
                        {
                            if (team.poule == pouleview.poule)
                            {
                                pouleview.Activate();
                                return;
                            }
                        }
                    }
                    PouleView pouleView = new PouleView(model, state, team.poule);
                    pouleView.Show(this.DockPanel);
                }
            };
        }

        private void TeamListView_FormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalState.OnMyChange -= new MyEventHandler(state_OnMyChange);
            model.OnMyChange -= state_OnMyChange;
        }

        private void objectListView1_CellClick(object sender, CellClickEventArgs e)
        {
            if (objectListView1.SelectedObjects.Count > 0)
            {

                List<Constraint> constraints = new List<Constraint>();
                foreach (Object obj in objectListView1.SelectedObjects)
                {
                    Team team = (Team)obj;
                    GlobalState.selectedClubs.Clear();
                    GlobalState.selectedClubs.Add(team.club);
                    if (team.poule != null)
                    {
                        constraints.AddRange(team.constraintList);
                    }
                    GlobalState.Changed();
                }
                GlobalState.ShowConstraints(constraints);
            }
        }
    }
}
