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
            firstHalf = File.ReadAllText(@"Data/html_first.html");
            secondHalf = File.ReadAllText(@"Data/html_second.html");
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
            lock (klvv) ;
            UpdateSerieList();
            UpdatePouleList();
            UpdateTeamList();
        }
        void UpdateSerieList()
        {
            objectListView1.BuildList(true);
        }
        void UpdatePouleList()
        {
            if (serie != null)
            {
                objectListView2.SetObjects(serie.poules.Values);
                objectListView2.SelectedObject = poule;
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
                UpdatePouleList();
                UpdateTeamList();
                UpdateWebBrowser();
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
            //button3.Enabled = (objectListView3.SelectedObjects.Count >0);
            teams.Clear();
            foreach (Team te in objectListView3.SelectedObjects)
            {
                teams.Add(te);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {// delete poule
            
            /*int i = 0;
            while(i<klvv.constraints.Count)
            {
                ConstraintSchemaTooClose c = klvv.constraints[i] as ConstraintSchemaTooClose;
                if (c != null && c.poule == poule)
                {
                    klvv.constraints.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }*/
            foreach (Poule p in objectListView2.SelectedObjects)
            {
                List<Team> tempList = new List<Team>(p.teams);
                foreach (Team team in tempList)
                {
                    p.RemoveTeam(team);
                }
                serie.poules.Remove(p.name);
                klvv.poules.Remove(p);
            }
            /*foreach (DockContent content in this.DockPanel.Contents)
            {
                PouleView pouleview = content as PouleView;
                if (pouleview != null)
                {
                    if (poule == pouleview.poule)
                    {
                        pouleview.Close();
                        break;
                    }
                }
            }*/

            UpdateSerieList();
            UpdatePouleList();
            klvv.RenewConstraints();
            klvv.Evaluate(null);
            klvv.Changed();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                char Letter = 'A';
                List<Poule> poules = serie.poules.Values.ToList();
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
                serie.poules.Add(poule.name, poule);
                if (poule.serie.Gewestelijk) poule.CreateMatches();
                else poule.CreateMatchesFromSchemaFiles();
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
            int minimumTeams = serie.teams.Count / serie.poules.Count;
            foreach (Team team in serie.teams)
            {
                if (team.RealTeam())
                {
                    if (team.poule != null)
                    {
                        int minDistance = team.poule.CalculateDistances(team);
                        // When teams are together in a poule, ensure that there is no reason to not move to another poule
                        foreach (Team t in team.poule.teams)
                        {
                            if (t.club == team.club & checkBox1.Checked) minDistance = int.MaxValue;
                        }
                        Poule currentPoule = team.poule;
                        Poule minPoule = null;
                        foreach (Poule p in serie.poules.Values)
                        {
                            bool allowed = true;
                            if (checkBox1.Checked)
                            {
                                foreach (Team t in p.teams)
                                {
                                    if (t != team && t.club == team.club) allowed = false;// ander team zit al in poule
                                }
                            }
                            if (allowed)
                            {
                                int distance = p.CalculateDistances(team);
                                if (distance < minDistance)
                                {
                                    minDistance = distance;
                                    minPoule = p;
                                }
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
                    foreach (Poule poule in serie.poules.Values)
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
                        if (poule.serie.Gewestelijk) poule.CreateMatches();
                        else poule.CreateMatchesFromSchemaFiles();
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
                    Selection def = null;
                    foreach (Poule poule in serie.poules.Values)
                    {
                        Selection sel = new Selection(poule.name, poule);
                        if (poule == team.poule) def = sel;
                        list.Add(sel);
                    }
                    list.Add(new Selection("No poule", null));
                    SelectionDialog diag = new SelectionDialog(list, def);
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
                    UpdateTeamList();
                    UpdatePouleList();
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
            List<Team> tms = teams;
            if (tms.Count == 0) tms = serie.teams;
            // Remove the current assigned poule
            foreach (Team team in tms)
            {
                if (team.poule != null) team.poule.RemoveTeam(team);
            }            
            // 
            tms.Sort(delegate(Team t1, Team t2)
            {
                int ct1 = tms.Count(t => t.club == t1.club);
                int ct2 = tms.Count(t => t.club == t2.club);
                return ct1.CompareTo(ct2);
            });
            List<Poule> tmPoules = new List<Poule>();
            foreach(Poule p in serie.poules.Values)
            {
                tmPoules.Add(p);
            }
            tmPoules.Sort(delegate(Poule p1, Poule p2)
            {
                int cp1 = (p1.maxTeams - p1.TeamCount);
                int cp2 = (p1.maxTeams - p2.TeamCount);
                return cp1.CompareTo(cp2);
            });
            int i = 0;
            foreach (Team team in tms)
            {
                Poule po = tmPoules[i % tmPoules.Count];
                po.AddTeam(team);
                i++;
            }
            // 3 times to be sure that a local minimum is found
            OptimizeDistance();
            OptimizeDistance();
            OptimizeDistance();

            klvv.RenewConstraints();
            klvv.Evaluate(null);
            klvv.Changed();
        }

    
    }
}
