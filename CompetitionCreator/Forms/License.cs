using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CompetitionCreator
{
    public partial class LicenseView : Form
    {
        Model klvv;
        public LicenseView(Model klvv)
        {
            InitializeComponent();
            this.klvv = klvv;
            ShowInfo();
        }
        private void ShowInfo()
        {
            label1.Text = "Computer: " + Security.FingerPrint.Value();
            if (klvv.licenseKey.Valid())
            {
                string expired = "";
                if (klvv.licenseKey.ValidUntil() < DateTime.Now) expired = " (Expired)";
                label2.Text = "Licensed until: " + klvv.licenseKey.ValidUntil() + expired;
                label3.Text = "User:  " + klvv.licenseKey.ValidUser();
            }
            else
            {
                label2.Text = "Licensed until: Not Valid";
                label2.Text = "User: ";
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(Security.FingerPrint.Value());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length < 40)
            {
                MessageBox.Show("License key is too small");
            }
            else
            {
                Security.LicenseKey tempKey = new Security.LicenseKey(textBox1.Text);
                bool valid = tempKey.Valid();
                if (valid)
                {
                    CompetitionCreator.Properties.Settings.Default.LicenseKey = tempKey.Key();
                    CompetitionCreator.Properties.Settings.Default.Save();
                    textBox1.Clear();
                    klvv.licenseKey = tempKey;
                    ShowInfo();
                    MessageBox.Show("License is updated");
                }
                else
                {
                    MessageBox.Show("License key is invalid");
                }
            }
        }

    }
}
