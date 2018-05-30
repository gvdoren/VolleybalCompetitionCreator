using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace XmlTransformer
{
    public class TeamInfo
    {
        public TeamInfo registrationTeam = null;
        //public string usedByTeamId = "";
        public int teamId = 0;
        public string teamName = "";
        public int SerieId = 0;
        public string SerieName = "";
        public int clubId = 0;
        public string clubName = "";
        public string poule = "";
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
    // https://www.volleyadmin2.be/download/seriesubscriptions/5_11/  Divisie inschrijvingen voor Limburg
    // http://volleyadmin2.be/services/series_xml.php?province_id=4&all=1  voor alle series (zodat ik de id's heb)
    class Program
    {
        static public Dictionary<int, TeamInfo> teamInfo = new Dictionary<int, TeamInfo>();
        static public Dictionary<int,int> matchesToBePlayed = new Dictionary<int,int>();
        static int MIN_NEW_SERIE_ID = 1973;
        static string startDate="1/1/2018";
        static string outDir = "C:\\Users\\Giel\\Documents\\CompetitionCreator\\KLVV 2017_2018\\2e ronde\\";

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
                DateTime date = DateTime.Parse(wedstrijd.Element("datum").Value);
                if (date > DateTime.Parse(startDate))
                {
                    if (matchesToBePlayed.Keys.Contains(thuisPloegId) == false)
                        matchesToBePlayed.Add(thuisPloegId, 0);
                    if (matchesToBePlayed.Keys.Contains(bezoekersPloegId) == false)
                        matchesToBePlayed.Add(bezoekersPloegId, 0);
                    matchesToBePlayed[thuisPloegId]++;
                    matchesToBePlayed[bezoekersPloegId]++;
                }
            }
        }

        static void Main(string[] args)
        {
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

            string filename = "https://www.volleyadmin2.be/download/seriesubscriptions/5_0_1/";
            XDocument doc = XDocument.Load(filename, LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
            XDocument doc_limburg = XDocument.Load("http://www.volleyadmin2.be/services/wedstrijden_xml.php?province_id=4", LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
            var wedstrijden_limburg = doc_limburg.Element("kalender").Elements("wedstrijd");
            CheckMatchesToBePlayed(wedstrijden_limburg);
            XDocument doc_nationaal = XDocument.Load("http://www.volleyadmin2.be/services/wedstrijden_xml.php?province_id=11", LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
            var wedstrijden_nationaal = doc_nationaal.Element("kalender").Elements("wedstrijd");
            CheckMatchesToBePlayed(wedstrijden_nationaal);

            var clubs = doc.Element("Registrations").Element("Clubs").Elements("Club");
            foreach (var club in clubs)
            {
                string clubId = club.Attribute("Id").Value;
                string clubName = club.Attribute("Name").Value;

                var teams = club.Element("Teams").Elements("Team");
                foreach (var t1 in teams)
                {
                    TeamInfo inf = new TeamInfo();
                    inf.reeks = GetReeks(t1.Attribute("SerieName").Value);
                    inf.teamId = int.Parse(t1.Attribute("Id").Value);
                    inf.teamName = t1.Attribute("Name").Value;
                    inf.SerieId = int.Parse(t1.Attribute("SerieId").Value);
                    inf.clubId = int.Parse(clubId);
                    inf.clubName = clubName;
                    inf.poule = GetReeksNamePouleLetter(t1.Attribute("SerieName").Value, ref inf.SerieName);
                    string parentIdstr = t1.Attribute("parent_id").Value;
                    if (parentIdstr.Length == 0 || parentIdstr == "0")
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
                    if (inf.SerieId >= MIN_NEW_SERIE_ID)
                    {
                        inf.newRegistration = true;
                        inf.registrationTeam.derivedNewRegistrationFromThis = true;
                    }
                    if (inf.registrationTeam != inf)
                        inf.registrationTeam.derivedExistingTeamFromThis = true;
                    // Set the original serie Id
                    if(inf.SerieId<MIN_NEW_SERIE_ID && inf.registrationTeam != inf)
                    {
                        t1.Attribute("SerieId").SetValue(inf.registrationTeam.SerieId);
                        t1.Attribute("SerieName").SetValue(inf.registrationTeam.SerieName);
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
            // Generate rankings for all teams that have a ranking
            string outname = outDir+"ranking_converted.xml";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(outname))
            {
                XDocument doc1 = XDocument.Load("http://www.volleyadmin2.be/services/rangschikking_xml.php?province_id=4", LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
                var rangschikkingen = doc1.Element("klassement").Elements("rangschikking");
                Dictionary<int, int> teamsPerSerie = new Dictionary<int, int>();
                foreach (var rangschikking in rangschikkingen)
                {
                    var gespeeldewedstrijden = int.Parse(rangschikking.Element("aantalGespeeldeWedstrijden").Value);
                    if (gespeeldewedstrijden > 0)
                    {
                        var ploegId = int.Parse(rangschikking.Element("ploegid").Value);
                        var rank = rangschikking.Element("volgorde").Value;
                        var punten = int.Parse(rangschikking.Element("puntentotaal").Value);
                        rank = rank.Replace(".", "");
                        teamInfo[ploegId].registrationTeam.ranking = rank.ToString();
                        teamInfo[ploegId].registrationTeam.score = punten.ToString();
                        teamInfo[ploegId].matchCount = gespeeldewedstrijden;
                        if (teamsPerSerie.ContainsKey(teamInfo[ploegId].SerieId) == false)
                            teamsPerSerie.Add(teamInfo[ploegId].SerieId, 0);
                        teamsPerSerie[teamInfo[ploegId].SerieId]++;
                    }
                }
                //foreach (var rangschikking in rangschikkingen)
                //{
                //    var gespeeldewedstrijden = int.Parse(rangschikking.Element("aantalGespeeldeWedstrijden").Value);
                //    if (gespeeldewedstrijden > 0)
                //    {
                //        var ploegId = int.Parse(rangschikking.Element("ploegid").Value); 
                //        var team = teamInfo[ploegId];
                //        if (team.reeks != team.registrationTeam.reeks) // if team has changed
                //            continue;
                //        string extra = "";
                //        //if (team.matchCount != (teamsPerSerie[team.SerieId] - 1) * 2)
                //        //    extra = "?";
                //        file.WriteLine("  <Ranking serieId='{0}' serieName='{1}' pouleName='' teamId='{2}' teamName='' ranking='{3}' score='{4}' /> ",
                //                        team.SerieId,
                //                        team.SerieName, // newSerieName waarom was dit nieuwseriename
                //                        team.teamId,
                //                        team.registrationTeam.ranking,
                //                        team.registrationTeam.score+extra);
                //    }
                //}
                
                file.WriteLine("<Rankings>");
                foreach (var club in clubs)
                {
                    List<XElement> teamsToDelete = new List<XElement>();
                    var teams = club.Element("Teams").Elements("Team");
                    foreach (var t1 in teams)
                    {
                        int teamId = int.Parse(t1.Attribute("Id").Value);
                        var info = teamInfo[teamId];
                        bool isRegistrationTeam = info.registrationTeam == info;
                        if (!info.newRegistration)
                        {
                            // remove beker series
                            if (t1.Attribute("SerieName").Value.Contains("Beker"))
                                teamsToDelete.Add(t1);
                            else
                            // Poules that were created, but not used anymore in the second half
                            if (!isRegistrationTeam && info.registrationTeam.matchesToBePlayed == 0)
                                teamsToDelete.Add(t1);
                            // Registration team, maar er is al een nieuw team van afgeleid
                            else if (isRegistrationTeam && info.derivedNewRegistrationFromThis)
                                teamsToDelete.Add(t1);
                            // Registration team from which a derived team exists that still has to play
                            else if (isRegistrationTeam && info.derivedExistingTeamFromThis && info.registrationTeam.matchesToBePlayed > 0)
                                teamsToDelete.Add(t1);
                            // Team dat in de eerste ronde aanwezig was, maar nu niet meer (dus deleted)
                            else if (isRegistrationTeam && !info.newRegistration && info.registrationTeam.matchesToBePlayed == 0)
                            {
                                // Teams from divisie should not be deleted
//                                if (t1.Attribute("Province").Value == "LIMBURG")
                                {
                                    teamsToDelete.Add(t1);
                                    //XAttribute attr = new XAttribute("Deleted", "true");
                                    //t1.Add(attr);
                                }
                            }
                        }
                    }
                    foreach (var t1 in teamsToDelete)
                        t1.Remove();
                    teams = club.Element("Teams").Elements("Team");
                    foreach (var t1 in teams)
                    {
                        int teamId = int.Parse(t1.Attribute("Id").Value);
                        var info = teamInfo[teamId];
                        if (info != info.registrationTeam)
                        {
                            string commentStart = "";
                            string commentEnd = "";
                            if (info.reeks != info.registrationTeam.reeks)
                            {
                                commentStart = "<!-- Team changed from Serie.  Ranking not valid";
                                commentEnd = "-->";
                            }
                            if (info.registrationTeam.ranking != "" && info.registrationTeam.ranking != "") // when team & registration team are created in second round
                            {
                                file.WriteLine(commentStart + "  <Ranking serieId='{0}' serieName='{1}' pouleName='' teamId='{2}' teamName='' ranking='{3}' score='{4}' /> " + commentEnd,
                                    info.SerieId,
                                    info.SerieName, // newSerieName waarom was dit nieuwseriename
                                    info.teamId,
                                    info.registrationTeam.ranking,
                                    info.registrationTeam.score);
                            }
                        }
                    }

                }
                doc.Save(outDir+"inschrijvingen_converted.xml");
                file.WriteLine("</Rankings>");
                file.Close();

                StreamWriter writer = new StreamWriter(outDir+"competition_converted.csv");
                GenerateCompetition(writer, wedstrijden_limburg);
                GenerateCompetition(writer, wedstrijden_nationaal);
                writer.Close();


            }
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
                if (teamInfo.Keys.Contains(thuisPloegId) == false)
                    continue;
                if (teamInfo.Keys.Contains(bezoekersPloegId) == false)
                    continue;
                // Alleen competitie van teams die nog na januari spelen.
                if (teamInfo[thuisPloegId].registrationTeam.matchesToBePlayed == 0)
                    continue;
                if (teamInfo[bezoekersPloegId].registrationTeam.matchesToBePlayed == 0)
                    continue;
                var thuisPloegInfo = teamInfo[thuisPloegId];
                var bezoekersPloegInfo = teamInfo[bezoekersPloegId];
                var thuisPloeg = thuisPloegInfo.teamName;
                var bezoekersPloeg = bezoekersPloegInfo.teamName;
                var poule = thuisPloegInfo.poule;
                // De serie van het geregistreerde team moet genomen worden om goed te mappen op de inschrijvingen.
                var serieName = teamInfo[thuisPloegId].registrationTeam.SerieName; //thuisPloegInfo.SerieName;
                var serieId = teamInfo[thuisPloegId].registrationTeam.SerieId;     // thuisPloegInfo.SerieId;
                var sporthal = wedstrijd.Element("sporthal").Value.Replace(",", " ");
                var sporthalId = -1;
                DateTime date = DateTime.Parse(wedstrijd.Element("datum").Value);
                var aanvangsuur = wedstrijd.Element("aanvangsuur").Value;
                string thuisclubname = thuisPloegInfo.clubName;
                int thuisclubId = thuisPloegInfo.clubId;
                string bezoekersclubname = bezoekersPloegInfo.clubName;
                int bezoekersclubId = bezoekersPloegInfo.clubId;
                writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
                    serieName, serieId, poule, -1, thuisPloeg, thuisPloegInfo.teamId, bezoekersPloeg, bezoekersPloegInfo.teamId, sporthal, sporthalId, date.ToShortDateString(), aanvangsuur, thuisclubname, thuisclubId, bezoekersclubname, bezoekersclubId);
            }
        }

        static string GetReeksNamePouleLetter(string reeks, ref string reeksName)
        {
            var last = reeks.Substring(reeks.Length-2, 2);
            var letter = reeks.Substring(reeks.Length-1, 1);
            if(last == " A") { reeksName = reeks.Substring(0, reeks.Length-1); return letter; }
            if(last == " B") { reeksName = reeks.Substring(0, reeks.Length-1); return letter; }
            if(last == " C") { reeksName = reeks.Substring(0, reeks.Length-1); return letter; }
            if(last == " D") { reeksName = reeks.Substring(0, reeks.Length-1); return letter; }
            if(last == " E") { reeksName = reeks.Substring(0, reeks.Length-1); return letter; }
            if(last == " F") { reeksName = reeks.Substring(0, reeks.Length-1); return letter; }
            if(last == " G") { reeksName = reeks.Substring(0, reeks.Length-1); return letter; }
            if(last == " H") { reeksName = reeks.Substring(0, reeks.Length-1); return letter; }
            if(last == " I") { reeksName = reeks.Substring(0, reeks.Length-1); return letter; }
            if(last == " J") { reeksName = reeks.Substring(0, reeks.Length-1); return letter; }
            if(last == " K") { reeksName = reeks.Substring(0, reeks.Length-1); return letter; }
            if(last == " L") { reeksName = reeks.Substring(0, reeks.Length-1); return letter; }
            if(last == " M") { reeksName = reeks.Substring(0, reeks.Length-1); return letter; }
            if(last == " N") { reeksName = reeks.Substring(0, reeks.Length-1); return letter; }
            if(last == " O") { reeksName = reeks.Substring(0, reeks.Length-1); return letter; }
            if(last == " P") { reeksName = reeks.Substring(0, reeks.Length-1); return letter; }
            if(last == " Q") { reeksName = reeks.Substring(0, reeks.Length-1); return letter; }
            if(last == " R") { reeksName = reeks.Substring(0, reeks.Length-1); return letter; }
            reeksName = reeks;
            return "A";
        }
        static Random RandomGenerator = new Random();
        static string GetReeks(string reeks)
        {
        	if(reeks.Contains("U17")) return "U17";
        	if(reeks.Contains("U 17")) return "U17";
        	if(reeks.Contains("U15")) return "U15";
        	if(reeks.Contains("U13")) return "U13";
        	if(reeks.Contains("3 - 3")) return "3 - 3";
        	if(reeks.Contains("Vorm 4")) return "Vorm 4";
        	if(reeks.Contains("Vorm 5")) return "Vorm 5";
            return RandomGenerator.NextDouble().ToString(); // never matches
        }
    }
}
