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
            state.OnMyChange += state_OnMyChange;
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
                if (e.Column.Index == 1)
                {
                    serie.evaluated = (e.NewValue == CheckState.Checked);
                }
                else if (e.Column.Index == 2)
                {
                    serie.optimizableNumber = (e.NewValue == CheckState.Checked);
                }
                else if (e.Column.Index == 3)
                {
                    serie.optimizableWeeks = (e.NewValue == CheckState.Checked);
                }
                else if (e.Column.Index == 4)
                {
                    serie.optimizableHomeVisit = (e.NewValue == CheckState.Checked);
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
            diag.WorkFunction += OptimizePoules;
            diag.CompletionFunction += OptimizePoulesCompleted;
            diag.Start("Optimizing", null);
        }
        private void OptimizePoules(object sender, MyEventArgs e)
        {
            int score;
            do
            {
                score = model.LastTotalConflicts;
                Console.WriteLine("OptimizePoules: score: {0}", score);
                foreach (Poule poule in model.poules)
                {
                    if (poule.serie != null && poule.optimizable == true)
                    {
                        {
                            lock (model)
                            {
                                IProgress intf = (IProgress)sender;
                                intf.SetText("Optimizing - " + poule.serie.name + poule.name);
                                poule.SnapShot(model);
                                poule.OptimizeTeamAssignment(model, intf);
                                if (intf.Cancelled() == false) poule.OptimizeHomeVisitor(model);
                                if (intf.Cancelled() == false) poule.OptimizeHomeVisitorReverse(model);
                                if (intf.Cancelled() == false) poule.OptimizeWeeks(model, intf);
                                poule.CopyAndClearSnapShot(model);
                                Console.WriteLine(" - {1}:totalConflicts: {0}", model.TotalConflicts(), poule.fullName);
                                if (intf.Cancelled()) return;
                            }
                        }
                        model.Changed();
                    }
                }
            } while (model.LastTotalConflicts<score);
        }
        private void OptimizePoulesCompleted(object sender, MyEventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ProgressDialog diag = new ProgressDialog();
            diag.WorkFunction += OptimizeTeams;
            diag.CompletionFunction += OptimizePoulesCompleted;
            diag.Start("Optimizing", null);
        }
        private void OptimizeTeams(object sender, MyEventArgs e)
        {
            List<Team> teamList = null;
            IProgress intf = (IProgress)sender;
            int score;
            do
            {
                score = model.LastTotalConflicts;
                Console.WriteLine("OptimizeTeams: score: {0}", score);
                teamList = new List<Team>();
                foreach (Poule poule in model.poules)
                {
                    if (poule.optimizable)
                    {
                        foreach (Team team in poule.teams)
                        {
                            if (team.conflict_cost > 0)
                            {
                                teamList.Add(team);
                            }
                        }
                    }
                }
                teamList.Sort(delegate(Team t1, Team t2) { return t1.conflict_cost.CompareTo(t2.conflict_cost); });
                teamList.Reverse();
                foreach (Team team in teamList)
                {
                    {
                        lock (model)
                        {
                            intf.SetText("Optimizing - " + team.poule.serie.name + team.poule.name + " - " + team.name);
                            team.poule.OptimizeTeam(model, intf, team);
                            if (intf.Cancelled()) return;
                        }
                    }
                    Console.WriteLine(" - {1}:totalConflicts: {0}", model.TotalConflicts(), team.poule.fullName);
                    model.Changed();
                }
            } while (teamList.Count > 0 && model.LastTotalConflicts<score);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            ProgressDialog diag = new ProgressDialog();
            diag.WorkFunction += OptimizePoulesSelectedClubs;
            diag.CompletionFunction += OptimizePoulesCompleted;
            diag.Start("Optimizing", null);
        }
        private void OptimizePoulesSelectedClubs(object sender, MyEventArgs e)
        {
            List<Poule> pouleList = null;
            int score;
            do
            {
                score = model.LastTotalConflicts;
                 pouleList = new List<Poule>();
                foreach (Club club in state.selectedClubs)
                {
                    foreach (Team team in club.teams)
                    {
                        if (team.poule != null && 
                            pouleList.Contains(team.poule) == false && 
                            team.poule.conflict_cost > 0 &&
                            team.poule.optimizable
                            )
                        {
                            pouleList.Add(team.poule);
                        }
                    }
                }

                foreach (Poule poule in pouleList)
                {
                    {
                        lock (model)
                        {
                            IProgress intf = (IProgress)sender;
                            intf.SetText("Optimizing - " + poule.serie.name + poule.name);
                            poule.SnapShot(model);
                            poule.OptimizeTeamAssignment(model, intf);
                            //poule.AnalyzeAndOptimizeTeamAssignment(model, intf);
                            poule.OptimizeHomeVisitor(model);
                            poule.OptimizeWeeks(model, intf);
                            poule.CopyAndClearSnapShot(model);
                            if (intf.Cancelled()) return;
                        }
                    }
                    model.Changed();
                }
            } while (pouleList.Count > 0 && model.LastTotalConflicts<score);

        }

        private void button4_Click(object sender, EventArgs e)
        {
            ProgressDialog diag = new ProgressDialog();
            diag.WorkFunction += OptimizeTeamsSelectedClubs;
            diag.CompletionFunction += OptimizePoulesCompleted;
            diag.Start("Optimizing", null);
        }
        private void OptimizeTeamsSelectedClubs(object sender, MyEventArgs e)
        {
            List<Team> teamList = null;
            int score;
            do
            {
                score = model.LastTotalConflicts;
                IProgress intf = (IProgress)sender;
                teamList = new List<Team>();
                foreach (Club club in state.selectedClubs)
                {
                    foreach (Team team in club.teams)
                    {
                        if (team.conflict_cost > 0 && team.poule != null && team.poule.optimizable)
                        {
                            teamList.Add(team);
                        }
                    }
                }
                teamList.Sort(delegate(Team t1, Team t2) { return t1.conflict_cost.CompareTo(t2.conflict_cost); });
                teamList.Reverse();
                foreach (Team team in teamList)
                {
                    {
                        lock (model)
                        {
                            intf.SetText("Optimizing - " + team.poule.serie.name + team.poule.name + " - " + team.name);
                            team.poule.OptimizeTeam(model, intf, team);
                            if (intf.Cancelled()) return;
                        }
                    }
                    model.Changed();
                }
            } while (teamList.Count > 0 && model.LastTotalConflicts<score);

        }
        
        private void OptimizeSeperatingABSelectedClubs(object sender, MyEventArgs e)
        {
            List<Match> matchList = null;
            do
            {
                IProgress intf = (IProgress)sender;
                matchList = new List<Match>();
                foreach (Club club in state.selectedClubs)
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

        private void button5_Click(object sender, EventArgs e)
        {
            ProgressDialog diag = new ProgressDialog();
            diag.WorkFunction += OptimizeClubsForever;
            diag.CompletionFunction += OptimizePoulesCompleted;
            diag.Start("Optimizing", null);
        }
        private void OptimizeClubsForever(object sender, MyEventArgs e)
        {
            do
            {
                int i = 0;
                IProgress intf = (IProgress)sender;
                OptimizePoulesSelectedClubs(sender, e);
                Console.WriteLine(string.Format("{0}. Optimize Poules selected clubs finished: {1}", i, model.TotalConflicts()));
                if (intf.Cancelled()) return;
                OptimizeTeamsSelectedClubs(sender, e);
                Console.WriteLine(string.Format("{0}. Optimize Team finished: {1}", i, model.TotalConflicts()));
                if (intf.Cancelled()) return;
                i++;
            } while (true);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ProgressDialog diag = new ProgressDialog();
            diag.WorkFunction += OptimizePoulesHomeVisit;
            diag.CompletionFunction += OptimizePoulesCompleted;
            diag.Start("Optimizing", null);
        }
        private void OptimizePoulesHomeVisit(object sender, MyEventArgs e)
        {
            int score;
            do
            {
                score = model.LastTotalConflicts;
                foreach (Poule poule in model.poules)
                {
                    if (poule.serie != null && poule.optimizable == true)
                    {
                        {
                            lock (model)
                            {
                                IProgress intf = (IProgress)sender;
                                intf.SetText("Optimizing - " + poule.serie.name + poule.name);
                                poule.SnapShot(model);
                                poule.OptimizeHomeVisitor(model);
                                poule.CopyAndClearSnapShot(model);
                                if (intf.Cancelled()) return;
                            }
                        }
                        model.Changed();
                    }
                }
            } while (model.LastTotalConflicts < score);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ProgressDialog diag = new ProgressDialog();
            diag.WorkFunction += OptimizePoulesForever;
            diag.CompletionFunction += OptimizePoulesCompleted;
            diag.Start("Optimizing", null);
        }
        private void OptimizePoulesForever(object sender, MyEventArgs e)
        {
            do
            {
                int i = 0;
                IProgress intf = (IProgress)sender;
                OptimizePoules(sender, e);
                Console.WriteLine(string.Format("{0}. Optimize Poules finished: {1}", i, model.TotalConflicts()));
                if (intf.Cancelled()) return;
                OptimizeTeams(sender, e);
                Console.WriteLine(string.Format("{0}. Optimize Team finished: {1}", i, model.TotalConflicts()));
                if (intf.Cancelled()) return;
                i++;
            } while (true);
        }

        private void objectListView1_CellEditStarting(object sender, CellEditEventArgs e)
        {
            // Ignore edit events for other columns
            if (e.Column.Index != 5) return;
            ComboBox cb = new ComboBox();
            cb.Bounds = e.CellBounds;
            cb.Font = ((ObjectListView)sender).Font;
            cb.DropDownStyle = ComboBoxStyle.DropDownList;
            foreach (Serie.ImportanceLevels item in Enum.GetValues(typeof(Serie.ImportanceLevels)))
            {
                cb.Items.Add(item.ToString());
            }
            cb.SelectedIndex = (int) ((Serie)e.RowObject).importance; // should select the entry that reflects the current value
            e.Control = cb;
        }

        private void objectListView1_CellEditFinishing(object sender, CellEditEventArgs e)
        {
            if (e.Control is ComboBox)
            {
                int value = ((ComboBox)e.Control).SelectedIndex;
                if (e.Column.Index == 5)
                {
                    ((Serie)e.RowObject).importance = (Serie.ImportanceLevels) value;
                    objectListView1.RefreshItem(e.ListViewItem);
                    //this.objectListView1.RefreshObject((Serie)e.RowObject);
                    e.NewValue = ((ComboBox)e.Control).SelectedText;
                }
            }
        }
    }

}
