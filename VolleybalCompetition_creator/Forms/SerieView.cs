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

namespace VolleybalCompetition_creator
{
    public partial class SerieView : DockContent
    {
        Klvv klvv = null;
        GlobalState state;
        Serie serie = null;
        Poule poule = null;
        List<Team> teams = new List<Team>();
        string firstHalf;
        string secondHalf;
        public SerieView(Klvv klvv, GlobalState state)
        {
            this.klvv = klvv;
            this.state = state;
            InitializeComponent();
            objectListView1.SetObjects(klvv.series);
            foreach (string reeks in klvv.annorama.reeksen)
            {
                comboBox1.Items.Add(reeks);
            }
            if (comboBox1.Items.Count > 0) comboBox1.SelectedIndex = 0;
            klvv.OnMyChange += state_OnMyChange;
            state.OnMyChange += state_OnMyChange;
            firstHalf = File.ReadAllText(@"html/html_first.html");
            secondHalf = File.ReadAllText(@"html/html_second.html");
            UpdateWebBrowser();
            UpdatePouleList();
        }
        public void state_OnMyChange(object source, MyEventArgs e)
        {
            if (e.klvv != null)
            {
                klvv.OnMyChange -= state_OnMyChange;
                klvv = e.klvv;
                klvv.OnMyChange += state_OnMyChange;
            }
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => state_OnMyChange(source, e)));
                return;
            }
            lock (klvv)
            {
                UpdateSerieList();
                UpdatePouleList();
                UpdateTeamList();
                UpdateWebBrowser();
            }
        }
        void UpdateSerieList()
        {
            objectListView1.SetObjects(klvv.series,true);
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
            }
            catch (Exception)
            { } // do nothing with this
            webBrowser1.DocumentText = html;
        }
        void UpdateWebBrowser()
        {
            List<Sporthal> sl = new List<Sporthal>();
            // create list of sporthalls
            if (serie != null)
            {
                string markers = "";
                bool first = true;
                foreach (Team t in serie.teams)
                {
                    string letter = "X";
                    if (t.sporthal != null)
                    {
                        if (t.poule != null)
                        {
                            letter = t.poule.name;
                        }
                        double lat = t.sporthal.lat;
                        double shift = 0.005 * (sl.Count(s => s == t.sporthal.sporthall));
                        double lng = t.sporthal.lng;
                        lat += shift;
                        lng += shift*4;
                        if (first == false) markers += ',';
                        markers += string.Format("['{0}',{1},{2},'{3}']\n", t.name, lat.ToString(CultureInfo.InvariantCulture), lng.ToString(CultureInfo.InvariantCulture), letter);
                        first = false;
                        sl.Add(t.sporthal.sporthall);
                    }
                }
                DisplayHtml(firstHalf + markers + secondHalf);
            }
        }
        private void objectListView1_SelectionChanged(object sender, EventArgs e)
        {
            if (serie != (Serie)objectListView1.SelectedObject)
            {
                serie = (Serie)objectListView1.SelectedObject;
                poule = null;
                teams.Clear();
                button1.Enabled = (objectListView1.SelectedObjects.Count == 1);
                state.Changed();
                //UpdatePouleList();
                //UpdateTeamList();
                //UpdateWebBrowser();
            }
        }

        private void objectListView2_SelectionChanged(object sender, EventArgs e)
        {
            button2.Enabled = (objectListView2.SelectedObjects.Count >= 1);
            button3.Enabled = (objectListView2.SelectedObjects.Count == 1 && objectListView3.SelectedObjects.Count>0);
            button5.Enabled = (objectListView1.SelectedObjects.Count >= 1);
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
                klvv.poules.Remove(p);
            }
            //UpdateSerieList();
            //UpdatePouleList();
            klvv.RenewConstraints();
            klvv.Evaluate(null);
            klvv.Changed();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                char Letter = 'A';
                List<Poule> poules = serie.poules.ToList();
                poules.Sort(delegate(Poule p1, Poule p2) { return p1.name.CompareTo(p2.name); });
                foreach (Poule p in poules)
                {
                    if (p.name == Letter.ToString()) Letter++;
                }
                List<Weekend> weekends = klvv.annorama.GetReeks(comboBox1.SelectedItem.ToString());

                int teamCount = int.Parse(comboBox1.SelectedItem.ToString());
                Poule poule = new Poule(Letter.ToString(), teamCount,serie);
                foreach (Weekend we in weekends)
                {
                    poule.weekends.Add(new Weekend(we.Saturday));
                }
                serie.poules.Add(poule);
                if (poule.serie.Gewestelijk) 
                    poule.CreateMatches();
                else 
                    poule.CreateMatchesFromSchemaFiles();
                klvv.poules.Add(poule);
                klvv.RenewConstraints();
                klvv.Evaluate(null);
                klvv.Changed();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach (Team team in objectListView3.SelectedObjects)
            {
                poule.AddTeam(team);
            }
            klvv.RenewConstraints();
            klvv.Evaluate(null);
            klvv.Changed();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OptimizeDistance();
        }
        private void OptimizeDistance()
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
                        if(SelectedPoules.Contains(team.poule)) // mag het team meedoen met de optimalisatie
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
                                    if (te.RealTeam() || team.poule.TeamCount>minimumTeams)
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
            klvv.RenewConstraints();
            klvv.Evaluate(null);
            klvv.Changed();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            foreach(Serie serie in objectListView1.SelectedObjects)
            {
                if (serie.optimizable)
                {
                    foreach (Poule poule in serie.poules)
                    {
                        List<Weekend> weekends = klvv.annorama.GetReeks(poule.maxTeams.ToString());
                        if (weekends.Count < (poule.maxTeams - 1) * 2)
                        {
                            System.Windows.Forms.MessageBox.Show(string.Format("Number of weekends in anorama ({0}) not sufficient for schema ({1})", weekends.Count, (poule.maxTeams - 1) * 2));
                        }
                        if (weekends.Count != (poule.maxTeams - 1) * 2)
                        {
                            //System.Windows.Forms.MessageBox.Show(string.Format("Number of weekends in anorama ({0}) do not match with schema ({1})", weekends.Count, (poule.maxTeams - 1) * 2));
                        }
                        poule.weekends.Clear();
                        foreach (Weekend we in weekends)
                        {
                            poule.weekends.Add(new Weekend(we.Saturday));
                        }
                        poule.matches.Clear();
                        if (poule.serie.Gewestelijk) 
                            poule.CreateMatches();
                        else 
                            poule.CreateMatchesFromSchemaFiles();
                        klvv.RenewConstraints();
                        klvv.Evaluate(null);
                        klvv.Changed();

                    }
                }
            }
        }

        private void SerieView_FormClosed(object sender, FormClosedEventArgs e)
        {
            klvv.OnMyChange -= state_OnMyChange;
            state.OnMyChange -= state_OnMyChange;
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
                    Poule newPoule = (Poule)diag.Selection.obj;
                    if (diag.Ok && newPoule != team.poule)
                    {
                        if(team.poule != null) team.poule.RemoveTeam(team);
                        if (newPoule != null)
                        {
                            newPoule.AddTeam(team);
                        }
                        klvv.Evaluate(null);
                        klvv.Changed();
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
            if (poule.TeamCount <= poule.maxTeams-2) e.Item.BackColor = Color.Orange;
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
            List<Team> tms = new List<Team>();
            foreach (Team team in serie.teams)
            {
                if (team.poule != null) team.poule.RemoveTeam(team);
                tms.Add(team);
            }            
            // 
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
            // Eerst alles leeg
            int maxTeams = 0;
            foreach (Poule p in serie.poules)
            {
                maxTeams += p.maxTeams;
            }
            // dan alles opnieuw indelen
            int remainingTeams = serie.teams.Count;
            int remainingPoules = serie.poules.Count;


            if (remainingTeams > maxTeams)
            {
                System.Windows.Forms.MessageBox.Show(string.Format("Number of teams ({0}) past niet in het aantal poules", remainingTeams));
                return;
            }
            List<Poule> poules = new List<Poule>();
            poules.AddRange(serie.poules);
            poules.Sort(delegate(Poule p1, Poule p2)
            {
                return p1.name.CompareTo(p2.name);
            });
            int it = 0;
            foreach(Poule p in poules)
            {
                double avg = Math.Ceiling(((double)remainingTeams) / remainingPoules);
                for(int i=0;i<avg;i++)
                {
                    p.AddTeam(tms[it++]);
                    remainingTeams--;
                }
                remainingPoules--;
            }
 
            klvv.RenewConstraints();
            klvv.Evaluate(null);
            klvv.Changed();
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
                t.DeleteTeam(klvv);
            }

            klvv.RenewConstraints();
            klvv.Evaluate(null);
            klvv.Changed();
        }

    
    }
}
