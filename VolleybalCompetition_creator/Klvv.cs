using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.Globalization;
using BrightIdeasSoftware;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace VolleybalCompetition_creator
{
    public class Klvv
    {
        public Security.LicenseKey licenseKey;
        public MySettings settings;
        WebClient wc = null;
        public bool stateNotSaved = false;
        public void MakeDirty() { stateNotSaved = true; }
        public string savedFileName = "Competition.xml";
        public int year;
        public List<Club> clubs;
        public List<Serie> series = new List<Serie>();
        public List<Poule> poules = new List<Poule>();
        public List<Team> teams = new List<Team>();
        //public List<TeamConstraint> teamConstraints = new List<TeamConstraint>(); 
        public void AddTeam(Team team)
        {
            teams.Add(team);
            //team.serie.AddTeam(team);
            team.club.AddTeam(team);
        }
        public void RemoveTeam(Team team)
        {
            // Remove constraints related to the team
            List<Constraint> tobedeleted = new List<Constraint>();
            foreach (Constraint con in constraints)
            {
                if (con.team == team) tobedeleted.Add(con);
            }
            foreach (Constraint con in tobedeleted)
            {
                constraints.Remove(con);
            }
            // Remove links from other teams to this team
            foreach (Team t in teams)
            {
                if (t.NotAtSameTime == team) t.NotAtSameTime = null;
            }
            teams.Remove(team);
            MakeDirty();
        }
        public Anorama annorama = new Anorama(DateTime.Now.Year);
        public List<Constraint> constraints = new List<Constraint>();
        public List<Sporthal> sporthalls = new List<Sporthal>();
        public void Optimize()
        {
            poules.Sort(delegate(Poule p1, Poule p2) { return p1.conflict_cost.CompareTo(p2.conflict_cost); });
            foreach (Poule p in poules)
            {
                //p.OptimizeTeamAssignment(this);
                Changed();
            }
        }
        public void Evaluate(Poule p)
        {
            lock (this)
            {
                foreach (Team team in teams) team.ClearConflicts();
                foreach (Poule poule in poules)
                {
                    poule.ClearConflicts();
                    foreach (Match match in poule.matches)
                    {
                        match.ClearConflicts();
                    }
                    foreach (Weekend weekend in poule.weekends)
                    {
                        weekend.ClearConflicts();
                    }
                }
                foreach (Serie serie in series)
                {
                    serie.ClearConflicts();
                }
                foreach (Club club in clubs)
                {
                    club.ClearConflicts();
                }
                foreach (Constraint constraint in constraints)
                {
                    constraint.Evaluate(this);
                }
            }
        }
        public void EvaluateRelatedConstraints(Poule p)
        {
            lock (this)
            {
                foreach (Club cl in clubs)
                {
                    cl.ClearConflicts();
                }
                foreach (Team te in teams)
                {
                    te.ClearConflicts();
                }
                foreach (Weekend we in p.weekends)
                {
                    we.ClearConflicts();
                }
                foreach (Constraint constraint in p.relatedConstraints)
                //foreach (Constraint constraint in constraints)
                {
                    constraint.Evaluate(this);
                }
            }
        }
        public int TotalConflicts()
        {
            lock (this)
            {
                int conflicts = 0;
                foreach (Constraint constraint in constraints)
                {
                    conflicts += constraint.conflict_cost;
                }
                return conflicts;
            }
        }
        public Klvv(int year)
        {
            string Key = VolleybalCompetition_creator.Properties.Settings.Default.LicenseKey;
            licenseKey = new Security.LicenseKey(Key);
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            HttpWebRequest.DefaultWebProxy = null;
            WebRequest.DefaultWebProxy = null;
            wc = new WebClient();
            wc.Proxy = null;
            this.year = year;
            this.annorama = new Anorama(year);


            //ImportSporthalls(XDocument.Load("http://klvv.be/server/sporthallsXML.php").Root);


            string sporthalls_json = wc.DownloadString("http://klvv.be/server/sporthalls.php");
            //string sporthalls_json = File.ReadAllText(System.Windows.Forms.Application.StartupPath + @"/InputData/sporthalls.json"); // local copy
            JArray sporthallObjects = JArray.Parse(sporthalls_json);
            foreach (JObject sporthallObject in sporthallObjects)
            {
                Sporthal newSporthal = new Sporthal(int.Parse(sporthallObject["id"].ToString()),sporthallObject["name"].ToString());
                sporthalls.Add(newSporthal);
                string location = sporthallObject["googleLocation"].ToString();
            }
            // lat & lng for each sporthal (must be included in the sporthalls.json
            //System.Windows.Forms.MessageBox.Show("KLVV reading sporthalls - lat & lng");
            string latlng_json = File.ReadAllText(System.Windows.Forms.Application.StartupPath + @"/InputData/sporthallen.json");// local copy
            JArray latlngObjects = JArray.Parse(latlng_json);
            foreach (JObject latlngObject in latlngObjects)
            {
                int id = int.Parse((string)latlngObject["id"]);
                double lat = double.Parse((string)latlngObject["lat"], CultureInfo.InvariantCulture);
                double lng = double.Parse((string)latlngObject["lng"], CultureInfo.InvariantCulture);
                Sporthal sporthal = sporthalls.Find(s => s.id == id);
                if (sporthal != null)
                {
                    sporthal.lat = lat;
                    sporthal.lng = lng;
                }
            }
            
            // read distances between sporthalls
            //System.Windows.Forms.MessageBox.Show("KLVV reading sporthalls - distances");
            string json2 = File.ReadAllText(System.Windows.Forms.Application.StartupPath + @"/InputData/sporthal_afstanden.json");// local copy
            JArray afstanden = JArray.Parse(json2);
            foreach (JObject afstand in afstanden)
            {
                int id1 = int.Parse((string)afstand["id1"]);
                int id2 = int.Parse((string)afstand["id2"]);
                int distance = int.Parse((string)afstand["afstand"]);
                Sporthal sporthal = sporthalls.Find(s => s.id == id1);
                if (sporthal != null) sporthal.distance.Add(id2, distance);
            }
 
            clubs = new List<Club>();

            Serie unknownSerie = new Serie(-1, "Unknown",this);
            unknownSerie.Nationaal = true;
            unknownSerie.optimizable = false;
            series.Add(unknownSerie);
            Sporthal unknownSporthall = new Sporthal(-1, "Unknown");
            sporthalls.Add(unknownSporthall);
            Club unknownClub = new Club(-1,"Unknown","??");
            unknownClub.sporthalls.Add(new SporthallClub(unknownSporthall));
            //clubs.Add(unknownClub);

            Evaluate(null);
            Changed();
        }
        public void OpenLastProject()
        {
            string lastOpenedProject = Properties.Settings.Default.LastOpenedProject;
            if (File.Exists(lastOpenedProject))
            {
                LoadFullCompetition(lastOpenedProject);
            }
        }
        public void RenewConstraints()
        {
            lock (this)
            {
                //constraints.Clear();
                constraints.RemoveAll(c => (c as TeamConstraint) == null); // alles behalve teamcontraints.
                foreach (Poule poule in this.poules)
                {
                    constraints.Add(new ConstraintSchemaTooClose(poule));
                    constraints.Add(new ConstraintPouleInconsistent(poule));
                    constraints.Add(new ConstraintPouleTwoTeamsOfSameClub(poule));
                    //constraints.Add(new ConstraintPouleFixedNumber(poule));
                    //constraints.Add(new ConstraintPouleOddEvenWeek(poule));
                    //constraints.Add(new ConstraintPouleFullLastTwoWeekends(poule));
                    constraints.Add(new ConstraintSchemaTooManyHomeAfterEachOther(poule));
                }

                foreach (Club club in clubs)
                {
                    constraints.Add(new ConstraintNotAllInSameHomeWeekend(club));
                    constraints.Add(new ConstraintDifferentGroupsInSameWeekend(club));
                    constraints.Add(new ConstraintPlayAtSameTime(club));
                    constraints.Add(new ConstraintNoPouleAssigned(club));
                    foreach (SporthallClub sp in club.sporthalls)
                    {
                        constraints.Add(new ConstraintSporthallNotAvailable(sp, club));
                    }
                }
                // Deze moet als laatste
                foreach (Club club in clubs)
                {
                    constraints.Add(new ConstraintClubTooManyConflicts(club));
                }
                foreach (Team team in teams)
                {
                    constraints.Add(new ConstraintTeamTooManyConflicts(team));
                }
                foreach (Poule poule in poules)
                {
                    poule.CalculateRelatedConstraints(this);
                }
            }
        }

        public int LastTotalConflicts = 0;
        public event MyEventHandler OnMyChange;
        public void Changed(Klvv p = null)
        {
            LastTotalConflicts = TotalConflicts();
            //call it then you need to update:
            if (OnMyChange != null)
            {
                MyEventArgs e = new MyEventArgs(p);
                //e.EventInfo = content;
                OnMyChange(this, e);
            }
        }
        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        public void ConvertKLVVCompetitionToCSV(string filename)
        {
            List<KeyValuePair<string, string>> output = new List<KeyValuePair<string, string>>();
            Dictionary<int, int> registrationMapping = new Dictionary<int, int>();
            Dictionary<int, int> clubMapping = new Dictionary<int, int>();
            string url3 = "http://klvv.be/server/series.php?season=2014-2015&trophy=false";
            var json3 = wc.DownloadString(url3);
            JArray Sorts = JArray.Parse(json3);
            foreach (JObject sort in Sorts)
            {
                foreach (JObject serieSorts in sort["serieSorts"])
                {
                    foreach (JObject serie in serieSorts["series"])
                    {
                        int serieId = int.Parse(serie["id"].ToString());
                        string url2 = "http://klvv.be/server/teamsinserie.php?serieId=" + serieId;
                        var json2 = wc.DownloadString(url2);
                        JArray teams = JArray.Parse(json2);
                        foreach (JObject team in teams)
                        {
                            int teamId = int.Parse(team["teamId"].ToString());
                            int registrationId = int.Parse(team["registrationId"].ToString());
                            int clubId = int.Parse(team["clubId"].ToString());
                            registrationMapping.Add(teamId, registrationId);
                            clubMapping.Add(teamId, clubId);
                        }
                    }
                }
            }


            foreach (Club club in clubs)
            {
                if (club.Id >= 0 && club.Id<1000000)
                {
                    string url = "http://klvv.be/server/clubmatches.php?clubId="+club.Id+"&hidePast=false&hideOut=true";
                    var json1 = wc.DownloadString(url);
                    //Console.WriteLine(club.name);
                    //string json1 = File.ReadAllText(@"InputData/clubmatches" + club.Id + ".json");// local copy
                    JArray poules = JArray.Parse(json1);
                    foreach (JObject poule in poules)
                    {
                        if (((int)poule["trophy"]) == 0)
                        {
                            string serieName = (string)poule["prefix"];
                            int serieId = int.Parse(poule["serieSortId"].ToString());
                            string pouleName = (string)poule["serieLetter"];

                          
                            foreach (JObject match in poule["matches"])
                            {
                                // "2013-09-28T20:30:00+0200"
                                // "2013-09-22T11:00:00+02:00"
                                // (string)match["datetime"]
                                CultureInfo provider = CultureInfo.InvariantCulture;
                                string datetimeStr = (string)match["datetime"];
                                //DateTime datetime = DateTime.ParseExact(datetimeStr, "MM/dd/yyyy HH:mm:ss", provider);

                                DateTime datetime = UnixTimeStampToDateTime(double.Parse(datetimeStr)/1000);

                                //datetime = DateTime.ParseExact("2013-09-22T11:00:00+02:00", "yyyy-MM-ddTHH:mm:sszzz", null);
                                string homeTeamName = (string)match["homeTeam"];
                                int homeTeamId = registrationMapping[(int)match["homeTeamId"]];
                                string visitorTeamName = (string)match["visitorTeam"];
                                int visitorTeamId = registrationMapping[(int)match["visitorTeamId"]];
                                int visitorClubId = clubMapping[(int)match["visitorTeamId"]];
                                string sporthal = "-";
                                int sporthalId = -1;
                                try
                                {
                                    sporthalId = (int)match["sporthallId"];
                                    sporthal = sporthalls.Find(s => s.id == sporthalId).name;
                                }
                                catch
                                {
                                    // just take a sporthall of the club
                                    sporthal = club.sporthalls[0].sporthall.name;
                                    sporthalId = club.sporthalls[0].sporthall.id;
                                }
                                sporthal = sporthal.Replace(",", " ");
                                string date = datetime.ToShortDateString();
                                string aanvangsuur = datetime.ToShortTimeString();
                                string thuisclubname = club.name;
                                int thuisclubId = club.Id;
                                string line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}, ,{14}",
                                     serieName, serieId, pouleName, "-1", homeTeamName, homeTeamId, visitorTeamName, visitorTeamId, sporthal, sporthalId, date, aanvangsuur, thuisclubname, thuisclubId, visitorClubId);
                                output.Add(new KeyValuePair<string,string>(serieName+pouleName, line));
 
                            }
                        }
                    }
                }
            }
            List<Selection> selection = new List<Selection>();
            List<string> selected = new List<string>();
            foreach(KeyValuePair<string, string> kvp in output)
            {
                if(selected.Contains(kvp.Key) == false)
                {

                    selected.Add(kvp.Key);
                    Selection sel = new Selection(kvp.Key);
                    sel.selected = true;
                    selection.Add(sel);
                }
            }
            SelectionDialog diag = new SelectionDialog(selection,true);
            diag.ShowDialog();
            selection = diag.Selections;
            StreamWriter writer = new StreamWriter(filename);
            foreach(KeyValuePair<string, string> kvp in output)
            {
                Selection s = selection.Find(sel => sel.label == kvp.Key);
                if(s != null) writer.WriteLine(kvp.Value);
            }
            writer.Close();
        }

        public void ConvertVVBCompetitionToCSV(string filename)
        {
            StreamWriter writer = new StreamWriter(filename);
            XDocument doc = XDocument.Load(@"http://vvb.volleyadmin.be/services/wedstrijden_xml.php");
            XElement kalender = doc.Element("kalender");
            foreach (XElement wedstrijd in kalender.Elements("wedstrijd"))
            {

                string aanvangsuur = wedstrijd.Element("aanvangsuur").Value;
                string datum = wedstrijd.Element("datum").Value;
                string thuisploeg = wedstrijd.Element("thuisploeg").Value;
                string bezoekersploeg = wedstrijd.Element("bezoekersploeg").Value;
                string sporthal = wedstrijd.Element("sporthal").Value;
                string poule = wedstrijd.Element("reeks").Value;
                int thuisploegId = 1000000 + int.Parse(wedstrijd.Element("thuisploeg_id").Value);
                int bezoekersploegId = 1000000 + int.Parse(wedstrijd.Element("bezoekersploeg_id").Value);
                string stamnummer_thuisclub = wedstrijd.Element("stamnummer_thuisclub").Value;
                string stamnummer_bezoekersclub = wedstrijd.Element("stamnummer_bezoekersclub").Value;
                string thuisclubname = "Unknown";
                int thuisclubId = -1;
                string bezoekersclubname = "Unknown";
                int bezoekersclubId = -1;
                Club thuisclub = clubs.Find(c => c.Stamnumber == stamnummer_thuisclub);
                if (thuisclub != null)
                {
                    thuisclubname = thuisclub.name;
                    thuisclubId = thuisclub.Id;
                }
                Club bezoekersclub = clubs.Find(c => c.Stamnumber == stamnummer_bezoekersclub);
                if (bezoekersclub != null)
                {
                    bezoekersclubname =bezoekersclub.name;
                    bezoekersclubId = bezoekersclub.Id;
                }
                DateTime date = DateTime.Parse(datum);
                sporthal = sporthal.Replace(",", " ");
                sporthal = sporthal.Replace(",", " ");
                int sporthalId = -1;
                writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
                    "Unknown", "-1", poule, "-1", thuisploeg, thuisploegId, bezoekersploeg, bezoekersploegId, sporthal, sporthalId, date.ToShortDateString(), aanvangsuur, thuisclubname, thuisclubId, bezoekersclubname, bezoekersclubId);
            }
            writer.Close();
        }
 /*     XML-based variant was not available. Was it due to website changes?
  *     public void ConvertVVBCompetitionToCSV(string filename)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(@"\vvb2014.txt");
            StreamWriter writer = new StreamWriter(filename);

            foreach (HtmlNode table in doc.DocumentNode.SelectNodes("//table"))
            {
                HtmlNode tr = table.SelectSingleNode(".//tr");
                HtmlNode td = tr.SelectSingleNode(".//td");
                string poule = td.InnerText;
                poule = poule.Substring(12);
                HtmlNodeCollection lines = table.SelectNodes(".//tr");
                for (int i = 2; i < lines.Count; i++)
                {
                    HtmlNode line = lines[i];
                    HtmlNodeCollection fields = line.SelectNodes(".//td");
                    string date = fields[2].InnerText;
                    DateTime dt = DateTime.Parse(date);
                    string aanvangsuur = fields[3].InnerText;
                    string thuisploeg = fields[4].InnerText;
                    int thuisploegId = -1;
                    string bezoekersploeg = fields[5].InnerText;
                    int bezoekersploegId = -1;
                    string sporthal = fields[6].InnerText;
                    sporthal = sporthal.Replace(",", " ");
                    int sporthalId = -1;
                    string thuisclubname = "Nationaal";
                    int thuisclubId = -1;
                    writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                        "Nationaal1", "-1", poule, "-1", thuisploeg, thuisploegId, bezoekersploeg, bezoekersploegId, sporthal, sporthalId, date, aanvangsuur, thuisclubname, thuisclubId);
                }
            }
            writer.Close();
        }
  * */
        public void ImportCSV(string fileName)
        {
            const int serieIndex = 0;
            const int serieIdIndex = 1;
            const int pouleIndex = 2;
            //const int pouleIdIndex = 3;
            const int homeTeamIndex = 4;
            const int homeTeamIdIndex = 5;
            const int visitorTeamIndex = 6;
            const int visitorTeamIdIndex = 7;
            //const int sporthallIndex = 8;
            const int sporthallIdIndex = 9;
            const int dateIndex = 10;
            const int timeIndex = 11;
            //const int homeClubIndex = 12;
            const int homeClubIdIndex = 13;
            //const int visitorClubIndex = 14;
            const int visitorClubIdIndex = 15;

            string[] lines = System.IO.File.ReadAllLines(fileName);
            List<string[]> ParameterLines = new List<string[]>();
            foreach (string line in lines)
            {
                ParameterLines.Add(line.Split(','));
            }
            foreach (string[] parameters in ParameterLines)
            {
                int SerieId = int.Parse(parameters[serieIdIndex]);
                Serie serie =  series.Find(s => s.id == SerieId);
                if (serie == null)
                {
                        System.Windows.Forms.MessageBox.Show(string.Format("Serie id '{0}' is unknown", SerieId));
                }
                int homeClubId = int.Parse(parameters[homeClubIdIndex]);
                Club homeClub = clubs.Find(s => s.Id == homeClubId);
                if (homeClub == null)
                {
                    System.Windows.Forms.MessageBox.Show(string.Format("Club id '{0}' is unknown", homeClubId));
                }
                Club visitorClub = null;
                if (parameters[visitorClubIdIndex].Length > 0)
                {
                    int visitorClubId = int.Parse(parameters[visitorClubIdIndex]);
                    visitorClub = clubs.Find(s => s.Id == visitorClubId);
                    if (visitorClub == null)
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format("Club id '{0}' is unknown", visitorClubId));
                    }
                }
                int sporthalId = int.Parse(parameters[sporthallIdIndex]);
                Sporthal sporthal = sporthalls.Find(s => s.id == sporthalId);
                if (sporthal == null)
                {
                    System.Windows.Forms.MessageBox.Show(string.Format("Sporthal id '{0}' is unknown", sporthalId));
                }
                // sporthal aan club toevoegen indien deze nog niet bestaat
                SporthallClub sporthallclub = homeClub.sporthalls.Find(sp => sp.sporthall == sporthal);
                if (sporthallclub == null)
                {
                    sporthallclub = new SporthallClub(sporthal);
                    homeClub.sporthalls.Add(sporthallclub);
                }
                // poules toevoegen indien ze niet bestaan
                Poule poule = serie.poules.Find(s => s.name == parameters[pouleIndex]);
                if (poule == null)
                {
                    // Calculate the size of the poule
                    List<string> teamsList = new List<string>();
                    List<string[]> SelectedLines = ParameterLines.FindAll(l => l[serieIndex] == parameters[serieIndex] && l[pouleIndex] == parameters[pouleIndex]);
                    foreach (string[] parameterline in SelectedLines)
                    {
                        if (teamsList.Contains(parameterline[homeTeamIndex]) == false) teamsList.Add(parameterline[homeTeamIndex]);
                        if (teamsList.Contains(parameterline[visitorTeamIndex]) == false) teamsList.Add(parameterline[visitorTeamIndex]);
                    }
                    int c = teamsList.Count;

                    poule = new Poule(parameters[pouleIndex], c, serie);
                    poules.Add(poule);
                    serie.poules.Add(poule);
                    poule.imported = true;
                }
                else
                {
                    // matches will be added
                    poule.matches.Clear();
                }
                poule.imported = true;
                if (parameters[homeTeamIndex].Length > 0)
                {
                    int teamId = int.Parse(parameters[homeTeamIdIndex]);
                    Team team = null;
                    if (teamId >= 0)
                    {
                        team = teams.Find(t => t.Id == teamId);
                    } else 
                    {
                        team = poule.teams.Find(t => t.name == parameters[homeTeamIndex]);
                    }
                    if (team == null)
                    {
                        team = new Team(teamId, parameters[homeTeamIndex], poule, serie, homeClub);
                        DateTime date = DateTime.Parse(parameters[dateIndex] + " " + parameters[timeIndex]);
                        Dictionary<DayOfWeek, int> dow = new Dictionary<DayOfWeek, int>();
                        Dictionary<Time, int> ti = new Dictionary<Time, int>();
                        foreach (string[] parameterline in ParameterLines)
                        {
                            if (parameterline[homeTeamIndex] == parameters[homeTeamIndex] &&
                                parameterline[homeTeamIdIndex] == parameters[homeTeamIdIndex])
                            {
                                DateTime d = DateTime.Parse(parameterline[dateIndex] + " " + parameterline[timeIndex]);
                                if(dow.ContainsKey(d.DayOfWeek) == false) dow.Add(d.DayOfWeek,0);
                                Time nt = new Time(d);
                                if(ti.ContainsKey(nt) == false) ti.Add(nt,0);
                                dow[d.DayOfWeek]++;
                                ti[nt]++;
                            }
                        }
                        if (dow.Count > 0)
                        {
                            team.defaultDay = dow.FirstOrDefault(x => x.Value == dow.Values.Max()).Key;
                            team.defaultTime = ti.FirstOrDefault(x => x.Value == ti.Values.Max()).Key;
                        }
                        else
                        {
                            team.defaultDay = date.DayOfWeek;
                            team.defaultTime = new Time(date);
                        }
                        teams.Add(team);
                        team.sporthal = homeClub.sporthalls[0]; // just first sporthal.
                        //serie.AddTeam(team);
                        homeClub.AddTeam(team);
                    }
                    poule.AddTeam(team);
                }
                // visitor teams (sommige competities hebben geen heen, en terug reeks
                if (parameters[visitorTeamIndex].Length > 0 && visitorClub != null)
                {
                    int teamId = int.Parse(parameters[visitorTeamIdIndex]);
                    Team team = null;
                    if (teamId >= 0)
                    {
                        team = teams.Find(t => t.Id == teamId);
                    }
                    else
                    {
                        team = poule.teams.Find(t => t.name == parameters[visitorTeamIndex]);
                    }
                    if (team == null)
                    {
                        team = new Team(teamId, parameters[visitorTeamIndex], poule, serie, visitorClub);
                        DateTime date = DateTime.Parse(parameters[dateIndex] + " " + parameters[timeIndex]);
                        Dictionary<DayOfWeek, int> dow = new Dictionary<DayOfWeek, int>();
                        Dictionary<Time, int> ti = new Dictionary<Time, int>();
                        foreach (string[] parameterline in ParameterLines)
                        {
                            if (parameterline[homeTeamIndex] == parameters[visitorTeamIndex] &&
                                parameterline[homeTeamIdIndex] == parameters[visitorTeamIdIndex])
                            {
                                DateTime d = DateTime.Parse(parameterline[dateIndex] + " " + parameterline[timeIndex]);
                                if (dow.ContainsKey(d.DayOfWeek) == false) dow.Add(d.DayOfWeek, 0);
                                Time nt = new Time(d);
                                if (ti.ContainsKey(nt) == false) ti.Add(nt, 0);
                                dow[d.DayOfWeek]++;
                                ti[nt]++;
                            }
                        }
                        if (dow.Count > 0)
                        {
                            team.defaultDay = dow.FirstOrDefault(x => x.Value == dow.Values.Max()).Key;
                            team.defaultTime = ti.FirstOrDefault(x => x.Value == ti.Values.Max()).Key;
                        }
                        else
                        {
                            team.defaultDay = date.DayOfWeek;
                            team.defaultTime = new Time(date);
                        }
                        teams.Add(team);
                        team.sporthal = visitorClub.sporthalls[0]; // just first sporthal.
                        //serie.AddTeam(team);
                        visitorClub.AddTeam(team);
                    }
                    poule.AddTeam(team);
                }
            }
            // Add matches
            foreach (string[] parameters in ParameterLines)
            {
                Serie serie = series.Find(s => s.id == int.Parse(parameters[serieIdIndex]));
                Poule poule = serie.poules.Find(s => s.name == parameters[pouleIndex]);
                if (parameters[homeTeamIndex].Length > 0 && parameters[visitorTeamIndex].Length > 0)
                {
                    Team homeTeam = null;
                    int homeTeamIndex1 = int.Parse(parameters[homeTeamIdIndex]);
                    if (homeTeamIndex1 >= 0) homeTeam = poule.teams.Find(t => t.Id == homeTeamIndex1);
                    else homeTeam = poule.BestFit(parameters[homeTeamIndex]);
                    Team visitorTeam = null;
                    int visitorTeamIndex1 = int.Parse(parameters[visitorTeamIdIndex]);
                    if (visitorTeamIndex1 >= 0) visitorTeam = poule.teams.Find(t => t.Id == visitorTeamIndex1);
                    else visitorTeam = poule.BestFit(parameters[visitorTeamIndex]);
                    if (homeTeam == null)
                        System.Windows.Forms.MessageBox.Show(string.Format("Team '{0}' is not added to the poule: {1}", parameters[homeTeamIndex],poule.fullName));
                    if (visitorTeam == null)
                        System.Windows.Forms.MessageBox.Show(string.Format("Team '{0}' is not added to the poule: {1}", parameters[visitorTeamIndex],poule.fullName));
                    DateTime dt = DateTime.Parse(parameters[dateIndex] + " " + parameters[timeIndex]);
                    poule.matches.Add(new Match(dt, homeTeam, visitorTeam, serie, poule));
                }
            }
            // create weekends in the poules
            foreach (Poule poule in poules)
            {
                bool done = false;
                poule.matches.Sort(delegate(Match m1, Match m2) { return m1.datetime.CompareTo(m2.datetime); });
                foreach (Match mat in poule.matches)
                {
                    if (mat.weekIndex < 0)
                    {
                        if (done == false)
                        {
                            poule.weekends.Sort(delegate(Weekend w1, Weekend w2) { return w1.Saturday.CompareTo(w2.Saturday); });
                            done = true;
                        }
                        Weekend we = new Weekend(mat.datetime);
                        mat.weekIndex = poule.weekends.FindIndex(w => w.Saturday == we.Saturday);
                    }
                }
            }
            RenewConstraints();
            Evaluate(null);
            Changed();
        }

        private void ImportSporthalls(XElement doc)
        {
            foreach (XElement sporthall in doc.Element("Sporthalls").Elements("Sporthall"))
            {
                int sporthallId = int.Parse(sporthall.Attribute("Id").Value.ToString());
                string sporthallName = sporthall.Attribute("Name").Value.ToString();

                Sporthal sh = sporthalls.Find(s => s.id == sporthallId);
                if (sh == null)
                {
                    sh = new Sporthal(sporthallId, sporthallName);
                    sporthalls.Add(sh);

                    XAttribute latAttr = sporthall.Attribute("Latitude");
                    XAttribute lngAttr = sporthall.Attribute("Longitude");
                    double sporthallLatitude;
                    double sporthallLongitude;
                    if (latAttr != null && lngAttr != null)
                    {
                        bool ok1 = double.TryParse(latAttr.Value.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out sporthallLatitude);
                        bool ok2 = double.TryParse(lngAttr.Value.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out sporthallLongitude);
                        if (ok1 && ok2)
                        {
                            sh.lat = sporthallLatitude;
                            sh.lng = sporthallLongitude;
                        }
                    }
                }
            }

            foreach (XElement sporthall in doc.Element("Distances").Elements("Distance"))
            {
                int Id1 = int.Parse(sporthall.Attribute("Id1").Value.ToString());
                int Id2 = int.Parse(sporthall.Attribute("Id2").Value.ToString());
                int distance = int.Parse(sporthall.Attribute("Distance").Value.ToString());
                int time = int.Parse(sporthall.Attribute("Time").Value.ToString());

                Sporthal sporthal = sporthalls.Find(s => s.id == Id1);
                if (sporthal != null) sporthal.distance.Add(Id2, distance);

            }
        }
        

        public void ImportTeamSubscriptions(XElement doc)
        {
            foreach (XElement club in doc.Elements("Club"))
            {
                int clubId = int.Parse(club.Attribute("Id").Value);
                string clubName = club.Attribute("Name").Value;
                XAttribute attrStamNumber = club.Attribute("StamNumber");
                string clubStamNumber = null;
                if (attrStamNumber != null)
                {
                    clubStamNumber = club.Attribute("StamNumber").Value;
                }
                Club cl = clubs.Find(c => c.Id == clubId);
                if (cl == null)
                {
                    cl = new Club(clubId, clubName, clubStamNumber);
                    clubs.Add(cl);
                } 
                XAttribute attr = club.Attribute("LinkedClub");
                if (attr != null)
                {
                    int groupClubId = int.Parse(attr.Value);
                    Club groupedClub = clubs.Find(c => c.Id == groupClubId);
                    if (groupedClub != null)
                    {
                        groupedClub.groupingWithClub = cl;
                        cl.groupingWithClub = groupedClub;
                    }

                }

                XElement freeformatconstraint = club.Element("FreeFormatConstraint");

                cl.FreeFormatConstraints = freeformatconstraint.Value;

                if (club.Element("Sporthalls") != null)
                {
                    foreach (var sporthal in club.Element("Sporthalls").Elements("Sporthall"))
                    {
                        var sporthallId = sporthal.Attribute("Id");
                        int id = int.Parse(sporthallId.Value);
                        if (id >= 1000000) id = -1; // TODO: need to be removed when files are converted
                        SporthallClub sp = cl.sporthalls.Find(t => t.id == id);
                        if (sp == null)
                        {
                            sp = cl.sporthalls.Find(t => t.id == id);
                            if (sp == null)
                            {
                                Sporthal sp1 = sporthalls.Find(s => s.id == id);
                                if (sp1 == null)
                                {
                                    System.Windows.Forms.MessageBox.Show(string.Format("Sporthal id {0} not known known", id));
                                }
                                sp = new SporthallClub(sp1);
                                cl.sporthalls.Add(sp);
                            }
                        }
                        //clear original dates (for re-reading the subscriptions)
                        sp.NotAvailable.Clear();
                        foreach (var date in sporthal.Element("NotAvailable").Elements("Date"))
                        {
                            DateTime dt = DateTime.Parse(date.Value.ToString());
                            if (sp.NotAvailable.Contains(dt) == false) sp.NotAvailable.Add(dt);

                        }
                        if(cl.sporthalls.Contains(sp)== false) cl.sporthalls.Add(sp);
                    }
                }

                foreach (var team in club.Element("Teams").Elements("Team"))
                {
                    var teamId = team.Attribute("Id");
                    int id = int.Parse(teamId.Value);
                    XAttribute attr1 = team.Attribute("SerieSortId");
                    if (attr1 == null) attr1 = team.Attribute("SerieId");
                    int serieId = int.Parse(attr1.Value);
                    string seriePrefix = team.Attribute("SerieName").Value;
                    Serie serie = series.Find(s => s.id == serieId);
                    if (serie == null)
                    {
                        serie = new Serie(serieId, seriePrefix,this);
                        series.Add(serie);
                    }
                    string teamName = team.Attribute("TeamName").Value;
                    // Search both on name & id, since the national Id's are not unique.
                    Team te = null;
                    if(id>=0) te= cl.teams.Find(t => t.Id == id && t.serie == serie);
                    else te = cl.teams.Find(t => t.Id == id && t.name == teamName && t.serie == serie);
                    int sporthalId = int.Parse(team.Attribute("SporthallId").Value);
                    if (sporthalId >= 1000000) sporthalId = -1;  //ToDo: remove when files are converted

                    SporthallClub sporthall = cl.sporthalls.Find(sp => sp.id == sporthalId);
                    if (sporthall == null)
                    {
                        // TODO: loopt door elkaar. Moet iets bedenken van 'onbekende sporthal'. Dat is beter gedefinieerd.
                        if (sporthall == null)
                        {
                            sporthall = new SporthallClub(new Sporthal(sporthalId, "Unknown"));
                            cl.sporthalls.Add(sporthall);
                        }
                    }
                    if (te == null)
                    {
                        te = new Team(id, teamName, null, serie, cl);
                        teams.Add(te);
                        //serie.AddTeam(te);
                                
                    }
                    else
                    {
                        // not removing poule. Used for re-reading subscriptions
                        //if (te.poule != null) te.poule.RemoveTeam(te);
                    }
                    // new team, or check on changed data (re-reading subscriptions)
                    string DayString = team.Attribute("Day").Value;
                    if (DayString == "7") DayString = "0";
                    DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), DayString);
                    Time time = new Time(DateTime.Parse(team.Attribute("StartTime").Value));
                    te.sporthal = sporthall;
                    te.defaultDay = day;
                    te.defaultTime = time;
                    string teamGroup = team.Attribute("Group").Value;
                    te.group = TeamGroups.NoGroup;
                    if (teamGroup == "A") te.group = TeamGroups.GroupX;
                    if (teamGroup == "B") te.group = TeamGroups.GroupY;
                    if (teamGroup == "X") te.group = TeamGroups.GroupX;
                    if (teamGroup == "Y") te.group = TeamGroups.GroupY;
                    XAttribute attrDel = team.Attribute("Deleted");
                    if (attrDel != null && attrDel.Value == "true") te.DeleteTeam(this); // remains only in the club administration.
                    XAttribute attrNot = team.Attribute("NotAtSameTime");
                    te.NotAtSameTime = null;
                    if (attrNot != null)
                    {
                        int idNot = int.Parse(attrNot.Value);
                        te.NotAtSameTime = cl.teams.Find(t => t.Id == idNot);
                    }
                    XAttribute attrExtraTime = team.Attribute("extraTimeBefore"); // bijvoorbeeld voor reserve wedstrijd 
                    if (attrExtraTime != null)
                    {
                        double extraTime;
                        bool ok = double.TryParse(attrExtraTime.Value.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out extraTime);
                        if (ok) serie.extraTimeBefore = extraTime;
                    }
                    attrNot = team.Attribute("FixedSchemaIndex");
                    if (attrNot != null)
                    {
                        int idFix = int.Parse(attrNot.Value);
                        te.FixedSchemaNumber = idFix;
                    }
                    attrNot = team.Attribute("EvenOdd");
                    if (attrNot != null)
                    {
                        string EvenOdd = attrNot.Value;
                        te.EvenOdd = Team.WeekendRestrictionEnum.All;
                        if (EvenOdd == "Even") te.EvenOdd = Team.WeekendRestrictionEnum.Even;
                        if (EvenOdd == "Odd") te.EvenOdd = Team.WeekendRestrictionEnum.Odd;
                    }
                    attrNot = team.Attribute("ContactEmail");
                    if (attrNot != null)
                    {
                        te.email = attrNot.Value;
                    }
                }
            }
            RenewConstraints();
            Evaluate(null);
            Changed();
        }
        public void WriteClubConstraints(string filename, bool unknown_also = true)
        {
            XmlWriter writer = XmlWriter.Create(filename);
            writer.WriteStartDocument();
            WriteClubConstraintsInt(writer,unknown_also);
            writer.WriteEndDocument();
            writer.Close();
        }
        private void WriteClubConstraintsInt(XmlWriter writer, bool unknown_also)
        {
            writer.WriteStartElement("Clubs");
            foreach (Club club in clubs)
            {
                if (unknown_also || club.Id >= 0)
                {
                    writer.WriteStartElement("Club");
                    writer.WriteAttributeString("Id", club.Id.ToString());
                    writer.WriteAttributeString("Name", club.name);
                    if(club.Stamnumber != null) writer.WriteAttributeString("StamNumber", club.Stamnumber);
                    if (club.groupingWithClub != null) writer.WriteAttributeString("LinkedClub", club.groupingWithClub.Id.ToString());
                    writer.WriteElementString("FreeFormatConstraint", club.FreeFormatConstraints);
                    writer.WriteStartElement("Sporthalls");
                    foreach (SporthallClub sporthal in club.sporthalls)
                    {
                        if (unknown_also || sporthal.id >= 0)
                        {
                            writer.WriteStartElement("Sporthall");
                            writer.WriteAttributeString("Id", sporthal.id.ToString());
                            writer.WriteAttributeString("Name", sporthal.name);
                            writer.WriteStartElement("NotAvailable");
                            foreach (DateTime date in sporthal.NotAvailable)
                            {
                                //writer.WriteStartElement("Date");
                                writer.WriteElementString("Date", date.ToShortDateString());
                                //writer.WriteEndElement();
                            }
                            writer.WriteEndElement();
                            writer.WriteEndElement();
                        }

                    }
                    writer.WriteEndElement();
                    writer.WriteStartElement("Teams");
                    foreach (Team team in club.teams)
                    {
                        if (unknown_also || team.serie.id >= 0) // Nationale teams eruit filteren?
                        {
                            string groupLetter = "0";
                            if (team.group == TeamGroups.GroupX) groupLetter = "X";
                            if (team.group == TeamGroups.GroupY) groupLetter = "Y";
                            writer.WriteStartElement("Team");
                            writer.WriteAttributeString("TeamName", team.name);
                            writer.WriteAttributeString("Id", team.Id.ToString());
                            writer.WriteAttributeString("StartTime", team.defaultTime.ToString());
                            writer.WriteAttributeString("Day", team.defaultDay.ToString());
                            writer.WriteAttributeString("SerieId", team.serie.id.ToString());
                            writer.WriteAttributeString("SerieName", team.serie.name);
                            if (team.serie.extraTimeBefore > 0.01) writer.WriteAttributeString("extraTimeBefore", team.serie.extraTimeBefore.ToString());
                            writer.WriteAttributeString("Group", groupLetter);
                            writer.WriteAttributeString("SporthallId", team.sporthal.id.ToString());
                            writer.WriteAttributeString("SporthallName", team.sporthal.name);
                            if (team.deleted) writer.WriteAttributeString("Deleted","true");
                            if (team.FixedSchema) writer.WriteAttributeString("FixedSchemaIndex", team.FixedSchemaNumber.ToString());
                            if (team.NotAtSameTime != null) writer.WriteAttributeString("NotAtSameTime", team.NotAtSameTime.Id.ToString());
                            if (team.EvenOdd != Team.WeekendRestrictionEnum.All) writer.WriteAttributeString("EvenOdd", team.EvenOdd.ToString());
                            writer.WriteEndElement();
                        }
                    }
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }
        public void WritePoules(XmlWriter writer, List<Serie> series)
        {
            writer.WriteStartElement("Series");
            foreach (Poule poule in poules)
            {
                if (series.Contains(poule.serie))
                {
                    writer.WriteStartElement("Serie");
                    writer.WriteAttributeString("Name", poule.name);
                    writer.WriteAttributeString("SerieId", poule.serie.id.ToString());
                    writer.WriteAttributeString("SerieName", poule.serie.name);
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }
        public void WriteTeams(XmlWriter writer, List<Serie> series)
        {
            writer.WriteStartElement("Teams");
            foreach (Poule poule in poules)
            {
                if (series.Contains(poule.serie))
                {
                    foreach (Team team in poule.teams)
                    {
                        if (team.RealTeam())
                        {
                            writer.WriteStartElement("Team");
                            writer.WriteAttributeString("SerieSortName", team.poule.serie.name);
                            writer.WriteAttributeString("SerieSortId", team.poule.serie.id.ToString());
                            writer.WriteAttributeString("SerieName", team.poule.name);
                            writer.WriteAttributeString("Id", team.Id.ToString());
                            writer.WriteAttributeString("Name", team.name);
                            // Not the id and the name of the clubsporthal is used, but the original (duplication of sporthalls)
                            writer.WriteAttributeString("SporthallId", team.sporthal.sporthall.id.ToString());
                            writer.WriteAttributeString("SporthallName", team.sporthal.sporthall.name);
                            writer.WriteAttributeString("ClubId", team.club.Id.ToString());
                            writer.WriteAttributeString("ClubName", team.club.name);
                            writer.WriteEndElement();
                        }
                    }
                }
            }
            writer.WriteEndElement();
        }
        public void WriteMatches(XmlWriter writer, List<Serie> series)
        {
            writer.WriteStartElement("Matches");
            foreach(Poule poule in poules)
            {
                if (series.Contains(poule.serie))
                {
                    foreach (Match match in poule.matches)
                    {
                        if (match.RealMatch())
                        {
                            writer.WriteStartElement("Match");
                            writer.WriteAttributeString("SerieName", match.poule.serie.name);
                            writer.WriteAttributeString("SerieId", match.poule.serie.id.ToString());
                            writer.WriteAttributeString("SerieName", match.poule.name);
                            writer.WriteAttributeString("homeTeamName", match.homeTeam.name);
                            writer.WriteAttributeString("homeTeamId", match.homeTeam.Id.ToString());
                            writer.WriteAttributeString("visitorTeamName", match.visitorTeam.name);
                            writer.WriteAttributeString("visitorTeamId", match.visitorTeam.Id.ToString());
                            writer.WriteAttributeString("date", match.datetime.ToShortDateString());
                            writer.WriteAttributeString("time", match.datetime.ToShortTimeString());
                            writer.WriteAttributeString("SporthallId", match.homeTeam.sporthal.id.ToString());
                            writer.WriteAttributeString("SporthallName", match.homeTeam.sporthal.name);
                            writer.WriteEndElement();
                        }
                    }
                }
            }
        }
        public void WriteStatistics(string filename)
        {
            StreamWriter writer = new StreamWriter(filename);
            writer.WriteLine("Poules:");
            writer.WriteLine("Poule, Matches, Conflicts, Percentage");
            foreach (Poule poule in poules)
            {
                if (poule.imported == false)
                {
                    if (poule.matches.Count > 0)
                    {
                        decimal perc = poule.conflict;
                        perc /= poule.matches.Count;
                        perc *= 100;
                        writer.WriteLine("{0},{1},{2},{3}", poule.fullName, poule.matches.Count, poule.conflict, perc.ToString("0.0", CultureInfo.InvariantCulture));
                    }
                }
            }
            writer.WriteLine("Clubs:");
            writer.WriteLine("Club, Matches, Conflicts, Percentage");
            foreach (Club club in clubs)
            {
                double count = 0;
                List<Poule> poules1 = new List<Poule>();
                foreach (Team t in club.teams)
                {
                    if (t.poule != null && poules1.Contains(t.poule) == false)
                    {
                        poules1.Add(t.poule);
                    }
                }
                foreach (Poule poule in poules1)
                {
                    if (poule.imported == false)
                    {
                        count += poule.matches.Count(m => m.homeTeam.club == club); ;
                    }
                }
                if (count > 0)
                {
                    double perc = club.conflict;
                    perc /= count;
                    perc *= 100;
                    writer.WriteLine("{0},{1},{2},{3}", club.name, count, club.conflict, perc.ToString("00.0", CultureInfo.InvariantCulture));
                }
            }
            writer.Close();
        }
        public void WriteMatches(string filename, List<Serie> series)
        {
            StreamWriter writer = new StreamWriter(filename);
            writer.WriteLine("Serie,Poule,Speeldag,datum,Speeltijd,homeClub,homeTeam,visitorClub,visitorTeam");
            foreach (Poule poule in poules)
            {
                if (series.Contains(poule.serie))
                {
                    foreach (Match match in poule.matches)
                    {
                        if (match.RealMatch())
                        {
                            writer.Write("{0},{1},{2},{3},{4},{5},{6},{7},{8}", poule.serie.name, poule.fullName, match.datetime.ToShortDateString(), match.DayString, match.Time.ToString(), match.homeTeam.club.name, match.homeTeam.name, match.visitorTeam.club.name, match.visitorTeam.name);

                            foreach (Constraint con in match.conflictConstraints)
                            {
                                writer.Write("," + con.name);
                            }
                            writer.WriteLine();
                        }
                    }
                }
            }
            writer.Close();

        }
        public void WriteSeriePoules(string filename)
        {
            StreamWriter writer = new StreamWriter(filename);
            writer.WriteLine("Serie,Poule,Poule-team-number,Team,Team-Id,Club,Club-Id,Speeldag,Speeltijd,Groep");
            foreach (Team team in teams)
            {
                if (team.club.Id >= 0)
                {
                    string pouleName = "-";
                    if (team.poule != null)
                    {
                        pouleName = team.poule.name;
                    }

                    writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", team.serie.name, pouleName, team.Index, team.name,team.Id, team.club.name, team.club.Id, team.defaultDay.ToString(), team.defaultTime.ToString(), team.group.ToString());
                }
            }
            writer.Close();
        }
        public void WriteExportToKLVVXml(string filename, List<Serie> series)
        {
            XmlWriter writer = XmlWriter.Create(filename);
            writer.WriteStartDocument();
            writer.WriteStartElement("Competition");
            WritePoules(writer,series);
            WriteTeams(writer,series);
            WriteMatches(writer,series);
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }
        public void WriteConflictReportPerType(string filename)
        {
            StreamWriter writer = new StreamWriter(filename);
            string name = "";
            string title = "";
            constraints.Sort(delegate(Constraint c1, Constraint c2) { return c1.Title.CompareTo(c2.Title); });
            foreach (Constraint constraint in constraints)
            {
                if (constraint.conflict_cost > 0)
                {
                    if (constraint.name != name)
                    {
                        name = constraint.name;
                        writer.WriteLine("{0}", name);
                    }
                    if (constraint.Title != title)
                    {
                        title = constraint.Title;
                        writer.WriteLine("{0}", title);
                    }
                    constraint.Sort();
                    foreach (Match match in constraint.conflictMatches)
                    {
                        writer.WriteLine("{0},{1},{2},{3},{4}", match.datetime, match.poule.fullName, match.homeTeam.name, match.visitorTeam.name, match.homeTeam.group.ToString());
                    }
                }
            }
            writer.Close();

        }

        public void WriteConflictReportPerClub(string filename)
        {
            StreamWriter writer = new StreamWriter(filename);
            string name = "";
            string title = "";
            foreach (Club club in clubs)
            {
                if (club.conflict_cost > 0)
                {
                    writer.WriteLine("{0}", club.name);
                    club.conflictConstraints.Sort(delegate(Constraint c1, Constraint c2) { return c1.Title.CompareTo(c2.Title); });
                    foreach (Constraint constraint in club.conflictConstraints)
                    {
                        if (constraint.conflict_cost > 0)
                        {
                            if (constraint.name != name)
                            {
                                name = constraint.name;
                                writer.WriteLine(" {0}", name);
                            }
                            if (constraint.Title != title)
                            {
                                title = constraint.Title;
                                writer.WriteLine("  - {0}", title);
                            }
                            constraint.Sort();
                            foreach (Match match in constraint.conflictMatches)
                            {
                                writer.WriteLine("{0},{1},{2},{3},{4}", match.datetime, match.poule.fullName, match.homeTeam.name, match.visitorTeam.name, match.homeTeam.group.ToString());
                            }
                        }
                    }
                }
            }
            writer.Close();
        }
        public void WriteCompetitionPerClub(string filename)
        {
            StreamWriter writer = new StreamWriter(filename);
            foreach (Club club in clubs)
            {
                List<Match> matches = new List<Match>();
                foreach (Team team in club.teams)
                {
                    if (team.poule != null)
                    {
                        foreach (Match match in team.poule.matches)
                        {
                            if (match.homeTeam.club == club)
                            {
                                if (matches.Contains(match) == false) matches.Add(match);
                            }
                        }
                    }
                }

                writer.WriteLine("{0}", club.name);
                matches.Sort(delegate(Match m1, Match m2) { return m1.datetime.CompareTo(m2.datetime); });
                foreach (Match match in matches)
                {
                    if (match.RealMatch())
                    {
                        string constraint = " - ";
                        if (match.conflictConstraints.Count > 0) constraint = " * ";
                        string conflicts = "";
                        foreach (Constraint constr in match.conflictConstraints)
                        {
                            conflicts += constr.name + ",";
                        }
                        writer.WriteLine("{5},{0},{1},{2},{3},{4},{7},{6}", match.datetime, match.poule.fullName, match.homeTeam.name, match.visitorTeam.name, match.homeTeam.group.ToString(), constraint, conflicts, match.homeTeam.sporthal.name.Replace(",", " "));
                    }
                }
            }
            writer.Close();
        }
        public void WriteProject(string fileName)
        {
            XmlWriter writer = XmlWriter.Create(fileName);
            writer.WriteStartDocument();
            writer.WriteStartElement("Competition");
            writer.WriteStartElement("Settings");
            writer.WriteElementString("Year", year.ToString());
            writer.WriteEndElement();
            WriteClubConstraintsInt(writer,true);
            writer.WriteStartElement("Poules");
            foreach (Poule poule in poules)
            {
                writer.WriteStartElement("Poule");
                writer.WriteAttributeString("Name", poule.name);
                writer.WriteAttributeString("SerieId", poule.serie.id.ToString());
                writer.WriteAttributeString("SerieName", poule.serie.name);
                if (poule.imported)
                {
                    writer.WriteAttributeString("SerieImported", "true");
                    writer.WriteAttributeString("Imported", "true");
                }
                writer.WriteAttributeString("MaxTeams", poule.maxTeams.ToString());
                writer.WriteStartElement("Teams");
                foreach (Team team in poule.teams)
                {
                    writer.WriteStartElement("Team");
                    writer.WriteAttributeString("TeamName", team.name);
                    writer.WriteAttributeString("Id", team.Id.ToString());
                    writer.WriteEndElement();

                }
                writer.WriteEndElement();
                writer.WriteStartElement("Weekends");
                foreach (Weekend weekend in poule.weekends)
                {
                    writer.WriteStartElement("Weekend");
                    writer.WriteAttributeString("Date", weekend.Saturday.Date.ToShortDateString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteStartElement("Matches");
                foreach (Match match in poule.matches)
                {
                    writer.WriteStartElement("Match");
                    writer.WriteAttributeString("homeTeam", match.homeTeamIndex.ToString());
                    writer.WriteAttributeString("visitorTeam", match.visitorTeamIndex.ToString());
                    writer.WriteAttributeString("weekend", match.weekIndex.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteStartElement("TeamConstraints");
            foreach (Constraint c in constraints)
            {
                TeamConstraint con = c as TeamConstraint;
                if (con != null)
                {
                    writer.WriteStartElement("Constraint");
                    writer.WriteAttributeString("TeamId", con.team.Id.ToString());
                    writer.WriteAttributeString("Date", con.date.ToShortDateString());
                    writer.WriteAttributeString("What", con.homeVisitNone.ToString());
                    writer.WriteAttributeString("Cost", con.cost.ToString());
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
            stateNotSaved = false;
            savedFileName = fileName;
            Properties.Settings.Default.LastOpenedProject = savedFileName;
            Properties.Settings.Default.Save();
            Changed();
        }

        public void KLVVAddRanking(IProgress intf)
        {
            string url3 = "http://klvv.be/server/series.php?season=2014-2015&trophy=false";
            var json3 = wc.DownloadString(url3);
            JArray Sorts = JArray.Parse(json3);
            int i = 0;
            int total = Sorts.Count;
            foreach (JObject sort in Sorts)
            {
                foreach (JObject serieSorts in sort["serieSorts"])
                {
                    foreach (JObject serie in serieSorts["series"])
                    {
                        int serieId = int.Parse(serie["id"].ToString());
                        string letter = serie["letter"].ToString();
                        if (letter.Length == 1)
                        {

                            string url4 = "http://klvv.be/server/seriedata.php?serieId=" + serieId;
                            var json4 = wc.DownloadString(url4);
                            JObject serieData = JObject.Parse(json4);
                            int ranking = 1;
                            JObject rankings = serieData["rankings"].First as JObject;
                            //foreach (JObject rankings in serieData["rankings"])
                            {
                                foreach (JObject rankingRows in rankings["rankingRows"])
                                {
                                    int teamId = int.Parse(rankingRows["registrationId"].ToString());
                                    int score = int.Parse(rankingRows["score"].ToString());
                                    foreach (Team t in teams)
                                    {
                                        if (t.Id == teamId)
                                        {
                                            t.Ranking = ranking.ToString("D2") + "." + (99 - score).ToString();
                                        }
                                    }
                                    ranking++;
                                }
                            }
                        }
                        if (intf.Cancelled()) return;
                    }
                }
                intf.Progress(++i, total);
                        
            }
        }
        private void ImportPoules(XElement poulesElement)
        {
            foreach (XElement poule in poulesElement.Elements("Poule"))
            {
                string PouleName = poule.Attribute("Name").Value;
                string SerieName = poule.Attribute("SerieName").Value;
                XAttribute attr1 = poule.Attribute("SerieSortId");
                if (attr1 == null) attr1 = poule.Attribute("SerieId");
                int SerieId = int.Parse(attr1.Value);
                Serie serie = series.Find(s => s.id == SerieId);
                if (serie == null)
                {
                    serie = new Serie(SerieId, SerieName, this);
                    series.Add(serie);
                }
                XAttribute imported = poule.Attribute("SerieImported");
                if(imported == null) imported = poule.Attribute("SerieSortImported");
                Poule po = poules.Find(p => p.serie == serie && p.name == PouleName);
                XAttribute attr = poule.Attribute("MaxTeams");
                int maxTeams = int.Parse(attr.Value);
                if (maxTeams == 0) maxTeams = 14;
                if (po == null)
                {
                    po = new Poule(PouleName, maxTeams, serie);
                    serie.poules.Add(po);
                    poules.Add(po);
                }

                if (SerieName == "Nationaal") po.imported = true; // TODO: temporarily, to import old files
                if (imported != null) po.imported = true;

                int index = 0;
                foreach (XElement team in poule.Element("Teams").Elements("Team"))
                {
                    int teamId = int.Parse(team.Attribute("Id").Value);
                    string teamName = team.Attribute("TeamName").Value;
                    Team te;
                    if (teamName == "----")
                    {
                        te = Team.CreateNullTeam(null, serie);
                    }
                    else
                    {
                        te = teams.Find(t => t.Id == teamId && t.name == teamName); // TODO: remove condition name == teamName. Now still required for national teams that are not numbered uniquely
                    }
                    if (te.poule != null) // gevonden team hoort behoort bij een andere poule
                    {
                        Team t = new Team(-1, te.name, null, serie, te.club);
                        t.defaultDay = te.defaultDay;
                        t.defaultTime = te.defaultTime;
                        t.group = te.group;
                        t.sporthal = te.sporthal;
                        //serie.AddTeam(t);
                        te = t;
                    }
                    po.AddTeam(te, index); //must at fixed position
                    index++;
                }
                foreach (XElement weekend in poule.Element("Weekends").Elements("Weekend"))
                {
                    DateTime date = DateTime.Parse(weekend.Attribute("Date").Value);
                    po.weekends.Add(new Weekend(date));
                }
                foreach (XElement match in poule.Element("Matches").Elements("Match"))
                {
                    int weekIndex = int.Parse(match.Attribute("weekend").Value);
                    int homeTeam = int.Parse(match.Attribute("homeTeam").Value);
                    int visitorTeam = int.Parse(match.Attribute("visitorTeam").Value);
                    po.matches.Add(new Match(weekIndex, homeTeam, visitorTeam, serie, po));
                }
            }
        }
        private void ImportTeamConstraints(XElement teamConstraints)
        {
            if (teamConstraints != null)
            {
                foreach (XElement con in teamConstraints.Elements("Constraint"))
                {
                    int teamId = int.Parse(con.Attribute("TeamId").Value);
                    DateTime date = DateTime.Parse(con.Attribute("Date").Value);
                    TeamConstraint.HomeVisitNone what = (TeamConstraint.HomeVisitNone)Enum.Parse(typeof(TeamConstraint.HomeVisitNone), con.Attribute("What").Value);
                    Team team = teams.Find(t => t.Id == teamId);
                    TeamConstraint tc = new TeamConstraint(team);
                    tc.homeVisitNone = what;
                    tc.date = date;
                    tc.cost = int.Parse(con.Attribute("Cost").Value);
                    this.constraints.Add(tc);
                }
            }
        }
        public Klvv LoadFullCompetitionIntern(string filename)
        {
            XDocument doc = XDocument.Load(filename);
            XElement competition = doc.Element("Competition");
            XElement settings = competition.Element("Settings");
            int year = int.Parse(settings.Element("Year").Value);
            Klvv klvvnew = new Klvv(year);
            klvvnew.ImportTeamSubscriptions(competition.Element("Clubs"));
            klvvnew.ImportPoules(competition.Element("Poules"));
            klvvnew.ImportTeamConstraints(competition.Element("TeamConstraints"));
            klvvnew.savedFileName = filename;
            klvvnew.RenewConstraints();
            return klvvnew;
        }
        public void LoadFullCompetition(string filename)
        {
            Klvv klvvnew = LoadFullCompetitionIntern(filename);
            Properties.Settings.Default.LastOpenedProject = filename;
            Properties.Settings.Default.Save();
            Changed();
            // klvvnew is distributed to the views
            Changed(klvvnew);
            klvvnew.Evaluate(null);
            klvvnew.Changed();
        }

        public void CompareRegistrations(string openFileName, string saveFileName)
        {
            StreamWriter writer = new StreamWriter(saveFileName);
            writer.WriteLine("Club,Serie,Team-Id,Team,Dag,Tijd,Groep,Commentaar");
            Klvv klvvOld = LoadFullCompetitionIntern(openFileName);
            // start comparing both projects
            foreach (Club club in clubs)
            {
                Club clubOld = klvvOld.clubs.Find(c => c.Id == club.Id);
                if (clubOld != null)
                {
                    if(club.name != "Nationaal") CompareClub(writer, klvvOld, club, clubOld);
                }
                else
                {

                }
            }
            writer.Close();
        }
        public void CompareClub(StreamWriter writer, Klvv klvvOld, Club club, Club clubOld)
        {
            foreach (Team team in club.teams)
            {
                Team teamOld = clubOld.teams.Find(t => t.Id == team.Id);
                WriteTeamDifference(writer, team, teamOld);
            }
            foreach (Team teamOld in clubOld.teams)
            {
                Team team = club.teams.Find(t => t.Id == teamOld.Id);
                if (team == null)
                {
                    WriteTeamDifference(writer, team, teamOld);
                }
            }
        }
        public void WriteTeamDifference(StreamWriter writer, Team team, Team teamOld)
        {
            string commentaar = "";
            Team t = null;
            if (team != null)
            {
                t = team;
                if (teamOld == null)
                {
                    commentaar = "Nieuw ingeschreven";
                }
                else
                {
                    List<string> changes = new List<string>();
                    if (team.group != teamOld.group)
                    {
                        changes.Add(string.Format("'{0}'  ->  '{1}'", teamOld.group.ToStringCustom(), team.group.ToStringCustom()));
                    }
                    if (team.defaultDay != teamOld.defaultDay)
                    {
                        changes.Add(string.Format("'{0}'  ->  '{1}'", teamOld.defaultDay.ToString(), team.defaultDay.ToString()));
                    }
                    if (team.defaultTime != teamOld.defaultTime)
                    {
                        changes.Add(string.Format("'{0}'  ->  '{1}'", teamOld.defaultTime.ToString(), team.defaultTime.ToString()));
                    }
                    if (changes.Count > 0)
                    {
                        commentaar = "Veranderde gegevens:";
                        bool first = true;
                        foreach (string change in changes)
                        {
                            if (first == false) commentaar += "  /  ";
                            commentaar += change;
                            first = false;
                        }
                    }
                }
            }
            else
            {
                t = teamOld;
                commentaar = "Forfait/uitgeschreven";
            }
            writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7}", t.club.name, t.serie.name, t.Id, t.name, t.defaultDay, t.defaultTime, t.group.ToStringCustom(), commentaar);
        }
    }
}
