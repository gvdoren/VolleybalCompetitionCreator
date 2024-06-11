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
    public partial class YearPlanView : DockContent
    {
        Model model = null;
        GlobalState state;
        List<MatchWeek> weeks = new List<MatchWeek>();
        public YearPlanView(Model model, GlobalState state)
        {
            this.model = model;
            this.state = state;
            InitializeComponent();
            DateTime current = model.yearPlans.start;
            MatchWeek week = new MatchWeek(current);
            UpdateForm();
        }
        private void UpdateForm()
        {
            objectListView1.AllColumns.RemoveRange(1, objectListView1.AllColumns.Count -1);
            objectListView1.Columns.Clear();
            objectListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            objectListView1.AllColumns[0]});

            List<YearPlanWeek> anWeeks = new List<YearPlanWeek>();
            MatchWeek week = new MatchWeek(model.yearPlans.start);
            DateTime current = model.yearPlans.start;
            while (week.Monday < model.yearPlans.end)
            {
                anWeeks.Add(new YearPlanWeek(week));
                current = current.AddDays(7);
                week = new MatchWeek(current);
            }
            foreach (YearPlan reeks in model.yearPlans.reeksen)
            {
                foreach (YearPlanWeek anWeek in reeks.weeks)
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
                olvColumn.Width = Math.Max(20 + reeks.Name.Length * 5, 25);
                //olvColumn.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
                olvColumn.Sortable = false;
                //olvColumn.AspectName = "weekNrString";
            }
            anWeeks.Sort((anw1, anw2) => { return anw1.week.CompareTo(anw2.week); });
            objectListView1.SetObjects(anWeeks);
            objectListView1.BuildList(true);
            label1.Text = model.yearPlans.title;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("When creating new year plans the old plans will be lost. Are you sure?", "New year plans", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                model.yearPlans = new YearPlans(model.year);
                model.yearPlans.WriteXML();
            }
            UpdateForm();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            InputForm form = new InputForm("Create year plan", "Give a name for the year plan");
            form.ShowDialog();
            if (form.Result)
            {
               model.yearPlans.reeksen.Add(model.yearPlans.CreateYearPlan(form.GetInputString()));
               UpdateForm();
               model.yearPlans.WriteXML();
            }
        }


        private void objectListView1_CellClick(object sender, CellClickEventArgs e)
        {
            if (e.Column == null || e.Column.Index > model.yearPlans.reeksen.Count) return;
            YearPlanWeek week = (YearPlanWeek)objectListView1.SelectedObject;
            if (e.Column.Index > 0)
            {
                YearPlan reeks = model.yearPlans.reeksen[e.Column.Index - 1];
                YearPlanWeek anWeek = reeks.weeks.Find(w => w.week == week.week);
                if (anWeek != null)
                    reeks.weeks.Remove(anWeek);
                else
                {
                    for (int i = 1; ; i++)
                    {
                        if (reeks.weeks.Exists(w => w.weekNr == i) == false)
                        {
                            YearPlanWeek w = new YearPlanWeek(week.week);
                            w.weekNr = i;
                            reeks.weeks.Add(w);
                            break;
                        }
                    }
                }
                objectListView1.BuildList(true);
                objectListView1.RedrawItems(e.Column.Index, e.Column.Index+1,  false);
                model.yearPlans.WriteXML();
               
            }
        }
    }
    class DelegateObject
    {
        public AspectPutterDelegate putter = null;
        public AspectGetterDelegate getter = null;
        public DelegateObject(YearPlan reeks)
        {
            putter = delegate(object rowObject, object newValue)
            {
                YearPlanWeek we = (YearPlanWeek)rowObject;
                YearPlanWeek anWe = reeks.weeks.Find(w => w.week == we.week);
            };
            getter = delegate(object rowObject) 
            {
                YearPlanWeek we = (YearPlanWeek)rowObject;
                YearPlanWeek anWe = reeks.weeks.Find(w => w.week == we.week);
                if (anWe != null) return anWe.weekNr.ToString();
                else return "-";
            };
        }
        
    }
}
