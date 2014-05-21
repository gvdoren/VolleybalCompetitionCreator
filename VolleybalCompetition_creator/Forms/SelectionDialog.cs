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
    public partial class SelectionDialog : Form
    {
        public string Text {
            set { label1.Text = value;}
        }
        public bool Ok = false;
        public SelectionDialog(List<Selection> list, object selected = null)
        {
            InitializeComponent();
            objectListView1.SetObjects(list);
            objectListView1.SelectedObject = selected;
            objectListView1.EnsureModelVisible(selected);
        }
        public Selection Selection
        {
            get
            {
                if (Ok) return (Selection)objectListView1.SelectedObject;
                return null;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Ok = false;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (objectListView1.SelectedObject != null) Ok = true;
            else Ok = false;
            this.Close();
        }

        private void objectListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (objectListView1.SelectedObject != null) Ok = true;
            else Ok = false;
            this.Close();
        }
    }
    public class Selection
    {
        public string label;
        public int value;
        public object obj;
        public Selection(string label)
        {
            this.label = label;
            this.value = 0;
        }
        public Selection(string label, int value)
        {
            this.label = label;
            this.value = value;
        }
        public Selection(string label, object obj)
        {
            this.label = label;
            this.obj = obj;
        }
    }
}
