using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace CompetitionCreator
{
    public partial class ClubListView : DockContent
    {
        Model model = null;
        GlobalState state;
        public ClubListView(Model model, GlobalState state)
        {
            this.model = model;
            this.state = state;
            InitializeComponent();
            objectListView1.SetObjects(model.clubs);
            model.OnMyChange += state_OnMyChange;
            GlobalState.OnMyChange += state_OnMyChange;
            
        }
        public void state_OnMyChange(object source, MyEventArgs e)
        {
            if (e.model != null)
            {
                model.OnMyChange -= state_OnMyChange;
                model = e.model;
                model.OnMyChange += state_OnMyChange;
                objectListView1.SetObjects(model.clubs);
            }
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => state_OnMyChange(source, e)));
                return;
            }
            lock (model)
            {
                this.objectListView1.SelectedIndexChanged -= this.objectListView1_SelectedIndexChanged;
                objectListView1.SelectedObjects = GlobalState.selectedClubs;
                objectListView1.BuildList(true);
                this.objectListView1.SelectedIndexChanged += this.objectListView1_SelectedIndexChanged;
            }
        }

        private void objectListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            GlobalState.selectedClubs.Clear();
            //GlobalState.selectedPoules.Clear();
            foreach (Object obj in objectListView1.SelectedObjects)
            {
                Club club = (Club)obj;
                GlobalState.selectedClubs.Add(club);
                GlobalState.shownPoules.Clear();
                foreach(Team team in club.teams)
                {
                    if (team.poule != null)
                        GlobalState.shownPoules.Add(team.poule);
                }
            }

            GlobalState.Changed();
        }
        private void objectListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hit = objectListView1.HitTest(e.Location);
            if (hit.Item != null)
            {
                Club club = objectListView1.GetModelObject(hit.Item.Index) as Club;
                if (club != null)
                {
                    int columnIndex = hit.Item.SubItems.IndexOf(hit.SubItem);
                    if (columnIndex == 1)
                    {
                        // check whether the PouleView is already existing
                        foreach (DockContent content in this.DockPanel.Contents)
                        {
                            PouleListView poulelistview = content as PouleListView;
                            if (poulelistview != null)
                            {
                                poulelistview.Activate();
                                GlobalState.selectedClubs.Clear();
                                foreach (Object obj in objectListView1.SelectedObjects)
                                {
                                    Club club1 = (Club)obj;
                                    GlobalState.selectedClubs.Add(club1);
                                }
                                GlobalState.Changed();
                                return;
                            }
                        }
                        PouleListView poulelistView = new PouleListView(model, state);
                        poulelistView.ShowHint = DockState.DockLeft;
                        poulelistView.Show(this.DockPanel);
                    }
                    else
                    {

                    }
                }
            };
        }

        private void objectListView1_CellClick(object sender, BrightIdeasSoftware.CellClickEventArgs e)
        {
            if (objectListView1.SelectedObjects.Count > 0)
            {
                List<Constraint> constraints = new List<Constraint>();
                foreach (Object obj in objectListView1.SelectedObjects)
                {
                    Club club = (Club)obj;
                    constraints.AddRange(club.conflictConstraints);
                }
                GlobalState.ShowConstraints(constraints);
            }
        }

        private void ClubListView_FormClosed(object sender, FormClosedEventArgs e)
        {
            model.OnMyChange -= state_OnMyChange;
            GlobalState.OnMyChange -= state_OnMyChange;
        }
    }
    internal static class ControlExtensionMethods
    {
        /// <summary>
        /// Invokes the given action on the given control's UI thread, if invocation is needed.
        /// </summary>
        /// <param name="control">Control on whose UI thread to possibly invoke.</param>
        /// <param name="action">Action to be invoked on the given control.</param>
        public static void MaybeInvoke(this Control control, Action action)
        {
            if (control != null && control.InvokeRequired)
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }
        }

        /// <summary>
        /// Maybe Invoke a Func that returns a value.
        /// </summary>
        /// <typeparam name="T">Return type of func.</typeparam>
        /// <param name="control">Control on which to maybe invoke.</param>
        /// <param name="func">Function returning a value, to invoke.</param>
        /// <returns>The result of the call to func.</returns>
        public static T MaybeInvoke<T>(this Control control, Func<T> func)
        {
            if (control != null && control.InvokeRequired)
            {
                return (T)(control.Invoke(func));
            }
            else
            {
                return func();
            }
        }
    }
}
