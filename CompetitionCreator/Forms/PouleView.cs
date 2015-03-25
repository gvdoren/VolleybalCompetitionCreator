﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using BrightIdeasSoftware;
using System.Drawing.Drawing2D;

namespace CompetitionCreator
{
    public partial class PouleView : DockContent
    {
        Model model = null;
        GlobalState state = null;
        public Poule poule = null;
        SimpleDropSink myDropSink = new SimpleDropSink();
        ProgressDialog diag = new ProgressDialog();
        Dictionary<Weekend, int> weekmapping = new Dictionary<Weekend, int>();
        List<Team> selectedTeams = new List<Team>();
        List<Weekend> selectedWeekends = new List<Weekend>();
        List<Match> selectedMatches = new List<Match>();

        public PouleView(Model model, GlobalState state, Poule poule)
        {
            this.model = model;
            this.state = state;
            this.poule = poule;
            InitializeComponent();
            this.Text = "Poule - " + poule.serie.name + poule.name;
            objectListView1.ShowGroups = false;
            objectListView1.SetObjects(new List<Team>(poule.teams));
            myDropSink.CanDropBetween = true;
            objectListView1.DropSink = myDropSink;
            //this.objectListView1.FormatRow += objectListView1_FormatRow;
            objectListView1.BuildList();
            objectListView1_SelectionChanged(null, null);
            objectListView1.HideSelection = false;
            objectListView2.ShowGroups = false;
            objectListView2.SetObjects(poule.matches);
            objectListView2.HideSelection = false;
            UpdateWeekMapping();
            objectListView3_SelectionChanged(null, null);
            objectListView3.FormatRow += objectListView3_FormatRow;
            objectListView3.ShowGroups = false;
            objectListView3.HideSelection = false;
            objectListView3.SetObjects(weekmapping);
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
                // If dynamically the poule is removed.
                if (model.poules.Contains(poule) == false)
                {
                    Close();
                    return;
                }

                selectedTeams.Clear();
                selectedMatches.Clear();
                selectedWeekends.Clear();
                foreach (Object obj in objectListView1.SelectedObjects)
                {
                    selectedTeams.Add((Team)obj);
                }
                objectListView1.BuildList(true);
                objectListView1.SelectedObjects = selectedTeams;
                foreach (Object obj in objectListView2.SelectedObjects)
                {
                    selectedMatches.Add((Match)obj);
                }
                objectListView2.Objects = poule.matches;
                objectListView2.BuildList(true);
                objectListView2.SelectedObjects = selectedMatches;
                foreach (Object obj in objectListView3.SelectedObjects)
                {
                    selectedWeekends.Add(((KeyValuePair<Weekend, int>)obj).Key);
                }
                UpdateWeekMapping();
                objectListView3.BuildList(true);
                foreach (Object obj in objectListView3.Objects)
                {
                    KeyValuePair<Weekend, int> kvp = (KeyValuePair<Weekend, int>)obj;
                    if (selectedWeekends.Contains(kvp.Key) == true) objectListView3.SelectObject(obj);
                }
            }
        }
       


        private void objectListView1_FormatRow(object sender, FormatRowEventArgs e)
        {
            e.Item.SubItems[1].Text = (e.DisplayIndex+1).ToString();
        }

        private void objectListView1_SelectionChanged(object sender, EventArgs e)
        {
            button1.Enabled = (objectListView1.SelectedObjects.Count == 1);
            button2.Enabled = (objectListView1.SelectedObjects.Count == 1);
            button3.Enabled = (objectListView1.SelectedObjects.Count >= 2);
            button9.Enabled = (objectListView1.SelectedObjects.Count == 1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (objectListView1.SelectedIndex > 0)
            {
                Team team1 = (Team) objectListView1.SelectedObject;
                int index = objectListView1.GetDisplayOrderOfItemIndex(objectListView1.SelectedItem.Index);
                OLVListItem next = objectListView1.GetNthItemInDisplayOrder(index - 1);
                Team team2 = (Team) next.RowObject;
                SwitchTeamList(team1, team2);
                objectListView1.SetObjects(poule.teams);
                objectListView1.SelectObject(team1);
                updateMatches();
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (objectListView1.SelectedIndex < objectListView1.GetItemCount())
            {
                Team team1 = (Team)objectListView1.SelectedObject;
                int index = objectListView1.GetDisplayOrderOfItemIndex(objectListView1.SelectedItem.Index);
                OLVListItem next = objectListView1.GetNthItemInDisplayOrder(index + 1);
                Team team2 = (Team)next.RowObject;
                SwitchTeamList(team1, team2);
                objectListView1.SetObjects(poule.teams);
                objectListView1.SelectObject(team1);
                updateMatches();
            }

        }
        private void SwitchTeamList(Team t1, Team t2)
        {
            List<Team> newList = new List<Team>();
            foreach (Team t in poule.teams)
            {
                if (t == t1)
                {
                    newList.Add(t2);
                }
                else if (t == t2)
                {
                    newList.Add(t1);
                }
                else
                {
                    newList.Add(t);
                }
            }
            poule.teams = newList;
        }
        private void Switch_Click(object sender, EventArgs e)
        {
            Team team1 = (Team) objectListView1.SelectedObjects[0];
            Team team2 = (Team) objectListView1.SelectedObjects[1];
            SwitchTeamList(team1, team2);
            objectListView1.SetObjects(poule.teams);
            List<Team> teams = new List<Team>();
            teams.Add(team1);
            teams.Add(team2);
            objectListView1.SelectObjects(teams);
            updateMatches();
        }

        void updateMatches()
        {
            model.Evaluate(null);
            model.Changed();
            objectListView2.Objects = poule.matches;
            objectListView2.BuildList();
            objectListView2.Refresh();
        }

        private void PouleView_FormClosing(object sender, FormClosingEventArgs e)
        {
            model.OnMyChange -= state_OnMyChange;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (poule.imported)
            {
                System.Windows.Forms.MessageBox.Show("Not allowed to change numbers in schema of an imported poule");
            }
            else if (poule.optimizable == false)
            {
                System.Windows.Forms.MessageBox.Show("Not allowed to change numbers in schema");
            }
            else
            {
                ProgressDialog diag = new ProgressDialog();
                diag.WorkFunction += OptimizeTeamAssignment;
                diag.CompletionFunction += OptimizeTeamAssignmentCompleted;
                diag.Start("Optimizing teams", null);
            }
        }
        private void OptimizeWeekAssignment(object sender, MyEventArgs e)
        {
            IProgress intf = (IProgress)sender;
            poule.SnapShot(model);
            poule.OptimizeWeekends(model, intf);
            poule.CopyAndClearSnapShot(model);
        }
        private void OptimizeWeekAssignmentCompleted(object sender, MyEventArgs e)
        {
//            UpdateWeekMapping();
//            objectListView3.SetObjects(weekmapping);
            model.Evaluate(null);
            model.Changed();
        }
        private void OptimizeTeamAssignment(object sender, MyEventArgs e)
        {
            IProgress intf = (IProgress)sender;
            poule.SnapShot(model);
            poule.OptimizeTeamAssignment(model, intf);
            poule.CopyAndClearSnapShot(model);
        }
        private void OptimizeTeamAssignmentCompleted(object sender, MyEventArgs e)
        {
//            objectListView1.SetObjects(poule.teams);
            model.Evaluate(null);
            model.Changed();
        }

        private void objectListView2_SelectionChanged(object sender, EventArgs e)
        {
            button5.Enabled = (objectListView2.SelectedObjects.Count == 1) ;
        }

        private void objectListView1_CellClick(object sender, CellClickEventArgs e)
        {
            if (objectListView1.SelectedObjects.Count > 0)
            {
                List<Constraint> constraints = new List<Constraint>();
                foreach (Object obj in objectListView1.SelectedObjects)
                {
                    Team team = (Team)obj;
                    constraints.AddRange(team.conflictConstraints);
                }
                state.ShowConstraints(constraints);
            }
        }

        private void objectListView2_CellClick(object sender, CellClickEventArgs e)
        {
            if (objectListView2.SelectedObjects.Count > 0)
            {
                List<Constraint> constraints = new List<Constraint>();
                foreach (Object obj in objectListView2.SelectedObjects)
                {
                    Match match = (Match)obj;
                    constraints.AddRange(match.conflictConstraints);
                }
                state.ShowConstraints(constraints);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (poule.imported)
            {
                System.Windows.Forms.MessageBox.Show("Not allowed to change home/visit for imported poules");
            }
            else if (poule.optimizableHomeVisit == false)
            {
                System.Windows.Forms.MessageBox.Show("Not allowed to change home/visit");
            }
            else
            {
                Match match1 = (Match)objectListView2.SelectedObject;
                poule.SwitchHomeTeamVisitorTeam(model, match1);
                model.Evaluate(null);
                model.Changed();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (poule.imported)
            {
                System.Windows.Forms.MessageBox.Show("Not allowed to change home/visit for imported poules");
            }
            else if (poule.optimizableHomeVisit == false)
            {
                System.Windows.Forms.MessageBox.Show("Not allowed to change home/visit");
            }
            else
            {
                poule.SnapShot(model);
                poule.OptimizeHomeVisitor(model);
                poule.OptimizeHomeVisitorReverse(model);
                poule.CopyAndClearSnapShot(model);
                model.Changed();
            }
        }

        private void objectListView3_FormatRow(object sender, FormatRowEventArgs e)
        {
            
            KeyValuePair<Weekend, int> kvp = (KeyValuePair<Weekend, int>)e.Model;
            if (kvp.Value > 0) e.Item.SubItems[1].Text = "Week " + kvp.Value.ToString();
            else e.Item.SubItems[1].Text = "----";
        }

        private void UpdateWeekMapping()
        {
            weekmapping.Clear();
            DateTime start = poule.weekendsFirst.Min(w => w.Saturday).AddDays(-7);
            DateTime end = poule.weekendsSecond.Max(w => w.Saturday).AddDays(7);
            for (DateTime date = start; date < end; date = date.AddDays(7))
            {
                Weekend weekend = new Weekend(date);
                int index = 0;
                int k = 0;
                for (int j = 0; j < poule.weekendsFirst.Count; j++, k++)
                {
                    if (poule.weekendsFirst[j].Saturday == date)
                    {
                        weekend = poule.weekendsFirst[j];
                        index = k + 1;
                    }
                }
                for (int j = 0; j < poule.weekendsSecond.Count; j++, k++)
                {
                    if (poule.weekendsSecond[j].Saturday == date)
                    {
                        weekend = poule.weekendsSecond[j];
                        index = k + 1;
                    }
                }
                weekmapping.Add(weekend, index);
            }
        }
        private void objectListView3_SelectionChanged(object sender, EventArgs e)
        {
            button7.Enabled = (objectListView3.SelectedObjects.Count == 2);
        }
        private void Switch1_Click(object sender, EventArgs e)
        {
            if (poule.imported)
            {
                System.Windows.Forms.MessageBox.Show("Not allowed to change week ordering for an imported poule");
            }
            else if (poule.optimizableWeekends == false)
            {
                System.Windows.Forms.MessageBox.Show("Not allowed to change week ordering");
            }
            else
            {
                /*KeyValuePair<Weekend, int> kvp1 = (KeyValuePair<Weekend, int>)objectListView3.SelectedObjects[0];
                KeyValuePair<Weekend, int> kvp2 = (KeyValuePair<Weekend, int>)objectListView3.SelectedObjects[1];
                int index1 = kvp1.Value - 1;
                int index2 = kvp2.Value - 1;
                if (index1 >= 0) poule.weekends[index1] = kvp2.Key;
                if (index2 >= 0) poule.weekends[index2] = kvp1.Key;
                UpdateWeekMapping();
                model.Evaluate(null);
                model.Changed();
                 */
            }
        }
        private void objectListView3_CellClick(object sender, CellClickEventArgs e)
        {
            if (objectListView3.SelectedObjects.Count > 0)
            {
                List<Constraint> constraints = new List<Constraint>();
                foreach (Object obj in objectListView3.SelectedObjects)
                {
                    KeyValuePair<Weekend, int> kvp = (KeyValuePair<Weekend, int>)obj;
                    constraints.AddRange(kvp.Key.conflictConstraints);
                }
                state.ShowConstraints(constraints);
            }
        }
        private void optimizeMatch_Click(object sender, EventArgs e)
        {
            if (poule.imported)
            {
                System.Windows.Forms.MessageBox.Show("Not allowed to change home/visit for imported poules");
            }
            else if (poule.optimizableMatch == false)
            {
                System.Windows.Forms.MessageBox.Show("Not allowed to change individual match");
            }
            else
            {
                poule.SnapShot(model);
                poule.OptimizeIndividualMatches(model);
                poule.CopyAndClearSnapShot(model);
                model.Changed();
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (poule.imported)
            {
                System.Windows.Forms.MessageBox.Show("Not allowed to change week ordering for imported teams");
            }
            else if (poule.optimizableWeekends == false)
            {
                System.Windows.Forms.MessageBox.Show("Not allowed to change week ordering");
            }
            else
            {

                ProgressDialog diag = new ProgressDialog();
                diag.WorkFunction += OptimizeWeekAssignment;
                diag.CompletionFunction += OptimizeWeekAssignmentCompleted;
                diag.Start("Optimizing weekends", null);
            }
        }
        Team optimizeTeam = null;
        private void button9_Click(object sender, EventArgs e)
        {
            optimizeTeam = (Team)objectListView1.SelectedObject;
            ProgressDialog diag = new ProgressDialog();
            diag.WorkFunction += OptimizeTeam;
            diag.CompletionFunction += OptimizeCompleted;
            diag.Start("Optimizing", null);
        }
        private void OptimizeTeam(object sender, MyEventArgs e)
        {
            IProgress intf = (IProgress)sender;
            intf.SetText("Optimizing team - ("+optimizeTeam.name+")");
            poule.OptimizeTeam(model, intf, optimizeTeam);
        }
        private void OptimizeCompleted(object sender, MyEventArgs e)
        {
            model.Evaluate(null);
            model.Changed();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            ProgressDialog diag = new ProgressDialog();
            diag.WorkFunction += OptimizePoule;
            diag.CompletionFunction += OptimizeCompleted;
            diag.Start("Optimizing", null);
        }
        private void OptimizePoule(object sender, MyEventArgs e)
        {
            IProgress intf = (IProgress)sender;
            intf.SetText("Optimizing poule - " + poule.serie.name + poule.name);
            poule.SnapShot(model);
            poule.OptimizeTeamAssignment(model, intf);
            poule.OptimizeHomeVisitor(model);
            poule.OptimizeWeekends(model, intf);
            poule.CopyAndClearSnapShot(model);
            model.Evaluate(null);
            if (intf.Cancelled()) return;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            ProgressDialog diag = new ProgressDialog();
            diag.WorkFunction += AnalyzePoule;
            diag.CompletionFunction += OptimizeCompleted;
            diag.Start("Analyzing", null);
        }
        private void AnalyzePoule(object sender, MyEventArgs e)
        {
            IProgress intf = (IProgress)sender;
            intf.SetText("Analysing poule - " + poule.serie.name + poule.name);
            poule.SnapShot(model);
            poule.AnalyzeTeamAssignment(model, intf);
            poule.CopyAndClearSnapShot(model);
            model.Evaluate(null);
            if (intf.Cancelled()) return;

        }


        private void button13_Click(object sender, EventArgs e)
        {
            if (poule.imported)
            {
                System.Windows.Forms.MessageBox.Show("Not allowed to change home/visit for imported teams");
            }
            else if (poule.optimizableHomeVisit == false)
            {
                System.Windows.Forms.MessageBox.Show("Not allowed to change home/visit");
            }
            else
            {
                poule.SnapShot(model);
                poule.OptimizeHomeVisitorReverse(model);
                poule.CopyAndClearSnapShot(model);
                model.Changed();
            }
        }

    }
}
