﻿using System;
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

namespace VolleybalCompetition_creator
{
    public class Klvv
    {
        public string fileName = "Competition.xml";
        public bool slow = false;
        public List<Club> clubs;
        public List<Serie> series = new List<Serie>();
        public List<Poule> poules = new List<Poule>();
        public List<Team> teams = new List<Team>();
        public void AddTeam(Team team)
        {
            teams.Add(team);
            team.serie.teams.Add(team);
            team.club.teams.Add(team);
        }
        public Anorama annorama = new Anorama();
        public List<Constraint> constraints = new List<Constraint>();
        public List<Sporthal> sporthalls = new List<Sporthal>();
        public void Optimize()
        {
            poules.Sort(delegate(Poule p1, Poule p2) { return p1.conflict.CompareTo(p2.conflict); });
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
                constraint.Evaluate(this,p);
            }
        }
        
        public int TotalConflicts()
        {
            lock (this) ;
            int conflicts = 0;
            foreach (Constraint constraint in constraints)
            {
                conflicts += constraint.conflict;
            }
            return conflicts;
        }
        public Klvv()
        {
            Serie nationaalSerie = new Serie(-1, "Nationaal");
            nationaalSerie.optimizable = false;
            series.Add(nationaalSerie);

            // Read series
            string classes_json = File.ReadAllText(@"InputData/series.json"); // local copy
            JArray classesObjects = JArray.Parse(classes_json);
            foreach (JObject classesObject in classesObjects)
            {
                foreach (JObject serieObject in classesObject["serieSorts"])
                {
                    Serie newSerie = new Serie(int.Parse(serieObject["id"].ToString()), 
                                               serieObject["prefix"].ToString());
                    series.Add(newSerie);
                }
            }
            // read sporthalls
            string sporthalls_json = File.ReadAllText(@"InputData/sporthalls.json"); // local copy
            JArray sporthallObjects = JArray.Parse(sporthalls_json);
            foreach (JObject sporthallObject in sporthallObjects)
            {
                Sporthal newSporthal = new Sporthal(int.Parse(sporthallObject["id"].ToString()),sporthallObject["name"].ToString());
                sporthalls.Add(newSporthal);
            }
            // lat & lng for each sporthal (must be included in the sporthalls.json
            string latlng_json = File.ReadAllText(@"InputData/sporthallen.json");// local copy
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
            string json2 = File.ReadAllText(@"InputData/sporthal_afstanden.json");// local copy
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
            //var json = new WebClient().DownloadString("http://klvv.be/server/clubs.php");
            string json = File.ReadAllText(@"InputData/clubs.json"); // local copy
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
        public void RenewConstraints()
        {
            lock (this) ;
            constraints.Clear();
            foreach (Poule poule in this.poules)
            {
                constraints.Add(new ConstraintSchemaTooClose(poule));
                constraints.Add(new ConstraintPouleInconsistent(poule));
                
            }

            foreach (Club club in clubs)
            {
                constraints.Add(new ConstraintNotAllInHomeWeekend(club));
                constraints.Add(new ConstraintNotAtWeekendHome(club));
                constraints.Add(new ConstraintNotAtSameTime(club));
                foreach (SporthallClub sp in club.sporthalls)
                {
                    constraints.Add(new ConstraintZaal(sp, club));
                }
            }
            foreach (Sporthal sp in sporthalls)
            {
                //constraints.Add(new SporthalSharedOnSameDay(sp));
            }
        }

        public void ImportKLVVCompetition()
        {
            foreach (Club club in clubs)
            {
                if (club.Id >= 0)
                {
                    //string url = "http://klvv.be/server/clubmatches.php?clubId="+club.Id+"&hidePast=false&hideOut=true";
                    //var json1 = new WebClient().DownloadString(url);
                    //File.WriteAllText(@"../../Data/clubmatches"+club.Id+".json",json1.ToString());
                    // partial data required, only get the series id out. That will be used to read the serie data
                    Console.WriteLine(club.name);
                    string json1 = File.ReadAllText(@"InputData/clubmatches" + club.Id + ".json");// local copy
                    JArray poules = JArray.Parse(json1);
                    foreach (JObject poule in poules)
                    {
                        if (((int)poule["trophy"]) == 0)
                        {
                            string serieName = (string)poule["prefix"];
                            int serieId = int.Parse(poule["serieSortId"].ToString());
                            string pouleName = (string)poule["serieLetter"];
                            if (pouleName.Length < 2) // exclude A2, etc
                            {
                                Console.WriteLine(serieName + pouleName);
                                Serie serie = series.Find(s => s.id == serieId);
                                if (serie == null)
                                {
                                    System.Windows.Forms.MessageBox.Show(string.Format("Serie {0} is niet bekend", serieName));
                                    Environment.Exit(0);
                                }
                                if (serie.poules.ContainsKey(pouleName) == false)
                                {
                                    serie.poules.Add(pouleName, new Poule(pouleName));
                                }
                                Poule po = serie.poules[pouleName];
                                po.serie = serie;
                                if (this.poules.Contains(po) == false)
                                {
                                    this.poules.Add(po);
                                }
                                  
                                foreach (JObject match in poule["matches"])
                                {
                                    // "2013-09-28T20:30:00+0200"
                                    // "2013-09-22T11:00:00+02:00"
                                    // (string)match["datetime"]
                                    CultureInfo provider = CultureInfo.InvariantCulture;
                                    string datetimeStr = (string)match["datetime"];
                                    DateTime datetime = DateTime.ParseExact(datetimeStr, "MM/dd/yyyy HH:mm:ss", provider);
                                    //datetime = DateTime.ParseExact("2013-09-22T11:00:00+02:00", "yyyy-MM-ddTHH:mm:sszzz", null);
                                    string teamName = (string)match["homeTeam"];
                                    int teamId = (int)match["homeTeamId"];
                                    Team homeTeam = teams.Find(t => t.Id == teamId);
                                    if (homeTeam == null)
                                    {
                                        homeTeam = new Team(teamId, teamName, po, po.serie);
                                        teams.Add(homeTeam);
                                    }
                                    homeTeam.club = club;
                                    homeTeam.poule = po;
                                    homeTeam.serie = po.serie;
                                    if (club.teams.Contains(homeTeam) == false)
                                    {
                                        club.teams.Add(homeTeam);
                                    }
                                    homeTeam.plannedMatches.Add(datetime);
                                    if (po.teams.Contains(homeTeam) == false)
                                    {
                                        po.teams.Add(homeTeam);
                                    }
                                    if (po.serie.teams.Contains(homeTeam) == false)
                                    {
                                        po.serie.teams.Add(homeTeam);
                                    }
                                    teamName = (string)match["visitorTeam"];
                                    teamId = (int)match["visitorTeamId"];
                                    Team visitorTeam = teams.Find(t => t.Id == teamId);
                                    if (visitorTeam == null)
                                    {
                                        visitorTeam = new Team(teamId, teamName, po, po.serie);
                                        teams.Add(visitorTeam);
                                    }
                                    visitorTeam.poule = po;
                                    visitorTeam.serie = po.serie;
                                    if (po.teams.Contains(visitorTeam) == false)
                                    {
                                        po.teams.Add(visitorTeam);
                                    }
                                    Match mat = new Match(datetime, homeTeam, visitorTeam, serie, po);
                                    po.matches.Add(mat);
                                    // check whether the sporthal is already added to the club
                                    string sporthalId_json = (string)match["sporthallId"];
                                    if (sporthalId_json != null)
                                    {
                                        int sporthalId = (int)match["sporthallId"];
                                        SporthallClub sporthal = club.sporthalls.Find(s => s.id == sporthalId);
                                        if (sporthal == null)
                                        {
                                            Sporthal sporth = sporthalls.Find(s => s.id == sporthalId);
                                            if (sporth == null)
                                            {
                                                System.Windows.Forms.MessageBox.Show(string.Format("Sporthal id={0} niet bekend", sporthalId));                                                
                                            }
                                            sporthal = new SporthallClub(sporth);
                                            club.sporthalls.Add(sporthal);
                                        
                                        }
                                        if (homeTeam.sporthal == null && sporthal != null)
                                        {
                                            homeTeam.sporthal = sporthal;
                                        }
                                    }

                                }
                                foreach (Team te in teams)
                                {
                                    SelectDefaultDayTime(te);
                                }
                                po.weekends.Sort(delegate(Weekend w1, Weekend w2)
                                {
                                    int v1 = (w1.Year * 52) + w1.WeekNr;
                                    int v2 = (w2.Year * 52) + w2.WeekNr;
                                    return v1.CompareTo(v2);
                                });
                                foreach (Match match in po.matches)
                                {
                                    match.SetWeekIndex();
                                }
                                po.matches.Sort(delegate(Match m1, Match m2) { return m1.datetime.CompareTo(m2.datetime); });
                            }

                        }
                    }
                }
            }
            foreach(Team t in teams)
            {
                if(t.sporthal == null)
                {
                    t.sporthal = t.club.sporthalls[0];
                    
                }
            }

        }
        private void SelectDefaultDayTime(Team team)
        {
            DayOfWeek preferredDay = DayOfWeek.Monday;
            int preferredDayCount = 0;
            Time preferredTime = null;
            int preferredTimeCount = 0;
            foreach (DateTime dt in team.plannedMatches)
            {
                int dayCount = 0;
                int timeCount = 0;
                foreach (DateTime dt1 in team.plannedMatches)
                {
                    if (dt.DayOfWeek == dt1.DayOfWeek)
                    {
                        dayCount++;
                    }
                    if (dt.TimeOfDay == dt1.TimeOfDay)
                    {
                        timeCount++;
                    }
                }
                if (dayCount > preferredDayCount)
                {
                    preferredDay = dt.DayOfWeek;
                    preferredDayCount = dayCount;
                }
                if (timeCount > preferredTimeCount)
                {
                    preferredTime = new Time(dt.Hour, dt.Minute);
                    preferredTimeCount = timeCount;
                }
            }
            team.defaultDay = preferredDay;
            team.defaultTime = preferredTime;
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
                        poule = new Poule(reeks);
                        nationaalSerie.poules.Add(reeks, poule);
                        poule.serie = nationaalSerie;
                        poules.Add(poule);
                    }
                    poule.serie.constraintsHold = true;
                    Team homeTeam = teams.Find(t => t.Id == -1 && t.name == thuisploeg && t.serie == nationaalSerie);
                    if (homeTeam == null)
                    {
                        homeTeam = new Team(-1, thuisploeg, poule, poule.serie);
                        homeTeam.sporthal = sporth;
                        teams.Add(homeTeam);
                    }
                    else
                    {
                        homeTeam.poule = poule;
                    }
                    if (poule.teams.Contains(homeTeam) == false) poule.teams.Add(homeTeam);
                    if (homeTeam.sporthal == null)
                    {
                        if (homeTeam.club != null && homeTeam.club.sporthalls.Count > 0)
                        {
                            homeTeam.sporthal = homeTeam.club.sporthalls[0];
                        }
                    }
                    

                    if (nationaalSerie.teams.Contains(homeTeam) == false)
                    {
                        nationaalSerie.teams.Add(homeTeam);
                    }
                    if (homeTeam.club == null)
                    {
                        homeTeam.club = club;
                        club.teams.Add(homeTeam);
                        Console.WriteLine(club.name);
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
                    else
                    {
                        visitorTeam.poule = poule;
                    }
                    if (poule.teams.Contains(visitorTeam) == false) poule.teams.Add(visitorTeam);
                    
                    Match match = new Match(dt, homeTeam, visitorTeam, nationaalSerie, poule);
                    poule.matches.Add(match);
                }
                reader.ReadEndElement();
            }
            foreach (Poule po in nationaalSerie.poules.Values)
            {
                foreach (Match match in po.matches)
                {
                    match.SetWeekIndex();
                }
            }
 

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
                            if (id == -1)
                            {
                                string sporthallName = sporthal.Attribute("Name").Value;
                                sp = cl.sporthalls.Find(t => t.id == id && t.name == sporthallName);
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
                                        System.Windows.Forms.MessageBox.Show(string.Format("Sporthal id={0} niet bekend", id));                                                
                                    }
                                    sp = new SporthallClub(sp1);
                                    cl.sporthalls.Add(sp);
                                }
                            }
                            
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
                        int serieId = int.Parse(serieIdstr);
                        string teamName = team.Attribute("TeamName").Value;
                        Serie serie = series.Find(s => s.id == serieId);
                        if (serie == null)
                        {
                            System.Windows.Forms.MessageBox.Show(string.Format("Serie {0} voor team {1} is niet bekend", serieId, teamName));
                            Environment.Exit(0);
                        }
                        // Search both on name & id, since the national Id's are not unique.
                        Team te = cl.teams.Find(t => t.Id == id && t.name == teamName && t.serie == serie);
                        int sporthalId = int.Parse(team.Attribute("SporthallId").Value);
                        SporthallClub sporthall;
                        if (sporthalId == -1)
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
                            int x = 0;
                        } 
                        if (te == null)
                        {
                            string DayString = team.Attribute("Day").Value;
                            if(DayString == "7") DayString = "0";
                            DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), DayString);
                            Time time = new Time(DateTime.Parse(team.Attribute("StartTime").Value));
                            te = new Team(id, teamName, null, serie);
                            te.sporthal = sporthall;
                            te.defaultDay = day;
                            te.defaultTime = time;
                            teams.Add(te);
                            serie.teams.Add(te);
                            cl.teams.Add(te);
                            te.club = cl;
                        }
                        else
                        {
                            if (te.poule != null) te.poule.teams.Remove(te);
                            te.poule = null;
                        }
                        string teamGroup = team.Attribute("Group").Value;
                        te.group = TeamGroups.NoGroup;
                        if (teamGroup == "A") te.group = TeamGroups.GroupA;
                        if (teamGroup == "B") te.group = TeamGroups.GroupB;
                        XAttribute attrNot = team.Attribute("NotAtSameTime");
                        if (attrNot != null)
                        {
                            int idNot = int.Parse(attrNot.Value);
                            te.NotAtSameTimeId_int = idNot;
                        }
                    }
                    foreach (Team te in cl.teams)
                    {
                        if (te.NotAtSameTimeId_int >= 0)
                        {
                            te.NotAtSameTime = cl.teams.Find(t => t.Id == te.NotAtSameTimeId_int);
                        }
                    }
                }
            }
        }


        public void WriteClubConstraints(XmlWriter writer)
        {
            writer.WriteStartElement("Clubs");
            foreach (Club club in clubs)
            {
                //if (club.Id > 0)
                {
                    writer.WriteStartElement("Club");
                    writer.WriteAttributeString("Id", club.Id.ToString());
                    writer.WriteAttributeString("Name", club.name);
                    if (club.groupingWithClub != null) writer.WriteAttributeString("LinkedClub", club.groupingWithClub.Id.ToString());
                    writer.WriteElementString("FreeFormatConstraint", club.FreeFormatConstraints);
                    writer.WriteStartElement("Sporthalls");
                    foreach (SporthallClub sporthal in club.sporthalls)
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
                    writer.WriteEndElement();
                    writer.WriteStartElement("Teams");
                    foreach (Team team in club.teams)
                    {
                        string groupLetter = "0";
                        if (team.group == TeamGroups.GroupA) groupLetter = "A";
                        if (team.group == TeamGroups.GroupB) groupLetter = "B";
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
                        if (team.NotAtSameTime != null) writer.WriteAttributeString("NotAtSameTime", team.NotAtSameTime.Id.ToString());
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }

    }
}
