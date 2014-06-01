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
    public partial class ConstraintView : DockContent
    {
        Klvv klvv = null;
        GlobalState state = null;
        public ConstraintView(Klvv klvv, GlobalState state)
        {
            this.klvv = klvv;
            this.state = state;
            InitializeComponent();
            klvv.OnMyChange += state_OnMyChange;
            objectListView1.ShowGroups = false;
            state.OnMyChange += state_OnMyChange;
            
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
            lock (klvv) ;
            UpdateTabPage2();
        }
        private void UpdateTabPage2()
        {
            objectListView1.ClearObjects();
            label1.Text = "";
            this.Text = "No conflicts selected";
            Constraint constraint = state.selectedConstraint;
            if (constraint != null)
            {
                objectListView1.Objects = constraint.conflictMatches;
                this.Text = constraint.Title;
                label1.Text = "Conflict wedstrijden (" + constraint.conflictMatches.Count.ToString() + ")";
                objectListView1.BuildList(true);
                richTextBox2.Clear();
                foreach (string str in constraint.GetTextDescription())
                {
                    richTextBox2.AppendText(str + Environment.NewLine);
                }
            }
        }

        private void objectListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hit = objectListView1.HitTest(e.Location);
            if (hit.Item != null)
            {
                Match match = objectListView1.GetModelObject(hit.Item.Index) as Match;
                if (match != null && match.poule != null)
                {
                    // check whether the PouleView is already existing
                    foreach (DockContent content in this.DockPanel.Contents)
                    {
                        PouleView pouleview = content as PouleView;
                        if (pouleview != null)
                        {
                            if (match.poule == pouleview.poule)
                            {
                                pouleview.Activate();
                                return;
                            }
                        }
                    }
                    PouleView pouleView = new PouleView(klvv, state, match.poule);
                    pouleView.Show(this.DockPanel);
                }
            }


        }

        private void ConstraintView_FormClosed(object sender, FormClosedEventArgs e)
        {
            klvv.OnMyChange -= state_OnMyChange;
            state.OnMyChange -= state_OnMyChange;
        }
    }
}
