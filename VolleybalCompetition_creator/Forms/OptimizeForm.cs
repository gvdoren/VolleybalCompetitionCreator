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
            }
            else if (e.Column.Index == 2)
            {
                serie.weekOrderChangeAllowed = (e.NewValue == CheckState.Checked);
            }
            else if (e.Column.Index == 3)
            {
                serie.homeVisitChangeAllowed = (e.NewValue == CheckState.Checked);
            }
            else if (e.Column.Index == 4)
            {
                serie.evaluated = (e.NewValue == CheckState.Checked);
            }
            else
            {
                serie.export = (e.NewValue == CheckState.Checked);
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
                            lock (klvv) ;
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
                        klvv.Changed();
                    }
                }
            } while (checkBox1.Checked && klvv.LastTotalConflicts<score);
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
                        lock (klvv) ;
                        intf.SetText("Optimizing - " + team.poule.serie.name + team.poule.name + " - " + team.name);
                        team.poule.SnapShot(klvv);
                        team.poule.OptimizeTeam(klvv, intf, team);
                        if (intf.Cancelled()) return;
                    }
                    klvv.Changed();
                }
            } while (checkBox1.Checked && teamList.Count > 0 && klvv.LastTotalConflicts<score);

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
                        lock (klvv) ;
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
                    klvv.Changed();
                }
            } while (checkBox1.Checked && pouleList.Count > 0 && klvv.LastTotalConflicts<score);

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
                        lock (klvv);
                        intf.SetText("Optimizing - " + team.poule.serie.name + team.poule.name + " - " + team.name);
                        team.poule.SnapShot(klvv);
                        team.poule.OptimizeTeam(klvv, intf, team);
                        if (intf.Cancelled()) return;
                    }
                    klvv.Changed();
                }
            } while (checkBox1.Checked && teamList.Count > 0 && klvv.LastTotalConflicts<score);

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
            diag.WorkFunction += OptimizeGroupsSelectedClubs;
            diag.CompletionFunction += OptimizePoulesCompleted;
            diag.Start("Optimizing", null);
        }
        private void OptimizeGroupsSelectedClubs(object sender, MyEventArgs e)
        {
            IProgress intf = (IProgress)sender;
            ConstraintDifferentGroupsInSameWeekend constraint = null;
            int clubindex = 0;
            foreach (Club club in state.selectedClubs)
            {
                intf.SetText(string.Format("Optimizing - {0} ({1}/{2})",club.name,clubindex,state.selectedClubs.Count));
                intf.Progress(clubindex++, state.selectedClubs.Count);
                // find the constraint for this club
                foreach(Constraint constr in klvv.constraints)
                {
                    ConstraintDifferentGroupsInSameWeekend con = constr as ConstraintDifferentGroupsInSameWeekend;
                    if(con != null && con.club == club)
                    {
                        constraint = con;
                    }
                }
                DateTime date = new DateTime();
                List<Match> done = new List<Match>();
                List<DateTime> givenUp = new List<DateTime>();
                if (constraint != null)
                {
                    while (constraint.conflictMatches.Count > 0)
                    {
                        if (intf.Cancelled()) return;
                        constraint.conflictMatches.Sort(delegate(Match m1, Match m2) { return m1.datetime.CompareTo(m2.datetime); });
                        int index = 0;
                        while (index < constraint.conflictMatches.Count &&
                              constraint.conflictMatches[index].datetime.Date <= date) index++;
                        if (index >= constraint.conflictMatches.Count) break;
                        date = constraint.conflictMatches[index].datetime.Date;
                        List<Match> matchesDate = new List<Match>(constraint.conflictMatches.FindAll(m => m.datetime.Date == date));
                        // Try switch
                        // - Team komt niet meer terug in conflicten => solved
                        // - Aantal 
                        int Acount = matchesDate.Count(m => m.homeTeam.group == TeamGroups.GroupX);
                        int Bcount = matchesDate.Count(m => m.homeTeam.group == TeamGroups.GroupY);
                        TeamGroups focusGroup = Acount < Bcount ? TeamGroups.GroupX : TeamGroups.GroupY;
                        List<Match> focusMatches = new List<Match>(matchesDate.FindAll(m => m.homeTeam.group == focusGroup));
                        foreach (Match match in focusMatches)
                        {
                            match.poule.SwitchHomeTeamVisitorTeam(klvv, match);
                        }
                        klvv.Evaluate(null);
                        bool undo = false;
                        foreach (Match match1 in constraint.conflictMatches)
                        {
                            if (match1.datetime.Date < date && givenUp.Contains(match1.datetime.Date) == false)
                            {
                                undo = true;
                            }
                        }
                        if (undo)
                        {
                            // undo change since it was not succesfull
                            foreach (Match match in focusMatches)
                            {
                                match.poule.SwitchHomeTeamVisitorTeam(klvv, match);
                            }
                            focusGroup = Acount >= Bcount ? TeamGroups.GroupX : TeamGroups.GroupY;
                            focusMatches = new List<Match>(matchesDate.FindAll(m => m.homeTeam.group == focusGroup));
                            foreach (Match match in focusMatches)
                            {
                                match.poule.SwitchHomeTeamVisitorTeam(klvv, match);
                            }
                            klvv.Evaluate(null);
                            undo = false;
                            foreach (Match match1 in constraint.conflictMatches)
                            {
                                if (match1.datetime.Date < date && givenUp.Contains(match1.datetime.Date) == false)
                                {
                                    undo = true;
                                }
                            }
                            if (undo)
                            {
                                // undo change since it was not succesfull
                                foreach (Match match in focusMatches)
                                {
                                    match.poule.SwitchHomeTeamVisitorTeam(klvv, match);
                                }
                                givenUp.Add(date);
                                klvv.Evaluate(null);
                                foreach (Match match in matchesDate)
                                {
                                    if (done.Contains(match) == false)
                                    {
                                        done.Add(match);
                                        int before = constraint.conflictMatches.Count;
                                        match.poule.SwitchHomeTeamVisitorTeam(klvv, match);
                                        klvv.Evaluate(null);
                                        if (before < constraint.conflictMatches.Count)
                                        {
                                            match.poule.SwitchHomeTeamVisitorTeam(klvv, match);
                                            klvv.Evaluate(null);
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }
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
                            lock (klvv) ;
                            IProgress intf = (IProgress)sender;
                            intf.SetText("Optimizing - " + poule.serie.name + poule.name);
                            poule.SnapShot(klvv);
                            //poule.OptimizeTeamAssignment(klvv, intf);
                            poule.OptimizeHomeVisitor(klvv);
                            //poule.OptimizeWeekends(klvv, intf);
                            klvv.Evaluate(null);
                            if (intf.Cancelled()) return;
                        }
                        klvv.Changed();
                    }
                }
            } while (checkBox1.Checked && klvv.LastTotalConflicts < score);
        }


    }

}
