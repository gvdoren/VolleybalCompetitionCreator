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
            objectListView1.SetObjects(klvv.series.Values);
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
            foreach (Poule poule in klvv.poules)
            {
                IProgress intf = (IProgress)sender;
                intf.SetText("Optimizing - " + poule.serie.name + poule.name);
                poule.OptimizeTeamAssignment(klvv, intf);
                poule.OptimizeHomeVisitor(klvv);
                poule.OptimizeWeekends(klvv, intf);
                klvv.Evaluate(null);
                if (intf.Cancelled()) return;
            }
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
            IProgress intf = (IProgress)sender;
                            
            List<Team> teamList = new List<Team>();
            foreach (Poule poule in klvv.poules)
            {
                if(poule.serie.optimizable)
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
                intf.SetText("Optimizing - " + team.poule.serie.name + team.poule.name +" - "+team.name);
                team.poule.OptimizeTeam(klvv,intf,team);
                if (intf.Cancelled()) return;
            }
        }

    }
}
