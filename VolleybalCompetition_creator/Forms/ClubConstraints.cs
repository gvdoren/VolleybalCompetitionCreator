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
            textBox1.Text = club.FreeFormatConstraints;
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
            lock (klvv) ;
            if (state.selectedClubs.Count > 0 && state.selectedClubs[0] != club)
            {
                club = state.selectedClubs[0];
                Text = "Club-registration: " + club.name;
            
                UpdateSporthalForm();
                UpdateTeamsTab();
                UpdateFreeFormatTab();
            }
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
                    dataGridView3.Rows.Add(w, sporthal.NotAvailable.Contains(w.Saturday) == false, sporthal.NotAvailable.Contains(w.Sunday) == false);
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

    }
}
