using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeyGenerator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            dateTimePicker1.Value = DateTime.Now.AddYears(1).AddDays(5);
            foreach (Security.LicenseKey.FeatureType f in Enum.GetValues(typeof(Security.LicenseKey.FeatureType)))
            {
                checkedListBox1.Items.Add(f.ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string Key;
            if (textBox1.Text.Length < 2 || textBox1.Text.Length >= 40)
            {
                MessageBox.Show("User name does not have teh correct length");
                Key = "";
            }
            else if (textBox2.Text.Length == 40)
            {
                MessageBox.Show("Computer ID does not have the correct length");
                Key = "";
            }
            else
            {
                List<Security.LicenseKey.FeatureType> list = new List<Security.LicenseKey.FeatureType>();
                foreach (int i in checkedListBox1.CheckedIndices)
                {
                    list.Add((Security.LicenseKey.FeatureType)i);
                }
                Key = Security.LicenseKey.Create(textBox1.Text, textBox2.Text, dateTimePicker1.Value, list);
                Clipboard.SetText(Key);
            }
            label4.Text = "Key: '" + Key + "'";
        }
    }
}
