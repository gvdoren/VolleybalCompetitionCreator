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
       Model klvv = null;
        GlobalState state;
        public OptimizeForm(Model klvv, GlobalState state)
        {
            this.klvv = klvv;
            this.state = state;
            InitializeComponent();
            objectListView1.SetObjects(klvv.series);
         }

        private void objectListView1_SubItemChecking(object sender, SubItemCheckingEventArgs e)
        {
            Serie serie = (Serie)e.RowObject;
            if (e.Column.Index == 1)
            {
                serie.optimizableNumber = (e.NewValue == CheckState.Checked);
            }
            else if (e.Column.Index == 2)
            {
                serie.optimizableWeekends = (e.NewValue == CheckState.Checked);
            }
            else if (e.Column.Index == 3)
            {
                serie.optimizableHomeVisit = (e.NewValue == CheckState.Checked);
            }
            klvv.Evaluate(null);
            klvv.Changed();
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
                score = klvv.LastTotalConflicts;
                foreach (Poule poule in klvv.poules)
                {
                    if (poule.serie != null && poule.optimizable == true)
                    {
                        {
                            lock (klvv)
                            {
                                IProgress intf = (IProgress)sender;
                                intf.SetText("Optimizing - " + poule.serie.name + poule.name);
                                poule.SnapShot(klvv);
                                poule.OptimizeTeamAssignment(klvv, intf);
                                poule.OptimizeHomeVisitor(klvv);
                                poule.OptimizeHomeVisitorReverse(klvv);
                                poule.OptimizeWeekends(klvv, intf);
                                klvv.Evaluate(null);
                                if (intf.Cancelled()) return;
                            }
                        }
                        klvv.Changed();
                    }
                }
            } while (klvv.LastTotalConflicts<score);
        }
        private void OptimizePoulesCompleted(object sender, MyEventArgs e)
        {
            klvv.Evaluate(null);
            klvv.Changed();
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
                score = klvv.LastTotalConflicts;
                teamList = new List<Team>();
                foreach (Poule poule in klvv.poules)
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
                        lock (klvv)
                        {
                            intf.SetText("Optimizing - " + team.poule.serie.name + team.poule.name + " - " + team.name);
                            team.poule.SnapShot(klvv);
                            team.poule.OptimizeTeam(klvv, intf, team);
                            if (intf.Cancelled()) return;
                        }
                    }
                    klvv.Changed();
                }
            } while (teamList.Count > 0 && klvv.LastTotalConflicts<score);

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
                score = klvv.LastTotalConflicts;
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
                        lock (klvv)
                        {
                            IProgress intf = (IProgress)sender;
                            intf.SetText("Optimizing - " + poule.serie.name + poule.name);
                            poule.SnapShot(klvv);
                            poule.OptimizeTeamAssignment(klvv, intf);
                            //poule.AnalyzeAndOptimizeTeamAssignment(klvv, intf);
                            poule.OptimizeHomeVisitor(klvv);
                            poule.OptimizeWeekends(klvv, intf);
                            klvv.Evaluate(null);
                            if (intf.Cancelled()) return;
                        }
                    }
                    klvv.Changed();
                }
            } while (pouleList.Count > 0 && klvv.LastTotalConflicts<score);

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
                score = klvv.LastTotalConflicts;
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
                        lock (klvv)
                        {
                            intf.SetText("Optimizing - " + team.poule.serie.name + team.poule.name + " - " + team.name);
                            team.poule.SnapShot(klvv);
                            team.poule.OptimizeTeam(klvv, intf, team);
                            if (intf.Cancelled()) return;
                        }
                    }
                    klvv.Changed();
                }
            } while (teamList.Count > 0 && klvv.LastTotalConflicts<score);

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
                Console.WriteLine(string.Format("{0}. Optimize Poules selected clubs finished: {1}", i, klvv.TotalConflicts()));
                if (intf.Cancelled()) return;
                OptimizeTeamsSelectedClubs(sender, e);
                Console.WriteLine(string.Format("{0}. Optimize Team finished: {1}", i, klvv.TotalConflicts()));
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
                score = klvv.LastTotalConflicts;
                foreach (Poule poule in klvv.poules)
                {
                    if (poule.serie != null && poule.optimizable == true)
                    {
                        {
                            lock (klvv)
                            {
                                IProgress intf = (IProgress)sender;
                                intf.SetText("Optimizing - " + poule.serie.name + poule.name);
                                poule.SnapShot(klvv);
                                //poule.OptimizeTeamAssignment(klvv, intf);
                                poule.OptimizeHomeVisitor(klvv);
                                //poule.OptimizeWeekends(klvv, intf);
                                klvv.Evaluate(null);
                                if (intf.Cancelled()) return;
                            }
                        }
                        klvv.Changed();
                    }
                }
            } while (klvv.LastTotalConflicts < score);
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
                Console.WriteLine(string.Format("{0}. Optimize Poules finished: {1}", i, klvv.TotalConflicts()));
                if (intf.Cancelled()) return;
                OptimizeTeams(sender, e);
                Console.WriteLine(string.Format("{0}. Optimize Team finished: {1}", i, klvv.TotalConflicts()));
                if (intf.Cancelled()) return;
                i++;
            } while (true);
        }

        private void objectListView1_CellEditStarting(object sender, CellEditEventArgs e)
        {
            // Ignore edit events for other columns
            if (e.Column.Index != 4) return;
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
                if (e.Column.Index == 4)
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
