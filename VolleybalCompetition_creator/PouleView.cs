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


namespace VolleybalCompetition_creator
{
    public partial class PouleView : DockContent
    {
        Klvv klvv = null;
        GlobalState state = null;
        public Poule poule = null;
        SimpleDropSink myDropSink = new SimpleDropSink();
        ProgressDialog diag = new ProgressDialog();

        public PouleView(Klvv klvv, GlobalState state, Poule poule)
        {
            this.klvv = klvv;
            this.state = state;
            this.poule = poule;
            InitializeComponent();
            this.Text = "Poule - " + poule.serie.name + poule.name;
            objectListView1.ShowGroups = false;
            objectListView1.SetObjects(new List<Team>(poule.teams));
            myDropSink.CanDropBetween = true;
            objectListView1.DropSink = myDropSink;
            this.objectListView1.FormatRow += objectListView1_FormatRow;
            objectListView1.BuildList();
            objectListView1_SelectionChanged(null, null);
            objectListView1.HideSelection = false;
            objectListView2.ShowGroups = false;
            objectListView2.SetObjects(poule.matches);
            klvv.OnMyChange += state_OnMyChange;
        }
        public void state_OnMyChange(object source, MyEventArgs e)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => state_OnMyChange(source, e)));
                return;
            }
            objectListView1.BuildList(true);
            objectListView2.BuildList(true);
        }
       

        private void objectListView1_ModelCanDrop(object sender, BrightIdeasSoftware.ModelDropEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            if (e.SourceModels.Count >= 1)
            {
                Team team = e.SourceModels[0] as Team;
                if (team != null)
                {
                    if (team.poule.serie == poule.serie)
                    {
                        e.Effect = DragDropEffects.Move;
                    }
                    else
                    {
                        e.InfoMessage = "Een team kan alleen naar een poule van dezelfde reeks worden gesleept";
                    }
                }
            }

        }

        private void objectListView1_ModelDropped(object sender, BrightIdeasSoftware.ModelDropEventArgs e)
        {
            if (e.SourceModels.Count >= 1)
            {
                Team team = e.SourceModels[0] as Team;
                if (team != null)
                {
                    if (e.DropTargetIndex>=0)
                    {
                        int offset = 0;
                        e.SourceListView.RemoveObjects(e.SourceModels);
                        if (e.DropTargetLocation == DropTargetLocation.BelowItem) offset = 1;
                        objectListView1.InsertObjects(e.DropTargetIndex+offset, e.SourceModels);
                        objectListView1.SelectedIndex = e.DropTargetIndex + offset;
                        objectListView1.BuildList();
                        //objectListView1.SelectedObjects = e.SourceModels;
                        if (e.SourceListView != objectListView1)
                        {
                            // remove the team from another poule in the series
                            foreach (Poule p in poule.serie.poules.Values)
                            {
                                if (p != poule)
                                {
                                    foreach(Team t in e.SourceModels)
                                    {
                                        p.teams.Remove(t);
                                    }
                                }
                            }
                        }
                        updateMatches();
                    }
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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (objectListView1.SelectedIndex > 0)
            {
                int index = objectListView1.SelectedIndex - 1;
                Object obj = objectListView1.SelectedObject;
                objectListView1.RemoveObject(obj);
                List<Object> list = new List<object>();
                list.Add(obj);
                objectListView1.InsertObjects(index, list);
                objectListView1.SelectObject(obj);
                updateMatches();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (objectListView1.SelectedIndex < objectListView1.GetItemCount())
            {
                int index = objectListView1.SelectedIndex + 1;
                Object obj = objectListView1.SelectedObject;
                objectListView1.RemoveObject(obj);
                List<Object> list = new List<object>();
                list.Add(obj);
                objectListView1.InsertObjects(index, list);
                objectListView1.SelectObject(obj);
                updateMatches();
            }

        }

        private void Switch_Click(object sender, EventArgs e)
        {
            List<int> indices = new List<int>();
            foreach (int index in objectListView1.SelectedIndices) indices.Add(index);
            List<Object> selected = new List<Object>();
            foreach (Object obj in objectListView1.SelectedObjects)
            {
                selected.Add(obj);
            }
            objectListView1.RemoveObjects(objectListView1.SelectedObjects);
            int i=-1;
            foreach (int index in indices)
            {
                List<Object> list = new List<object>();
                list.Add(selected[(selected.Count+i) % selected.Count]);
                objectListView1.InsertObjects(index, list);
                i++;
            }
            objectListView1.SelectedObjects = selected;
            updateMatches();
        }

        void updateMatches()
        {
            poule.teams.Clear();
            foreach (Object obj1 in objectListView1.Objects)
            {
                poule.teams.Add((Team)obj1);
            }
            /*Console.WriteLine("Start Evaluate");
            foreach (Team team in poule.teams)
            {
                Console.WriteLine("{0} -> {1}",team.name,team.conflict);
            }
             * */
            klvv.Evaluate(null);
            klvv.Changed();

            /*Console.WriteLine("After Evaluate");
            foreach (Team team in poule.teams)
            {
                Console.WriteLine("{0} -> {1}", team.name, team.conflict);
            }
             * */
            objectListView2.BuildList();
            objectListView2.Refresh();
        }

        private void objectListView1_ItemsChanged(object sender, ItemsChangedEventArgs e)
        {
            //updateMatches();
        }

        private void objectListView1_DragDrop(object sender, DragEventArgs e)
        {
            updateMatches();
        }

        private void PouleView_FormClosing(object sender, FormClosingEventArgs e)
        {
            klvv.OnMyChange -= state_OnMyChange;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ProgressDialog diag = new ProgressDialog();
            diag.WorkFunction += OptimizeTeamAssignment;
            diag.CompletionFunction += OptimizeTeamAssignmentCompleted; 
            diag.Start("Optimizing", null);
        }
        private void OptimizeTeamAssignment(object sender, MyEventArgs e)
        {
            IProgress intf = (IProgress)sender;
            poule.OptimizeTeamAssignment(klvv,intf);
        }
        private void OptimizeTeamAssignmentCompleted(object sender, MyEventArgs e)
        {
            objectListView1.SetObjects(poule.teams);
            klvv.Evaluate(null);
            klvv.Changed();
        }

        private void objectListView2_SelectionChanged(object sender, EventArgs e)
        {
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
    }
}
