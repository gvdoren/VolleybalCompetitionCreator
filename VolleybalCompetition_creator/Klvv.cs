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

namespace VolleybalCompetition_creator
{
    public class Klvv
    {
        public List<Club> clubs;
        public Dictionary<string,Serie> series = new Dictionary<string,Serie>();
        public List<Poule> poules = new List<Poule>();
        public Dictionary<int, Team> teams = new Dictionary<int, Team>();
        public Annorama annorama = new Annorama();
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
                            string teamName = (string)match["homeTeam"];
                            int teamId = (int)match["homeTeamId"];
                            if (teams.ContainsKey(teamId) == false)
                            {
                                teams.Add(teamId, new Team(teamName,po));
                            }
                            Team homeTeam = teams[teamId];
                            homeTeam.club = club;
                            if (club.teams.Contains(homeTeam) == false)
                            {
                                club.teams.Add(homeTeam);
                            }
                            if (po.teams.Contains(homeTeam) == false)
                            {
                                po.teams.Add(homeTeam);
                            }
                            if (club.series.Contains(serie) == false)
                            {
                                club.series.Add(serie);
                            }
                            teamName = (string)match["visitorTeam"];
                            teamId = (int)match["visitorTeamId"];
                            if (teams.ContainsKey(teamId) == false)
                            {
                                teams.Add(teamId, new Team(teamName,po));
                            }
                            Team visitorTeam = teams[teamId];
                            if (po.teams.Contains(visitorTeam) == false)
                            {
                                po.teams.Add(visitorTeam);
                            }
                            // "2013-09-28T20:30:00+0200"
                            // "2013-09-22T11:00:00+02:00"
                            // (string)match["datetime"]
                            CultureInfo provider = CultureInfo.InvariantCulture;
                            string datetimeStr = (string)match["datetime"];
                            DateTime datetime = DateTime.ParseExact(datetimeStr, "MM/dd/yyyy HH:mm:ss", provider);
                            //datetime = DateTime.ParseExact("2013-09-22T11:00:00+02:00", "yyyy-MM-ddTHH:mm:sszzz", null);
                            Match mat = new Match(datetime, homeTeam,visitorTeam, serie, po);
                            po.matches.Add(mat);
                        }
                    }
                }
            }
        }
    }
}
