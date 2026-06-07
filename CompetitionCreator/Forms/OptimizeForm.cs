using BrightIdeasSoftware;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using WeifenLuo.WinFormsUI.Docking;

namespace CompetitionCreator
{

    public partial class OptimizeForm : DockContent
    {
        Model model = null;
        uint currentThreshold = 0;
        GlobalState state;
        public OptimizeForm(Model model, GlobalState state)
        {
            this.model = model;
            this.state = state;
            InitializeComponent();
            objectListView1.SetObjects(model.series);
            model.OnMyChange += state_OnMyChange;
            GlobalState.OnMyChange += state_OnMyChange;
            OnMyIteration += OnIteration;
            if(model.licenseKey.Feature(Security.LicenseKey.FeatureType.Expert))
            {
                this.groupBox3.Visible = true;
            }
            else
            {
                this.groupBox3.Visible = false;
            }
            if (model.licenseKey.Feature(Security.LicenseKey.FeatureType.Expert))
            {
                this.evaluatedColumn.IsVisible = true;
                this.importedColumn.IsVisible = true;
                objectListView1.RebuildColumns();
            }
            else
            {
                this.evaluatedColumn.IsVisible = false;
                this.importedColumn.IsVisible = false;
                objectListView1.RebuildColumns();
            }
            comboBox1.Items.Add("Fast");
            comboBox1.Items.Add("Normal");
            comboBox1.Items.Add("Deep (slow - better results)");
            comboBox1.SelectedItem = comboBox1.Items[2];
            NumberOptimization.Checked = model.OptimizeNumber;
            HomeVisitOptimization.Checked = model.OptimizeHomeVisit;
            SchemaOptimization.Checked = model.OptimizeSchema;

        }
        public void state_OnMyChange(object source, MyEventArgs e)
        {
            if (e.model != null)
            {
                model.OnMyChange -= state_OnMyChange;
                model = e.model;
                objectListView1.SetObjects(model.series);
                model.OnMyChange += state_OnMyChange;
            }
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => state_OnMyChange(source, e)));
                return;
            }
            lock (model)
            {
                objectListView1.BuildList(true);
            }
         }

        private void objectListView1_SubItemChecking(object sender, SubItemCheckingEventArgs e)
        {
            Serie serie1 = (Serie)e.RowObject;
            List<Serie> series = new List<Serie>();
            if(objectListView1.SelectedObjects.Contains(serie1))
            {
                foreach (Object obj in objectListView1.SelectedObjects)
                {
                    series.Add((Serie)obj);
                }
            } else
            {
                series.Add(serie1);
            }
            foreach (Serie serie in series)
            {
                if (e.Column == this.evaluatedColumn)
                {
                    serie.evaluated = (e.NewValue == CheckState.Checked);
                }
            }
            objectListView1.BuildList(true);
            model.RenewConstraints();
            model.Evaluate(null);
            model.Changed();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            ProgressDialog diag = new ProgressDialog();
            diag.WorkFunction += OptimizeAllPoules;
            diag.CompletionFunction += OptimizePoulesCompleted;
            diag.Start("Optimizing", null);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            ProgressDialog diag = new ProgressDialog();
            diag.WorkFunction += ResumeOptimizeAllPoules;
            diag.CompletionFunction += OptimizePoulesCompleted;
            diag.Start("Optimizing", null);
        }

        private void OptimizeAllPoules(IProgress intf)
        {
            uint threshold = 0;
            uint.TryParse(OptimizingThreshold.Text, out threshold);
            OptimizePoules(intf, model.poules, threshold);
        }

        private void ResumeOptimizeAllPoules(IProgress intf)
        {
            OptimizePoules(intf, model.poules, currentThreshold);
        }

        public void SetThreshold(string str)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => SetThreshold(str)));
                return;
            }
            OptimizingThreshold.Text = str;
        }


        private void OptimizePoules(IProgress intf, List<Poule> poules, uint startThreshold)
        {
            using (var guard = new SleepGuard())
            {
                // Alles hierbinnen houdt de pc wakker
                uint smallDelta = 1;
                uint bigDelta = 20;
                currentThreshold = startThreshold;
                DateTime start = DateTime.Now;

                Int64 score;
                int iteration = 0;
                var MainForm = Application.OpenForms["Form1"] as Form1;
                int conflictMatches = 0;
                Int64 dummy1 = 0;
                do
                {
                    // if (threshold <= bigDelta)
                    //     threshold = 0; // To let it finish

                    Poule.OptimizeThreshold = currentThreshold;
                    do
                    {
                        Iterated(iteration, MainForm.CalculatePercentage(ref conflictMatches, ref dummy1), currentThreshold, model.TotalConflicts(), start, conflictMatches);
//                        score = model.TotalConflictsSnapshot;
                        score = model.TotalConflicts();
                        foreach (Poule poule in poules)
                        {
                            if (poule.serie != null && poule.Optimize(model) == true)
                            {
                                {
                                    lock (model)
                                    {
                                        intf.SetText("Optimizing - " + poule.serie.name + poule.name);
                                        poule.SetInitialSnapShot(model);

                                        // Optimize number
                                        poule.OptimizeTeamAssignment(model, intf);

                                        // Home visit
                                        if (intf.Cancelled() == false) poule.OptimizeHomeVisitor(model, intf, GlobalState.optimizeLevel > 0);
                                        if (intf.Cancelled() == false) poule.OptimizeHomeVisitorReverse(model, intf, GlobalState.optimizeLevel > 0);


                                        // Optimize Schema
                                        if (poule.OptimizeSchema(model))
                                        {
                                            poule.RestoreSnapShot(poule.bestSnapShot);
                                            if (intf.Cancelled() == false) poule.OptimizeWeeks(model, intf, GlobalState.optimizeLevel);
                                            poule.RestoreSnapShot(poule.bestSnapShot);
                                            {
                                                if (intf.Cancelled() == false && GlobalState.optimizeLevel > 0) poule.OptimizeSchema(model, intf, 5, GlobalState.optimizeLevel);
                                                if (intf.Cancelled() == false && GlobalState.optimizeLevel > 0) poule.OptimizeSchema(model, intf, 4, GlobalState.optimizeLevel);
                                                if (intf.Cancelled() == false && GlobalState.optimizeLevel > 0) poule.OptimizeSchema(model, intf, 3, GlobalState.optimizeLevel);
                                                if (intf.Cancelled() == false && GlobalState.optimizeLevel > 0) poule.OptimizeSchema(model, intf, 2, GlobalState.optimizeLevel);

                                                // Iets doet deze anders, want zorgt wel voor extra optimalisaties
                                                if (intf.Cancelled() == false && GlobalState.optimizeLevel > 0) poule.OptimizeSchema3(model, intf, GlobalState.optimizeLevel);
                                            }
                                            if (poule.maxTeams > 6)
                                            {
                                                while (intf.Cancelled() == false && GlobalState.optimizeLevel > 0 && poule.OptimizeSchema6(model, intf, GlobalState.optimizeLevel) == true) ;
                                            }
                                            poule.GenerateAllMatchCombinationsExt(model, intf);
                                        }
                                        if (intf.Cancelled() == false) poule.RestoreSnapShot(poule.bestSnapShot);

                                        model.Evaluate(poule);
                                        if (intf.Cancelled()) return;
                                    }
                                }
                                model.Evaluate(poule);
                                model.Changed();
                                try
                                {
                                    ImportExport.WriteProject(model, model.savedFileName, true);
                                }
                                catch { } // negeer een eventueel falen van back-up wegschrijven. 1x gezien. Mogelijk virus scanner die hem claimed
                            }
                        }
                        model.Evaluate(null);
                        iteration++;
                        if (model.TotalConflicts() < score && currentThreshold >= smallDelta)
                            currentThreshold -= smallDelta;
                    } while (model.TotalConflicts() < score);
                    if (currentThreshold >= bigDelta)
                        currentThreshold -= bigDelta;
                    else if (currentThreshold >= smallDelta)
                        currentThreshold -= smallDelta;
                } while (currentThreshold > 0);
                Iterated(iteration, MainForm.CalculatePercentage(ref conflictMatches, ref dummy1), currentThreshold, model.TotalConflicts(), start, conflictMatches);
            }
        }
        private void OptimizePoulesCompleted(IProgress intf)
        {
        }


        private void button3_Click(object sender, EventArgs e)
        {
            ProgressDialog diag = new ProgressDialog();
            dataGridView1.Rows.Clear();
            diag.WorkFunction += StartOptimizePoulesSelectedClubs;
            diag.CompletionFunction += OptimizePoulesCompleted;
            diag.Start("Optimizing", null);
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            ProgressDialog diag = new ProgressDialog();
            dataGridView1.Rows.Clear();
            diag.WorkFunction += ResumeOptimizePoulesSelectedClubs;
            diag.CompletionFunction += OptimizePoulesCompleted;
            diag.Start("Optimizing", null);
        }

        private void StartOptimizePoulesSelectedClubs(IProgress intf)
        {
            uint threshold = 0;
            uint.TryParse(OptimizingThreshold.Text, out threshold);
            OptimizePoulesSelectedClubs(intf, threshold);
        }
        private void ResumeOptimizePoulesSelectedClubs(IProgress intf)
        {
            OptimizePoulesSelectedClubs(intf, currentThreshold);
        }
        private void OptimizePoulesSelectedClubs(IProgress intf, uint startThreshold)
        {
            List<Poule> pouleList =  new List<Poule>();
            foreach (Club club in GlobalState.selectedClubs)
            {
                foreach (Team team in club.teams)
                {
                    if (team.poule != null && 
                        pouleList.Contains(team.poule) == false && 
                        team.poule.conflict_cost > 0 &&
                        team.poule.Optimize(model)
                        )
                    {
                        pouleList.Add(team.poule);
                    }
                }
            }
            uint threshold = 0;
            uint.TryParse(OptimizingThreshold.Text, out threshold);
            OptimizePoules(intf, pouleList, threshold);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ProgressDialog diag = new ProgressDialog();
            diag.WorkFunction += OptimizeTeamsSelectedClubs;
            diag.CompletionFunction += OptimizePoulesCompleted;
            diag.Start("Optimizing", null);
        }

        private void OptimizeTeamsSelectedClubs(IProgress intf)
        {
            List<Team> teamList = null;
            Int64 score;
            do
            {
                score = model.TotalConflictsSnapshot;
                teamList = new List<Team>();
                foreach (Club club in GlobalState.selectedClubs)
                {
                    foreach (Team team in club.teams)
                    {
                        if (team.conflict_cost > 0 && team.poule != null && team.poule.Optimize(model))
                        {
                            teamList.Add(team);
                        }
                    }
                }
                teamList.Sort(delegate (Team t1, Team t2) { return t1.conflict_cost.CompareTo(t2.conflict_cost); });
                teamList.Reverse();
                foreach (Team team in teamList)
                {
                    {
                        lock (model)
                        {
                            intf.SetText("Optimizing - " + team.poule.serie.name + team.poule.name + " - " + team.name);
                            team.poule.OptimizeTeam(model, intf, team, GlobalState.optimizeLevel);
                            if (intf.Cancelled()) return;
                        }
                    }
                    model.Changed();
                }
            } while (teamList.Count > 0 && model.TotalConflictsSnapshot < score);

        }

        private void OptimizeSeperatingABSelectedClubs(IProgress intf)
        {
            List<Match> matchList = null;
            do
            {
                matchList = new List<Match>();
                foreach (Club club in GlobalState.selectedClubs)
                {
                    foreach (Team team in club.teams)
                    {
                        foreach(Match match in team.poule.matches)
                        {
                            if(matchList.Contains(match) == false) matchList.Add(match);
                        }
                    }
                }
            } while (true);
        }
        private void button6_Click(object sender, EventArgs e)
        {
            ProgressDialog diag = new ProgressDialog();
            diag.WorkFunction += OptimizePoulesHomeVisit;
            diag.CompletionFunction += OptimizePoulesCompleted;
            diag.Start("Optimizing", null);
        }
        private void OptimizePoulesHomeVisit(IProgress intf)
        {
            Int64 score;
            do
            {
                score = model.TotalConflictsSnapshot;
                foreach (Poule poule in model.poules)
                {
                    if (poule.serie != null && poule.Optimize(model) == true)
                    {
                        {
                            lock (model)
                            {
                                intf.SetText("Optimizing - " + poule.serie.name + poule.name);
                                poule.SetInitialSnapShot(model);
                                poule.OptimizeHomeVisitor(model);
                                poule.SetBestSnapShot(poule.bestSnapShot);
                                if (intf.Cancelled()) return;
                            }
                        }
                        model.Changed();
                    }
                }
            } while (model.TotalConflictsSnapshot < score);
        }

        private void objectListView1_CellEditStarting(object sender, CellEditEventArgs e)
        {
        }

        private void objectListView1_CellEditFinishing(object sender, CellEditEventArgs e)
        {
        }

        private void OptimizeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalState.OnMyChange -= new MyEventHandler(state_OnMyChange);
            model.OnMyChange -= new MyEventHandler(state_OnMyChange);
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            GlobalState.optimizeLevel = comboBox1.SelectedIndex;
        }
        private void NumberOptimization_CheckedChanged(object sender, EventArgs e)
        {
            model.OptimizeNumber = NumberOptimization.Checked;
        }

        private void HomeVisitOptimization_CheckedChanged(object sender, EventArgs e)
        {
            model.OptimizeHomeVisit = HomeVisitOptimization.Checked;
        }

        private void SchemaOptimization_CheckedChanged(object sender, EventArgs e)
        {
            model.OptimizeSchema = SchemaOptimization.Checked;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }



        static public event MyIterationEventHandler OnMyIteration;
        static public void Iterated(int iteration, double percentage, uint temperature, Int64 cost, DateTime start, int conflictMatches)
        {
            //call it then you need to update:
            if (OnMyIteration != null)
            {
                MyIterationEventArgs e = new MyIterationEventArgs(iteration, percentage, temperature, cost, start, conflictMatches);
                //e.EventInfo = content;
                OnMyIteration(null, e);
            }
        }

        public void OnIteration(object source, MyIterationEventArgs e)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => OnIteration(source, e)));
                return;
            }
            TimeSpan delta = DateTime.Now - e.start;

            dataGridView1.Rows.Add(e.iteration, e.conflictMatches, e.percentage.ToString("F2"), e.cost, e.temperature, delta.ToString(@"hh\:mm"));
            int lastRow = dataGridView1.Rows.Count - 1;
            dataGridView1.FirstDisplayedScrollingRowIndex = lastRow;
            dataGridView1.ClearSelection();
        }
    }

    public class MyIterationEventArgs : EventArgs
    {
        public int iteration;
        public double percentage;
        public uint temperature;
        public Int64 cost;
        public DateTime start;
        public int conflictMatches;
        public MyIterationEventArgs(int iteration, double percentage, uint temperature, Int64 cost, DateTime start, int conflictMatches)
        {
            this.iteration = iteration;
            this.percentage = percentage;
            this.temperature = temperature;
            this.cost = cost;
            this.start = start;
            this.conflictMatches = conflictMatches;
        }
    }
    public sealed class SleepGuard : IDisposable
    {
        [DllImport("kernel32.dll")]
        private static extern uint SetThreadExecutionState(uint esFlags);

        private const uint ES_CONTINUOUS = 0x80000000;
        private const uint ES_SYSTEM_REQUIRED = 0x00000001;

        private bool _disposed;

        public SleepGuard()
        {
            // PC wakker houden, scherm mag uit
            SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED);
        }

        public void Dispose()
        {
            if (_disposed) return;

            // Terug naar normaal gedrag
            SetThreadExecutionState(ES_CONTINUOUS);

            _disposed = true;
        }
    }
}
