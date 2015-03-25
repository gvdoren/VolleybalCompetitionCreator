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
        Model model;
        public LicenseView(Model model)
        {
            InitializeComponent();
            this.model = model;
            ShowInfo();
        }
        private void ShowInfo()
        {
            label1.Text = "Computer: " + Security.FingerPrint.Value();
            if (model.licenseKey.Valid())
            {
                string expired = "";
                if (model.licenseKey.ValidUntil() < DateTime.Now) expired = " (Expired)";
                label2.Text = "Licensed until: " + model.licenseKey.ValidUntil() + expired;
                label3.Text = "User:  " + model.licenseKey.ValidUser();
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
            string user = "";
            string oldLicense = "";
            if (model.licenseKey.Valid())
            {
                user = model.licenseKey.ValidUser();
                oldLicense = Properties.Settings.Default.LicenseKey;
            }
            System.Diagnostics.Process.Start(string.Format("http://competitioncreator.doren.be/license.php?HwId={0}&user={1}&oldLicense={2}",Security.FingerPrint.Value(), user, oldLicense));
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
                    model.licenseKey = tempKey;
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
