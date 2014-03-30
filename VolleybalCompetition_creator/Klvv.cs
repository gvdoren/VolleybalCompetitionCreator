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

namespace VolleybalCompetition_creator
{
    public class Klvv
    {
        public List<Club> clubs;
        public Dictionary<string,Serie> series = new Dictionary<string,Serie>();
        public List<Poule> poules = new List<Poule>();
        public Dictionary<int, Team> teams = new Dictionary<int, Team>();
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
            foreach (Team team in teams.Values) team.ClearConflicts();
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
            foreach (Serie serie in series.Values)
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
        
        public Klvv()
        {
            //var json = new WebClient().DownloadString("http://klvv.be/server/clubs.php");
            string json = File.ReadAllText(@"../../Data/clubs.json"); // local copy
            clubs = JsonConvert.DeserializeObject<List<Club>>(json);

            foreach (Club club in clubs)
            {
                //string url = "http://klvv.be/server/clubmatches.php?clubId="+club.Id+"&hidePast=false&hideOut=true";
                //var json1 = new WebClient().DownloadString(url);
                //File.WriteAllText(@"../../Data/clubmatches"+club.Id+".json",json1.ToString());
                // partial data required, only get the series id out. That will be used to read the serie data
                Console.WriteLine(club.name);
                string json1 = File.ReadAllText(@"../../Data/clubmatches"+club.Id+".json");// local copy
                JArray poules = JArray.Parse(json1);
                foreach (JObject poule in poules)
                {
                    if (((int)poule["trophy"]) == 0)
                    {
                        string serieName = (string)poule["prefix"];
                        string pouleName = (string)poule["serieLetter"];
                        if (pouleName.Length <2)
                        {
                            Console.WriteLine(serieName + pouleName);
                            if (series.ContainsKey(serieName) == false)
                            {
                                series.Add(serieName, new Serie(serieName));
                            }
                            Serie serie = series[serieName];
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
                                if (teams.ContainsKey(teamId) == false)
                                {
                                    Team t = new Team(teamId,teamName, po);
                                    teams.Add(teamId, t);
                                }
                                Team homeTeam = teams[teamId];
                                homeTeam.club = club;
                                if (club.teams.Contains(homeTeam) == false)
                                {
                                    club.teams.Add(homeTeam);
                                }
                                homeTeam.plannedMatches.Add(datetime);
                                if (po.teams.Contains(homeTeam) == false)
                                {
                                    po.teams.Add(homeTeam);
                                }
                                if (club.series.Contains(serie) == false)
                                {
                                    club.series.Add(serie);
                                }
                                if (po.serie.teams.Contains(homeTeam) == false)
                                {
                                    po.serie.teams.Add(homeTeam);
                                }
                                teamName = (string)match["visitorTeam"];
                                teamId = (int)match["visitorTeamId"];
                                if (teams.ContainsKey(teamId) == false)
                                {
                                    teams.Add(teamId, new Team(teamId,teamName, po));
                                }
                                Team visitorTeam = teams[teamId];
                                if (po.teams.Contains(visitorTeam) == false)
                                {
                                    po.teams.Add(visitorTeam);
                                }
                                Match mat = new Match(datetime, homeTeam, visitorTeam, serie, po);
                                po.matches.Add(mat);
                                // check whether the sporthal is already added to the club
                                string sporthalId_json = (string)match["sporthallId"];
                                if (sporthalId_json!= null)
                                {
                                    int sporthalId = (int)match["sporthallId"];
                                    if (sporthalls.Find(s => s.id == sporthalId) == null)
                                    {
                                        string sporthalName = (string)match["sporthallName"];
                                        Sporthal sporthal1 = new Sporthal(sporthalId, sporthalName, homeTeam.club);
                                        sporthalls.Add(sporthal1);
                                    }
                                    Sporthal sporthal = sporthalls.Find(s => s.id == sporthalId);
                                    if (club.sporthalls.Contains(sporthal) == false) club.sporthalls.Add(sporthal);
                                    if(homeTeam.sporthal == null)
                                    {
                                        homeTeam.sporthal = sporthal;
                                    }
                                }

                            }
                            foreach(Team te in teams.Values)
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
            // Lees de 'nationale' poule in. Die voegen we ertussen. Hopelijk matchen de clubnamen.
            ReadVVB();



            foreach (Poule poule in this.poules)
            {
                constraints.Add(new ConstraintSchemaTooClose(poule));
            }

            ReadClubConstraints();
            foreach (Team team in this.teams.Values)
            {
                constraints.Add(new ConstraintNotAtWeekendHome(team));
            }
            foreach (Club club in clubs)
            {
                constraints.Add(new ConstraintNotAllInHomeWeekend(club));
                foreach (Sporthal sp in club.sporthalls)
                {
                    constraints.Add(new ConstraintZaal(sp,club));
                }
            }
            Evaluate(null);
            Changed();
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
        public event MyEventHandler OnMyChange;
        public void Changed()
        {
            //call it then you need to update:
            if (OnMyChange != null)
            {
                MyEventArgs e = new MyEventArgs();
                //e.EventInfo = content;
                OnMyChange(this, e);
            }
        }

        public void ReadVVB()
        {
            int minTeamId = -1;
            Club nationaal = new Club();
            nationaal.name = "Nationaal";
            nationaal.Id = -1;
            clubs.Add(nationaal);
            Serie nationaalSerie = new Serie("Nationaal");
            nationaalSerie.optimizable = false;
            series.Add("nationaal",nationaalSerie);
            using (XmlReader reader = XmlReader.Create("C:\\Users\\Giel1.Giel_laptop.001\\Documents\\VVBWedstrijden.xml"))
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
                    if (club.series.Contains(nationaalSerie) == false)
                    {
                        club.series.Add(nationaalSerie);
                    }
                    Sporthal sporth = club.sporthalls.Find(sp => sp.name.ToLower() == sporthal.ToLower());
                    if (sporth == null)
                    {
                        sporth = new Sporthal(-1, sporthal, club);
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
                    poule.serie.constraintsHold = false;
                    Team homeTeam = poule.teams.Find(t => t.name == thuisploeg);
                    if (homeTeam == null)
                    {
                        homeTeam = new Team(-1, thuisploeg, poule);
                        homeTeam.sporthal = sporth;
                        //teams.Add(minTeamId,team);
                        minTeamId--;
                        poule.teams.Add(homeTeam);
                    }
                    if (nationaalSerie.teams.Contains(homeTeam) == false)
                    {
                        nationaalSerie.teams.Add(homeTeam);
                    }
                    if (homeTeam.club == null)
                    {
                        homeTeam.club = club;
                        club.teams.Add(homeTeam);
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

                    Team visitorTeam = poule.teams.Find(t => t.name == bezoekersploeg);
                    if (visitorTeam == null)
                    {
                        visitorTeam = new Team(-1, bezoekersploeg, poule);
                        int hour = int.Parse(aanvangsuur.Substring(0, 2));
                        int minute = int.Parse(aanvangsuur.Substring(aanvangsuur.Length - 2, 2));
                        //teams.Add(minTeamId, team);
                        minTeamId--;
                        poule.teams.Add(visitorTeam);
                    }
                    Match match = new Match(dt, homeTeam, visitorTeam, nationaalSerie, poule);
                    poule.matches.Add(match);
                    /*
                    int clubId = int.Parse(club.Attribute("ID").Value);
                    Club cl = clubs.Find(c => c.Id == clubId);
                    cl.ConstraintNotAtTheSameTime = bool.Parse(club.Attribute("NotAtSameTime").Value);
                    cl.ConstraintAllInOneWeekend = bool.Parse(club.Attribute("AllInOneWeekend").Value);
                    foreach (var team in club.Element("Teams").Elements("Team"))
                    {
                        var teamId = team.Attribute("ID");
                        int id = int.Parse(teamId.Value);
                        Team te = cl.teams.Find(t => t.Id == id);
                        foreach (var team1 in team.Element("NotAtSameTime").Elements("Team"))
                        {
                            var team1Id = team1.Attribute("ID");
                            int id1 = int.Parse(team1Id.Value);
                            Team te1 = cl.teams.Find(t => t.Id == id1);
                            if (te.NotAtSameWeekend.Contains(te1) == false) te.NotAtSameWeekend.Add(te1);
                        }

                    }
                    if (club.Element("Sporthalls") != null)
                    {
                        foreach (var sporthal in club.Element("Sporthalls").Elements("Sporthall"))
                        {
                            var sporthallId = sporthal.Attribute("ID");
                            int id = int.Parse(sporthallId.Value);
                            Sporthal te = cl.sporthalls.Find(t => t.id == id);
                            foreach (var date in sporthal.Element("NotAvailable").Elements("Date"))
                            {
                                DateTime dt = DateTime.Parse(date.Value.ToString());
                                te.NotAvailable.Add(dt);

                            }

                        }
                    }
                     * */
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




        public void ReadClubConstraints()
        {
            using (XmlReader reader = XmlReader.Create("C:\\Users\\Giel1.Giel_laptop.001\\Documents\\ClubConstraints.xml"))
            {
                reader.ReadStartElement("Clubs");
                while (reader.Name == "Club")
                {
                    XElement club = (XElement)XNode.ReadFrom(reader);
                    int clubId = int.Parse(club.Attribute("ID").Value);
                    Club cl = clubs.Find(c => c.Id == clubId);
                    XElement freeformatconstraint = club.Element("FreeFormatConstraint");
                    cl.FreeFormatConstraints = freeformatconstraint.Value;
                    
                    foreach (var team in club.Element("Teams").Elements("Team"))
                    {
                        var teamId = team.Attribute("ID");
                        int id = int.Parse(teamId.Value);
                        Team te = cl.teams.Find(t => t.Id == id);
                        var teamGroup = team.Attribute("Group");
                        te.group = (TeamGroups) Enum.Parse(typeof(TeamGroups), teamGroup.Value);    
                    }
                    if (club.Element("Sporthalls") != null)
                    {
                        foreach (var sporthal in club.Element("Sporthalls").Elements("Sporthall"))
                        {
                            var sporthallId = sporthal.Attribute("ID");
                            int id = int.Parse(sporthallId.Value);
                            Sporthal te = cl.sporthalls.Find(t => t.id == id);
                            foreach (var date in sporthal.Element("NotAvailable").Elements("Date"))
                            {
                                DateTime dt = DateTime.Parse(date.Value.ToString());
                                te.NotAvailable.Add(dt);

                            }

                        }
                    }
                }
                reader.ReadEndElement();
            }

        }


        public void WriteClubConstraints()
        {
            using (XmlWriter writer = XmlWriter.Create("C:\\Users\\Giel1.Giel_laptop.001\\Documents\\ClubConstraints.xml"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Clubs");
                foreach (Club club in clubs)
                {
                    if (club.name != "Nationaal")
                    {
                        writer.WriteStartElement("Club");
                        writer.WriteAttributeString("ID", club.Id.ToString());
                        writer.WriteElementString("FreeFormatConstraint", club.FreeFormatConstraints);
                        writer.WriteStartElement("Teams");
                        foreach (Team team in club.teams)
                        {
                            writer.WriteStartElement("Team");
                            writer.WriteAttributeString("ID", team.Id.ToString());
                            writer.WriteAttributeString("Group", team.group.ToString());
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                        writer.WriteStartElement("Sporthalls");
                        foreach (Sporthal sporthal in club.sporthalls)
                        {
                            writer.WriteStartElement("Sporthall");
                            writer.WriteAttributeString("ID", sporthal.id.ToString());
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
                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }        

        }
    }

}
