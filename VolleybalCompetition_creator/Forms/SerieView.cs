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
            
            objectListView3.BuildList(true);
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
                        double shift = 0.005 * (sl.Count(s => s == t.sporthal));
                        double lng = t.sporthal.lng;
                        lat += shift;
                        lng += shift*4;
                        if (first == false) markers += ',';
                        markers += string.Format("['{0}',{1},{2},'{3}']\n", t.name, lat.ToString(CultureInfo.InvariantCulture), lng.ToString(CultureInfo.InvariantCulture), letter);
                        first = false;
                        sl.Add(t.sporthal);
                    }
                }
                DisplayHtml(firstHalf + markers + secondHalf);
            }
        }
        private void objectListView1_SelectionChanged(object sender, EventArgs e)
        {
            serie = (Serie)objectListView1.SelectedObject;
            button1.Enabled = (objectListView1.SelectedObjects.Count == 1);
            UpdatePouleList();
            UpdateTeamList();
            UpdateWebBrowser();
        }

        private void objectListView2_SelectionChanged(object sender, EventArgs e)
        {
            button2.Enabled = (objectListView2.SelectedObjects.Count == 1);
            button3.Enabled = (objectListView2.SelectedObjects.Count == 1 && objectListView3.SelectedObjects.Count>0);
            poule = (Poule)objectListView2.SelectedObject;
        }
        private void objectListView3_SelectionChanged(object sender, EventArgs e)
        {
            button3.Enabled = (objectListView2.SelectedObjects.Count == 1 && objectListView3.SelectedObjects.Count > 0);
            teams.Clear();
            foreach (Team te in objectListView3.SelectedObjects)
            {
                teams.Add(te);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {// delete poule
            foreach (Team team in poule.teams)
            {
                team.poule = null;
            }
            int i = 0;
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
            }
            serie.poules.Remove(poule.name);
            klvv.poules.Remove(poule);
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

                Poule poule = new Poule(Letter.ToString());
                poule.serie = serie;
                foreach (Weekend we in weekends)
                {
                    poule.weekends.Add(new Weekend(we.Saturday));
                }
                serie.poules.Add(poule.name, poule);
                int teamCount = int.Parse(comboBox1.SelectedItem.ToString());
                poule.CreateMatches(teamCount);
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
                poule.teams.Add(team);
                if (team.poule != null)
                {
                    team.poule.teams.Remove(team);
                }
                team.poule = poule;
            }
            klvv.RenewConstraints();
            klvv.Evaluate(null);
            klvv.Changed();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach(Team team in teams)
            {
                int minDistance = team.poule.CalculateDistances(team);
                Poule currentPoule = team.poule;
                Poule minPoule = null;
                foreach (Poule p in serie.poules.Values)
                {
                    bool allowed = true;
                    if (checkBox1.Checked)
                    {
                        foreach (Team t in p.teams)
                        {
                            if (t.club == team.club) allowed=false;
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
                        bool allowed = true;
                        if (checkBox1.Checked)
                        {
                            foreach (Team t in currentPoule.teams)
                            {
                                if (te.club == t.club) allowed=false;
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
                if (minPoule != null && minTeam != null && minTeam != team && minPoule != currentPoule)
                {
                    currentPoule.teams.Remove(team);
                    minPoule.teams.Add(team);
                    team.poule = minPoule;
                    minPoule.teams.Remove(minTeam);
                    currentPoule.teams.Add(minTeam);
                    minTeam.poule = currentPoule;
                    
                }
            }
            klvv.RenewConstraints();
            klvv.Evaluate(null);
            klvv.Changed();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            foreach(Serie serie in klvv.series)
            {
                if (serie.optimizable)
                {
                    foreach (Poule poule in serie.poules.Values)
                    {
                        int teamCount = poule.teams.Count;
                        while (teamCount != 6 && teamCount != 10 && teamCount != 12 && teamCount != 14) teamCount++;
                        List<Weekend> weekends = klvv.annorama.GetReeks(teamCount.ToString());
                        if (weekends.Count < (teamCount - 1) * 2)
                        {
                            System.Windows.Forms.MessageBox.Show("Number of weekends not sufficient for number of teams");
                        }
                        poule.weekends.Clear();
                        foreach (Weekend we in weekends)
                        {
                            poule.weekends.Add(new Weekend(we.Saturday));
                        }
                        poule.matches.Clear();
                        poule.CreateMatches(teamCount);
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

    
    }
}
