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

namespace CompetitionCreator
{
    public partial class ConstraintListView : DockContent, IModelFilter
    {
        Model model;
        GlobalState state;
        public ConstraintListView(Model model, GlobalState state)
        {
            this.model = model;
            this.state = state;
            InitializeComponent();
            objectListView1.SetObjects(model.constraints);
            objectListView1.ModelFilter = this;
            objectListView1.UseFiltering = true;
            UpdateConflictCount();
            model.OnMyChange += state_OnMyChange;
            GlobalState.OnMyChange += state_OnMyChange;
            
        }
        public void state_OnMyChange(object source, MyEventArgs e)
        {
            if (e.model != null)
            {
                model.OnMyChange -= state_OnMyChange;
                model = e.model;
                objectListView1.SetObjects(model.constraints);
                model.OnMyChange += state_OnMyChange;
            }
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => state_OnMyChange(source, e)));
                return;
            }
            lock (model)
            {
                if (GlobalState.selectedConstraint != null && GlobalState.showConstraints.Contains(GlobalState.selectedConstraint) == false)
                {
                    if (GlobalState.showConstraints.Count > 0)
                    {

                        GlobalState.selectedConstraint = GlobalState.showConstraints[0];
                        objectListView1.SelectObject(GlobalState.selectedConstraint, true);
                    }
                    else
                    {
                        GlobalState.selectedConstraint = null;
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
            int conflicts = 0;
            foreach (Constraint constraint in model.constraints)
            {
                conflicts += constraint.conflict_cost;
                
            }
            int totalMatches = 0;
            int conflictMatches = 0;
            foreach (Poule poule in model.poules)
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
            label2.Text = "Conflict-matches: " + conflictMatches.ToString() + string.Format(" ({0:F1}%)     Cost: {1}", percentage, conflicts.ToString());
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
                        constraintView = new ConstraintView(model, state);
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
            return GlobalState.showConstraints.Contains(constraint);
        }


        private void objectListView1_SelectionChanged(object sender, EventArgs e)
        {
            Constraint constraint = objectListView1.SelectedObject as Constraint;
            //this.DockPanel.
            // check whether the PouleView is already existing
            ConstraintView constraintView = (ConstraintView)this.DockPanel.Contents.FirstOrDefault(d => (d as ConstraintView) != null);
            if (constraintView == null)
            {
                constraintView = new ConstraintView(model, state);
                constraintView.Show(Pane, DockAlignment.Bottom, 0.45);
            }
            // Show the correct constraint
            if (constraint != GlobalState.selectedConstraint)
            {
                GlobalState.selectedConstraint = constraint;
                GlobalState.Changed();
            }
        }

        private void ConstraintListView_FormClosed(object sender, FormClosedEventArgs e)
        {
            model.OnMyChange -= state_OnMyChange;
            GlobalState.OnMyChange -= state_OnMyChange;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            model.Evaluate(null);
            model.Changed();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            GlobalState.showConstraints.Clear();
            foreach (Constraint con in model.constraints)
            {
                if (con.conflictMatches.Count > 0 || con.conflict_cost >0)
                {
                    GlobalState.showConstraints.Add(con);
                }
            }
            GlobalState.Changed();
        }
    }

}
