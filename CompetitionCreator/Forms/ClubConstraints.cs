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
            //this.tabControl2.Controls.Remove(this.tabPage1); // Initially do not show this, only show when used
            GlobalState.OnMyChange += new MyEventHandler(state_OnMyChange);
            if (GlobalState.selectedClubs.Count > 0) SetClub(GlobalState.selectedClubs[0]);
            objectListView2.SetObjects(model.constraints.FindAll(c => (c as DateConstraint) != null && c.club == club));
        }


        private void UpdateSporthalForm()
        {
            dataGridView2.Rows.Clear();
            dataGridView2.ClearSelection();
            bool selected = false;
            foreach (SporthallAvailability sporthal in club.sporthalls)
            {
                if (sporthal.team != null) dataGridView2.Rows.Add(sporthal, sporthal.team.Id);
                else
                {
                    int usedByCount = club.teams.Count(t => t.sporthal == sporthal);
                    if (usedByCount == 0)
                    {
                        dataGridView2.Rows.Add(sporthal, "None");
                        dataGridView2.Rows[dataGridView2.RowCount - 1].Selected = false;
                    }
                    else
                    {
                        dataGridView2.Rows.Add(sporthal, "All");
                        if (selected == false)
                        {
                            selected = true;
                            dataGridView2.Rows[dataGridView2.RowCount - 1].Selected = true;
                        }
                    }
                }
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
                //if (model != this.model)
                //{
                //    Close();
                //    return;
                //}

                if (GlobalState.selectedClubs.Count > 0 && GlobalState.selectedClubs[0] != club)
                {
                    SetClub(GlobalState.selectedClubs[0]);
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
            checkBox1.Checked = club.GroupAllWeek;
            checkBox2.Checked = club.GroupAllSporthalls;
            checkBox3.Checked = club.GroupAllDay;
        }

        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                sporthal = (SporthallAvailability)dataGridView2.SelectedRows[0].Cells[0].Value;
                availabilityGrid.Enabled = true;
            }
            else
            {
                sporthal = null;
                availabilityGrid.Enabled = false;
            }
            UpdateWeekForm();
        }
        private void UpdateWeekForm()
        {
            if (sporthal == null)
            {
                availabilityGrid.Visible = false;
                return;
            } 
            List<DayOfWeek> days = new List<DayOfWeek>();
            if (sporthal.team != null)
            {
                days.Add(sporthal.team.defaultDay);
            }
            else 
            {
                foreach(Team team in club.teams)
                {
                    if (team.sporthal == sporthal && days.Contains(team.defaultDay) == false)
                        days.Add(team.defaultDay);
                }
            }
            if (days.Count == 0)
            {
                availabilityGrid.Visible = false;
                label1.Text = string.Format("Sporthal {0} is not used by any team", sporthal.name);
            }
            else
            {
                availabilityGrid.Visible = true;
                label1.Text = string.Format("Sporthal '{0}'\n Available on:", sporthal.name);
                monday.Visible = false;
                tuesday.Visible = false;
                wednesday.Visible = false;
                thursday.Visible = false;
                friday.Visible = false;
                saturday.Visible = false;
                sunday.Visible = false;
                foreach(DayOfWeek day in days)
                {
                    if (day == DayOfWeek.Monday) monday.Visible = true;
                    if (day == DayOfWeek.Tuesday) tuesday.Visible = true;
                    if (day == DayOfWeek.Wednesday) wednesday.Visible = true;
                    if (day == DayOfWeek.Thursday) thursday.Visible = true;
                    if (day == DayOfWeek.Friday) friday.Visible = true;
                    if (day == DayOfWeek.Saturday) saturday.Visible = true;
                    if (day == DayOfWeek.Sunday) sunday.Visible = true;
                }
                DateTime begin = new DateTime(model.year, 9, 1);
                DateTime end = new DateTime(model.year+1, 5, 1);
                availabilityGrid.Rows.Clear();
                for (DateTime current = begin; current < end; current = current.AddDays(7))
                {
                    MatchWeek w = new MatchWeek(current);
                    availabilityGrid.Rows.Add(w, w.Monday.ToString("dd-MM"), w.Sunday.ToString("dd-MM"),
                                                    w.WeekNr(),
                                                    sporthal.NotAvailable.Contains(w.Monday) == false, 
                                                    sporthal.NotAvailable.Contains(w.Tuesday) == false,
                                                    sporthal.NotAvailable.Contains(w.Wednesday) == false,
                                                    sporthal.NotAvailable.Contains(w.Thursday) == false,
                                                    sporthal.NotAvailable.Contains(w.Friday) == false,
                                                    sporthal.NotAvailable.Contains(w.Saturday) == false,
                                                    sporthal.NotAvailable.Contains(w.Sunday) == false
                                                    );
                }
                
            }
        }
        private void UpdateTeamsTab()
        {
            objectListView1.SetObjects(club.teams.Where(t => t.serie.evaluated),true);
            objectListView1_SelectionChanged(null, null); // otherwise it is not triggered for some reason.
            //objectListView1.BuildList(true);
            
        }
        private void UpdateTeamConstraints()
        {
            objectListView2.SetObjects(model.constraints.FindAll(c => (c as DateConstraint) != null && c.club == club)); 
            objectListView2.BuildList(true);
            this.tabPage2.Text = "Date constraints (" + objectListView2.Items.Count + ")";
            
            var list = model.teamConstraints.FindAll(c => (c.team1 != null && c.team1.club == club) || (c.team2 != null && c.team2.club == club));

            //this.tabControl2.Controls.Add(this.tabPage1);
            objectListView3.SetObjects(list);
            objectListView3.BuildList(true);
            this.tabPage1.Text = "Team constraints (" + objectListView3.Items.Count + ")";
            
        }
        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex > 1 && e.RowIndex >= 0)
            {
                DataGridViewRow row = availabilityGrid.Rows[e.RowIndex];
                MatchWeek w = (MatchWeek)row.Cells[0].Value;
                DataGridViewCheckBoxCell cell = (DataGridViewCheckBoxCell)row.Cells[e.ColumnIndex];
                cell.TrueValue = true;
                cell.FalseValue = false;
                DataGridViewColumn col = availabilityGrid.Columns[e.ColumnIndex];
                bool b = ((bool)cell.Value) == false;
                if (col == monday)
                    changeAvailability(w.Monday, b);
                if (col == tuesday)
                    changeAvailability(w.Tuesday, b);
                if (col == wednesday)
                    changeAvailability(w.Wednesday, b);
                if (col == thursday)
                    changeAvailability(w.Thursday, b);
                if (col == friday)
                    changeAvailability(w.Friday, b);
                if (col == saturday)
                    changeAvailability(w.Saturday, b);
                if (col == sunday)
                    changeAvailability(w.Sunday, b);
            }
 
            model.Evaluate(null);
            //state.Changed();
            model.Changed();
        }
        private void changeAvailability(DateTime date, bool available)
        {
            if (available)
            {
                sporthal.NotAvailable.Remove(date);
            }
            else
            {
                if (sporthal.NotAvailable.Contains(date) == false) sporthal.NotAvailable.Add(date);
            }
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
            GlobalState.OnMyChange -= new MyEventHandler(state_OnMyChange);
            //model.OnMyChange -= state_OnMyChange;

        }

        private void objectListView1_CellEditFinishing(object sender, CellEditEventArgs e)
        {
            model.RenewConstraints();
            model.Evaluate(null);
            model.Changed();
        }

        private void objectListView1_CellClick(object sender, CellClickEventArgs e)
        {
            model.RenewConstraints();
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
                        team.field = null;
                        model.MakeDirty();
                    }
                    objectListView1.BuildList(true);

                }
                if (columnindex == olvColumn19.Index) // sporthal
                {
                    List<Selection> list = new List<Selection>();
                    foreach (Field field in team.sporthal.fields)
                    {
                        Selection sel = new Selection(field.Name, field);
                        if (field == team.field) sel.selected = true;
                        list.Add(sel);
                    }
                    SelectionDialog diag = new SelectionDialog(list);
                    diag.Text = "Select the field:";
                    diag.ShowDialog();
                    if (diag.Ok)
                    {
                        Field field  = (Field)diag.Selection.obj;
                        team.field = field;
                        model.MakeDirty();
                    }
                    objectListView1.BuildList(true);

                }
                if (columnindex == olvColumn2.Index) // name
                {
                    List<Selection> list = new List<Selection>();
                    Selection sel;
                    sel = new Selection(club.name);list.Add(sel);if (sel.ToString() == team.name) sel.selected = true;
                    sel = new Selection(club.name + " A");list.Add(sel);if (sel.ToString() == team.name) sel.selected = true;
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
            UpdateSporthalForm();
            model.RenewConstraints();
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
            UpdateTeamConstraints();
            model.RenewConstraints();
            model.Evaluate(null);
            model.Changed();
        }

        private void objectListView1_SelectionChanged(object sender, EventArgs e)
        {
            button1.Enabled = (objectListView1.SelectedObject != null);
            buttonNewTeamRequirement.Enabled = (objectListView1.SelectedObject != null);
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
            buttonDeleteTeamRequirements.Enabled = objectListView3.SelectedItems.Count > 0;
        }

        private void buttonDeleteTeamRequirements_Click(object sender, EventArgs e)
        {
            foreach(TeamConstraint con in objectListView3.SelectedObjects)
            {
                model.teamConstraints.Remove(con);
            }
            UpdateTeamConstraints();
            model.RenewConstraints();
            model.Evaluate(null);
            //state.Changed();
            model.Changed();

        }

        private void buttonNewTeamRequirements_Click(object sender, EventArgs e)
        {
            Team team = (Team)objectListView1.SelectedObject;
            if (team != null)
            {
                TeamConstraint con = new TeamConstraint(model, team.Id, team.Id, TeamConstraint.What.HomeInSameWeekend);
                model.teamConstraints.Add(con);
                UpdateTeamConstraints();
            }
            model.Evaluate(null);
            model.Changed();

        }

        private void objectListView3_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void objectListView3_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Point mousePosition = objectListView3.PointToClient(Control.MousePosition);
            ListViewHitTestInfo hit = objectListView3.HitTest(e.X, e.Y);
            int columnindex = hit.Item.SubItems.IndexOf(hit.SubItem);
            TeamConstraint con = (TeamConstraint) objectListView3.SelectedObject;
            if (columnindex == olvColumn22.Index) // Team 1
            {
                List<Selection> list = new List<Selection>();
                foreach( Team team in club.teams)
                {
                    Selection sel = new Selection(club.name + " - " + team.name +" ("+team.serie.name+")", team);
                    if (team.Id == con.team1Id) sel.selected = true;
                    list.Add(sel);
                }
                SelectionDialog diag = new SelectionDialog(list);
                diag.Width = 400;
                diag.Text = "Select the first team:";
                diag.ShowDialog();
                if (diag.Ok)
                {
                    Team team = (Team)diag.Selection.obj;
                    con.team1Id = team.Id;
                    model.MakeDirty();
                }
                objectListView3.BuildList(true);

            } else if (columnindex == olvColumn24.Index) // Team 2 
            {
                List<Selection> list = new List<Selection>();
                foreach (Club club in model.clubs)
                {
                    foreach (Team team in club.teams)
                    {
                        Selection sel = new Selection(club.name + " - " + team.name + " (" + team.serie.name + ")", team);
                        if (team.Id == con.team2Id) sel.selected = true;
                        list.Add(sel);
                    }
                }
                SelectionDialog diag = new SelectionDialog(list);
                diag.Width = 400;
                diag.Text = "Select the second team:";
                diag.ShowDialog();
                if (diag.Ok)
                {
                    Team team = (Team)diag.Selection.obj;
                    con.team2Id = team.Id;
                    model.MakeDirty();
                }
                objectListView3.BuildList(true);

            } else if (columnindex == olvColumn20.Index)
            {
                List<Selection> list = new List<Selection>();
                foreach (TeamConstraint.What w in Enum.GetValues(typeof(TeamConstraint.What)))
                {
                    Selection sel = new Selection(w.ToString(), w);
                    if (con.what == w) sel.selected = true;
                    list.Add(sel);
                }
                SelectionDialog diag = new SelectionDialog(list);
                diag.Text = "Select the constraint:";
                diag.ShowDialog();
                if (diag.Ok)
                {
                    con.what = (TeamConstraint.What) diag.Selection.obj;
                    model.MakeDirty();
                }
                objectListView3.BuildList(true);
            }
            model.RenewConstraints();
            model.Evaluate(null);
            model.Changed();
        }

        private void objectListView3_CellEditFinishing(object sender, CellEditEventArgs e)
        {
            model.RenewConstraints();
            model.Evaluate(null);
            model.Changed();
        }

        private void objectListView2_CellEditFinishing(object sender, CellEditEventArgs e)
        {
            model.RenewConstraints();
            model.Evaluate(null);
            model.Changed();

        }

        private void objectListView2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Point mousePosition = objectListView2.PointToClient(Control.MousePosition);
            ListViewHitTestInfo hit = objectListView2.HitTest(e.X, e.Y);
            int columnindex = hit.Item.SubItems.IndexOf(hit.SubItem);
            DateConstraint con = (DateConstraint) objectListView2.SelectedObject;
            if (columnindex == olvColumn12.Index) 
            {
                List<Selection> list = new List<Selection>();
                foreach( Team team in club.teams)
                {
                    Selection sel = new Selection(club.name + " - " + team.name +" ("+team.serie.name+")", team);
                    if (team == con.team) sel.selected = true;
                    list.Add(sel);
                }
                SelectionDialog diag = new SelectionDialog(list);
                diag.Width = 400;
                diag.Text = "Select the team:";
                diag.ShowDialog();
                if (diag.Ok)
                {
                    Team team = (Team)diag.Selection.obj;
                    con.team = team;
                    model.MakeDirty();
                }
                objectListView3.BuildList(true);

            }
            UpdateTeamConstraints();
            model.RenewConstraints();
            model.Evaluate(null);
            model.Changed();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            club.GroupAllWeek = checkBox1.Checked;
            UpdateTeamConstraints();
            model.RenewConstraints();
            model.Evaluate(null);
            model.Changed();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            club.GroupAllSporthalls = checkBox2.Checked;
            UpdateTeamConstraints();
            model.RenewConstraints();
            model.Evaluate(null);
            model.Changed();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            club.GroupAllDay = checkBox3.Checked;
            UpdateTeamConstraints();
            model.RenewConstraints();
            model.Evaluate(null);
            model.Changed();
        }
    }
}
