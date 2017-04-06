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
        public string orgTeamId = "";
        public string usedByTeamId = "";
        public string teamId = "";
        public string teamName = "";
        public string SerieId = "";
        public string SerieName = "";
        public string clubId = "";
        public string clubName = "";
        public string poule = "";
        public int matchCount = 0;
    }

    // http://volleyadmin2.be/services/series_xml.php?province_id=4&all=1  voor alle series (zodat ik de id's heb)
    class Program
    {
        static public Dictionary<string, TeamInfo> teamInfo = new Dictionary<string, TeamInfo>();

        static public TeamInfo orgInfo(string TeamId)
        {
            if (teamInfo.ContainsKey(TeamId) == false)
                return null;
            TeamInfo inf = teamInfo[TeamId];
            while (inf.orgTeamId.Length > 0 && inf.orgTeamId != teamInfo[inf.orgTeamId].orgTeamId)
                inf = teamInfo[inf.orgTeamId];
            return inf;
        }
        static public bool TeamStillUsed(string TeamId)
        {
            if (orgInfo(TeamId) == null)
                return false;
            return orgInfo(TeamId).usedByTeamId == TeamId;
        }

        static void Main(string[] args)
        {
            List<string> Teams = new List<string>();
            //string filename = "C:\\Users\\Giel\\Documents\\CompetitionCreator\\KLVV competitie 2016_2017\\2e helft\\inschrijvingen_20161210.xml";
            string filename = "http://volleyadmin2.be/download/seriesubscriptions/3_4_1/";
            XDocument doc = XDocument.Load(filename, LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
            var clubs = doc.Element("Registrations").Element("Clubs").Elements("Club");
            foreach (var club in clubs)
            {
                string clubId = club.Attribute("Id").Value;
                string clubName = club.Attribute("Name").Value;

                var teams = club.Element("Teams").Elements("Team");
                foreach (var t1 in teams)
                {
                    TeamInfo inf = new TeamInfo();
                    inf.teamId = t1.Attribute("Id").Value;
                    inf.teamName = t1.Attribute("Name").Value;
                    string parentId = t1.Attribute("parent_id").Value;
                    inf.SerieId = t1.Attribute("SerieId").Value;
                    inf.clubId = clubId;
                    inf.clubName = clubName;
                    inf.usedByTeamId = inf.teamId;
                    inf.poule = GetReeksNamePouleLetter(t1.Attribute("SerieName").Value, ref inf.SerieName);
                    teamInfo.Add(inf.teamId, inf);
                    if (parentId.Length > 1)
                    {
                        var orgTeam = orgInfo(parentId);
                        if (orgTeam == null || orgTeam.clubId != inf.clubId)
                        {
                            Console.WriteLine("Error: the parentId {0} is not from this team {1}", parentId, inf.teamId);
                        }
                        else
                        {
                            inf.orgTeamId = parentId;
                            if(int.Parse(orgTeam.usedByTeamId) < int.Parse(inf.teamId))
                            {
                                orgTeam.usedByTeamId = inf.teamId;
                                orgTeam.poule = inf.poule;
                            }
                        }
                    }
                    //else
                    //    inf.orgTeamId = inf.teamId;
                }
            }
            // New constraints for teams can refer to the old teams. These should be translated.
            clubs = doc.Element("Registrations").Element("Clubs").Elements("Club");
            foreach (var club in clubs)
            {
                var constraints = club.Element("TeamConstraints").Elements("TeamConstraint");
                foreach (var con in constraints)
                {
                    try
                    {
                        var teamId1 = con.Attribute("Team1Id");
                        teamId1.SetValue(orgInfo(teamId1.Value).teamId);
                        var teamId2 = con.Attribute("Team2Id");
                        teamId2.SetValue(orgInfo(teamId2.Value).teamId);
                    }
                    catch
                    {
                        con.Remove(); // in case team id's do not exist
                    }
                }
            }
            string outname = "C:\\Users\\Giel\\Documents\\CompetitionCreator\\KLVV competitie 2016_2017\\2e helft\\ranking_converted.xml";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(outname))
            {
                file.WriteLine("<Rankings>");
                XDocument doc1 = XDocument.Load("http://www.volleyadmin2.be/services/rangschikking_xml.php?province_id=4", LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
                var rangschikkingen = doc1.Element("klassement").Elements("rangschikking");
                foreach (var rangschikking in rangschikkingen)
                {
                    var gespeeldewedstrijden = int.Parse(rangschikking.Element("aantalGespeeldeWedstrijden").Value);
                    var ploegId = rangschikking.Element("ploegid").Value;
                    if (gespeeldewedstrijden > 0)
                    {
                        orgInfo(ploegId).matchCount = gespeeldewedstrijden;
                        var rank = rangschikking.Element("volgorde").Value;
                        var punten = int.Parse(rangschikking.Element("puntentotaal").Value);
                        rank = rank.Replace(".", "");
                        file.WriteLine("  <Ranking serieId='{0}' serieName='{1}' pouleName='' teamId='{2}' teamName='' ranking='{3}' score='{4}' /> ",
                                orgInfo(ploegId).teamId,
                                orgInfo(ploegId).SerieName, // newSerieName waarom was dit nieuwseriename
                                orgInfo(ploegId).teamId,
                                rank,
                                punten);
                    }
                }
                file.WriteLine("</Rankings>");
                file.Close();
            }

            foreach (var club in clubs)
            {
                List<XElement> teamsToDelete = new List<XElement>();
                var teams = club.Element("Teams").Elements("Team");
                foreach (var t1 in teams)
                {
                    string teamId = t1.Attribute("Id").Value;
                    //XAttribute attr1 = new XAttribute("Poule", teamInfo[teamId].poule);
                    //t1.Add(attr1);
                    if (teamInfo[teamId].orgTeamId != "")
                        teamsToDelete.Add(t1);
                    else
                        if (teamInfo[teamId].matchCount == 0)
                        {
                            XAttribute attr = new XAttribute("Deleted", "true");
                            t1.Add(attr);
                        }
                }
                foreach (var t1 in teamsToDelete)
                    t1.Remove();
            }
            doc.Save("C:\\Users\\Giel\\Documents\\CompetitionCreator\\KLVV competitie 2016_2017\\2e helft\\inschrijvingen_converted.xml");

            StreamWriter writer = new StreamWriter("C:\\Users\\Giel\\Documents\\CompetitionCreator\\KLVV competitie 2016_2017\\2e helft\\competition_converted.csv");
            XDocument doc2 = XDocument.Load("http://www.volleyadmin2.be/services/wedstrijden_xml.php?province_id=4", LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
            var wedstrijden = doc2.Element("kalender").Elements("wedstrijd");
            foreach (var wedstrijd in wedstrijden)
            {
                if(wedstrijd.Element("thuisploeg_id") == null)
                    continue;
                if (wedstrijd.Element("bezoekersploeg_id") == null)
                    continue;
                var thuisPloegId = wedstrijd.Element("thuisploeg_id").Value;
                var bezoekersPloegId = wedstrijd.Element("bezoekersploeg_id").Value;
                if (!TeamStillUsed(thuisPloegId))
                    continue;
                if (!TeamStillUsed(bezoekersPloegId))
                    continue;
                var thuisPloegInfo = orgInfo(thuisPloegId);
                var bezoekersPloegInfo = orgInfo(bezoekersPloegId);
                var thuisPloeg = thuisPloegInfo.teamName;
                var bezoekersPloeg = bezoekersPloegInfo.teamName;
                var poule = thuisPloegInfo.poule;

                var serieName = teamInfo[thuisPloegId].SerieName; //thuisPloegInfo.SerieName;
                var serieId = teamInfo[thuisPloegId].SerieId;     // thuisPloegInfo.SerieId;
                var sporthal = wedstrijd.Element("sporthal").Value.Replace(",", " ");
                var sporthalId = -1;
                DateTime date = DateTime.Parse(wedstrijd.Element("datum").Value);
                var aanvangsuur = wedstrijd.Element("aanvangsuur").Value;
                string thuisclubname = thuisPloegInfo.clubName;
                string thuisclubId = thuisPloegInfo.clubId;
                string bezoekersclubname = bezoekersPloegInfo.clubName;
                string bezoekersclubId = bezoekersPloegInfo.clubId;
                writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
                    serieName, serieId, poule, -1, thuisPloeg, thuisPloegInfo.teamId, bezoekersPloeg, bezoekersPloegInfo.teamId, sporthal, sporthalId, date.ToShortDateString(), aanvangsuur, thuisclubname, thuisclubId, bezoekersclubname, bezoekersclubId);
            }
            writer.Close();


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

    }
}
