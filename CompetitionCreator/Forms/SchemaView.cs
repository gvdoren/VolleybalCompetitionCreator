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
using System.IO;

namespace CompetitionCreator
{
    public partial class SchemaView : DockContent
    {
        Model model = null;
        GlobalState state;
        Dictionary<int, Schema> schemas = new Dictionary<int, Schema>();
        Schema selectedSchema = null;

        public SchemaView(Model model, GlobalState state)
        {
            this.model = model;
            this.state = state;
            InitializeComponent();

            string[] files = Directory.GetFiles(System.Windows.Forms.Application.StartupPath + @"/Schemas/");
            foreach(string file in files)
            {
                FileInfo fi = new FileInfo(file);
                Schema newSchema = new Schema();
                newSchema.Read(file);
                schemas.Add(schemas.Count, newSchema);
                comboBox1.Items.Add(fi.Name);
            }
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedItem = comboBox1.Items[0];
            }
            objectListView1.Scrollable = true;
            objectListView2.Scrollable = true;
            objectListView3.Scrollable = true;
            objectListView4.Scrollable = true;
            objectListView1.ShowGroups = false;
            objectListView2.ShowGroups = false;
            objectListView3.ShowGroups = false;
            objectListView4.ShowGroups = false;
//            if (model.licenseKey.Feature(Security.LicenseKey.FeatureType.Expert))
//            {
//                textBox1.Visible = true;
//            }
//            else 
//            {
//                textBox1.Visible = false;
//            }
        }
        private void UpdateForm(int schemaNr)
        {
            for (int round = 0; round < 4; round++)
            {
                ObjectListView view = objectListView1;
                TabPage tab = tabPage1;
                if (round == 1)
                {
                    view = objectListView2;
                    tab = tabPage2;
                }
                if (round == 2)
                {
                    view = objectListView3;
                    tab = tabPage3;
                }
                if (round == 3)
                {
                    view = objectListView4;
                    tab = tabPage4;
                }
                tabControl1.TabPages.Remove(tab);
                if (selectedSchema.weeks.Values.Count(w => w.round == round) > 0)
                    tabControl1.TabPages.Add(tab);

                //objectListView1.AllColumns.Clear();
                //view.BeginUpdate();
                view.Columns.Clear();
                view.ClearObjects();
                List<int> matchnumbers = new List<int>();
                for (int i = 1; i <= selectedSchema.weeks[0].matches.Count; i++)
                {
                    matchnumbers.Add(i);
                }
                view.SetObjects(matchnumbers);
                foreach (var week in selectedSchema.weeks.Where(w => w.Value.round == round))
                {
                    BrightIdeasSoftware.OLVColumn olvColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
                    olvColumn.AspectGetter = new DelegateSchemaObject(week.Value).getter;
                    olvColumn.IsEditable = false;
                    olvColumn.CellPadding = null;
                    olvColumn.CheckBoxes = false;
                    olvColumn.Text = (week.Key + 1).ToString();
                    olvColumn.Width = 45;
                    olvColumn.AutoResize(ColumnHeaderAutoResizeStyle.None);
                    //olvColumn.AspectName = "weekNrString";
                    view.Columns.Add(olvColumn);
                }
                view.BuildList(true);
                //view.EndUpdate();
            }
            Analysis();
        }
        private void Analysis()
        {
            textBox1.Clear();
            int[] homeVisit = new int[selectedSchema.teamCount];
            int[] matchCount = new int[selectedSchema.teamCount];
            int[] maxCount = new int[selectedSchema.teamCount];
            bool[] played = new bool[selectedSchema.teamCount];
            int[] homeCount = new int[selectedSchema.teamCount];
            int[] visitCount = new int[selectedSchema.teamCount];
            for (int i = 0; i < selectedSchema.teamCount; i++)
            {
                homeVisit[i] = 0;
                matchCount[i] = 1;
                maxCount[i] = 0;
                visitCount[i] = 0;
                homeCount[i] = 0;
            }
            foreach(SchemaWeek week in selectedSchema.weeks.Values)
            {
                for (int i = 0; i < selectedSchema.teamCount; i++)
                {
                    played[i] = false;
                }
                foreach(SchemaMatch match in week.matches)
                {
                    played[match.team1] = true;
                    played[match.team2] = true;
                    homeCount[match.team1]++;
                    visitCount[match.team2]++;
                    if (homeVisit[match.team1] != 1)
                    {
                        homeVisit[match.team1] = 1;
                        matchCount[match.team1] = 1;
                    }
                    else matchCount[match.team1]++;
                    if (homeVisit[match.team2] != 2)
                    {
                        homeVisit[match.team2] = 2;
                        matchCount[match.team2] = 1;
                    }
                    else matchCount[match.team2]++;
                }
                for (int i = 0; i < selectedSchema.teamCount; i++)
                {
                    if (matchCount[i] > maxCount[i]) maxCount[i] = matchCount[i];
                    if (played[i] == false)
                    {
                        textBox1.AppendText("Team " + (i + 1).ToString() + " speelt niet elke week!!" + Environment.NewLine);
                    }
                }
            }
            for (int i = 0; i < selectedSchema.teamCount; i++)
            {
                if (visitCount[i] != homeCount[i])
                {
                    textBox1.AppendText("Team " + (i + 1).ToString() + " speelt " + homeCount[i] + "thuis, en " + visitCount[i] + "uit" + Environment.NewLine);
                }
            }
            for (int i = 0; i < selectedSchema.teamCount; i++)
            {
                textBox1.AppendText("Team " + (i + 1).ToString() + " speelt " + maxCount[i].ToString() + " achter elkaar thuis/uit."+Environment.NewLine);
            }
 
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedSchema = schemas[comboBox1.SelectedIndex];
            UpdateForm(comboBox1.SelectedIndex);
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

    }
    class DelegateSchemaObject
    {
        public AspectPutterDelegate putter = null;
        public AspectGetterDelegate getter = null;
        public DelegateSchemaObject(SchemaWeek week)
        {
            putter = delegate(object rowObject, object newValue)
            {
                int matchNr = (int)rowObject;
            };
            getter = delegate(object rowObject)
            {
                int matchNr = (int)rowObject;
                if (week.matches.Count < matchNr)
                    return "-";
                return (week.matches[matchNr - 1].team1 + 1).ToString() + " - " + (week.matches[matchNr - 1].team2 + 1).ToString();
            };
        }

    }
}
