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
    public partial class InschrijvingenView : DockContent
    {
        Klvv klvv = null;
        GlobalState state;
        Club club;
        SporthallClub sporthal;
        public InschrijvingenView(Klvv klvv, GlobalState state)
        {
            this.Text = "Inschrijvingen";
            this.klvv = klvv;
            this.state = state;
            InitializeComponent();
            comboBox2.Items.Add("Not shared");
            foreach (Club cl in klvv.clubs)
            {
                comboBox2.Items.Add(cl);
            }
            comboBox2.SelectedIndex = 0;
            state.OnMyChange += new MyEventHandler(state_OnMyChange);
            if (state.selectedClubs.Count > 0) SetClub(state.selectedClubs[0]);
            objectListView2.SetObjects(klvv.teamConstraints.FindAll(c => c.Club == club));
                        
            //klvv.OnMyChange += state_OnMyChange;
        }


        private void UpdateSporthalForm()
        {
            dataGridView2.Rows.Clear();
            foreach (SporthallClub sporthal in club.sporthalls)
            {
                dataGridView2.Rows.Add(sporthal);

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
            if (e.klvv != null)
            {
                klvv.OnMyChange -= state_OnMyChange;
                klvv = e.klvv;
                klvv.OnMyChange += state_OnMyChange;
            }
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => state_OnMyChange(source, e)));
                return;
            }
            lock (klvv);
            if (state.selectedClubs.Count > 0 && state.selectedClubs[0] != club)
            {
                SetClub(state.selectedClubs[0]);
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
                sporthal = (SporthallClub)dataGridView2.SelectedRows[0].Cells[0].Value;
                dataGridView3.Enabled = true;
            }
            else
            {
                sporthal = null;
                dataGridView3.Enabled = false;
            }
            UpdateWeekendsForm();
        }
        private void UpdateWeekendsForm()
        {
            if (sporthal == null)
            {
                dataGridView3.Visible = false;
                label1.Visible = false;
            }
            else
            {
                dataGridView3.Visible = true;
                label1.Visible = true;
                label1.Text = string.Format("Sporthal '{0}' is beschikbaar op:", sporthal.name);
                DateTime begin = new DateTime(2014, 9, 1);
                DateTime end = new DateTime(2015, 5, 1);
                dataGridView3.Rows.Clear();
                for (DateTime current = begin; current < end; current = current.AddDays(7))
                {
                    Weekend w = new Weekend(current);
                    dataGridView3.Rows.Add(w, sporthal.NotAvailable.Contains(w.Saturday) == false, sporthal.NotAvailable.Contains(w.Sunday) == false,w.EvenOdd.ToString());
                }
                
            }
        }
        private void UpdateTeamsTab()
        {
            objectListView1.SetObjects(club.teams);
            objectListView1.BuildList(true);
            if (club.groupingWithClub == null) comboBox2.SelectedIndex = 0;
            else comboBox2.SelectedItem = club.groupingWithClub;
        }
        private void UpdateTeamConstraints()
        {
            objectListView2.SetObjects(klvv.teamConstraints.FindAll(c => c.Club == club));
            objectListView2.BuildList(true);
            
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
                Weekend w = (Weekend)row.Cells[0].Value;
                DataGridViewCheckBoxCell cell = (DataGridViewCheckBoxCell)row.Cells[e.ColumnIndex];
                cell.TrueValue = true;
                cell.FalseValue = false;
                if (e.ColumnIndex == 1)
                {
                    if (((bool)cell.EditedFormattedValue) == true)
                    {
                        sporthal.NotAvailable.Remove(w.Saturday);
                    }
                    else
                    {
                        if (sporthal.NotAvailable.Contains(w.Saturday) == false) sporthal.NotAvailable.Add(w.Saturday);
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
 
            klvv.Evaluate(null);
            //state.Changed();
            klvv.Changed();
        }

        private void dataGridView3_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex > 0 && e.RowIndex>=0)
            {
                DataGridViewRow row = dataGridView3.Rows[e.RowIndex];
                Weekend w = (Weekend)row.Cells[0].Value;
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
            klvv.Evaluate(null);
            //state.Changed();
            klvv.Changed();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            club.FreeFormatConstraints = textBox1.Text;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (club != null)
            {
                if (comboBox2.SelectedIndex == 0)
                {
                    if (club.groupingWithClub != null) club.groupingWithClub.groupingWithClub = null;
                    club.groupingWithClub = null;
                }
                else
                {
                    club.groupingWithClub = (Club)comboBox2.SelectedItem;
                    club.groupingWithClub.groupingWithClub = club;
                }
                klvv.Evaluate(null);
                klvv.Changed();
            }
        }
        private void ClubConstraints_FormClosed(object sender, FormClosedEventArgs e)
        {
            state.OnMyChange -= new MyEventHandler(state_OnMyChange);
            //klvv.OnMyChange -= state_OnMyChange;

        }

        private void objectListView1_CellEditFinishing(object sender, CellEditEventArgs e)
        {
            klvv.Evaluate(null);
            klvv.Changed();
        }

        private void objectListView1_CellClick(object sender, CellClickEventArgs e)
        {
            klvv.Evaluate(null);
            klvv.Changed();

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
                    Selection def = null;
                    foreach (Serie serie in klvv.series)
                    {
                        Selection sel = new Selection(serie.name, serie);
                        if (serie == team.serie) def = sel;
                        list.Add(sel);
                    }
                    SelectionDialog diag = new SelectionDialog(list, def);
                    diag.Text = "Select the serie:";
                    diag.ShowDialog();
                    if (diag.Ok)
                    {
                        Serie newSerie = (Serie)diag.Selection.obj;
                        if (team.poule != null) team.poule.RemoveTeam(team);
                        newSerie.AddTeam(team);
                        klvv.MakeDirty();
                    }
                    objectListView1.BuildList(true);
                }
                if (columnindex == olvColumn6.Index) // sporthal
                {
                    List<Selection> list = new List<Selection>();
                    Selection def = null;
                    foreach (SporthallClub sporthal in club.sporthalls)
                    {
                        Selection sel = new Selection(sporthal.name, sporthal);
                        if (sporthal == team.sporthal) def = sel;
                        list.Add(sel);
                    }
                    SelectionDialog diag = new SelectionDialog(list, def);
                    diag.Text = "Select the sporthal:";
                    diag.ShowDialog();
                    if (diag.Ok)
                    {
                        SporthallClub sporthal = (SporthallClub)diag.Selection.obj;
                        team.sporthal = sporthal;
                        klvv.MakeDirty();
                    }
                    objectListView1.BuildList(true);

                }
                if (columnindex == olvColumn2.Index) // name
                {
                    List<Selection> list = new List<Selection>();
                    Selection def = null;
                    Selection sel;
                    sel = new Selection(club.name);list.Add(sel);if (sel.ToString() == team.name) def = sel;
                    sel = new Selection(club.name+" A");list.Add(sel);if (sel.ToString() == team.name) def = sel;
                    sel = new Selection(club.name+" B");list.Add(sel);if (sel.ToString() == team.name) def = sel;
                    sel = new Selection(club.name+" C");list.Add(sel);if (sel.ToString() == team.name) def = sel;
                    sel = new Selection(club.name+" D");list.Add(sel);if (sel.ToString() == team.name) def = sel;
                    sel = new Selection(club.name+" E");list.Add(sel);if (sel.ToString() == team.name) def = sel;
                    sel = new Selection(club.name+" F");list.Add(sel);if (sel.ToString() == team.name) def = sel;
                    sel = new Selection(club.name+" G");list.Add(sel);if (sel.ToString() == team.name) def = sel;
                    sel = new Selection(club.name+" H");list.Add(sel);if (sel.ToString() == team.name) def = sel;
                    SelectionDialog diag = new SelectionDialog(list, def);
                    diag.Text = "Select the name:";
                    diag.ShowDialog();
                    if (diag.Ok)
                    {
                        string newName = diag.Selection.label;
                        team.name = newName;
                        klvv.MakeDirty();
                    }
                    objectListView1.BuildList(true);
                }
                if (columnindex == olvColumn14.Index) // club
                {
                    List<Selection> list = new List<Selection>();
                    Selection def = null;
                    foreach (Club club in klvv.clubs)
                    {
                        Selection sel = new Selection(club.name, club);
                        if (club == team.club) def = sel;
                        list.Add(sel);
                    }
                    SelectionDialog diag = new SelectionDialog(list, def);
                    diag.Text = "Select the club:";
                    diag.ShowDialog();
                    if (diag.Ok)
                    {
                        Club newClub = (Club)diag.Selection.obj;
                        newClub.AddTeam(team);
                        klvv.MakeDirty();
                    }
                    objectListView1.BuildList(true);

                }
                if (columnindex == olvColumn5.Index) // time
                {
                    List<Selection> list = new List<Selection>();
                    Selection def = null;
                    Time time = new Time(9, 30);
                    while(time < new Time(21,00))
                    {
                        Selection sel = new Selection(time.ToString(),new Time(time));
                        if (team.defaultTime == time) def = sel;
                        list.Add(sel);
                        time.AddMinutes(30);
                    }
                    SelectionDialog diag = new SelectionDialog(list, def);
                    diag.Text = "Select the time:";
                    diag.ShowDialog();
                    if (diag.Ok)
                    {
                        Time newTime = (Time)diag.Selection.obj;
                        team.defaultTime = newTime;
                        klvv.MakeDirty();
                    }
                    objectListView1.BuildList(true);
                }
            }
            klvv.Evaluate(null);
            klvv.Changed();
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            Team team = (Team)objectListView1.SelectedObject;
            if (team != null)
            {
                TeamConstraint prevCon = null;
                foreach (TeamConstraint con in klvv.teamConstraints)
                {
                    if (con.Club == team.club) prevCon = con;
                }
                TeamConstraint tc = new TeamConstraint(team);
                if (prevCon != null)
                {
                    tc.date = prevCon.date;
                    tc.homeVisitNone = prevCon.homeVisitNone;
                    tc.cost = prevCon.cost;
                }
                klvv.teamConstraints.Add(tc);
                objectListView2.SetObjects(klvv.teamConstraints.FindAll(c => c.Club == club));
                objectListView2.BuildList(true);
            }
            klvv.Evaluate(null);
            klvv.Changed();
        }

        private void objectListView1_SelectionChanged(object sender, EventArgs e)
        {
            button1.Enabled = (objectListView1.SelectedObject != null);
            button4.Enabled = (objectListView1.SelectedObject != null);
        }

        private void objectListView2_SelectionChanged(object sender, EventArgs e)
        {
            button2.Enabled = (objectListView2.SelectedObject != null);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TeamConstraint tc = (TeamConstraint)objectListView2.SelectedObject;
            klvv.teamConstraints.Remove(tc);
            objectListView2.SetObjects(klvv.teamConstraints.FindAll(c => c.Club == club));
            objectListView1.BuildList(true);
            klvv.Evaluate(null);
            klvv.Changed();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Team t = (Team)objectListView1.SelectedObject;
            string s = string.Format("Het team '{0}' van club '{1}' in serie '{2}' wordt volledig verwijderd. Wil je dat?", t.name, t.club.name, t.serie.name);
            DialogResult dialogResult = MessageBox.Show(s, "Delete team", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                t.RemoveTeam(klvv);
            }
            UpdateTeamsTab();
            UpdateTeamConstraints();
            klvv.RenewConstraints();
            klvv.Evaluate(null);
            klvv.Changed();
        }

        private void objectListView1_FormatRow(object sender, FormatRowEventArgs e)
        {
            Team t = (Team)e.Model;
            int count = t.club.teams.Count(te => te.name == t.name && t.serie == te.serie);
            if (count > 1)
            {
                e.Item.BackColor = Color.Red;
            }
        }
    }
}
