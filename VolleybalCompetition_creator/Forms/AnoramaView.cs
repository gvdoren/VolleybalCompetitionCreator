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
    public partial class AnoramaView : DockContent
    {
        Klvv klvv = null;
        GlobalState state;
        public AnoramaView(Klvv klvv, GlobalState state)
        {
            this.klvv = klvv;
            this.state = state;
            InitializeComponent();
            objectListView1.SetObjects(klvv.annorama.weekends);
            UpdateForm();
        }
        private void UpdateForm()
        {
            while (objectListView1.AllColumns.Count > 1) objectListView1.AllColumns.RemoveAt(1);
            while (objectListView1.Columns.Count > 1) objectListView1.Columns.RemoveAt(1);
            for (int i = 0; i < klvv.annorama.reeksen.Count; i++)
            {
                BrightIdeasSoftware.OLVColumn olvColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
                objectListView1.AllColumns.Add(olvColumn);
                objectListView1.Columns.Add(olvColumn);
                //olvColumn.AspectName = string.Format("reeks{0}",i);
                olvColumn.AspectGetter = new DelegateObject(i).getter;
                olvColumn.AspectPutter = new DelegateObject(i).putter;
                olvColumn.CellPadding = null;
                olvColumn.CheckBoxes = true;
                olvColumn.Text = klvv.annorama.reeksen[i];
                olvColumn.Width = 25;
                olvColumn.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
            objectListView1.BuildList(true);
            label1.Text = klvv.annorama.title;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Bij het creeren van een nieuwe anorama gaat de oude verloren. Wil je dit?", "Nieuwe anorama", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                klvv.annorama.CreateAnorama(klvv.year);
                klvv.annorama.WriteXML(klvv.year);
            }
            UpdateForm();
        }

        private void objectListView1_SubItemChecking_1(object sender, SubItemCheckingEventArgs e)
        {
            AnoramaWeekend weekend = (AnoramaWeekend)e.RowObject;
            weekend.reeksen[e.Column.Index-1] = (e.NewValue == CheckState.Checked); 
            klvv.annorama.WriteXML(klvv.year);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            InputForm form = new InputForm("Creeer weekend reeks voor de anorama", "Geef een naam (bv. 14, of 12)");
            form.ShowDialog();
            if (form.Result)
            {
                klvv.annorama.CreateReeks(form.GetInputString());
                UpdateForm();
                klvv.annorama.WriteXML(klvv.year);
            }

        }
    }
    class DelegateObject
    {
        int index = 0;
        public AspectPutterDelegate putter = null;
        public AspectGetterDelegate getter = null;
        public DelegateObject(int i)
        {
            index = i;
            putter = delegate(object rowObject, object newValue)
            {
                AnoramaWeekend we = (AnoramaWeekend)rowObject;
                we.reeksen[index] = (bool)newValue;
            };
            getter = delegate(object rowObject) 
            { 
                AnoramaWeekend we = (AnoramaWeekend)rowObject; 
                return we.reeksen[index]; 
            };
        }
        
    }
}
