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
    public partial class MatchesView : DockContent
    {
        Model model = null;
        GlobalState state = null;
        public MatchesView(Model model, GlobalState state)
        {
            this.model = model;
            this.state = state;
            InitializeComponent();
            model.OnMyChange += state_OnMyChange;
            objectListView1.ShowGroups = false;
            GlobalState.OnMyChange += state_OnMyChange;
            UpdateTabPage2();
            
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
                UpdateTabPage2();
            }
        }
        private void UpdateTabPage2()
        {
            if(GlobalState.selectedPoules.Count ==0 && GlobalState.selectedClubs.Count == 0)
            {
                objectListView1.ClearObjects();
                return;
            }
            int conflicts = 0;
            int allSelectedMatches = 0;
            List<Match> matches = new List<Match>();
            if(GlobalState.selectedClubs.Count>0)
            {
                foreach(Club club in GlobalState.selectedClubs)
                {
                    foreach(Team team in club.teams)
                    {
                        if(team.poule != null)
                        {
                            foreach(Match match in team.poule.matches)
                            {
                                if (match.homeTeam.club == club)
                                {
                                    allSelectedMatches++;
                                    if(checkBoxConflictsOnly.Checked == false || match.conflict >0 )
                                        matches.Add(match);
                                    if (match.conflict > 0)
                                        conflicts++;
                                }
                            }
                        }
                    }
                }
            }
            else if(GlobalState.selectedPoules.Count >0) 
            {
                foreach(Poule poule in GlobalState.selectedPoules)
                {
                    foreach (Match match in poule.matches)
                    {
                        allSelectedMatches++;
                        if (checkBoxConflictsOnly.Checked == false || match.conflict > 0)
                            matches.Add(match);
                        if (match.conflict > 0)
                            conflicts++;
                    }
                }
            }
            objectListView1.SetObjects(matches, true);
            label1.Text = string.Format("Matches:{0}  Conflicts:{1}  ({2:F1}%)", allSelectedMatches, conflicts, ((double)conflicts * 100) / allSelectedMatches);
            /*
            objectListView1.ClearObjects();
            label1.Text = "";
            this.Text = "No conflicts selected";
            richTextBox2.Clear();

            Constraint constraint = GlobalState.selectedConstraint;
            if (constraint != null)
            {
                objectListView1.SetObjects(constraint.conflictMatches, true);
                //objectListView1.Objects = constraint.conflictMatches;
                this.Text = constraint.Title;
                label1.Text = "Conflict wedstrijden (" + constraint.conflictMatches.Count.ToString() + ")";
                //objectListView1.BuildList(true);
                richTextBox2.Clear();
                foreach (string str in constraint.GetTextDescription())
                {
                    richTextBox2.AppendText(str + Environment.NewLine);
                }
            }
             */
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
                    PouleView pouleView = new PouleView(model, state, match.poule);
                    pouleView.Show(this.DockPanel);
                }
            }


        }

        private void ConstraintView_FormClosed(object sender, FormClosedEventArgs e)
        {
            model.OnMyChange -= state_OnMyChange;
            GlobalState.OnMyChange -= state_OnMyChange;
        }

        private void objectListView1_SelectionChanged(object sender, EventArgs e)
        {
            Match match = objectListView1.SelectedObject as Match;
            if(match != null)
            {
                labelTeam.Text = "Team: " + match.homeTeam.name;
                labelSeriePoule.Text = "Serie-Poule: " + match.homeTeam.seriePouleName;
                labelGroup.Text = "Group: " + match.homeTeam.group.ToStringCustom();
                string context = "";
                string conflict = "";
                foreach(Constraint con in match.conflictConstraints)
                {
                    foreach (string line in match.conflictConstraints[0].GetTextDescription())
                    {
                        context += line + Environment.NewLine;
                    }
                    if (conflict == "")
                        conflict = con.Title;
                }
                conflictLabel.Text = "Conflict: " + conflict;
                richTextConflict.Text = context;
            } else
            {
                labelTeam.Text = "Team: ";
                labelSeriePoule.Text = "Serie-Poule: ";
                labelGroup.Text = "Group: ";
                conflictLabel.Text = "Conflict: ";
                richTextConflict.Clear();
            }
        }

        private void checkBoxConflictsOnly_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTabPage2();
        }
    }
}
