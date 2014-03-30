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
        Sporthal sporthal;
        public InschrijvingenView(Klvv klvv, GlobalState state)
        {
            this.Text = "Inschrijvingen";
            this.klvv = klvv;
            this.state = state;
            InitializeComponent();
            foreach (Club cl in klvv.clubs)
            {
                comboBox1.Items.Add(cl);
            }
            comboBox1.SelectedIndex = 0;
        }


        private void UpdateSporthalForm()
        {
            dataGridView2.Rows.Clear();
            foreach (Sporthal sporthal in club.sporthalls)
            {
                dataGridView2.Rows.Add(sporthal);

            }
        }
        private void UpdateFreeFormatTab()
        {
            textBox1.Text = club.FreeFormatConstraints;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            club = (Club) comboBox1.SelectedItem;
            UpdateSporthalForm();
            UpdateTeamsTab();
            UpdateFreeFormatTab();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            klvv.WriteClubConstraints();
        }

        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                sporthal = (Sporthal)dataGridView2.SelectedRows[0].Cells[0].Value;
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
                DateTime begin = new DateTime(2013, 9, 1);
                DateTime end = new DateTime(2014, 5, 1);
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
        }
        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView3.CommitEdit(DataGridViewDataErrorContexts.Commit);
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
                if (e.ColumnIndex == 1)
                {
                    if (cell.Value == cell.TrueValue)
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
                    if (cell.Value == cell.TrueValue)
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
    }
}
