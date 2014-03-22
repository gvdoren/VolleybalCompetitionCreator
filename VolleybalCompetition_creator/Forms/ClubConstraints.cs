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
    public partial class ClubConstraints : DockContent
    {
        Klvv klvv = null;
        GlobalState state;
        Club club;
        Sporthal sporthal;
        public ClubConstraints(Klvv klvv, GlobalState state)
        {
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

        private void UpdateWeekendForm()
        {
            radioButton1.Checked = true;
            radioButton2.Checked = club.ConstraintAllInOneWeekend;
            radioButton3.Checked = club.ConstraintNotAtTheSameTime;
            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();
            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
            col.HeaderText = "Teams";
            col.Width = 150;
            dataGridView1.Columns.Add(col);

            for (int i = 0; i < club.teams.Count; i++)
            {
                DataGridViewCheckBoxColumn doWork = new DataGridViewCheckBoxColumn();
                doWork.HeaderText = string.Format("T-{0}", i);
                doWork.FalseValue = "0";
                doWork.TrueValue = "1";
                doWork.Width = 30;
                dataGridView1.Columns.Add(doWork);
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Cells[i + 1].Style.BackColor = Color.Gray;
                dataGridView1.Rows[i].Cells[i + 1].ReadOnly = true;
            }
            for (int j = 0; j < club.teams.Count; j++)
            {
                dataGridView1.Rows[j].Cells[0].Value = string.Format("{0} - {1}", j, club.teams[j].poule.serie.name);
                for (int i = 0; i < club.teams.Count; i++)
                {
                    DataGridViewCheckBoxCell cell = ((DataGridViewCheckBoxCell)dataGridView1.Rows[j].Cells[i + 1]);
                    if (i != j)
                    {
                        bool value = club.teams[j].NotAtSameWeekend.Contains(club.teams[i]);
                        if (value)
                        {
                            cell.Value = cell.TrueValue;
                        }
                        else
                        {
                            cell.Value = cell.FalseValue;
                        }
                    }
                    else
                    {
                        cell.Value = cell.FalseValue;
                    }
                }
            }
            dataGridView1.Refresh();
            klvv.Evaluate(null);
            state.Changed();
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            club = (Club) comboBox1.SelectedItem;
            UpdateWeekendForm();
            UpdateSporthalForm();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            klvv.Evaluate(null);
            state.Changed();
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex > 0)
            {
                ((DataGridViewCheckBoxCell)dataGridView1.Rows[e.ColumnIndex - 1].Cells[e.RowIndex + 1]).Value = ((DataGridViewCheckBoxCell)dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex]).Value;
                Team team0 = club.teams[e.RowIndex];
                Team team1 = club.teams[e.ColumnIndex - 1];
                //bool value = (bool)((DataGridViewCheckBoxCell)dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex]).Value;
                if (((DataGridViewCheckBoxCell)dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex]).Value == ((DataGridViewCheckBoxCell)dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex]).TrueValue)
                {
                    if (team0.NotAtSameWeekend.Contains(team1) == false) team0.NotAtSameWeekend.Add(team1);
                    if (team1.NotAtSameWeekend.Contains(team0) == false) team1.NotAtSameWeekend.Add(team0);
                }
                else
                {
                    team0.NotAtSameWeekend.Remove(team1);
                    team1.NotAtSameWeekend.Remove(team0);
                }
            }
            //DataGridViewCheckBoxCell cb = dataGridView1.
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
        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            klvv.Evaluate(null);
            state.Changed();
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

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            club.ConstraintNotAtTheSameTime = radioButton3.Checked;
            if (radioButton3.Checked)
            {
                dataGridView1.Enabled = true;
                dataGridView1.ForeColor = SystemColors.ControlText;
                dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.WindowText;

            }
            else
            {
                dataGridView1.Enabled = false;
                dataGridView1.ForeColor = Color.LightGray;
                dataGridView1.EnableHeadersVisualStyles = false;
                dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.LightGray;
                dataGridView1.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.LightGray;
                //dataGridView1.EnableHeadersVisualStyles = false;
            }
            klvv.Evaluate(null);
            state.Changed();

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            club.ConstraintAllInOneWeekend = radioButton2.Checked;
        }


    }
}
