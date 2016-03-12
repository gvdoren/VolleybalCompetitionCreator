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

            this.propertyGrid1.ContextMenuStrip = contextMenuStrip1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            mySettings.Save();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            mySettings.ResetToDefault();
            propertyGrid1.Refresh();
        }

        private void propertyGrid1_Click(object sender, EventArgs e)
        {
            GridItem item = propertyGrid1.SelectedGridItem;
            contextMenuStrip1.Enabled = item.PropertyDescriptor.CanResetValue(propertyGrid1.SelectedObject);
        }

        private void contextMenuStrip1_Click(object sender, EventArgs e)
        {
            GridItem item = propertyGrid1.SelectedGridItem;
            PropertyDescriptor descr = item.PropertyDescriptor;
            if (descr.CanResetValue(propertyGrid1.SelectedObject))
            {
                descr.ResetValue(propertyGrid1.SelectedObject);
                item.Select();     // Causes value to be refreshed?!!
            }

        }
    }
}
