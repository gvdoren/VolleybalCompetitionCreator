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
        List<Week> weeks = new List<Week>();
        public AnoramaView(Model model, GlobalState state)
        {
            this.model = model;
            this.state = state;
            InitializeComponent();
            DateTime current = model.annorama.start;
            Week week = new Week(current);
            while (week.FirstDayInWeek < model.annorama.end)
            {
                weeks.Add(week);
                current = current.AddDays(7);
                week = new Week(current);
            }
            objectListView1.SetObjects(weeks);
            UpdateForm();
        }
        private void UpdateForm()
        {
            while (objectListView1.AllColumns.Count > 1) objectListView1.AllColumns.RemoveAt(1);
            while (objectListView1.Columns.Count > 1) objectListView1.Columns.RemoveAt(1);
            foreach(AnnoramaReeks reeks in model.annorama.reeksen)
            {
                BrightIdeasSoftware.OLVColumn olvColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
                //objectListView1.AllColumns.Add(olvColumn);
                objectListView1.Columns.Add(olvColumn);
                //olvColumn.AspectName = string.Format("reeks{0}",i);
                olvColumn.AspectGetter = new DelegateObject(reeks).getter;
                //olvColumn.AspectPutter = new DelegateObject(reeks).putter;
                olvColumn.IsEditable = false;
                olvColumn.CellPadding = null;
                olvColumn.CheckBoxes = false;
                olvColumn.Text = reeks.Name;
                olvColumn.Width = 25;
                olvColumn.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
                //olvColumn.AspectName = "weekNrString";
            }
            objectListView1.BuildList(false);
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
            Week week = (Week)e.RowObject;
            model.annorama.reeksen[e.Column.Index-1].weeks.Find(w => w.week == week).match = (e.NewValue == CheckState.Checked); 
            model.annorama.WriteXML(model.year);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            InputForm form = new InputForm("Creeer weken-reeks voor de anorama", "Geef een naam voor de anorama-reeks");
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


        private void objectListView1_MouseClick(object sender, MouseEventArgs e)
        {
        }

        private void objectListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
        }

        private void objectListView1_CellClick(object sender, CellClickEventArgs e)
        {
            if (e.Column.Index > model.annorama.reeksen.Count) return;
            Week week = (Week)objectListView1.SelectedObject;
            AnnoramaReeks reeks = model.annorama.reeksen[e.Column.Index-1];
            AnnoramaWeek anWeek = reeks.weeks.Find(w => w.week == week);
            if (anWeek != null)
            {
                if (e.Column.Index > 0)
                {
                    List<Selection> list = new List<Selection>();
                    foreach(int i in anWeek.weekNrs)
                    {
                        Selection sel = new Selection(i.ToString(), i);
                        sel.selected = true;
                        list.Add(sel);
                    }
                    for (int i = 1; i <= (reeks.Count)*2; i++)
                    {
                        if (reeks.weeks.Find(w => w.weekNrs.Contains(i)) == null)
                        {
                            string reserve = "";
                            if (i > (reeks.Count - 1) * 2) reserve = " (Reserve)";
                            Selection sel = new Selection(i.ToString()+reserve, i);
                            list.Add(sel);
                        }
                        if (list.Find(el => el.selected == true) == null && list.Count > 0) list[0].selected = true;
                    }


                    SelectionDialog diag = new SelectionDialog(list,true);
                    diag.Text = "Select the week number:";
                    diag.ShowDialog();
                    if (diag.Ok)
                    {
                        anWeek.weekNrs = new List<int>();
                        if (diag.Selections.Count <= 2)
                        {
                            foreach (Selection sel in diag.Selections)
                            {
                                anWeek.weekNrs.Add(sel.value);
                            }
                        }
                    }
                    objectListView1.BuildList(true);
                }
                model.annorama.WriteXML(model.year);
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
                Week we = (Week)rowObject;
                AnnoramaWeek anWe = reeks.weeks.Find(w => w.week == we);
                anWe.match = (bool)newValue;
            };
            getter = delegate(object rowObject) 
            { 
                Week we = (Week)rowObject;
                AnnoramaWeek anWe = reeks.weeks.Find(w => w.week == we);
                return anWe.weekNrString(((reeks.Count-1)*2));
                //return anWe.match;
            };
        }
        
    }
}
