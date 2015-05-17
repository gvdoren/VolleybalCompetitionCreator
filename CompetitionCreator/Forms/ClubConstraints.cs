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
    public partial class InschrijvingenView : DockContent
    {
        Model model = null;
        GlobalState state;
        Club club;
        SporthallAvailability sporthal;
        public InschrijvingenView(Model model, GlobalState state)
        {
            this.Text = "Inschrijvingen";
            this.model = model;
            this.state = state;
            InitializeComponent();
            state.OnMyChange += new MyEventHandler(state_OnMyChange);
            if (state.selectedClubs.Count > 0) SetClub(state.selectedClubs[0]);
            objectListView2.SetObjects(model.constraints.FindAll(c => (c as DateConstraint) != null && c.club == club));
                        
            //model.OnMyChange += state_OnMyChange;
        }


        private void UpdateSporthalForm()
        {
            dataGridView2.Rows.Clear();
            foreach (SporthallAvailability sporthal in club.sporthalls)
            {
                if(sporthal.team!= null) dataGridView2.Rows.Add(sporthal,sporthal.team.Id);
                else dataGridView2.Rows.Add(sporthal, "-");

            }
        }
        private void UpdateFreeFormatTab()
        {
            textBox1.Clear();
            char[] delimiter = { '\n' };
            string[] parts = club.FreeFormatConstraints.Split(delimiter);
            foreach (string part in parts)
            {
                textBox1.AppendText(part);
                textBox1.AppendText(Environment.NewLine);
            }
            //textBox1.Text = club.FreeFormatConstraints;
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
                if (state.selectedClubs.Count > 0 && state.selectedClubs[0] != club)
                {
                    SetClub(state.selectedClubs[0]);
                }
            }
        }
        private void SetClub(Club club)
        {
            this.club = club;
            Text = "Club-registration: " + club.name;
            UpdateSporthalForm();
            UpdateTeamsTab();
            UpdateFreeFormatTab();
            UpdateTeamConstraints();
        }

        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                sporthal = (SporthallAvailability)dataGridView2.SelectedRows[0].Cells[0].Value;
                dataGridView3.Enabled = true;
            }
            else
            {
                sporthal = null;
                dataGridView3.Enabled = false;
            }
            UpdateWeekForm();
        }
        private void UpdateWeekForm()
        {
            if (sporthal == null)
            {
                dataGridView3.Visible = false;
                label1.Visible = false;
            }
            else
            {
                if(sporthal.team != null)
                {
                    Day1.HeaderText = sporthal.team.defaultDay.ToString();
                    Day2.Visible = false;
                }
                else 
                {
                    Day1.HeaderText = "Saturday";
                    Day2.HeaderText = "Sunday";
                    Day2.Visible = true;
                }
                dataGridView3.Visible = true;
                label1.Visible = true;
                label1.Text = string.Format("Sporthal '{0}' is beschikbaar op:", sporthal.name);
                DateTime begin = new DateTime(model.year, 9, 1);
                DateTime end = new DateTime(model.year+1, 5, 1);
                dataGridView3.Rows.Clear();
                for (DateTime current = begin; current < end; current = current.AddDays(7))
                {
                    MatchWeek w = new MatchWeek(current);
                    if (sporthal.team != null)
                    {
                        dataGridView3.Rows.Add(w, sporthal.NotAvailable.Contains(w.PlayTime(sporthal.team.defaultDay)) == false, false, "-");
                    }
                    else
                    {
                        dataGridView3.Rows.Add(w, sporthal.NotAvailable.Contains(w.Saturday) == false, sporthal.NotAvailable.Contains(w.Sunday) == false, "-");
                    }
                }
                
            }
        }
        private void UpdateTeamsTab()
        {
            objectListView1.SetObjects(club.teams,true);
            objectListView1_SelectionChanged(null, null); // otherwise it is not triggered for some reason.
            //objectListView1.BuildList(true);
            
        }
        private void UpdateTeamConstraints()
        {
            objectListView2.SetObjects(model.constraints.FindAll(c => (c as DateConstraint) != null && c.club == club)); 
            objectListView2.BuildList(true);
            tabControl2.TabPages[1].Text = "Special requirements (" + objectListView2.Items.Count + ")";
            
            var list = model.teamConstraints.FindAll(c => (c.team1 != null && c.team1.club == club) || (c.team2 != null && c.team2.club == club));

            objectListView3.SetObjects(list);
            objectListView3.BuildList(true);
            tabControl2.TabPages[0].Text = "Team requirements (" + objectListView3.Items.Count + ")";
            
        }
        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            /*
            this.dataGridView3.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView3_CellValueChanged);
            dataGridView3.CommitEdit(DataGridViewDataErrorContexts.Commit);
            this.dataGridView3.CellValueChanged -= new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView3_CellValueChanged);
             * */
            if (e.ColumnIndex > 0 && e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView3.Rows[e.RowIndex];
                MatchWeek w = (MatchWeek)row.Cells[0].Value;
                DataGridViewCheckBoxCell cell = (DataGridViewCheckBoxCell)row.Cells[e.ColumnIndex];
                cell.TrueValue = true;
                cell.FalseValue = false;
                if (e.ColumnIndex == 1)
                {
                    if (((bool)cell.EditedFormattedValue) == true)
                    {
                        if (sporthal.team != null)
                        {
                            sporthal.NotAvailable.Remove(w.PlayTime(sporthal.team.defaultDay));
                        }
                        else
                        {
                            sporthal.NotAvailable.Remove(w.Saturday);
                        }
                    }
                    else
                    {
                        if (sporthal.team != null)
                        {
                            if (sporthal.NotAvailable.Contains(w.PlayTime(sporthal.team.defaultDay)) == false) sporthal.NotAvailable.Add(w.PlayTime(sporthal.team.defaultDay)); 
                        }
                        else
                        {
                            if (sporthal.NotAvailable.Contains(w.Saturday) == false) sporthal.NotAvailable.Add(w.Saturday);
                        }
                    }
                }
                else
                {
                    if (((bool)cell.EditedFormattedValue) == true)
                    {
                        sporthal.NotAvailable.Remove(w.Sunday);
                    }
                    else
                    {
                        if (sporthal.NotAvailable.Contains(w.Sunday) == false) sporthal.NotAvailable.Add(w.Sunday);
                    }

                }
            }
 
            model.Evaluate(null);
            //state.Changed();
            model.Changed();
        }

        private void dataGridView3_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex > 0 && e.RowIndex>=0)
            {
                DataGridViewRow row = dataGridView3.Rows[e.RowIndex];
                MatchWeek w = (MatchWeek)row.Cells[0].Value;
                DataGridViewCheckBoxCell cell = (DataGridViewCheckBoxCell)row.Cells[e.ColumnIndex];
                cell.TrueValue = true;
                cell.FalseValue = false;
                if (e.ColumnIndex == 1)
                {
                    if (((bool)cell.Value) == true)
                    {
                        sporthal.NotAvailable.Remove(w.Saturday);
                    }
                    else
                    {
                        if(sporthal.NotAvailable.Contains(w.Saturday) == false) sporthal.NotAvailable.Add(w.Saturday);
                    }
                }
                else
                {
                    if (((bool)cell.Value) == true)
                    {
                        sporthal.NotAvailable.Remove(w.Sunday);
                    }
                    else
                    {
                        if (sporthal.NotAvailable.Contains(w.Sunday) == false) sporthal.NotAvailable.Add(w.Sunday);
                    }

                }
            }
            //DataGridViewCheckBoxCell cb = dataGridView1.
        }

        private void objectListView1_ItemsChanged(object sender, ItemsChangedEventArgs e)
        {
            model.Evaluate(null);
            //state.Changed();
            model.Changed();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            club.FreeFormatConstraints = textBox1.Text;
        }

        private void ClubConstraints_FormClosed(object sender, FormClosedEventArgs e)
        {
            state.OnMyChange -= new MyEventHandler(state_OnMyChange);
            //model.OnMyChange -= state_OnMyChange;

        }

        private void objectListView1_CellEditFinishing(object sender, CellEditEventArgs e)
        {
            model.Evaluate(null);
            model.Changed();
        }

        private void objectListView1_CellClick(object sender, CellClickEventArgs e)
        {
            model.Evaluate(null);
            model.Changed();

        }

        private void objectListView1_MouseClick(object sender, MouseEventArgs e)
        {
            Point mousePosition = objectListView1.PointToClient(Control.MousePosition);
            ListViewHitTestInfo hit = objectListView1.HitTest(mousePosition);
            int columnindex = hit.Item.SubItems.IndexOf(hit.SubItem);
            Team team = (Team) objectListView1.SelectedObject;
            if (team != null)
            {
                if (columnindex == 0)
                {
                    List<Selection> list = new List<Selection>();
                    foreach (Serie serie in model.series)
                    {
                        Selection sel = new Selection(serie.name, serie);
                        if (serie == team.serie) sel.selected = true;
                        list.Add(sel);
                    }
                    SelectionDialog diag = new SelectionDialog(list);
                    diag.Text = "Select the serie:";
                    diag.ShowDialog();
                    if (diag.Ok)
                    {
                        Serie newSerie = (Serie)diag.Selection.obj;
                        if (team.poule != null) team.poule.RemoveTeam(team);
                        team.serie = newSerie;
                        model.MakeDirty();
                    }
                    objectListView1.BuildList(true);
                }
                if (columnindex == olvColumn6.Index) // sporthal
                {
                    List<Selection> list = new List<Selection>();
                    foreach (SporthallAvailability sporthal in club.sporthalls)
                    {
                        Selection sel = new Selection(sporthal.name, sporthal);
                        if (sporthal == team.sporthal) sel.selected = true;
                        list.Add(sel);
                    }
                    SelectionDialog diag = new SelectionDialog(list);
                    diag.Text = "Select the sporthal:";
                    diag.ShowDialog();
                    if (diag.Ok)
                    {
                        SporthallAvailability sporthal = (SporthallAvailability)diag.Selection.obj;
                        team.sporthal = sporthal;
                        model.MakeDirty();
                    }
                    objectListView1.BuildList(true);

                }
                if (columnindex == olvColumn2.Index) // name
                {
                    List<Selection> list = new List<Selection>();
                    Selection sel;
                    sel = new Selection(club.name);list.Add(sel);if (sel.ToString() == team.name) sel.selected = true;
                    sel = new Selection(club.name+" A");list.Add(sel);if (sel.ToString() == team.name) sel.selected = true;
                    sel = new Selection(club.name + " B"); list.Add(sel); if (sel.ToString() == team.name) sel.selected = true;
                    sel = new Selection(club.name + " C"); list.Add(sel); if (sel.ToString() == team.name) sel.selected = true;
                    sel = new Selection(club.name + " D"); list.Add(sel); if (sel.ToString() == team.name) sel.selected = true;
                    sel = new Selection(club.name + " E"); list.Add(sel); if (sel.ToString() == team.name) sel.selected = true;
                    sel = new Selection(club.name + " F"); list.Add(sel); if (sel.ToString() == team.name) sel.selected = true;
                    sel = new Selection(club.name + " G"); list.Add(sel); if (sel.ToString() == team.name) sel.selected = true;
                    sel = new Selection(club.name + " H"); list.Add(sel); if (sel.ToString() == team.name) sel.selected = true;
                    SelectionDialog diag = new SelectionDialog(list);
                    diag.Text = "Select the name:";
                    diag.ShowDialog();
                    if (diag.Ok)
                    {
                        string newName = diag.Selection.label;
                        team.name = newName;
                        model.MakeDirty();
                    }
                    objectListView1.BuildList(true);
                }
                if (columnindex == olvColumn14.Index) // club
                {
                    List<Selection> list = new List<Selection>();
                    foreach (Club club in model.clubs)
                    {
                        Selection sel = new Selection(club.name, club);
                        if (club == team.club) sel.selected = true;
                        list.Add(sel);
                    }
                    SelectionDialog diag = new SelectionDialog(list);
                    diag.Text = "Select the club:";
                    diag.ShowDialog();
                    if (diag.Ok)
                    {
                        Club newClub = (Club)diag.Selection.obj;
                        newClub.AddTeam(team);
                        model.MakeDirty();
                    }
                    objectListView1.BuildList(true);

                }
                if (columnindex == olvColumn5.Index) // time
                {
                    List<Selection> list = new List<Selection>();
                    Time time = new Time(9, 30);
                    while(time < new Time(21,00))
                    {
                        Selection sel = new Selection(time.ToString(),new Time(time));
                        if (team.defaultTime == time) sel.selected = true;
                        list.Add(sel);
                        time.AddMinutes(30);
                    }
                    SelectionDialog diag = new SelectionDialog(list);
                    diag.Text = "Select the time:";
                    diag.ShowDialog();
                    if (diag.Ok)
                    {
                        Time newTime = (Time)diag.Selection.obj;
                        team.defaultTime = newTime;
                        model.MakeDirty();
                    }
                    objectListView1.BuildList(true);
                }
            }
            model.Evaluate(null);
            model.Changed();
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            Team team = (Team)objectListView1.SelectedObject;
            if (team != null)
            {
                DateConstraint prevCon = null;
                foreach (Constraint c in model.constraints)
                {
                    DateConstraint con = c as DateConstraint;
                    if (con != null && con.club == team.club) prevCon = con;
                }
                DateConstraint tc = new DateConstraint(team);
                if (prevCon != null)
                {
                    tc.date = prevCon.date;
                    tc.homeVisitNone = prevCon.homeVisitNone;
                    tc.cost = prevCon.cost;
                }
                model.constraints.Add(tc);
                objectListView2.SetObjects(model.constraints.FindAll(c => c.club == club && ((c as DateConstraint) != null)));
                objectListView2.BuildList(true);
            }
            model.Evaluate(null);
            model.Changed();
        }

        private void objectListView1_SelectionChanged(object sender, EventArgs e)
        {
            button1.Enabled = (objectListView1.SelectedObject != null);
            button4.Enabled = (objectListView1.SelectedObject != null && ((Team)objectListView1.SelectedObject).deleted == false);
            button5.Enabled = (objectListView1.SelectedObject != null && ((Team)objectListView1.SelectedObject).deleted == true);
            button5.Visible = (objectListView1.SelectedObject != null && ((Team)objectListView1.SelectedObject).deleted == true);
            Team t = (Team)objectListView1.SelectedObject;
            if(t!= null && t.sporthal!= null)
            {
                bool selected = false;
                foreach(DataGridViewRow row in dataGridView2.Rows)
                {
                    SporthallAvailability sp = (SporthallAvailability)row.Cells[0].Value;
                    if(t.sporthal == sp && sp.team == t)
                    {
                        dataGridView2.ClearSelection();
                        row.Selected = true;
                        dataGridView2.CurrentCell = row.Cells[0];
                        selected = true;
                    }
                }
                if(selected == false)
                {
                    foreach (DataGridViewRow row in dataGridView2.Rows)
                    {
                        SporthallAvailability sp = (SporthallAvailability)row.Cells[0].Value;
                        if (t.sporthal == sp)
                        {
                            dataGridView2.ClearSelection();
                            row.Selected = true;
                            dataGridView2.CurrentCell = row.Cells[0];
                            selected = true;
                        }
                    }

                }
                sporthal = t.sporthal;
                dataGridView3.Enabled = true;
            } else
            {
                dataGridView3.Enabled = false;
            }
        }

        private void objectListView2_SelectionChanged(object sender, EventArgs e)
        {
            button2.Enabled = (objectListView2.SelectedObject != null);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DateConstraint tc = (DateConstraint)objectListView2.SelectedObject;
            model.constraints.Remove(tc);
            objectListView2.SetObjects(model.constraints.FindAll(c => (c as DateConstraint) != null && c.club == club));
            objectListView1.BuildList(true);
            model.Evaluate(null);
            model.Changed();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Team t = (Team)objectListView1.SelectedObject;
            string s = string.Format("Het team '{0}' van club '{1}' in serie '{2}' wordt volledig verwijderd. Wil je dat? Undelete is mogelijk, maar het team moet opnieuw in een poule ingedeeld worden.", t.name, t.club.name, t.serie.name);
            DialogResult dialogResult = MessageBox.Show(s, "Delete team", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                t.DeleteTeam(model);
            }
            UpdateTeamsTab();
            UpdateTeamConstraints();
            model.RenewConstraints();
            model.Evaluate(null);
            model.Changed();
        }

        private void objectListView1_FormatRow(object sender, FormatRowEventArgs e)
        {
            Team t = (Team)e.Model;
            int count = t.club.teams.Count(te => te.name == t.name && t.serie == te.serie && t.serie.imported == false);
            if (count > 1)
            {
                e.Item.BackColor = Color.Red;
            }
            if (t.deleted)
            {
                e.Item.Font = new Font(e.Item.Font, FontStyle.Strikeout); 
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Team t = (Team)objectListView1.SelectedObject;
            t.UndeleteTeam(model);
            UpdateTeamsTab();
            UpdateTeamConstraints();
            model.RenewConstraints();
            model.Evaluate(null);
            model.Changed();

        }

        private void objectListView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void objectListView3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
