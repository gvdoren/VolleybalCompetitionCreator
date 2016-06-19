using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using BrightIdeasSoftware;
using System.IO;
using System.Globalization;

namespace CompetitionCreator
{
    public partial class SerieView : DockContent
    {
        Model model = null;
        GlobalState state;
        Serie serie = null;
        int selectedSeriesCount = 0;
        Poule poule = null;
        List<Team> teams = new List<Team>();
        string firstHalf;
        string secondHalf;
        string noLatLng;
        List<Schema> schemas = new List<Schema>();
            
        public SerieView(Model model, GlobalState state)
        {
            this.model = model;
            this.state = state;
            InitializeComponent();
            objectListView1.SetObjects(model.series);
//            foreach (YearPlan reeks in model.yearPlans.reeksen)
//            {
//                comboBox1.Items.Add(reeks.Name);
//            }
            string[] files = Directory.GetFiles(System.Windows.Forms.Application.StartupPath + @"/Schemas/");
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                Schema newSchema = new Schema();
                newSchema.Read(file);
                schemas.Add(newSchema);
            }
            schemas.Sort((s1,s2) => s1.teamCount.CompareTo(s2.teamCount));
            foreach (Schema schema in schemas)
            {
                comboBox1.Items.Add(schema);
                if (comboBox1.Items.Count > 0) comboBox1.SelectedIndex = 0;
            }
            model.OnMyChange += state_OnMyChange;
            GlobalState.OnMyChange += state_OnMyChange;
            firstHalf = File.ReadAllText(@"html/html_first.html");
            secondHalf = File.ReadAllText(@"html/html_second.html");
            noLatLng = File.ReadAllText(@"html/no_latlng.html");
            UpdateWebBrowser();
            UpdatePouleList();
        }
        public void state_OnMyChange(object source, MyEventArgs e)
        {
            if (e.model != null)
            {
                model.OnMyChange -= state_OnMyChange;
                model = e.model;
                model.OnMyChange += state_OnMyChange;
            }
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => state_OnMyChange(source, e)));
                return;
            }
            lock (model)
            {
                UpdateSerieList();
                UpdatePouleList();
                UpdateTeamList();
                UpdateWebBrowser();
            }
        }
        void UpdateSerieList()
        {
            objectListView1.SetObjects(model.series, true);
            if (serie != null)
            {
                objectListView1.SelectedObject = serie;
            }
            objectListView1.BuildList(true);
        }
        void UpdatePouleList()
        {
            if (serie != null)
            {
                List<Poule> selectedPoules = new List<Poule>();
                foreach (object obj in objectListView2.SelectedObjects)
                {
                    Poule p = obj as Poule;
                    if (serie.poules.Contains(p)) selectedPoules.Add(p);
                }
                objectListView2.SetObjects(serie.poules);
                objectListView2.SelectedObjects = selectedPoules;
            }
            else
            {
                objectListView2.SetObjects(null);
            }
            objectListView2.BuildList(true);
            objectListView2_SelectionChanged(null, null);
        }
        void UpdateTeamList()
        {
            if (serie != null)
            {
                objectListView3.SetObjects(serie.teams);
            }
            else
            {
                objectListView3.SetObjects(null);
            }

            objectListView3.BuildList(false);
            objectListView3.SelectedObjects = teams;
            objectListView3_SelectionChanged(null, null);
        }
        private void DisplayHtml(string html)
        {
            Console.Write(html);
            webBrowser1.Navigate("about:blank");
            try
            {
                if (webBrowser1.Document != null)
                {
                    webBrowser1.Document.Write(string.Empty);
                }
                webBrowser1.DocumentText = html;
            }
            catch (Exception)
            {
                Error.AddManualError("Error showing html. Possible cause: no internet connection.");
                MessageBox.Show("Error showing html. Possible cause: no internet connection.");
            } // do nothing with this
        }
        void UpdateWebBrowser()
        {
            List<Sporthal> sl = new List<Sporthal>();
            if (objectListView1.SelectedObjects.Count > 0)
            {
                bool geoInfo = false;
                string markers = "";
                bool first = true;
                button4.Enabled = objectListView1.SelectedObjects.Count == 1; // Not optimizing when multiple are selected
                foreach (Object obj in objectListView1.SelectedObjects)
                {
                    Serie serie = obj as Serie;
                    // create list of sporthalls
                    foreach (Team t in serie.teams)
                    {
                        string letter = "X";
                        if (t.sporthal != null && Math.Abs(t.sporthal.lat) > 0.01 && Math.Abs(t.sporthal.lng) > 0.01)
                        {
                            if (t.poule != null)
                            {
                                letter = t.poule.name;
                            }
                            double lat = t.sporthal.lat;
                            double shift = 0.005 * (sl.Count(s => s == t.sporthal.sporthall));
                            double lng = t.sporthal.lng;
                            lat += shift;
                            lng += shift * 4;
                            if (first == false) markers += ',';
                            char oldChar = '\'';
                            char newChar = ' ';
                            string name = t.name.Replace(oldChar, newChar);
                            markers += string.Format("['{0}',{1},{2},'{3}']\n", name, lat.ToString(CultureInfo.InvariantCulture), lng.ToString(CultureInfo.InvariantCulture), letter);
                            first = false;
                            sl.Add(t.sporthal.sporthall);
                            geoInfo = true;
                        }
                        else
                        {
                            button4.Enabled = false;
                        }
                    }
                }
                if (geoInfo) DisplayHtml(firstHalf + markers + secondHalf);
                else
                {
                    DisplayHtml(noLatLng);
                }
            }
            else
            {

            }
        }
        private void objectListView1_SelectionChanged(object sender, EventArgs e)
        {
            
            if (serie != (Serie)objectListView1.SelectedObject || objectListView1.SelectedObjects.Count != selectedSeriesCount)
            {
                selectedSeriesCount = objectListView1.SelectedObjects.Count;
                serie = (Serie)objectListView1.SelectedObject;
                poule = null;
                teams.Clear();
                button1.Enabled = (objectListView1.SelectedObjects.Count == 1);
                button5.Enabled = (objectListView1.SelectedObjects.Count >= 1);
                GlobalState.Changed();
                //UpdatePouleList();
                //UpdateTeamList();
                //UpdateWebBrowser();
            }
        }
        private void objectListView2_SelectionChanged(object sender, EventArgs e)
        {
            button2.Enabled = (objectListView2.SelectedObjects.Count >= 1);
            button3.Enabled = (objectListView2.SelectedObjects.Count == 1 && objectListView3.SelectedObjects.Count > 0);
            poule = (Poule)objectListView2.SelectedObject;
        }
        private void objectListView3_SelectionChanged(object sender, EventArgs e)
        {
            button3.Enabled = (objectListView2.SelectedObjects.Count == 1 && objectListView3.SelectedObjects.Count > 0);
            button7.Enabled = (objectListView3.SelectedObjects.Count == 1);

            teams.Clear();
            foreach (Team te in objectListView3.SelectedObjects)
            {
                teams.Add(te);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {// delete poule

            foreach (Poule p in objectListView2.SelectedObjects)
            {
                List<Team> tempList = new List<Team>(p.teams);
                foreach (Team team in tempList)
                {
                    p.RemoveTeam(team);
                }
                serie.poules.Remove(p);
                model.poules.Remove(p);
            }
            //UpdateSerieList();
            //UpdatePouleList();
            model.RenewConstraints();
            model.Evaluate(null);
            model.Changed();
        }
        private bool selectYearPlan(int weekCount, ref YearPlan result)
        {
            List<YearPlan> yearPlans = model.yearPlans.reeksen.FindAll(p => (p.Count-1)*2 == weekCount);
            List<Selection> possiblePlans = new List<Selection>();
            foreach (YearPlan plan in yearPlans)
            {
                Selection sel = new Selection(plan.Name, plan);
                possiblePlans.Add(sel);
            }
            if(possiblePlans.Count > 0)
            {
                possiblePlans[0].selected = true;
                SelectionDialog diag = new SelectionDialog(possiblePlans);
                diag.Text = "Select the year plan to be used";
                diag.ShowDialog();
                if (diag.Ok)
                {
                    result = (YearPlan)diag.Selection.obj;
                    return true;
                }
            } else
            {
                MessageBox.Show("No year plan available for a schema with "+weekCount.ToString()+"weeks.");
            }
            return false;
        }
        private void createPoule(Serie serie, int teamCount, YearPlan reeks, Schema schema)
        {
            char Letter = 'A';
            List<Poule> poules = serie.poules.ToList();
            poules.Sort(delegate(Poule p1, Poule p2) { return p1.name.CompareTo(p2.name); });
            foreach (Poule p in poules)
            {
                if (p.name == Letter.ToString()) Letter++;
            }
            List<YearPlanWeek> weeks = reeks.weeks;
            Poule poule = new Poule(Letter.ToString(), teamCount, serie);
            // Create the week lists
            int k = 1;
            while (k <= reeks.weeks.Count)
            {
                int index = reeks.weeks.FindIndex(w => w.weekNr == k);
                poule.weeks.Add(reeks.weeks[index].week);
                k++;
            }
            if (schema == null)
            {
                poule.CreateMatches(); // schema is generated
            }
            else
                poule.CreateMatchesFromSchemaFiles(schema, serie, poule);
            serie.poules.Add(poule);
            model.poules.Add(poule);

        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                Schema schema = (Schema) comboBox1.SelectedItem;
                int weekCount = schema.weeks.Count;

                YearPlan plan = null;
                if(selectYearPlan(weekCount, ref plan))
                {
                    createPoule(serie, plan.Count, plan, schema);
                }
                else return;
                model.RenewConstraints();
                model.Evaluate(null);
                model.Changed();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach (Team team in objectListView3.SelectedObjects)
            {
                poule.AddTeam(team);
            }
            model.RenewConstraints();
            model.Evaluate(null);
            model.Changed();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OptimizeDistance();
        }
        private void OptimizeDistance()
        {
            try
            {
                // determine the selected poules. If nothing is selected, all poules in the series are included.
                List<Poule> SelectedPoules = new List<Poule>();
                foreach (Poule p in objectListView2.SelectedObjects)
                {
                    SelectedPoules.Add(p);
                }
                if (SelectedPoules.Count == 0) SelectedPoules.AddRange(serie.poules);

                int minimumTeams = serie.teams.Count / serie.poules.Count;
                foreach (Team team in serie.teams)
                {
                    if (team.RealTeam())
                    {
                        if (team.poule != null)
                        {
                            if (SelectedPoules.Contains(team.poule)) // mag het team meedoen met de optimalisatie
                            {
                                int minDistance = team.poule.CalculateDistances(team);
                                // When teams are together in a poule, ensure that there is no reason to not move to another poule
                                foreach (Team t in team.poule.teams)
                                {
                                    if (t.club == team.club & checkBox1.Checked) minDistance = int.MaxValue;
                                }
                                Poule currentPoule = team.poule;
                                Poule minPoule = null;
                                foreach (Poule p in SelectedPoules)
                                {
                                    int distance = p.CalculateDistances(team);
                                    // When teams are together in a poule, ensure that there is no reason to not move to another poule
                                    foreach (Team t in p.teams)
                                    {
                                        if (t != team && t.club == team.club & checkBox1.Checked) distance = int.MaxValue;
                                    }

                                    if (distance < minDistance)
                                    {
                                        minDistance = distance;
                                        minPoule = p;
                                    }
                                }
                                minDistance = int.MaxValue;
                                Team minTeam = null;
                                if (minPoule != null)
                                {
                                    foreach (Team te in minPoule.teams)
                                    {
                                        if (te.RealTeam() || team.poule.TeamCount > minimumTeams)
                                        {
                                            bool allowed = true;
                                            if (checkBox1.Checked)
                                            {
                                                foreach (Team t in currentPoule.teams)
                                                {
                                                    if (te.club == t.club) allowed = false;
                                                }
                                            }
                                            if (allowed)
                                            {
                                                int distance = currentPoule.CalculateDistances(te);
                                                if (distance < minDistance)
                                                {
                                                    minDistance = distance;
                                                    minTeam = te;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (minPoule != null && minTeam != null && minTeam != team && minPoule != currentPoule)
                                {
                                    currentPoule.RemoveTeam(team);
                                    minPoule.RemoveTeam(minTeam);
                                    minPoule.AddTeam(team);
                                    currentPoule.AddTeam(minTeam);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Error.AddManualError("Error occured while optimizing distances.", ex.ToString());
                MessageBox.Show(ex.ToString());
            }
            model.RenewConstraints();
            model.Evaluate(null);
            model.Changed();
        }

        private void SerieView_FormClosed(object sender, FormClosedEventArgs e)
        {
            model.OnMyChange -= state_OnMyChange;
            GlobalState.OnMyChange -= state_OnMyChange;
        }

        private void objectListView3_DoubleClick(object sender, EventArgs e)
        {
            Point mousePosition = objectListView3.PointToClient(Control.MousePosition);
            ListViewHitTestInfo hit = objectListView3.HitTest(mousePosition);
            int columnindex = hit.Item.SubItems.IndexOf(hit.SubItem);
            Team team = (Team)objectListView3.SelectedObject;
            if (team != null)
            {
                if (columnindex == 1)
                {
                    List<Selection> list = new List<Selection>();
                    foreach (Poule poule in serie.poules)
                    {
                        Selection sel = new Selection(poule.name, poule);
                        if (poule == team.poule) sel.selected = true;
                        list.Add(sel);
                    }
                    list.Add(new Selection("No poule", null));
                    SelectionDialog diag = new SelectionDialog(list);
                    diag.Text = "Select the poule:";
                    diag.ShowDialog();
                    if (diag.Ok)
                    {
                        Poule newPoule = (Poule)diag.Selection.obj;
                        if (newPoule != team.poule)
                        {
                            if (team.poule != null) team.poule.RemoveTeam(team);
                            if (newPoule != null)
                            {
                                newPoule.AddTeam(team);
                            }
                            model.RenewConstraints();
                            model.Evaluate(null);
                            model.Changed();
                        }
                    }
                    //objectListView1.BuildList(true);
                    //UpdateTeamList();
                    //UpdatePouleList();
                }
            }

        }

        private void objectListView2_FormatRow(object sender, FormatRowEventArgs e)
        {
            Poule poule = (Poule)e.Model;
            if (poule.TeamCount > poule.maxTeams) e.Item.BackColor = Color.Red;
            if (poule.TeamCount <= poule.maxTeams - 2) e.Item.BackColor = Color.Orange;
        }

        private void objectListView3_FormatCell(object sender, FormatCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                Team team = (Team)e.Model;
                if (team.poule == null) e.SubItem.BackColor = Color.Orange;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            DivideTeams(serie);
        }
        private void DivideTeams(Serie serie)
        {
            int maxTeams = 0;
            foreach (Poule p in serie.poules)
            {
                maxTeams += p.maxTeams;
            }
            if (serie.teams.Count > maxTeams)
            {
                System.Windows.Forms.MessageBox.Show(string.Format("Number of teams ({0}) past niet in het aantal poules", serie.teams.Count));
                return;
            }
            // alles leegmaken
            List<Team> tms = new List<Team>();
            foreach (Team team in serie.teams)
            {
                if (team.poule != null) team.poule.RemoveTeam(team);
                tms.Add(team);
            }
            tms.Sort(delegate(Team t1, Team t2)
            {
                if (t1.Ranking != t2.Ranking)
                {
                    if (t1.Ranking == null) return 1; // t1 is greater (no ranking is always worse than a ranking)
                    if (t2.Ranking == null) return -1; // t2 is greater (no ranking is always worse than a ranking)
                    double d1;
                    double d2;
                    bool b1 = double.TryParse(t1.Ranking, out d1);
                    bool b2 = double.TryParse(t2.Ranking, out d2);
                    if (b1 && b2)
                    {
                        return d1.CompareTo(d2);
                    }
                    else
                    {
                        return 0;
                    }
                }
                return 0;
            });
            List<Poule> poules = new List<Poule>();
            poules.AddRange(serie.poules);
            poules.Sort(delegate(Poule p1, Poule p2)
            {
                return p1.name.CompareTo(p2.name);
            });
            // Bereken in welke minimale poule ze moeten zitten (indien meerdere teams van 1 club)
            Dictionary<Poule, List<Team>> pouleteams = new Dictionary<Poule, List<Team>>();
            foreach(Poule poule in poules)
            {
                pouleteams[poule] = new List<Team>();
            }
            foreach(Team team in tms.Reverse<Team>())
            {
                foreach(Poule poule in poules.Reverse<Poule>())
                {
                    if(pouleteams[poule].Exists( t => t.club == team.club) == false)
                    {
                        pouleteams[poule].Add(team);
                        break;
                    }
                }
            }
            // dan alles opnieuw indelen
            int remainingPoules = serie.poules.Count;
            foreach (Poule p in poules)
            {
                double avg = Math.Ceiling(((double)tms.Count) / remainingPoules);
                foreach (Team team in pouleteams[p])
                {
                    p.AddTeam(team);
                    tms.Remove(team);
                }
                for (int i = p.TeamCount; i < avg; i++)
                {
                    int index = tms.FindIndex(t1 => p.teams.Exists(t2 => t2.club == t1.club) == false);
                    if (index == -1)
                        index = 0; // teams of dezelfde club moeten bij elkaar (bijv. maar 1 poule)
                    p.AddTeam(tms[index]);
                    // verwijder team uit de lijst van minimale plekken
                    foreach (Poule poule in poules)
                    {
                        if (pouleteams[poule].Contains(tms[index]))
                            pouleteams[poule].Remove(tms[index]);
                    }
                    tms.RemoveAt(index);
                }
                remainingPoules--;
            }
            model.RenewConstraints();
            model.Evaluate(null);
            model.Changed();
        }

        private void objectListView1_FormatRow(object sender, FormatRowEventArgs e)
        {
            Color color = Color.White;
            Serie serie = (Serie)e.Model;
            foreach (Team team in serie.teams)
            {
                if (team.poule == null) color = Color.Red;
            }
            foreach (Poule poule in serie.poules)
            {
                if (poule.TeamCount > poule.maxTeams) color = Color.Red;
                if (poule.TeamCount <= poule.maxTeams - 2 && color != Color.Red) color = Color.Orange;
            }
            e.Item.BackColor = color;

        }

        private void checkboxWithinSelectedPoules_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            Team t = (Team)objectListView3.SelectedObject;
            string s = string.Format("Het team '{0}' van club '{1}' in serie '{2}' wordt volledig verwijderd, dus ook weggehaald bij de inschrijvingen van de club. Wil je dat?", t.name, t.club.name, t.serie.name);
            DialogResult dialogResult = MessageBox.Show(s, "Delete team", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                t.DeleteTeam(model);
            }

            model.RenewConstraints();
            model.Evaluate(null);
            model.Changed();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                Schema schema = (Schema)comboBox1.SelectedItem;
                int weekCount = schema.weeks.Count;

                YearPlan plan = null;
                if (selectYearPlan(weekCount, ref plan))
                {
                    foreach (Serie serie in objectListView1.SelectedObjects)
                    {
                        while (serie.teams.Count > serie.poules.Sum(p => p.maxTeams))
                        {
                            createPoule(serie, plan.Count, plan, schema);
                        }
                        // Divide teams
                        DivideTeams(serie);
                    }
                }
                else return;
                model.RenewConstraints();
                model.Evaluate(null);
                model.Changed();
            }
        }
    }
}
 