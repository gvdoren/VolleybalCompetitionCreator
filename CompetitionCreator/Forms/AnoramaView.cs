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

namespace CompetitionCreator
{
    public partial class AnoramaView : DockContent
    {
        Model model = null;
        GlobalState state;
        List<Weekend> weekends = new List<Weekend>();
        public AnoramaView(Model model, GlobalState state)
        {
            this.model = model;
            this.state = state;
            InitializeComponent();
            DateTime current = model.annorama.start;
            Weekend weekend = new Weekend(current);
            while (weekend.Saturday < model.annorama.end)
            {
                weekends.Add(weekend);
                current = current.AddDays(7);
                weekend = new Weekend(current);
            }
            objectListView1.SetObjects(weekends);
            UpdateForm();
        }
        private void UpdateForm()
        {
            while (objectListView1.AllColumns.Count > 1) objectListView1.AllColumns.RemoveAt(1);
            while (objectListView1.Columns.Count > 1) objectListView1.Columns.RemoveAt(1);
            foreach(AnnoramaReeks reeks in model.annorama.reeksen)
            {
                BrightIdeasSoftware.OLVColumn olvColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
                objectListView1.AllColumns.Add(olvColumn);
                objectListView1.Columns.Add(olvColumn);
                //olvColumn.AspectName = string.Format("reeks{0}",i);
                olvColumn.AspectGetter = new DelegateObject(reeks).getter;
                olvColumn.AspectPutter = new DelegateObject(reeks).putter;
                olvColumn.CellPadding = null;
                olvColumn.CheckBoxes = true;
                olvColumn.Text = reeks.Name;
                olvColumn.Width = 25;
                olvColumn.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
            objectListView1.BuildList(true);
            label1.Text = model.annorama.title;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Bij het creeren van een nieuwe anorama gaat de oude verloren. Wil je dit?", "Nieuwe anorama", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                model.annorama = new Annorama(model.year);
                model.annorama.WriteXML(model.year);
            }
            UpdateForm();
        }

        private void objectListView1_SubItemChecking_1(object sender, SubItemCheckingEventArgs e)
        {
            Weekend weekend = (Weekend)e.RowObject;
            model.annorama.reeksen[e.Column.Index-1].weekends.Find(w => w.weekend.Saturday == weekend.Saturday).match = (e.NewValue == CheckState.Checked); 
            model.annorama.WriteXML(model.year);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            InputForm form = new InputForm("Creeer weekend reeks voor de anorama", "Geef een naam voor de anorama-reeks");
            form.ShowDialog();
            if (form.Result)
            {
                InputForm form1 = new InputForm("Voor hoeveel teams is dit", "Geef het max. aantal teams voor in deze anorama-reeks");
                form1.ShowDialog();
                if(form1.Result)
                {
                    int count = 0;
                    if (int.TryParse(form1.GetInputString(), out count))
                    {
                        model.annorama.CreateReeks(form.GetInputString(),count);
                        UpdateForm();
                        model.annorama.WriteXML(model.year);
                    }
                }
            }

        }
    }
    class DelegateObject
    {
        public AspectPutterDelegate putter = null;
        public AspectGetterDelegate getter = null;
        public DelegateObject(AnnoramaReeks reeks)
        {
            putter = delegate(object rowObject, object newValue)
            {
                Weekend we = (Weekend)rowObject;
                reeks.weekends.Find(w => w.weekend.Saturday == we.Saturday).match = (bool)newValue;
            };
            getter = delegate(object rowObject) 
            { 
                Weekend we = (Weekend)rowObject; 
                return reeks.weekends.Find(w => w.weekend.Saturday == we.Saturday).match;
            };
        }
        
    }
}
