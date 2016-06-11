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
        List<MatchWeek> weeks = new List<MatchWeek>();
        public AnoramaView(Model model, GlobalState state)
        {
            this.model = model;
            this.state = state;
            InitializeComponent();
            DateTime current = model.annorama.start;
            MatchWeek week = new MatchWeek(current);
            UpdateForm();
        }
        private void UpdateForm()
        {
            while (objectListView1.AllColumns.Count > 1) objectListView1.AllColumns.RemoveAt(1);
            while (objectListView1.Columns.Count > 1) objectListView1.Columns.RemoveAt(1);
            List<AnnoramaWeek> anWeeks = new List<AnnoramaWeek>();
            MatchWeek week = new MatchWeek(model.annorama.start);
            DateTime current = model.annorama.start;
            while (week.Monday < model.annorama.end)
            {
                anWeeks.Add(new AnnoramaWeek(week));
                current = current.AddDays(7);
                week = new MatchWeek(current);
            }
            foreach (AnnoramaReeks reeks in model.annorama.reeksen)
            {
                foreach (AnnoramaWeek anWeek in reeks.weeks)
                {
                    if(!anWeeks.Exists(w => w.week == anWeek.week))
                    {
                        anWeeks.Add(anWeek);
                    }
                }
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
                olvColumn.Sortable = false;
                //olvColumn.AspectName = "weekNrString";
            }
            anWeeks.Sort((anw1, anw2) => { return anw1.week.CompareTo(anw2.week); });
            objectListView1.SetObjects(anWeeks);
            objectListView1.BuildList(false);
            label1.Text = model.annorama.title;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Bij het creeren van een nieuwe anorama gaat de oude verloren. Wil je dit?", "Nieuwe anorama", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                model.annorama = new Annorama(model.year);
                model.annorama.WriteXML();
            }
            UpdateForm();
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
                        model.annorama.reeksen.Add(model.annorama.CreateReeks(form.GetInputString(),count));
                        UpdateForm();
                        model.annorama.WriteXML();
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
            AnnoramaWeek week = (AnnoramaWeek)objectListView1.SelectedObject;
            if (e.Column.Index > 0)
            {
                AnnoramaReeks reeks = model.annorama.reeksen[e.Column.Index - 1];
                AnnoramaWeek anWeek = reeks.weeks.Find(w => w.week == week.week);
                if (e.Column.Index > 0)
                {
                    List<Selection> list = new List<Selection>();
                    for (int i = 1; i <= (reeks.Count) * 2; i++)
                    {
                        if (reeks.weeks.Exists(w => w.weekNr == i) == false)
                        {
                            AnnoramaWeek w = new AnnoramaWeek(week.week);
                            w.weekNr = i;
                            string reserve = "";
                            if (i > (reeks.Count - 1) * 2)
                            {
                                w.week.round = i - ((reeks.Count - 1) * 2) - 1;
                                reserve = " (Reserve round-"+(w.week.round+1).ToString() + ")";
                            }
                            Selection sel = new Selection(i.ToString() + reserve, w);
                            list.Add(sel);
                        }
                    }
                    Selection sel1 = new Selection("No matches", null);
                    list.Add(sel1);
                    if (list.Find(el => el.selected == true) == null && list.Count > 0) list[0].selected = true;

                    SelectionDialog diag = new SelectionDialog(list, false);
                    diag.Text = "Select the week number:";
                    diag.ShowDialog();
                    if (diag.Ok)
                    {
                        if (anWeek != null)
                            reeks.weeks.Remove(anWeek);
                        if (diag.Selection.obj != null)
                        {
                            reeks.weeks.Add((AnnoramaWeek) diag.Selection.obj);
                        } 
                    }
                    objectListView1.BuildList(true);
                    model.annorama.WriteXML();
                }
            }
        }

        private void objectListView1_CellRightClick(object sender, CellRightClickEventArgs e)
        {
            if (e.Column.Index > model.annorama.reeksen.Count) return;
            AnnoramaWeek week = (AnnoramaWeek)objectListView1.SelectedObject;
            if (e.Column.Index > 0)
            {
                List<Selection> days = new List<Selection>();
                foreach (DayOfWeek day in typeof(DayOfWeek).GetEnumValues())
                {
                    Selection sel = new Selection(day.ToString(), day);
                    days.Add(sel);
                }
                SelectionDialog diag = new SelectionDialog(days);
                diag.Text = "Overrule the day when match must take place";
                diag.ShowDialog();
                if(diag.Ok)
                {
                    AnnoramaWeek anWeek = new AnnoramaWeek(week.week);
                    week.week.dayOverruled = true;
                    week.week.OverruledDay = (DayOfWeek)diag.Selection.obj;
                    AnnoramaReeks reeks = model.annorama.reeksen[e.Column.Index - 1];
                    reeks.weeks.Add(anWeek);
                    model.annorama.WriteXML();
                    UpdateForm();
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
                AnnoramaWeek we = (AnnoramaWeek)rowObject;
                AnnoramaWeek anWe = reeks.weeks.Find(w => w.week == we.week);
            };
            getter = delegate(object rowObject) 
            {
                AnnoramaWeek we = (AnnoramaWeek)rowObject;
                AnnoramaWeek anWe = reeks.weeks.Find(w => w.week == we.week);
                if (anWe!= null) return anWe.weekNrString(((reeks.Count - 1) * 2));
                else return "-";
            };
        }
        
    }
}
