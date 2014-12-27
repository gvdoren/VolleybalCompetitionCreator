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
        public bool Ok = false;
        private bool multi = false;
        private void SetMultiSelect()
        {
            multi = true;
            objectListView1.CheckBoxes = true;
        }
        public SelectionDialog(List<Selection> list, bool multi = false)
        {
            InitializeComponent();
            if (multi) SetMultiSelect();
            objectListView1.SetObjects(list);
            if (multi)
            {
                objectListView1.CheckedObjects = list.FindAll(l => l.selected == true);
            }
            else
            {
                objectListView1.SelectedObject = list.Find(l => l.selected == true);
            }
            //objectListView1.SelectedObject = selected;
            objectListView1.EnsureModelVisible(objectListView1.SelectedObject);
        }
        public Selection Selection
        {
            get
            {
                if (multi) throw new Exception("Single selection cannot be used for selection dialog in multi-select-mode");
                if (Ok) return (Selection)objectListView1.SelectedObject;
                return null;
            }
        }
        public List<Selection> Selections
        {
            get
            {
                if (multi==false) throw new Exception("Multi selections cannot be used for selection dialog in single-select-mode");
                if (Ok)
                {
                    List<Selection> selections = new List<Selection>();
                    foreach (object obj in objectListView1.CheckedObjects)
                    {
                        selections.Add((Selection)obj);
                    }
                    return selections;
                }
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
            if (multi == false && objectListView1.SelectedObject != null) Ok = true;
            else if (multi == true) Ok = true;
            else Ok = false;
            this.Close();
        }

        private void objectListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (multi == false && objectListView1.SelectedObject != null) Ok = true;
            else if (multi == true) Ok = true;
            else Ok = false;
            this.Close();
        }
    }
    public class Selection
    {
        public string label;
        public int value;
        public object obj;
        public bool selected;
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
