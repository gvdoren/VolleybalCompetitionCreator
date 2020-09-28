﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Globalization;

namespace XmlTransformer
{
    public class TeamInfo
    {
        public TeamInfo registrationTeam = null;
        //public string usedByTeamId = "";
        public int teamId = 0;
        public string teamName = "";
        public int SerieId = 0;
        public int clubId = 0;
        public string clubName = "";
        public string ranking = "";
        public string score = "";
        public string evenOdd = "";
        public string reeks = "";
        public bool newRegistration = false;
        public bool derivedNewRegistrationFromThis = false;
        public bool derivedExistingTeamFromThis = false;
        public int matchesToBePlayed = 0;
        public int matchCount = 0;
    }

    public class SerieInfo
    {
        public int serieId;
        public string serieNameVVB;
        public string serieName;
        public string poule;
    }
    public class ProvincieInfo
    {
        public int Id;
        //public List<string> Prefix;
        public string Name;

    }

    // https://www.volleyadmin2.be/download/seriesubscriptions/5_11/  Divisie inschrijvingen voor Limburg
    // http://volleyadmin2.be/services/series_xml.php?province_id=4&all=1  voor alle series (zodat ik de id's heb)
    class Program
    {
        static public Dictionary<int, SerieInfo> serieInfos = new Dictionary<int, SerieInfo>();
        static public Dictionary<int, TeamInfo> teamInfo = new Dictionary<int, TeamInfo>();
        static public Dictionary<int,int> matchesToBePlayed = new Dictionary<int,int>();
        //static int MIN_NEW_SERIE_ID = 3272;
        //static string startDate = "1/1/2019";
        static string outDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CompetitionCreator\\";

        static DateTime generationDate = DateTime.Now.AddDays(0);
        static int maxSerieIdWithMatches = 0;

        static public List<ProvincieInfo> provincies = new List<ProvincieInfo>();
        static public List<string> reeksMatches = new List<string>();
        static public List<int> reeksMatchesExist = new List<int>();
        static void CheckMatchesToBePlayed(IEnumerable<XElement> wedstrijden, bool isProvinciaal)
        {
            foreach (var wedstrijd in wedstrijden)
            {
                if (wedstrijd.Element("thuisploeg_id") == null)
                    continue;
                if (wedstrijd.Element("bezoekersploeg_id") == null)
                    continue;
                var thuisPloegId = int.Parse(wedstrijd.Element("thuisploeg_id").Value);
                var bezoekersPloegId = int.Parse(wedstrijd.Element("bezoekersploeg_id").Value);
                var reeksId = int.Parse(wedstrijd.Element("reeksid").Value);
                var reeks = wedstrijd.Element("reeks").Value;
                //Console.WriteLine(wedstrijd.Element("datum").Value);
                //DateTime date = DateTime.Parse(wedstrijd.Element("datum").Value);

                DateTime date;
                DateTime.TryParseExact(wedstrijd.Element("datum").Value, "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

                if (date > generationDate)
                {
                    if (matchesToBePlayed.Keys.Contains(thuisPloegId) == false)
                        matchesToBePlayed.Add(thuisPloegId, 0);
                    if (matchesToBePlayed.Keys.Contains(bezoekersPloegId) == false)
                        matchesToBePlayed.Add(bezoekersPloegId, 0);
                    matchesToBePlayed[thuisPloegId]++;
                    matchesToBePlayed[bezoekersPloegId]++;
                    if (!reeksMatches.Contains(reeks))
                    {
                        reeksMatches.Add(reeks);
                        if (isProvinciaal)
                            Console.WriteLine("Provinciaal :Wedstrijden van reeks {0} worden meegenomen in de planning", reeks);
                        else
                            Console.WriteLine("Nationaal :Wedstrijden van reeks {0} worden meegenomen in de planning", reeks);
                    }
                }
                if (date < generationDate && isProvinciaal)
                {
                    if (reeksId > maxSerieIdWithMatches)
                        maxSerieIdWithMatches = reeksId;
                    reeksMatchesExist.Add(reeksId);
                }
            }
        }

        static void Main(string[] args)
        {
            ProvincieInfo limburg = new ProvincieInfo();
            limburg.Name = "LIMBURG";
            limburg.Id = 4; //VVB internal DB number
            //limburg.Prefix = new List<string>(new string[] { "L-" } );
            provincies.Add(limburg);
            ProvincieInfo vlaamsbrabant = new ProvincieInfo();
            vlaamsbrabant.Name = "VLAAMS-BRABANT";
            vlaamsbrabant.Id = 7; //VVB internal DB number
            //vlaamsbrabant.Prefix = new List<string>(new string[] { "VB-" });
            provincies.Add(vlaamsbrabant);
            ProvincieInfo antwerpen = new ProvincieInfo();
            antwerpen.Name = "ANTWERPEN";
            antwerpen.Id = 1; //VVB internal DB number
            //vlaamsbrabant.Prefix = new List<string>(new string[] { "AA-", "AH-", "AM-", "AT-"});
            provincies.Add(antwerpen);


            List<string> Teams = new List<string>();
            //string filename = "C:\\Users\\Giel\\Documents\\CompetitionCreator\\KLVV competitie 2016_2017\\2e helft\\inschrijvingen_20161210.xml";
            // string filename = "http://volleyadmin2.be/download/seriesubscriptions/3_4_1/";
            // Alle inschrijvingen voor Limburg (divisie + provinciaal)
            // 4 is het seizoen: 4 = 2017, dus jaartal-2013 is het getal dat je nodig hebt.
            // _1: om de reeksen zonder punten systeem eruit te filteren (beker reeksen), al is dit niet volledig. Dus er is nog filtering nodig op 'Beker'

            // Hoe vind ik het originele team-id van de inschrijvingen
            // Indien parent-id 0 of leeg is, dan is het team-id het originele team
            // Anders: is het parent-id het originele teamId

            // Rangschikking:
            // 1. Een team met wedstrijdn heeft een rangschikking.
            // 2. Deze moet geadministreerd worden bij het 'originele team' van de inschrijving
            // 3. De nieuwe inschrijving moet ook bij zijn originele inschrijving kijken, voor de rangschikking
            // 4. Rangschikking is alleen geldig indien de reeks nog gelijk is

            // Provincies:
            // http://www.volleyadmin2.be/services/wedstrijden_xml.php?province_id=1 A?- Antwerpen?
            // http://www.volleyadmin2.be/services/wedstrijden_xml.php?province_id=0 Liga
            // http://www.volleyadmin2.be/services/wedstrijden_xml.php?province_id=5 O-  (Oostvlaanderen)
            // http://www.volleyadmin2.be/services/wedstrijden_xml.php?province_id=6 RO- 
            // http://www.volleyadmin2.be/services/wedstrijden_xml.php?province_id=7 VB- Vlaams brabant
            // http://www.volleyadmin2.be/services/wedstrijden_xml.php?province_id=9 W-  West-vlaanderen
            // http://www.volleyadmin2.be/services/wedstrijden_xml.php?province_id=10 BRV- Brugs Recreatief



            Console.WriteLine("Competition creator - Data importer Vlaamse Volleybal Bond");
            Console.WriteLine(" Selecteer het nummer en druk op <return/enter>");
            int v = 0;
            string val;
            do
            {
                Console.WriteLine("Welke provincie:");
                int number = 0;
                foreach (var prov in provincies)
                {
                    Console.WriteLine("{0}. {1}", number, prov.Name);
                    number++;
                }
                val = Console.ReadLine();
            } while (!int.TryParse(val, out v) || v < 0 || v >= provincies.Count);
            Console.WriteLine("De bestanden zijn in opgeslagen in: " + outDir);
            ProvincieInfo provincie = provincies[v];
            outDir += provincie.Name+"_";
            int year = DateTime.Now.AddMonths(-2).Year;
            // Inschrijvingen alle reeksen alle provincies zonder beker
            string filename = "https://www.volleyadmin2.be/download/seriesubscriptions/"+ (year-2013).ToString() +"_0_1/";
            XDocument doc = XDocument.Load(filename, LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);

            // Wedstrijden van geselecteerde provincie + alle wedstrijden nationaal
            XDocument doc_limburg = XDocument.Load("http://www.volleyadmin2.be/services/wedstrijden_xml.php?province_id=" + provincie.Id.ToString(), LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
            var wedstrijden_limburg = doc_limburg.Element("kalender").Elements("wedstrijd");
            CheckMatchesToBePlayed(wedstrijden_limburg, true);
            XDocument doc_nationaal = XDocument.Load("http://www.volleyadmin2.be/services/wedstrijden_xml.php?province_id=11", LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
            var wedstrijden_nationaal = doc_nationaal.Element("kalender").Elements("wedstrijd");
            CheckMatchesToBePlayed(wedstrijden_nationaal, false);

            Console.WriteLine("Min Serie Id: {0}", maxSerieIdWithMatches);

            List<XElement> toBeRemoved = new List<XElement>();
            var clubs = doc.Element("Registrations").Element("Clubs").Elements("Club");

            // Remove clubs outside the province and remove beker teams
            foreach (var club in clubs)
            {
                bool remove = true;
                var teams = club.Element("Teams").Elements("Team");
                foreach (var t in teams)
                {
                    if (t.Attribute("Province") != null)
                    {
                        var prov = t.Attribute("Province").Value.ToString();
                        if (prov == provincie.Name)
                            remove = false;
                    }
                }
                string stamNummer = club.Attribute("StamNumber").Value;
                if (remove)
                    toBeRemoved.Add(club);
                
                List<XElement> teamsToBeRemoved = new List<XElement>();
                foreach (var t in teams)
                {
                    var serieName = t.Attribute("SerieName").Value;
                    if (serieName.Contains("Beker") || serieName.Contains("beker"))
                        teamsToBeRemoved.Add(t);
                }
                foreach (var t in teamsToBeRemoved)
                    t.Remove();
            }
            foreach (var club in toBeRemoved)
                club.Remove();

            // Determine the poule letter since this info is not explicitly present in volley admin, except implicitly in name
            foreach (var club in clubs)
            {
                var teams = club.Element("Teams").Elements("Team");
                foreach (var t1 in teams)
                {
                    SerieInfo info = new SerieInfo();
                    info.serieId = int.Parse(t1.Attribute("SerieId").Value);
                    info.serieNameVVB = t1.Attribute("SerieName").Value;
                    if (info.serieNameVVB == "")
                        info.serieNameVVB = "Reeks zonder naam";

                    if (serieInfos.ContainsKey(info.serieId) == false)
                        serieInfos.Add(info.serieId, info);
                }
            }
            foreach (var info1 in serieInfos)
            {
                info1.Value.poule = "A";
                info1.Value.serieName = info1.Value.serieNameVVB;
                foreach (var info2 in serieInfos)
                {
                    if (info1.Key != info2.Key)
                    {
                        var name1 = info1.Value.serieNameVVB.Replace(" ", "").Replace("-", "").ToLower();
                        var name2 = info2.Value.serieNameVVB.Replace(" ", "").Replace("-", "").ToLower();
                        var name1_short = name1.Remove(name1.Length - 1);
                        var name2_short = name2.Remove(name2.Length - 1);
                        if (name1_short == name2_short && name1 != name2)
                        {
                            char pouleLetter = name1[name1.Length - 1].ToString().ToUpper()[0];
                            if ((pouleLetter >= 'A' && pouleLetter <= 'Z'))
                            {
                                info1.Value.poule = pouleLetter.ToString();
                                var name3 = info1.Value.serieNameVVB;
                                var name3_short = name3.Remove(name3.Length - 1).Trim();
                                info1.Value.serieName = name3_short; 
                                break;
                            }
                        }
                    }
                }
                if (info1.Value.poule == "A")
                {
                    string sub = info1.Value.serieName;
                    sub = sub.Substring(sub.Length - 2);
                    if (sub == " A")
                        info1.Value.serieName = info1.Value.serieName.Substring(0, info1.Value.serieName.Length - 2);
                }
            }
            foreach (var info1 in serieInfos)
            {
                Console.WriteLine("{0} = {1} + {2}", info1.Value.serieNameVVB, info1.Value.serieName, info1.Value.poule);
            }

                // Build up info on how the teams are linked
                foreach (var club in clubs)
            {
                string clubId = club.Attribute("Id").Value;
                string clubName = club.Attribute("Name").Value;

                var teams = club.Element("Teams").Elements("Team");
                foreach (var t1 in teams)
                {
                    TeamInfo inf = new TeamInfo();
                    inf.teamId = int.Parse(t1.Attribute("Id").Value);
                    inf.teamName = t1.Attribute("Name").Value;
                    inf.SerieId = int.Parse(t1.Attribute("SerieId").Value);
                    inf.clubId = int.Parse(clubId);
                    inf.clubName = clubName;
                    string parentIdstr = t1.Attribute("parent_id").Value;
                    if (parentIdstr.Length == 0 || parentIdstr == "0" /*|| !teamInfo.ContainsKey(int.Parse(parentIdstr))*/)
                    {
                        inf.registrationTeam = inf;
                    }
                    else
                    {
                        int parentId = int.Parse(parentIdstr);
                        inf.registrationTeam = teamInfo[parentId];
                    }
                    if (matchesToBePlayed.Keys.Contains(inf.teamId))
                        inf.registrationTeam.matchesToBePlayed = matchesToBePlayed[inf.teamId];
                    if (reeksMatchesExist.Contains(inf.SerieId) == false)
//                    if (inf.SerieId >= maxSerieIdWithMatches)
                    {
                        inf.newRegistration = true;
                        if (inf.registrationTeam != inf)
                            inf.registrationTeam.derivedNewRegistrationFromThis = true;
                    } else if (inf.registrationTeam != inf)
                    { 
                        inf.registrationTeam.derivedExistingTeamFromThis = true;
                        t1.Attribute("SerieId").SetValue(inf.registrationTeam.SerieId);
                        t1.Attribute("SerieName").SetValue(serieInfos[inf.registrationTeam.SerieId].serieNameVVB);
                    }
                    teamInfo.Add(inf.teamId, inf);
                    var attr = t1.Attribute("EvenOdd");
                    if(attr == null)
                    {
                        XAttribute eo = new XAttribute("EvenOdd", "");
                        t1.Add(eo);
                    }
                    inf.evenOdd = t1.Attribute("EvenOdd").Value;
                    // Copy even/odd info from registration team
                    if (inf.registrationTeam.matchesToBePlayed > 0)
                    {
                        t1.Attribute("EvenOdd").SetValue(inf.registrationTeam.evenOdd);
                    }
                }
            }

            // Duplicate constraints for the new teams
            foreach (var t in teamInfo)
            {
                var team = t.Value;
                if (team.newRegistration == false && team.registrationTeam.matchesToBePlayed == 0)
                    continue;
                clubs = doc.Element("Registrations").Element("Clubs").Elements("Club");
                foreach (var club in clubs)
                {
                    var constraints = club.Element("TeamConstraints").Elements("TeamConstraint");
                    foreach (var con in constraints)
                    {
                        XElement duplicate = con;
                        if (int.Parse(duplicate.Attribute("Team1Id").Value) == team.registrationTeam.teamId && team.teamId != team.registrationTeam.teamId)
                        {
                            duplicate.Attribute("Team1Id").SetValue(team.teamId);
                            club.Element("TeamConstraints").Add(duplicate);
                        }
                        else if (int.Parse(duplicate.Attribute("Team2Id").Value) == team.registrationTeam.teamId && team.teamId != team.registrationTeam.teamId)
                        {
                            duplicate.Attribute("Team2Id").SetValue(team.teamId);
                            club.Element("TeamConstraints").Add(duplicate);
                        }
                    }
                }
            }

            // Remove teams that are not needed               
            foreach (var club in clubs)
            {
                List<XElement> teamsToDelete = new List<XElement>();
                var teams = club.Element("Teams").Elements("Team");
                foreach (var t1 in teams)
                {
                    int teamId = int.Parse(t1.Attribute("Id").Value);
                    int serieId = int.Parse(t1.Attribute("SerieId").Value);
                    var info = teamInfo[teamId];
                    bool rootTeam = info.registrationTeam == info;
                    // Van de oude teams zoveel mogelijk weggooien
                    if (reeksMatchesExist.Contains(serieId))
//                        if (serieId < maxSerieIdWithMatches)
                    {
                        // Poules that were created, but not used anymore in the second half
                        if (!rootTeam && info.registrationTeam.matchesToBePlayed == 0)
                            teamsToDelete.Add(t1);
                    }
                    if (rootTeam && info.derivedNewRegistrationFromThis)
                        teamsToDelete.Add(t1);
                    // Registration team from which a derived team exists that still has to play
                    else if (rootTeam && info.derivedExistingTeamFromThis && info.registrationTeam.matchesToBePlayed > 0)
                        teamsToDelete.Add(t1);
                    // Team dat in de eerste ronde aanwezig was, maar nu niet meer (dus deleted)
                    else if (rootTeam && !info.newRegistration && info.registrationTeam.matchesToBePlayed == 0)
                        teamsToDelete.Add(t1);
                }
                foreach (var t1 in teamsToDelete)
                    t1.Remove();
            }

            doc.Save(outDir + "inschrijvingen_converted.xml");

            StreamWriter writer = new StreamWriter(outDir + "competition_converted.csv");
            GenerateCompetition(writer, wedstrijden_limburg);
            GenerateCompetition(writer, wedstrijden_nationaal);
            writer.Close();
        }
        static void GenerateCompetition(StreamWriter writer, IEnumerable<XElement> wedstrijden)
        {
            foreach (var wedstrijd in wedstrijden)
            {
                if (wedstrijd.Element("thuisploeg_id") == null)
                    continue;
                if (wedstrijd.Element("bezoekersploeg_id") == null)
                    continue;
                var thuisPloegId = int.Parse(wedstrijd.Element("thuisploeg_id").Value);
                var bezoekersPloegId = int.Parse(wedstrijd.Element("bezoekersploeg_id").Value);
                // Are it relevant teams?
                if (!teamInfo.Keys.Contains(thuisPloegId) && !teamInfo.Keys.Contains(bezoekersPloegId))
                    continue;
                // Are there still matches to be played
                if (teamInfo.Keys.Contains(thuisPloegId) && teamInfo[thuisPloegId].registrationTeam.matchesToBePlayed == 0)
                    continue;
                if (teamInfo.Keys.Contains(bezoekersPloegId) && teamInfo[bezoekersPloegId].registrationTeam.matchesToBePlayed == 0)
                    continue;

                var thuisPloeg = wedstrijd.Element("thuisploeg").Value;
                var bezoekersPloeg = wedstrijd.Element("bezoekersploeg").Value;
                string thuisclubname = "??";
                int thuisclubId = 0;
                string bezoekersclubname = "??";
                int bezoekersclubId = 0;
                if (teamInfo.Keys.Contains(thuisPloegId))
                {
                    if (teamInfo[thuisPloegId].registrationTeam.matchesToBePlayed == 0)
                        continue;
                    var thuisPloegInfo = teamInfo[thuisPloegId];
                    thuisPloeg = thuisPloegInfo.teamName;
                    thuisclubname = thuisPloegInfo.clubName;
                    thuisclubId = thuisPloegInfo.clubId;
                } 
                if (teamInfo.Keys.Contains(bezoekersPloegId))
                {
                    if (teamInfo[bezoekersPloegId].registrationTeam.matchesToBePlayed == 0)
                        continue;
                    var bezoekersPloegInfo = teamInfo[bezoekersPloegId];
                    bezoekersPloeg = bezoekersPloegInfo.teamName;
                    bezoekersclubname = bezoekersPloegInfo.clubName;
                    bezoekersclubId = bezoekersPloegInfo.clubId;
                }

                var serieId = int.Parse(wedstrijd.Element("reeksid").Value);
                var poule = serieInfos[serieId].poule;
                var serieName = serieInfos[serieId].serieName;


                var sporthal = wedstrijd.Element("sporthal").Value.Replace(",", " ");
                var sporthalId = -1;

                DateTime date;
                DateTime.TryParseExact(wedstrijd.Element("datum").Value, "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

                //DateTime date = DateTime.Parse(wedstrijd.Element("datum").Value);
                var aanvangsuur = wedstrijd.Element("aanvangsuur").Value;
                writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
                    serieName, serieId, poule, -1, thuisPloeg, thuisPloegId, bezoekersPloeg, bezoekersPloegId, sporthal, sporthalId, date.ToString("yyyy-MM-dd"), aanvangsuur, thuisclubname, thuisclubId, bezoekersclubname, bezoekersclubId);
            }
        }

    }
}
