using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BrightIdeasSoftware;
using WeifenLuo.WinFormsUI.Docking;

namespace VolleybalCompetition_creator
{
    public partial class ConstraintListView : DockContent, IModelFilter
    {
        Klvv klvv;
        GlobalState state;
        public ConstraintListView(Klvv klvv, GlobalState state)
        {
            this.klvv = klvv;
            this.state = state;
            InitializeComponent();
            objectListView1.SetObjects(klvv.constraints);
            objectListView1.ModelFilter = this;
            objectListView1.UseFiltering = true;
            UpdateConflictCount();
            klvv.OnMyChange += state_OnMyChange;
            state.OnMyChange += state_OnMyChange;
            
        }
        public void state_OnMyChange(object source, MyEventArgs e)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => state_OnMyChange(source, e)));
                return;
            }
            lock (klvv) ;
            objectListView1.BuildList(true);
            objectListView1.SelectObject(state.selectedConstraint);
            UpdateConflictCount();
         }
        private void UpdateConflictCount()
        {
            int conflicts = 0;
            foreach (Constraint constraint in klvv.constraints)
            {
                conflicts += constraint.conflict;
            }
            label1.Text = "Conflicts: " + conflicts.ToString();
        }
        private void objectListView1_MouseClick(object sender, MouseEventArgs e)
        {
            objectListView1_MouseDoubleClick(sender, e);
        }

        private void objectListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {/*
            ListViewHitTestInfo hit = objectListView1.HitTest(e.Location);
            if (hit.Item != null)
            {
                ConstraintView constraintView = null;
                Constraint constraint = objectListView1.GetModelObject(hit.Item.Index) as Constraint;
                if (constraint != null)
                {
                    //this.DockPanel.
                    // check whether the PouleView is already existing
                    foreach (DockContent content in this.DockPanel.Contents)
                    {
                        ConstraintView temp = content as ConstraintView;
                        if (temp != null)
                        {
                            constraintView = temp; 
                            constraintView.Activate();
                            
                        }
                    }
                    if (constraintView == null)
                    {
                        constraintView = new ConstraintView(klvv, state);
                        constraintView.Show(Pane, DockAlignment.Bottom, 0.4);
                    }
                    // Show the correct constraint
                    constraintView.Show("matchView", constraint);
                    state.selectedConstraint = constraint;
                }
            };
          * */
        }
        public bool Filter(object modelObject)
        {
            Constraint constraint = (Constraint)modelObject;
            if (checkBox1.Checked && constraint.conflict == 0) return false;
            //if (state.showConstraints.Count == 0) return true;
            return state.showConstraints.Contains(constraint);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            objectListView1.BuildList(true);

        }

        private void objectListView1_SelectionChanged(object sender, EventArgs e)
        {
            ConstraintView constraintView = null;
            Constraint constraint = objectListView1.SelectedObject as Constraint;
            if (constraint != null)
            {
                //this.DockPanel.
                // check whether the PouleView is already existing
                foreach (DockContent content in this.DockPanel.Contents)
                {
                    ConstraintView temp = content as ConstraintView;
                    if (temp != null)
                    {
                        constraintView = temp;
                        constraintView.Activate();

                    }
                }
                if (constraintView == null)
                {
                    constraintView = new ConstraintView(klvv, state);
                    constraintView.Show(Pane, DockAlignment.Bottom, 0.45);
                }
                // Show the correct constraint
                constraintView.Show("matchView", constraint);
                state.selectedConstraint = constraint;
            }
        }

        private void ConstraintListView_FormClosed(object sender, FormClosedEventArgs e)
        {
            klvv.OnMyChange -= state_OnMyChange;
            state.OnMyChange -= state_OnMyChange;

        }
    }

}
