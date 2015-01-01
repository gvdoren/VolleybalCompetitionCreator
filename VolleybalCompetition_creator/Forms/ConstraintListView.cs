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
            if (e.klvv != null)
            {
                klvv.OnMyChange -= state_OnMyChange;
                klvv = e.klvv;
                objectListView1.SetObjects(klvv.constraints);
                klvv.OnMyChange += state_OnMyChange;
            }
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => state_OnMyChange(source, e)));
                return;
            }
            lock (klvv)
            {
                if (state.selectedConstraint != null && state.showConstraints.Contains(state.selectedConstraint) == false && state.selectedConstraint.error == false)
                {
                    if (state.showConstraints.Count > 0)
                    {

                        state.selectedConstraint = state.showConstraints[0];
                        objectListView1.SelectObject(state.selectedConstraint,true);
                    }
                    else
                    {
                        state.selectedConstraint = null;
                        objectListView1.SelectedObjects.Clear();
                    }
                }
                objectListView1.BuildList(true);
                //objectListView1_SelectionChanged(null, null);
                UpdateConflictCount();
            }
         }
        private void UpdateConflictCount()
        {
            bool error = false;
            int conflicts = 0;
            foreach (Constraint constraint in klvv.constraints)
            {
                conflicts += constraint.conflict_cost;
                error |= constraint.error;
                
            }
            int totalMatches = 0;
            int conflictMatches = 0;
            foreach (Poule poule in klvv.poules)
            {
                if (poule.evaluated)
                {
                    foreach (Match mat in poule.matches)
                    {
                        if (mat.RealMatch())
                        {
                            if (mat.conflict > 0)
                            {
                                conflictMatches++;
                            }
                            totalMatches++;
                        }
                    }
                }
            }


            if (error) label1.ForeColor = Color.Red;
            else label1.ForeColor = Color.Black;
            label1.Text = "Conflicts: " + conflicts.ToString();
            double percentage = 0;
            if (totalMatches > 0)
            {
                percentage = conflictMatches*100;
                percentage /= totalMatches;
            }
            else
            {
                percentage = 0;
            }
            label2.Text = "Conflict-matches: " + conflictMatches.ToString() + string.Format(" ({0:F1}%)",percentage);
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
            return state.showConstraints.Contains(constraint) || constraint.error;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            objectListView1.BuildList(true);

        }

        private void objectListView1_SelectionChanged(object sender, EventArgs e)
        {
            Constraint constraint = objectListView1.SelectedObject as Constraint;
            //this.DockPanel.
            // check whether the PouleView is already existing
            ConstraintView constraintView = (ConstraintView)this.DockPanel.Contents.FirstOrDefault(d => (d as ConstraintView) != null);
            if (constraintView == null)
            {
                constraintView = new ConstraintView(klvv, state);
                constraintView.Show(Pane, DockAlignment.Bottom, 0.45);
            }
            // Show the correct constraint
            if (constraint != state.selectedConstraint)
            {
                state.selectedConstraint = constraint;
                state.Changed();
            }
        }

        private void ConstraintListView_FormClosed(object sender, FormClosedEventArgs e)
        {
            klvv.OnMyChange -= state_OnMyChange;
            state.OnMyChange -= state_OnMyChange;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            klvv.Evaluate(null);
            klvv.Changed();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            state.showConstraints.Clear();
            foreach (Constraint con in klvv.constraints)
            {
                if (con.conflictMatches.Count > 0 || con.conflict_cost >0)
                {
                    state.showConstraints.Add(con);
                }
            }
            state.Changed();
        }
    }

}
