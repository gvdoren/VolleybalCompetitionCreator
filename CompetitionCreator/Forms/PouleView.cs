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
        Dictionary<MatchWeek, int> weekmapping = new Dictionary<MatchWeek, int>();
        List<Team> selectedTeams = new List<Team>();
        List<MatchWeek> selectedWeeks = new List<MatchWeek>();
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
            objectListView1.Sort(olvColumn3, SortOrder.Ascending);

            myDropSink.CanDropBetween = true;
            objectListView1.DropSink = myDropSink;
            //this.objectListView1.FormatRow += objectListView1_FormatRow;
            objectListView1.BuildList();
            objectListView1_SelectionChanged(null, null);
            objectListView1.HideSelection = false;
            objectListView1.PrimarySortColumn = invisible;
            objectListView1.PrimarySortOrder = SortOrder.Ascending;
            objectListView2.ShowGroups = false;
            objectListView2.SetObjects(poule.matches);
            objectListView2.HideSelection = false;
            UpdateWeekMapping();
            objectListView3_SelectionChanged(null, null);
            objectListWeeks.FormatRow += objectListView3_FormatRow;
            objectListWeeks.ShowGroups = false;
            objectListWeeks.HideSelection = false;
            objectListWeeks.PrimarySortColumn = olvColumn11;
            objectListWeeks.PrimarySortOrder = SortOrder.Ascending;
            objectListWeeks.SetObjects(weekmapping);
            model.OnMyChange += state_OnMyChange;
            weekSwitchButton.Visible = false;
            if (model.licenseKey.Feature(Security.LicenseKey.FeatureType.Expert))
            {
                buttonOptimizeMatch.Visible = true;
            }
            else
            {
                buttonOptimizeMatch.Visible = false;
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
                selectedWeeks.Clear();
                foreach (Object obj in objectListView1.SelectedObjects)
                {
                    selectedTeams.Add((Team)obj);
                }
                //objectListView1.BuildList(true);
                objectListView1.SelectedObjects = selectedTeams;
                updateTeams();
                foreach (Object obj in objectListView2.SelectedObjects)
                {
                    selectedMatches.Add((Match)obj);
                }
                objectListView2.Objects = poule.matches;
                objectListView2.BuildList(true);
                objectListView2.SelectedObjects = selectedMatches;
                foreach (Object obj in objectListWeeks.SelectedObjects)
                {
                    selectedWeeks.Add(((KeyValuePair<MatchWeek, int>)obj).Key);
                }
                UpdateWeekMapping();
                objectListWeeks.BuildList(true);
                foreach (Object obj in objectListWeeks.Objects)
                {
                    KeyValuePair<MatchWeek, int> kvp = (KeyValuePair<MatchWeek, int>)obj;
                    if (selectedWeeks.Contains(kvp.Key) == true) objectListWeeks.SelectObject(obj);
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
                updateTeams();
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
                updateTeams();
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
        private void InsertTeamList(Team t1, int position)
        {
            List<Team> newList = new List<Team>();
            int index = 1;
            foreach (Team t in poule.teams)
            {
                if (index == position)
                    newList.Add(t1);
                if (t != t1)
                {
                    newList.Add(t);
                    index++;
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
            objectListView1.Sort(olvColumn3, SortOrder.Ascending);
            List<Team> teams = new List<Team>();
            teams.Add(team1);
            teams.Add(team2);
            objectListView1.SelectObjects(teams);
            updateMatches();
            updateTeams();
        }

        void updateMatches()
        {
            model.Evaluate(null);
            model.Changed();
            objectListView2.Objects = poule.matches;
            objectListView2.BuildList();
            objectListView2.Refresh();
        }
        void updateTeams()
        {
            //objectListView1.Sort(olvColumn3, SortOrder.Ascending);
            objectListView1.BuildList(true);
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
        private void OptimizeWeekAssignment(IProgress intf)
        {
            poule.SnapShot(model);
            poule.OptimizeWeeks(model, intf, state.optimizeLevel);
            poule.CopyAndClearSnapShot(model);
        }
        private void OptimizeWeekAssignmentCompleted(IProgress intf)
        {
//            UpdateWeekMapping();
//            objectListView3.SetObjects(weekmapping);
            model.Evaluate(null);
            model.Changed();
        }
        private void OptimizeTeamAssignment(IProgress intf)
        {
            poule.SnapShot(model);
            poule.OptimizeTeamAssignment(model, intf);
            poule.CopyAndClearSnapShot(model);
        }
        private void OptimizeTeamAssignmentCompleted(IProgress intf)
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
            
            KeyValuePair<MatchWeek, int> kvp = (KeyValuePair<MatchWeek, int>)e.Model;
            if (kvp.Value > 0) e.Item.SubItems[2].Text = "Week " + kvp.Value.ToString();
            else e.Item.SubItems[2].Text = "----";
        }

        private void UpdateWeekMapping()
        {
            weekmapping.Clear();
            List<MatchWeek> list = new List<MatchWeek>();
            list.AddRange(poule.weeks);
            list.Sort();
            foreach(MatchWeek week in list)
            {
                weekmapping.Add(week, FindIndex(week));
            }
        }
        private int FindIndex(MatchWeek week)
        {
            int index = 0;
            for (int j = 0; j < poule.weeks.Count; j++)
            {
                if (poule.weeks[j] == week)
                {
                    index = j + 1;
                }
            }
            return index;
        }
        private void objectListView3_SelectionChanged(object sender, EventArgs e)
        {
            weekSwitchButton.Enabled = (objectListWeeks.SelectedObjects.Count == 2);
        }
        private void Switch1_Click(object sender, EventArgs e)
        {
            if (poule.imported)
            {
                System.Windows.Forms.MessageBox.Show("Not allowed to change week ordering for an imported poule");
            }
            else if (poule.optimizableWeeks == false)
            {
                System.Windows.Forms.MessageBox.Show("Not allowed to change week ordering");
            }
            else
            {
                /*KeyValuePair<MatchWeek, int> kvp1 = (KeyValuePair<MatchWeek, int>)objectListWeeks.SelectedObjects[0];
                KeyValuePair<MatchWeek, int> kvp2 = (KeyValuePair<MatchWeek, int>)objectListWeeks.SelectedObjects[1];
                int index1 = kvp1.Value - 1;
                int index2 = kvp2.Value - 1;
                if (index1 >= 0) poule.weeks[index1] = kvp2.Key;
                if (index2 >= 0) poule.weeks[index2] = kvp1.Key;
                UpdateWeekMapping();
                model.Evaluate(null);
                model.Changed();
                 */
            }
        }
        private void objectListView3_CellClick(object sender, CellClickEventArgs e)
        {
            if (objectListWeeks.SelectedObjects.Count > 0)
            {
                List<Constraint> constraints = new List<Constraint>();
                foreach (Object obj in objectListWeeks.SelectedObjects)
                {
                    KeyValuePair<MatchWeek, int> kvp = (KeyValuePair<MatchWeek, int>)obj;
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
            else if (poule.optimizableWeeks == false)
            {
                System.Windows.Forms.MessageBox.Show("Not allowed to change week ordering");
            }
            else
            {

                ProgressDialog diag = new ProgressDialog();
                diag.WorkFunction += OptimizeWeekAssignment;
                diag.CompletionFunction += OptimizeWeekAssignmentCompleted;
                diag.Start("Optimizing weeks", null);
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
        private void OptimizeTeam(IProgress intf)
        {
            intf.SetText("Optimizing team - ("+optimizeTeam.name+")");
            poule.OptimizeTeam(model, intf, optimizeTeam, state.optimizeLevel);
        }
        private void OptimizeCompleted(IProgress intf)
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
        private void OptimizePoule(IProgress intf)
        {
            intf.SetText("Optimizing poule - " + poule.serie.name + poule.name);
            poule.SnapShot(model);
            poule.OptimizeTeamAssignment(model, intf);
            poule.OptimizeHomeVisitor(model);
            poule.OptimizeWeeks(model, intf, state.optimizeLevel);
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
        private void AnalyzePoule(IProgress intf)
        {
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

        private void objectListView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void objectListView1_ModelDropped(object sender, ModelDropEventArgs e)
        {
            // If they didn't drop on anything, then don't do anything
            if (e.TargetModel == null)
                return;
            if (e.SourceModels.Count != 1)
                return;

            Team targetTeam = (Team)e.TargetModel;
            Team sourceTeam = (Team)e.SourceModels[0];

            if(e.DropTargetLocation == DropTargetLocation.Item) // switch teams
            {
                SwitchTeamList(targetTeam, sourceTeam);
                objectListView1.SetObjects(poule.teams);
            }
            int index = targetTeam.Index;
            if (sourceTeam.Index < index) index--;
            if (e.DropTargetLocation == DropTargetLocation.AboveItem)
            {
                InsertTeamList(sourceTeam, index);
            }
            if (e.DropTargetLocation == DropTargetLocation.BelowItem)
            {
                InsertTeamList(sourceTeam, index+1);
            }
            // Force them to refresh
            e.RefreshObjects();
            objectListView1.SelectedObject = sourceTeam;
            updateMatches();
        }

        private void objectListView1_ModelCanDrop(object sender, ModelDropEventArgs e)
        {
            if (e.TargetModel == null)
                e.Effect = DragDropEffects.None;
            else if (e.SourceModels.Count != 1)
                e.Effect = DragDropEffects.None;
            else if (poule.optimizable == false)
                e.Effect = DragDropEffects.None;
            else
                e.Effect = DragDropEffects.Move | DragDropEffects.Scroll;

        }

        private void button12_Click(object sender, EventArgs e)
        {
            ProgressDialog diag = new ProgressDialog();
            diag.WorkFunction += AnalyzeAndOptimizePoule;
            diag.CompletionFunction += OptimizeCompleted;
            diag.Start("Analyzing", null);
        }
        private void AnalyzeAndOptimizePoule(IProgress intf)
        {
            intf.SetText("Analysing poule - " + poule.serie.name + poule.name);
            poule.SnapShot(model);
            poule.OptimizeNumberOnAnalysis(model, intf);
            poule.CopyAndClearSnapShot(model);
            model.Evaluate(null);
            if (intf.Cancelled()) return;

        }

        private void button7_Click(object sender, EventArgs e)
        {
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (poule.imported)
            {
                System.Windows.Forms.MessageBox.Show("Not allowed to change week ordering for imported teams");
            }
            else if (poule.optimizableWeeks == false)
            {
                System.Windows.Forms.MessageBox.Show("Not allowed to change week ordering");
            }
            else
            {

                ProgressDialog diag = new ProgressDialog();
                diag.WorkFunction += OptimizeFullSchema;
                diag.CompletionFunction += OptimizeFullSchemaCompleted;
                diag.Start("Optimizing weeks", null);
            }

        }
        private void OptimizeFullSchema(IProgress intf)
        {
            poule.SnapShot(model);
            poule.OptimizeSchema6(model, intf, state.optimizeLevel);
            poule.CopyAndClearSnapShot(model);
        }
        private void OptimizeFullSchemaCompleted(IProgress intf)
        {
            //            UpdateWeekMapping();
            //            objectListView3.SetObjects(weekmapping);
            model.Evaluate(null);
            model.Changed();
        }

    }
}
