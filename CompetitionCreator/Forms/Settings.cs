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
    public partial class Settings : Form
    {
        MySettings mySettings;
        public Settings(MySettings mySettings)
        {
            InitializeComponent();

            this.propertyGrid1.SelectedObject = mySettings;
            this.mySettings = mySettings;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            mySettings.Save();
        }
    }
}
