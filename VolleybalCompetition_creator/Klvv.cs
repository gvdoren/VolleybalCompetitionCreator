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
        WebClient wc = null;
        public bool stateNotSaved = false;
        public void MakeDirty() { stateNotSaved = true; }
        public string savedFileName = "Competition.xml";
        public int year;
        public List<Club> clubs;
        public List<Serie> series = new List<Serie>();
        public List<Poule> poules = new List<Poule>();
        public List<Team> teams = new List<Team>();
        public List<TeamConstraint> teamConstraints = new List<TeamConstraint>(); 
        public void AddTeam(Team team)
        {
            teams.Add(team);
            team.serie.AddTeam(team);
            team.club.AddTeam(team);
        }
        public void RemoveTeam(Team team)
        {
            // Remove constraints related to the team
            List<TeamConstraint> tobedeleted = new List<TeamConstraint>();
            foreach (TeamConstraint con in teamConstraints)
            {
                if (con.team == team) tobedeleted.Add(con);
            }
            foreach (TeamConstraint con in tobedeleted)
            {
                teamConstraints.Remove(con);
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
            lock (this) ;
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
        public void EvaluateRelatedConstraints(Poule p)
        {
            lock (this) ;
            foreach (Club cl in clubs)
            {
                cl.conflict = 0;
            } 
            foreach (Team te in teams)
            {
                te.conflict = 0;
            }
            foreach (Constraint constraint in p.relatedConstraints)
            //foreach (Constraint constraint in constraints)
            {
                constraint.Evaluate(this);
            }
        }
        public int TotalConflicts()
        {
            lock (this) ;
            int conflicts = 0;
            foreach (Constraint constraint in constraints)
            {
                conflicts += constraint.conflict_cost;
            }
            return conflicts;
        }
        public Klvv(int year)
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            HttpWebRequest.DefaultWebProxy = null;
            WebRequest.DefaultWebProxy = null;
            wc = new WebClient();
            wc.Proxy = null;
            this.year = year;
            this.annorama = new Anorama(year);
            //System.Windows.Forms.MessageBox.Show("KLVV initialized" + System.Windows.Forms.Application.StartupPath);
            Serie nationaalSerie = new Serie(-1, "Nationaal");
            nationaalSerie.Nationaal = true;
            nationaalSerie.optimizable = false;
            series.Add(nationaalSerie);

            // Read series
            //System.Windows.Forms.MessageBox.Show("KLVV reading series");
            string classes_json = wc.DownloadString("http://klvv.be/server/seriesorts.php?trophy=false");
            //string classes_json = File.ReadAllText(System.Windows.Forms.Application.StartupPath + @"/InputData/series.json"); // local copy
            JArray classesObjects = JArray.Parse(classes_json);
            char[] delimiters = new char[] { ':' };
            foreach (JObject classesObject in classesObjects)
            {
                string name = classesObject["name"].ToString();
                string[] parts = name.Split(delimiters, 2);
                Serie newSerie = new Serie(int.Parse(classesObject["id"].ToString()),parts[0]);
                series.Add(newSerie);
            }
            // read sporthalls
            //System.Windows.Forms.MessageBox.Show("KLVV reading sporthalls");
            string sporthalls_json = wc.DownloadString("http://klvv.be/server/sporthalls.php");
            //string sporthalls_json = File.ReadAllText(System.Windows.Forms.Application.StartupPath + @"/InputData/sporthalls.json"); // local copy
            JArray sporthallObjects = JArray.Parse(sporthalls_json);
            foreach (JObject sporthallObject in sporthallObjects)
            {
                Sporthal newSporthal = new Sporthal(int.Parse(sporthallObject["id"].ToString()),sporthallObject["name"].ToString());
                sporthalls.Add(newSporthal);
                string location = sporthallObject["googleLocation"].ToString();
                /*  Lat & lng data op klvv niet volledig correct (of niet goed parse-baar)
                 * Regex reg = new Regex(@"sll=([0-9\.]+),([0-9\.]+)");
                if (reg.IsMatch(location))
                {
                    MatchCollection matches = reg.Matches(location);
                    string first = matches[0].Groups[1].ToString();
                    string second = matches[0].Groups[2].ToString();
                    newSporthal.lat = double.Parse(first, CultureInfo.InvariantCulture);
                    newSporthal.lng = double.Parse(second, CultureInfo.InvariantCulture);
                }
                 * */
                
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
            // Read clubs
            //var json = wc.DownloadString("http://klvv.be/server/clubs.php");
            string json = File.ReadAllText(System.Windows.Forms.Application.StartupPath + @"/InputData/clubs.json"); // local copy
            clubs = JsonConvert.DeserializeObject<List<Club>>(json);


            /*
                * ImportKLVVCompetition();
                * ImportTeamSubscriptions();
                * ImportVVBCompetition();
                * RenewConstraints();
                * */


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
            lock (this) ;
            constraints.Clear();
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
            constraints.Add(new ConstraintSpecialTeamRequirement(this));
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
        public void ImportVVBCompetition()
        {
            Club nationaal = clubs.Find(c => c.Id == -1 && c.name == "Nationaal");
            if (nationaal == null)
            {
                nationaal = new Club();
                nationaal.name = "Nationaal";
                nationaal.Id = -1;
                clubs.Add(nationaal);
            }
            Serie nationaalSerie = series.Find(s => s.id == -1);
            using (XmlReader reader = XmlReader.Create("InputData\\VVBWedstrijden.xml"))
            {
                reader.ReadStartElement("kalender");
                while (reader.Name == "wedstrijd")
                {
                    XElement wedstrijd = (XElement)XNode.ReadFrom(reader);
                    string reeks = wedstrijd.Element("reeks").Value;
                    DateTime dt = DateTime.Parse(wedstrijd.Element("datum").Value);
                    string aanvangsuur = wedstrijd.Element("aanvangsuur").Value;
                    string thuisploeg = wedstrijd.Element("thuisploeg").Value;
                    string bezoekersploeg = wedstrijd.Element("bezoekersploeg").Value;
                    string sporthal = wedstrijd.Element("sporthal").Value;
                    string clubname = thuisploeg;
                    // verwijder A, B, C in naam ploeg
                    if (thuisploeg.EndsWith(" A") ||
                       thuisploeg.EndsWith(" B") ||
                       thuisploeg.EndsWith(" C") ||
                        thuisploeg.EndsWith(" D"))
                    {
                        clubname = thuisploeg.Substring(0, thuisploeg.Length - 2);
                    }
                    Club club = clubs.Find(cl => cl.name.ToLower() == clubname.ToLower());
                    if (club == null)
                    {
                        club = nationaal;
                    }
                    SporthallClub sporth = club.sporthalls.Find(sp => sp.name.ToLower() == sporthal.ToLower());
                    if (sporth == null)
                    {
                        sporth = new SporthallClub(new Sporthal(-1, sporthal));
                        club.sporthalls.Add(sporth);
                    }
                    Poule poule = null;
                    if (nationaalSerie.poules.ContainsKey(reeks) == true)
                    {
                        poule = nationaalSerie.poules[reeks];
                    }
                    else
                    {
                        poule = new Poule(reeks, 14, nationaalSerie);
                        nationaalSerie.poules.Add(reeks, poule);
                        poule.serie = nationaalSerie;
                        poules.Add(poule);
                    }
                    poule.serie.evaluated = true;
                    Team homeTeam = teams.Find(t => t.Id == -1 && t.name == thuisploeg && t.serie == nationaalSerie);
                    if (homeTeam == null)
                    {
                        homeTeam = new Team(-1, thuisploeg, poule, poule.serie);
                        homeTeam.sporthal = sporth;
                        teams.Add(homeTeam);
                    }
                    poule.AddTeam(homeTeam);
                    if (homeTeam.sporthal == null)
                    {
                        if (homeTeam.club != null && homeTeam.club.sporthalls.Count > 0)
                        {
                            homeTeam.sporthal = homeTeam.club.sporthalls[0];
                        }
                    }
                    

                    nationaalSerie.AddTeam(homeTeam);
                    if (homeTeam.club == null)
                    {
                        club.AddTeam(homeTeam);
                    }
                    if (homeTeam.defaultTime == null)
                    {
                        int hour = int.Parse(aanvangsuur.Substring(0, 2));
                        int minute = int.Parse(aanvangsuur.Substring(aanvangsuur.Length - 2, 2));
                        homeTeam.defaultTime = new Time(hour, minute);
                    }
                    if (homeTeam.defaultDay == DayOfWeek.Monday) homeTeam.defaultDay = dt.DayOfWeek;
                    
                    string clubnamevisitor = bezoekersploeg;
                    // verwijder A, B, C in naam ploeg
                    if (bezoekersploeg.EndsWith(" A") ||
                       bezoekersploeg.EndsWith(" B") ||
                       bezoekersploeg.EndsWith(" C") ||
                        bezoekersploeg.EndsWith(" D"))
                    {
                        clubnamevisitor = thuisploeg.Substring(0, thuisploeg.Length - 2);
                    }

                    Team visitorTeam = teams.Find(t => t.Id == -1 && t.name == bezoekersploeg && t.serie == nationaalSerie);
                    if (visitorTeam == null)
                    {
                        visitorTeam = new Team(-1, bezoekersploeg, poule, poule.serie);
                        int hour = int.Parse(aanvangsuur.Substring(0, 2));
                        int minute = int.Parse(aanvangsuur.Substring(aanvangsuur.Length - 2, 2));
                        teams.Add(visitorTeam);
                    }
                    poule.AddTeam(visitorTeam);
                    
                    Match match = new Match(dt, homeTeam, visitorTeam, nationaalSerie, poule);
                    poule.matches.Add(match);
                }
                reader.ReadEndElement();
            }
            foreach (Poule po in nationaalSerie.poules.Values)
            {
                foreach (Match match in po.matches)
                {
                    //match.SetWeekIndex();
                }
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
            StreamWriter writer = new StreamWriter(filename);

            Dictionary<int, int> registrationMapping = new Dictionary<int, int>();
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
                            registrationMapping.Add(teamId, registrationId);
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
                                writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}, ,",
                                     serieName, serieId, pouleName, "-1", homeTeamName, homeTeamId, visitorTeamName, visitorTeamId, sporthal, sporthalId, date, aanvangsuur, thuisclubname, thuisclubId);
 
                            }
                        }
                    }
                }
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
                DateTime date = DateTime.Parse(datum);
                sporthal = sporthal.Replace(",", " ");
                int thuisploegId = -1;
                int bezoekersploegId = -1;
                sporthal = sporthal.Replace(",", " ");
                int sporthalId = -1;
                string thuisclubname = "Nationaal";
                int thuisclubId = -1;
                string uitclubname = "Nationaal";
                int uitclubId = -1;
                writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
                    "Nationaal", "-1", poule, "-1", thuisploeg, thuisploegId, bezoekersploeg, bezoekersploegId, sporthal, sporthalId, date.ToShortDateString(), aanvangsuur, thuisclubname, thuisclubId, uitclubname, uitclubId);
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
            const int pouleIdIndex = 3;
            const int homeTeamIndex = 4;
            const int homeTeamIdIndex = 5;
            const int visitorTeamIndex = 6;
            const int visitorTeamIdIndex = 7;
            const int sporthallIndex = 8;
            const int sporthallIdIndex = 9;
            const int dateIndex = 10;
            const int timeIndex = 11;
            const int homeClubIndex = 12;
            const int homeClubIdIndex = 13;
            const int visitorClubIndex = 14;
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
                Serie serie = null;
                // reeksen toevoegen indien ze niet bestaan
                if (SerieId >= 0) serie = series.Find(s => s.id == SerieId);
                if (serie == null) serie = series.Find(s => s.name == parameters[serieIndex]);
                if (serie == null)
                {
                    if (SerieId < 0)
                    {
                        // create unique id
                        if (series.Count > 0)
                        {
                            SerieId = Math.Max(1000000, 1 + series.Max(delegate(Serie s) { return s.id; }));
                        }
                        else
                        {
                            SerieId = 1000000;
                        }
                    }
                    serie = new Serie(SerieId, parameters[serieIndex]);
                    serie.optimizable = false;
                    series.Add(serie);
                }
                // poules toevoegen indien ze niet bestaan
                List<Poule> poules1 = serie.poules.Values.ToList();
                Poule poule = poules1.Find(s => s.name == parameters[pouleIndex]);
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
                    serie.poules.Add(poule.name, poule);
                    poule.imported = true;
                }
                else
                {
                    // matches will be added
                    poule.matches.Clear();
                }
                poule.imported = true;
                // clubs toevoegen indien ze niet bestaan
                Club homeClub = null;
                if (parameters[homeClubIdIndex].Length > 0)
                {
                    int homeClubId = int.Parse(parameters[homeClubIdIndex]);
                    if (homeClubId >= 0) homeClub = clubs.Find(s => s.Id == homeClubId);
                    if (homeClub == null) homeClub = clubs.Find(c => c.name == parameters[homeClubIndex]);
                    if (homeClub == null)
                    {
                        if (homeClubId < 0)
                        {
                            // create unique id
                            if (clubs.Count > 0)
                            {
                                homeClubId = Math.Max(1000000, 1 + clubs.Max(delegate(Club c) { return c.Id; }));
                            }
                            else
                            {
                                homeClubId = 1000000;
                            }
                        }
                        homeClub = new Club();
                        homeClub.name = parameters[homeClubIndex];
                        homeClub.Id = homeClubId;
                        clubs.Add(homeClub);

                    }
                }
                Club visitorClub = null;
                if (parameters[visitorClubIdIndex].Length > 0)
                {
                    int visitorClubId = int.Parse(parameters[visitorClubIdIndex]);
                    if (visitorClubId >= 0) visitorClub = clubs.Find(s => s.Id == visitorClubId);
                    if (visitorClub == null) visitorClub = clubs.Find(c => c.name == parameters[visitorClubIndex]);
                    if (visitorClub == null)
                    {
                        if (visitorClubId < 0)
                        {
                            // create unique id
                            if (clubs.Count > 0)
                            {
                                visitorClubId = Math.Max(1000000, 1 + clubs.Max(delegate(Club c) { return c.Id; }));
                            }
                            else
                            {
                                visitorClubId = 1000000;
                            }
                        }
                        visitorClub = new Club();
                        visitorClub.name = parameters[homeClubIndex];
                        visitorClub.Id = visitorClubId;
                        clubs.Add(visitorClub);

                    }
                }
                int sporthalId = int.Parse(parameters[sporthallIdIndex]);
                Sporthal sporthal = null;
                if (sporthalId >= 0) sporthal = sporthalls.Find(s => s.id == sporthalId);
                if (sporthal == null) sporthal = sporthalls.Find(s => s.name == parameters[sporthallIndex]);
                // sporthalls toevoegen indien ze niet bestaan
                if (sporthal == null)
                {
                    if (sporthalId < 0)
                    {
                        // create unique id
                        if (sporthalls.Count > 0)
                        {
                            sporthalId = Math.Max(1000000, 1 + sporthalls.Max(delegate(Sporthal s) { return s.id; }));
                        }
                        else
                        {
                            sporthalId = 1000000;
                        }
                    }
                    sporthal = new Sporthal(sporthalId,parameters[sporthallIndex]);
                    sporthalls.Add(sporthal);
                }
                // sporthal aan club toevoegen indien deze nog niet bestaat
                SporthallClub sporthallclub = homeClub.sporthalls.Find(sp => sp.sporthall == sporthal);
                if (sporthallclub == null)
                {
                    sporthallclub = new SporthallClub(sporthal);
                    homeClub.sporthalls.Add(sporthallclub);
                }
                if (parameters[homeTeamIndex].Length > 0)
                {
                    int teamId = int.Parse(parameters[homeTeamIdIndex]);
                    Team team = null;
                    if (teamId >= 0) team = teams.Find(t => t.Id == teamId);
                    if (team == null) team = poule.teams.Find(t => t.name == parameters[homeTeamIndex]);
                    if (team == null)
                    {
                        if (teamId < 0)
                        {
                            // create unique id
                            if (teams.Count > 0)
                            {
                                teamId = Math.Max(1000000, 1 + teams.Max(delegate(Team t) { return t.Id; }));
                            }
                            else
                            {
                                teamId = 1000000;
                            }
                        }
                        team = new Team(teamId, parameters[homeTeamIndex], poule, serie);
                        DateTime date = DateTime.Parse(parameters[dateIndex] + " " + parameters[timeIndex]);
                        team.defaultDay = date.DayOfWeek;
                        team.defaultTime = new Time(date);
                        teams.Add(team);
                        team.sporthal = sporthallclub;
                        poule.AddTeam(team);
                        serie.AddTeam(team);
                    }
                    if(team.club == null) homeClub.AddTeam(team);
                }
                // visitor teams (sommige competities hebben geen heen, en terug reeks
                if (parameters[visitorTeamIndex].Length > 0)
                {
                    int teamId = int.Parse(parameters[visitorTeamIdIndex]);
                    Team team = null;
                    if (teamId >= 0) team = teams.Find(t => t.Id == teamId);
                    if (team == null) team = poule.teams.Find(t => t.name == parameters[visitorTeamIndex]);
                    if (team == null)
                    {
                        if (teamId < 0)
                        {
                            // create unique id
                            if (teams.Count > 0)
                            {
                                teamId = Math.Max(1000000, 1 + teams.Max(delegate(Team t) { return t.Id; }));
                            }
                            else
                            {
                                teamId = 1000000;
                            }
                        }
                        team = new Team(teamId, parameters[visitorTeamIndex], poule, serie);
                        DateTime date = DateTime.Parse(parameters[dateIndex] + " " + parameters[timeIndex]);
                        team.defaultDay = date.DayOfWeek;
                        team.defaultTime = new Time(date);
                        teams.Add(team);
                        team.sporthal = sporthallclub;
                        poule.AddTeam(team);
                        serie.AddTeam(team);
                    }
                    if(team.club == null && visitorClub != null) visitorClub.AddTeam(team); 
                }
            }
            // Add matches
            foreach (string[] parameters in ParameterLines)
            {
                Serie serie = series.Find(s => s.name == parameters[serieIndex]);
                List<Poule> poules1 = serie.poules.Values.ToList();
                Poule poule = poules1.Find(s => s.name == parameters[pouleIndex]);
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
        
        public void ImportTeamSubscriptions(XElement doc)
        {
            foreach (XElement club in doc.Elements("Club"))
            {
                int clubId = int.Parse(club.Attribute("Id").Value);
                Club cl;
                if (clubId == -1)
                {
                    string clubName = club.Attribute("Name").Value;
                    cl = clubs.Find(c => c.Id == clubId && c.name == clubName);
                    if (cl == null)
                    {
                        cl = new Club();
                        cl.name = clubName;
                        cl.Id = clubId;
                        clubs.Add(cl);
                    }
                }
                else
                {
                    cl = clubs.Find(c => c.Id == clubId);
                }
                {
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
                            SporthallClub sp;
                            if (id == -1 || id>=1000000)
                            {
                                string sporthallName = sporthal.Attribute("Name").Value;
                                sp = cl.sporthalls.Find(t => t.name == sporthallName);
                                if (sp == null)
                                {
                                    sp = new SporthallClub(new Sporthal(-1, sporthallName));
                                    cl.sporthalls.Add(sp);
                                }
                            }
                            else
                            {
                                sp = cl.sporthalls.Find(t => t.id == id);
                                if (sp == null)
                                {
                                    Sporthal sp1 = sporthalls.Find(s => s.id == id);
                                    if (sp1 == null)
                                    {
                                        if(id < 1000000) System.Windows.Forms.MessageBox.Show(string.Format("Sporthal id={0} niet bekend", id));
                                        sp1 = new Sporthal(id, "Unknown");
                                    }
                                    sp = new SporthallClub(sp1,id>=1000000);
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
                        string serieIdstr = team.Attribute("SerieSortId").Value;
                        // string seriePrefix = team.Attribute("SeriePrefix").Value;
                        // if (seriePrefix.StartsWith("Be") == false)
                        {
                            int serieId = int.Parse(serieIdstr);
                            string teamName = team.Attribute("TeamName").Value;
                            Serie serie = series.Find(s => s.id == serieId);
                            if (serie == null)
                            {
                                System.Windows.Forms.MessageBox.Show(string.Format("Serie {0} voor team {1} is niet bekend", serieId, teamName));
                                Environment.Exit(0);
                            }
                            // Search both on name & id, since the national Id's are not unique.
                            Team te = null;
                            if(id>=0) te= cl.teams.Find(t => t.Id == id && t.serie == serie);
                            else te = cl.teams.Find(t => t.Id == id && t.name == teamName && t.serie == serie);
                            int sporthalId = int.Parse(team.Attribute("SporthallId").Value);
                            SporthallClub sporthall;
                            if (sporthalId == -1 )
                            {
                                string sporthalName = team.Attribute("SporthallName").Value;
                                sporthall = cl.sporthalls.Find(sp => sp.id == sporthalId && sp.name == sporthalName);
                            }
                            else
                            {
                                sporthall = cl.sporthalls.Find(sp => sp.id == sporthalId);
                            }
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
                                te = new Team(id, teamName, null, serie);
                                teams.Add(te);
                                serie.AddTeam(te);
                                cl.AddTeam(te);
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
                            XAttribute attrNot = team.Attribute("NotAtSameTime");
                            te.NotAtSameTime = null;
                            if (attrNot != null)
                            {
                                int idNot = int.Parse(attrNot.Value);
                                te.NotAtSameTime = cl.teams.Find(t => t.Id == idNot);
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
                            if (team.group == TeamGroups.GroupX) groupLetter = "A";
                            if (team.group == TeamGroups.GroupY) groupLetter = "B";
                            writer.WriteStartElement("Team");
                            writer.WriteAttributeString("TeamName", team.name);
                            writer.WriteAttributeString("Id", team.Id.ToString());
                            writer.WriteAttributeString("StartTime", team.defaultTime.ToString());
                            writer.WriteAttributeString("Day", team.defaultDay.ToString());
                            writer.WriteAttributeString("SerieSortId", team.serie.id.ToString());
                            writer.WriteAttributeString("SerieName", team.serie.name);
                            writer.WriteAttributeString("Group", groupLetter);
                            writer.WriteAttributeString("SporthallId", team.sporthal.id.ToString());
                            writer.WriteAttributeString("SporthallName", team.sporthal.name);
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
        public void WritePoules(XmlWriter writer)
        {
            writer.WriteStartElement("Series");
            foreach (Poule poule in poules)
            {
                if (poule.serie.export)
                {
                    writer.WriteStartElement("Serie");
                    writer.WriteAttributeString("Name", poule.name);
                    writer.WriteAttributeString("SerieSort", poule.serie.id.ToString());
                    writer.WriteAttributeString("SerieSortName", poule.serie.name);
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }
        public void WriteTeams(XmlWriter writer)
        {
            writer.WriteStartElement("Teams");
            foreach (Poule poule in poules)
            {
                if (poule.serie.export)
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
        public void WriteMatches(XmlWriter writer)
        {
            writer.WriteStartElement("Matches");
            foreach(Poule poule in poules)
            {
                if (poule.serie.export)
                {
                    foreach (Match match in poule.matches)
                    {
                        if (match.RealMatch())
                        {
                            writer.WriteStartElement("Match");
                            writer.WriteAttributeString("SerieSortName", match.poule.serie.name);
                            writer.WriteAttributeString("SerieSortId", match.poule.serie.id.ToString());
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
        public void WriteMatches(string filename)
        {
            StreamWriter writer = new StreamWriter(filename);
            writer.WriteLine("Serie,Poule,Speeldag,datum,Speeltijd,homeClub,homeTeam,visitorClub,visitorTeam");
            foreach (Poule poule in poules)
            {
                if (poule.serie.export)
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
        public void WriteExportToKLVVXml(string filename)
        {
            XmlWriter writer = XmlWriter.Create(filename);
            writer.WriteStartDocument();
            writer.WriteStartElement("Competition");
            WritePoules(writer);
            WriteTeams(writer);
            WriteMatches(writer);
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
                writer.WriteAttributeString("SerieSortId", poule.serie.id.ToString());
                writer.WriteAttributeString("SerieName", poule.serie.name);
                if (poule.imported)
                {
                    writer.WriteAttributeString("SerieSortImported", "true");
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
            foreach (TeamConstraint con in teamConstraints)
            {
                writer.WriteStartElement("Constraint");
                writer.WriteAttributeString("TeamId", con.team.Id.ToString());
                writer.WriteAttributeString("Date", con.date.ToShortDateString());
                writer.WriteAttributeString("What", con.homeVisitNone.ToString());
                writer.WriteAttributeString("Cost", con.cost.ToString());
                writer.WriteEndElement();
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
                                        t.Ranking = ranking.ToString("D2")+"."+(99-score).ToString();
                                    }
                                }
                                ranking++;
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
                int SerieId = int.Parse(poule.Attribute("SerieSortId").Value);
                Serie serie = series.Find(s => s.id == SerieId && s.name == SerieName);
                XAttribute imported = poule.Attribute("SerieSortImported");
                Poule po = poules.Find(p => p.serie == serie && p.name == PouleName);
                XAttribute attr = poule.Attribute("MaxTeams");
                int maxTeams = int.Parse(attr.Value);
                if (maxTeams == 0) maxTeams = 14;
                if (po == null)
                {
                    po = new Poule(PouleName, maxTeams, serie);
                    serie.poules.Add(po.name, po);
                    poules.Add(po);
                }
                // temporarily, to import old files
                if (SerieName == "Nationaal") po.imported = true;
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
                    if (te.poule != null)
                    {
                        Team t = new Team(-1, te.name, null, serie);
                        t.defaultDay = te.defaultDay;
                        t.defaultTime = te.defaultTime;
                        t.group = te.group;
                        t.sporthal = te.sporthal;
                        te.club.AddTeam(t);
                        serie.AddTeam(t);
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
                    this.teamConstraints.Add(tc);
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
