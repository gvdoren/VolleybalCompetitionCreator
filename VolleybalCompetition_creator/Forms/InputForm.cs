using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VolleybalCompetition_creator
{
    public partial class InputForm : Form
    {
        public bool Result = false;
        public string GetInputString()
        {
            return textBox1.Text;
            
        }
        public InputForm(string Title, string Label)
        {
            InitializeComponent();
            this.Text = Title;
            label1.Text = Label;
            AcceptButton = button1;
            CancelButton = button2;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Result = true;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Result = false;
            Close();
        }
    }
}
