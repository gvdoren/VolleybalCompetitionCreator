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
    public partial class ConstraintListView : DockContent
    {
        Model model;
        GlobalState state;
        public ConstraintListView(Model model, GlobalState state)
        {
            this.model = model;
            this.state = state;
            InitializeComponent();
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
                model.OnMyChange += state_OnMyChange;
            }
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => state_OnMyChange(source, e)));
                return;
            }
            lock (model)
            {
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
            label1.Text = "Conflict-matches: " + conflictMatches.ToString() + string.Format(" ({0:F1}%)     Cost: {1}", percentage, conflicts.ToString());
        }
        private void ConstraintListView_FormClosed(object sender, FormClosedEventArgs e)
        {
            model.OnMyChange -= state_OnMyChange;
            GlobalState.OnMyChange -= state_OnMyChange;

        }

    }

}
