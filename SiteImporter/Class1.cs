using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Globalization;

namespace SiteImporter
{

    public class TeamInfo
    {
        public int teamId = 0;
        public int parentId = 0;
        public string teamName = "";
        public int SerieId = 0;
        public int clubId = 0;
        public string clubName = "";
        public string evenOdd = "";
        public bool vervolgReeks = false;
        public bool generateMatches = false;
    }

    public class ProvincieInfo
    {
        public int Id;
        public string Name;
        public bool RemoveTeams;
    }

    public class SiteImporter
    {
        public static ProvincieInfo[] provincies = {
            new ProvincieInfo { Name = "LIMBURG", Id = 4 , RemoveTeams = false},
            new ProvincieInfo { Name = "VLAAMS-BRABANT", Id = 7 , RemoveTeams = false },
            new ProvincieInfo { Name = "ANTWERPEN", Id = 1 , RemoveTeams = false },
            new ProvincieInfo { Name = "WEST-VLAANDEREN", Id = 9 , RemoveTeams = false },
            new ProvincieInfo { Name = "OOST-VLAANDEREN", Id = 5 , RemoveTeams = false },
            new ProvincieInfo { Name = "VOLLEY VLAANDEREN", Id = 11 , RemoveTeams = true }
        };
        // https://www.volleyadmin2.be/download/seriesubscriptions/5_11/  Divisie inschrijvingen voor Limburg
        // http://volleyadmin2.be/services/series_xml.php?province_id=4&all=1  voor alle series (zodat ik de id's heb)
        static public Dictionary<int, string> serieNames = new Dictionary<int,string>();
        static public Dictionary<int, TeamInfo> teamInfo = new Dictionary<int, TeamInfo>();
        static public Dictionary<int, int> matchesToBePlayed = new Dictionary<int, int>();
        static string outDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CompetitionCreator\\";
        static DateTime generationDate = DateTime.Now.AddDays(30);
        static public List<int> reeksMatchesExist = new List<int>();
        static public Dictionary<int, int> teamIdMapping = new Dictionary<int, int>();
        static void CheckMatchesToBePlayed(IEnumerable<XElement> wedstrijden)
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
                }
                if (reeksMatchesExist.Contains(reeksId) == false)
                    reeksMatchesExist.Add(reeksId);
            }
        }


        public static void ImportSite(ProvincieInfo provincie)
        {
            bool nationaal = provincie.RemoveTeams; // For nationaal the teams are removed that are not part of volley vlaanderen
            serieNames = new Dictionary<int, string>();
            teamInfo = new Dictionary<int, TeamInfo>();
            matchesToBePlayed = new Dictionary<int, int>();
            reeksMatchesExist = new List<int>();
            teamIdMapping = new Dictionary<int, int>();

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

            // Rangschikking is eruit gehaald. Wat doen om dit terug te krijgen:
            // Score gaat naar de parent toe. Nieuwe team neem score van parent over. Hoe controleer je of het team van reeks is veranderd. Zou nieuw team moeten zijn.


            // Provincies:
            // http://www.volleyadmin2.be/services/wedstrijden_xml.php?province_id=1 A?- Antwerpen?
            // http://www.volleyadmin2.be/services/wedstrijden_xml.php?province_id=0 Liga
            // http://www.volleyadmin2.be/services/wedstrijden_xml.php?province_id=5 O-  (Oostvlaanderen)
            // http://www.volleyadmin2.be/services/wedstrijden_xml.php?province_id=6 RO- 
            // http://www.volleyadmin2.be/services/wedstrijden_xml.php?province_id=7 VB- Vlaams brabant
            // http://www.volleyadmin2.be/services/wedstrijden_xml.php?province_id=9 W-  West-vlaanderen
            // http://www.volleyadmin2.be/services/wedstrijden_xml.php?province_id=10 BRV- Brugs Recreatief



            int year = DateTime.Now.AddMonths(-2).Year;
            // Inschrijvingen alle reeksen alle provincies zonder beker
            string filename = "https://www.volleyadmin2.be/download/seriesubscriptions/" + (year - 2013).ToString() + "_0_1/";
            XDocument doc = XDocument.Load(filename, LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);

            // Wedstrijden van geselecteerde provincie + alle wedstrijden nationaal
            XDocument doc_provincie = XDocument.Load("http://www.volleyadmin2.be/services/wedstrijden_xml.php?province_id=" + provincie.Id.ToString(), LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
            var wedstrijden = doc_provincie.Element("kalender").Elements("wedstrijd");
            CheckMatchesToBePlayed(wedstrijden);
            XDocument doc_nationaal = XDocument.Load("http://www.volleyadmin2.be/services/wedstrijden_xml.php?province_id=11", LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
            var wedstrijden_nationaal = doc_nationaal.Element("kalender").Elements("wedstrijd");
            CheckMatchesToBePlayed(wedstrijden_nationaal);

            List<XElement> toBeRemoved = new List<XElement>();
            var clubs = doc.Element("Registrations").Element("Clubs").Elements("Club");

            // Remove clubs outside the province and remove beker teams
            foreach (var club in clubs)
            {
                bool remove = true;
                if (club.Element("Teams") != null)
                {
                    var teams = club.Element("Teams").Elements("Team");
                    List<XElement> teamsToBeRemoved = new List<XElement>();
                    foreach (var t in teams)
                    {
                        if (t.Attribute("Province") != null)
                        {
                            var prov = t.Attribute("Province").Value.ToString();
                            if (prov == provincie.Name)
                                remove = false;
                            else if (provincie.RemoveTeams)
                                teamsToBeRemoved.Add(t);
                        }
                        

                    }
                    if (remove)
                        toBeRemoved.Add(club);

                    foreach (var t in teams)
                    {
                        var serieName = t.Attribute("SerieName").Value;
                        if ((serieName.Contains("Beker") || serieName.Contains("beker")) && !teamsToBeRemoved.Contains(t))
                            teamsToBeRemoved.Add(t);
                    }
                    foreach (var t in teamsToBeRemoved)
                        t.Remove();
                }
                else
                    toBeRemoved.Add(club);

            }
            foreach (var club in toBeRemoved)
                club.Remove();

            // Create lookup for teamId -> teamName
            foreach (var club in clubs)
            {
                if (club.Element("Teams") != null)
                {

                    var teams = club.Element("Teams").Elements("Team");
                    foreach (var t1 in teams)
                    {
                        var serieId = int.Parse(t1.Attribute("SerieId").Value);
                        string serieName = t1.Attribute("SerieName").Value;
                        if (serieName == "")
                            serieName = "Reeks zonder naam";

                        if (!serieNames.ContainsKey(serieId))
                            serieNames.Add(serieId, serieName);
                    }
                }
            }
            // Read all information to handle crosslinks that have forward linkage
            foreach (var club in clubs)
            {
                string clubId = club.Attribute("Id").Value;
                string clubName = club.Attribute("Name").Value;
                if (club.Element("Teams") != null)
                {

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
                        if (parentIdstr.Length == 0 || parentIdstr == "0")
                        {
                            inf.parentId = inf.teamId; // Of parentId 0
                        }
                        else
                        {
                            inf.parentId = int.Parse(parentIdstr);
                        }
                        teamInfo.Add(inf.teamId, inf);
                        // Ensure that EvenOdd is allways present and store it in the info
                        var attr = t1.Attribute("EvenOdd");
                        if (attr == null)
                        {
                            XAttribute eo = new XAttribute("EvenOdd", "");
                            t1.Add(eo);
                        }
                        inf.evenOdd = t1.Attribute("EvenOdd").Value;
                    }
                    // Verwijder teams waar de parent niet voor bestaat
                    List<XElement> teamsToDelete = new List<XElement>();

                    teams = club.Element("Teams").Elements("Team");
                    foreach (var t1 in teams)
                    {
                        var teamId = int.Parse(t1.Attribute("Id").Value);
                        TeamInfo inf = teamInfo[teamId];
                        if (teamInfo.ContainsKey(inf.parentId))
                            t1.Attribute("EvenOdd").Value = teamInfo[inf.parentId].evenOdd;
                        else
                        {
                            Console.WriteLine("Warning: Team parent Id does not exist (anymore)");
                            teamInfo.Remove(teamId);
                            teamsToDelete.Add(t1);
                            continue;
                        }
                    }
                    foreach (var t1 in teamsToDelete)
                        t1.Remove();
                    // Verwijder (teams in) vervolg reeksen die wedstrijden hadden, maar nu niet meer (oude reeksen) - behalve hoofdreeksen
                    teamsToDelete = new List<XElement>();

                    teams = club.Element("Teams").Elements("Team");
                    foreach (var t1 in teams)
                    {
                        var teamId = int.Parse(t1.Attribute("Id").Value);
                        var serieId = int.Parse(t1.Attribute("SerieId").Value);
                        TeamInfo inf = teamInfo[teamId];
                        if (inf.parentId != teamId)
                        {
                            if (!matchesToBePlayed.ContainsKey(teamId) && reeksMatchesExist.Contains(serieId))
                                teamsToDelete.Add(t1);
                            else
                                teamInfo[inf.parentId].vervolgReeks = true;
                        }
                    }
                    foreach (var t1 in teamsToDelete)
                        t1.Remove();
                    // Verwijder teams in hoofdreeksen waarvan vervolg reeksen bestaan zonder wedstrijden.
                    teamsToDelete = new List<XElement>();
                    teams = club.Element("Teams").Elements("Team");
                    foreach (var t1 in teams)
                    {
                        var teamId = int.Parse(t1.Attribute("Id").Value);
                        TeamInfo inf = teamInfo[teamId];
                        if (inf.vervolgReeks == true)
                            teamsToDelete.Add(t1);
                    }
                    foreach (var t1 in teamsToDelete)
                        t1.Remove();
                    // Administrate for which teams matches must be generated.
                    teams = club.Element("Teams").Elements("Team");
                    foreach (var t1 in teams)
                    {
                        var teamId = int.Parse(t1.Attribute("Id").Value);
                        TeamInfo inf = teamInfo[teamId];
                        inf.generateMatches = true;
                    }
                    // Create team id mapping (from original to new team)
                    teams = club.Element("Teams").Elements("Team");
                    foreach (var t1 in teams)
                    {
                        var teamId = int.Parse(t1.Attribute("Id").Value);
                        TeamInfo inf = teamInfo[teamId];
                        if (teamId != inf.parentId)
                        {
                            if (teamIdMapping.ContainsKey(teamId))
                                Console.WriteLine("Warning: A team is mapped multiple times!");
                            teamIdMapping[inf.parentId] = teamId;
                        }
                    }
                }
            }
            // Rewrite constraints for the new teams Ids
            clubs = doc.Element("Registrations").Element("Clubs").Elements("Club");
            foreach (var club in clubs)
            {
                if (club.Element("TeamConstraints") != null)
                {
                    var constraints = club.Element("TeamConstraints").Elements("TeamConstraint");
                    foreach (var con in constraints)
                    {
                        var team1Id = int.Parse(con.Attribute("Team1Id").Value);
                        var team2Id = int.Parse(con.Attribute("Team2Id").Value);
                        if (teamIdMapping.ContainsKey(team1Id))
                            con.Attribute("Team1Id").Value = teamIdMapping[team1Id].ToString();
                        if (teamIdMapping.ContainsKey(team2Id))
                            con.Attribute("Team2Id").Value = teamIdMapping[team2Id].ToString();
                    }
                }
            }
            doc.Save(outDir + provincie.Name + "_inschrijvingen.xml");

            StreamWriter writer = new StreamWriter(outDir + provincie.Name + "_huidige_wedstrijden.csv");
            if (!nationaal) GenerateCompetition(writer, wedstrijden);
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
                var thuisPloeg = wedstrijd.Element("thuisploeg").Value;
                var bezoekersPloeg = wedstrijd.Element("bezoekersploeg").Value;
                if (!teamInfo.Keys.Contains(thuisPloegId) && !teamInfo.Keys.Contains(bezoekersPloegId))
                    continue;
                string thuisclubname = "??";
                int thuisclubId = 0;
                string bezoekersclubname = "??";
                int bezoekersclubId = 0;
                if (teamInfo.Keys.Contains(thuisPloegId))
                {
                    if (!teamInfo[thuisPloegId].generateMatches)
                        continue;
                    var thuisPloegInfo = teamInfo[thuisPloegId];
                    thuisPloeg = thuisPloegInfo.teamName;
                    thuisclubname = thuisPloegInfo.clubName;
                    thuisclubId = thuisPloegInfo.clubId;
                }
                if (teamInfo.Keys.Contains(bezoekersPloegId))
                {
                    if (!teamInfo[bezoekersPloegId].generateMatches)
                        continue;
                    var bezoekersPloegInfo = teamInfo[bezoekersPloegId];
                    bezoekersPloeg = bezoekersPloegInfo.teamName;
                    bezoekersclubname = bezoekersPloegInfo.clubName;
                    bezoekersclubId = bezoekersPloegInfo.clubId;
                }

                var serieId = int.Parse(wedstrijd.Element("reeksid").Value);
                var poule = "-"; // All imported teams have - as poule (serie is unique)
                var serieName = serieNames[serieId];


                var sporthal = wedstrijd.Element("sporthal").Value.Replace(",", " ");
                var sporthalId = 0;

                DateTime date;
                DateTime.TryParseExact(wedstrijd.Element("datum").Value, "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

                var aanvangsuur = wedstrijd.Element("aanvangsuur").Value;
                writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
                    serieName, serieId, poule, -1, thuisPloeg, thuisPloegId, bezoekersPloeg, bezoekersPloegId, sporthal, sporthalId, date.ToString("yyyy-MM-dd"), aanvangsuur, thuisclubname, thuisclubId, bezoekersclubname, bezoekersclubId);
            }
        }
    }
}
