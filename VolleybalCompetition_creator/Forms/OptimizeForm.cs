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

namespace VolleybalCompetition_creator
{
    public partial class OptimizeForm : DockContent
    {
       Klvv klvv = null;
        GlobalState state;
        public OptimizeForm(Klvv klvv, GlobalState state)
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
                serie.optimizable = (e.NewValue == CheckState.Checked);
            } else 
            {
                serie.constraintsHold = (e.NewValue == CheckState.Checked);
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
            do
            {
                foreach (Poule poule in klvv.poules)
                {
                    {
                        lock (klvv) ;
                        IProgress intf = (IProgress)sender;
                        intf.SetText("Optimizing - " + poule.serie.name + poule.name);
                        poule.OptimizeTeamAssignment(klvv, intf);
                        poule.OptimizeHomeVisitor(klvv);
                        poule.OptimizeWeekends(klvv, intf);
                        klvv.Evaluate(null);
                        if (intf.Cancelled()) return;
                    }
                    klvv.Changed();
                }
            } while (checkBox1.Checked);
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
            do
            {
                teamList = new List<Team>();
                foreach (Poule poule in klvv.poules)
                {
                    if (poule.serie.optimizable)
                    {
                        foreach (Team team in poule.teams)
                        {
                            if (team.conflict > 0)
                            {
                                teamList.Add(team);
                            }
                        }
                    }
                }
                teamList.Sort(delegate(Team t1, Team t2) { return t1.conflict.CompareTo(t2.conflict); });
                teamList.Reverse();
                foreach (Team team in teamList)
                {
                    {
                        lock (klvv) ;
                        intf.SetText("Optimizing - " + team.poule.serie.name + team.poule.name + " - " + team.name);
                        team.poule.OptimizeTeam(klvv, intf, team);
                        if (intf.Cancelled()) return;
                    }
                    klvv.Changed();
                }
            } while (checkBox1.Checked && teamList.Count > 0);
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
            do
            {
                 pouleList = new List<Poule>();
                foreach (Club club in state.selectedClubs)
                {
                    foreach (Team team in club.teams)
                    {
                        if (team.poule != null && 
                            pouleList.Contains(team.poule) == false && 
                            team.poule.conflict > 0 &&
                            team.poule.serie.optimizable
                            )
                        {
                            pouleList.Add(team.poule);
                        }
                    }
                }

                foreach (Poule poule in pouleList)
                {
                    {
                        lock (klvv) ;
                        IProgress intf = (IProgress)sender;
                        intf.SetText("Optimizing - " + poule.serie.name + poule.name);
                        poule.OptimizeTeamAssignment(klvv, intf);
                        poule.OptimizeHomeVisitor(klvv);
                        poule.OptimizeWeekends(klvv, intf);
                        klvv.Evaluate(null);
                        if (intf.Cancelled()) return;
                    }
                    klvv.Changed();
                }
            } while (checkBox1.Checked && pouleList.Count > 0);
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
            do
            {
                IProgress intf = (IProgress)sender;
                teamList = new List<Team>();
                foreach (Club club in state.selectedClubs)
                {
                    foreach (Team team in club.teams)
                    {
                        if (team.conflict > 0 && team.poule != null && team.poule.serie.optimizable)
                        {
                            teamList.Add(team);
                        }
                    }
                }
                teamList.Sort(delegate(Team t1, Team t2) { return t1.conflict.CompareTo(t2.conflict); });
                teamList.Reverse();
                foreach (Team team in teamList)
                {
                    {
                        lock (klvv);
                        intf.SetText("Optimizing - " + team.poule.serie.name + team.poule.name + " - " + team.name);
                        team.poule.OptimizeTeam(klvv, intf, team);
                        if (intf.Cancelled()) return;
                    }
                    klvv.Changed();
                }
            } while (checkBox1.Checked && teamList.Count > 0);
        }

    }
}
