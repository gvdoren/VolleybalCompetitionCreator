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
using System.Xml;

namespace CompetitionCreator
{
    public partial class OptimizeForm : DockContent
    {
       Model model = null;
        GlobalState state;
        public OptimizeForm(Model model, GlobalState state)
        {
            this.model = model;
            this.state = state;
            InitializeComponent();
            objectListView1.SetObjects(model.series);
            model.OnMyChange += state_OnMyChange;
            GlobalState.OnMyChange += state_OnMyChange;
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
            ProgressDialog diag = new ProgressDialog();
            diag.WorkFunction += OptimizeAllPoules;
            diag.CompletionFunction += OptimizePoulesCompleted;
            diag.Start("Optimizing", null);
        }
        private void OptimizeAllPoules(IProgress intf)
        {
            OptimizePoules(intf, model.poules);
        }

        private void OptimizePoules(IProgress intf, List<Poule> poules)
        {
            int score;
            do
            {
                score = model.TotalConflictsSnapshot;
                foreach (Poule poule in poules)
                {
                    if (poule.serie != null && poule.Optimize(model) == true)
                    {
                        {
                            lock (model)
                            {
                                intf.SetText("Optimizing - " + poule.serie.name + poule.name);
                                poule.SetInitialSnapShot(model);
                                poule.OptimizeTeamAssignment(model, intf);
                                //poule.OptimizeTeams(model, intf, state.optimizeLevel);
                                if (poule.OptimizeSchema(model))
                                {
                                    poule.RestoreSnapShot(poule.bestSnapShot);
                                    if (intf.Cancelled() == false) poule.OptimizeWeeks(model, intf, GlobalState.optimizeLevel);
                                    poule.RestoreSnapShot(poule.bestSnapShot);
                                    if (poule.maxTeams > 6)
                                    {
                                        if (intf.Cancelled() == false && GlobalState.optimizeLevel > 0) poule.OptimizeSchema(model, intf, 4, GlobalState.optimizeLevel);
                                        if (intf.Cancelled() == false && GlobalState.optimizeLevel > 0) poule.OptimizeSchema(model, intf, 3, GlobalState.optimizeLevel);
                                        if (intf.Cancelled() == false && GlobalState.optimizeLevel > 0) poule.OptimizeSchema(model, intf, 2, GlobalState.optimizeLevel);
                                        if (intf.Cancelled() == false && GlobalState.optimizeLevel > 0) poule.OptimizeSchema(model, intf, 1, GlobalState.optimizeLevel);
                                        //  if (intf.Cancelled() == false && GlobalState.optimizeLevel > 0) poule.OptimizeSchema3(model, intf, GlobalState.optimizeLevel);
                                    }
                                    else
                                    {
                                       while (intf.Cancelled() == false && GlobalState.optimizeLevel > 0 && poule.OptimizeSchema6(model, intf, GlobalState.optimizeLevel) == true) ;
                                    } 
                                }
                                if (intf.Cancelled() == false) poule.OptimizeHomeVisitor(model, GlobalState.optimizeLevel > 0);
                                if (intf.Cancelled() == false) poule.OptimizeHomeVisitorReverse(model, GlobalState.optimizeLevel > 0);
                                poule.RestoreSnapShot(poule.bestSnapShot);

                                model.Evaluate(poule);
                                if (intf.Cancelled()) return;
                            }
                        }
                        model.Evaluate(poule);
                        model.Changed();
                    }
                    lock (model)
                    {
                        ImportExport.WriteProject(model, model.savedFileName, true);
                    }
                }

                model.Evaluate(null);
            } while (model.TotalConflictsSnapshot<score);
        }
        private void OptimizePoulesCompleted(IProgress intf)
        {
        }


        private void button3_Click(object sender, EventArgs e)
        {
            ProgressDialog diag = new ProgressDialog();
            diag.WorkFunction += OptimizePoulesSelectedClubs;
            diag.CompletionFunction += OptimizePoulesCompleted;
            diag.Start("Optimizing", null);
        }
        private void OptimizePoulesSelectedClubs(IProgress intf)
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
            OptimizePoules(intf, pouleList);
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
            int score;
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
            int score;
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

    }

}
