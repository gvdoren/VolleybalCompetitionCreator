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
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, string> serieName_serieId = new Dictionary<string, string>();
            Dictionary<string, string> teamId_teamId = new Dictionary<string, string>();
            Dictionary<string, string> serieName_serieName = new Dictionary<string, string>();
            Dictionary<string, string> teamId_serieId = new Dictionary<string, string>();
            Dictionary<string, string> teamId_serieName = new Dictionary<string, string>();
            Dictionary<string, string> teamId_clubId = new Dictionary<string, string>();
            Dictionary<string, string> teamId_clubName = new Dictionary<string, string>();
            Dictionary<string, string> teamId_poule = new Dictionary<string, string>();

            serieName_serieName["Gewestelijke U-17 Heren"] = "Gewestelijke U17 Heren R2";
            serieName_serieName["Gewestelijke U-15 Heren"] = "Gewestelijke U15 Heren R2";
            serieName_serieName["Gewestelijke U-13 Heren"] = "Gewestelijke U13 Heren R2";
            serieName_serieName["U-11 3 tegen 3 Jongens"] = "U11 3 tegen 3 Jongens R2";

            serieName_serieName["Gewestelijke U-17 Dames"] = "Gewestelijke U17 Dames R2";
            serieName_serieName["Gewestelijke U-15 Dames"] = "Gewestelijke U15 Dames R2";
            serieName_serieName["Gewestelijke U-13 Dames"] = "Gewestelijke U13 Dames R2";
            serieName_serieName["U-11 3 tegen 3 Meisjes"] = "U11 3 tegen 3 Meisjes R2";
            serieName_serieName["U-11 2 tegen 2 Vorm 4 Dames"] = "U11 2 tegen 2 Vorm 4 Dames R2";

            List<string> deletedTeams = new List<string>();
            //string filename = "C:\\Users\\Giel\\Documents\\CompetitionCreator\\KLVV competitie 2016_2017\\2e helft\\inschrijvingen_20161210.xml";
            string filename = "http://volleyadmin2.be/download/seriesubscriptions/3_4_1/";
            XDocument doc = XDocument.Load(filename, LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
            var clubs = doc.Element("Registrations").Element("Clubs").Elements("Club");
            foreach (var club in clubs)
            {
                string clubId = club.Attribute("Id").Value;
                string clubName = club.Attribute("Name").Value;
                var sporthalls = club.Element("Sporthalls").Elements("Sporthall");
                foreach (var sporthall in sporthalls)
                {
                    // can also be arranged by first importing the registrations.
                }
                var teams = club.Element("Teams").Elements("Team");
                List<XElement> ToBeRemoved = new List<XElement>();
                foreach (var t1 in teams)
                {
                    string Id1 = t1.Attribute("Id").Value;
                    string serieName1 = t1.Attribute("SerieName").Value;
                    string serieId1 = t1.Attribute("SerieId").Value;
                    serieName_serieId[serieName1] = serieId1;
                    string name1 = t1.Attribute("Name").Value;
                    name1 = name1.Replace(" *", "");
                    if (EndsWithLetter(serieName1) == false) // registration teams
                    {
                        teamId_clubName[Id1] = clubName;
                        teamId_clubId[Id1] = clubId;
                        teamId_serieName[Id1] = serieName1;
                        teamId_serieId[Id1] = serieId1;
                        teamId_teamId[Id1] = Id1;
                        teamId_poule[Id1] = "A";
                    }
                    else
                    {
                        string serieName = serieName1;
                        if (EndsWithLetter(serieName1))
                            serieName = serieName.Substring(0,serieName.Length - 2);
                        if (serieName_serieName.ContainsValue(serieName))
                            serieName = serieName_serieName.First(kvp => kvp.Value == serieName).Key;
                        bool found = false;
                        foreach (var t2 in teams)
                        {
                            string Id2 = t2.Attribute("Id").Value;
                            string name2 = t2.Attribute("Name").Value;
                            name2 = name2.Replace(" *", "");
                            string serieName2 = t2.Attribute("SerieName").Value;
                            if (name1 == name2 && serieName2 == serieName && int.Parse(Id1) > int.Parse(Id2))
                            {
                                teamId_teamId[Id1] = Id2;
                                string reeks = null;
                                teamId_poule[Id1] = GetReeksNamePouleLetter(serieName1, ref reeks);
                                found = true;
                            }
                        }
                        if (!found)
                        {
                            // This section does not work completely, since it keeps an teamId that cannot be imported in the VVB.
                            // However, this allows to import the competition, ranking, etc.
                            Console.WriteLine("Error: new teams? - team: {0} ({1})", name1, serieName1);
                            foreach (var t2 in teams)
                            {
                                int compareId = int.Parse(Id1);
                                string Id2 = t2.Attribute("Id").Value;
                                string name2 = t2.Attribute("Name").Value;
                                name2 = name2.Replace(" *", "");
                                string serieName2 = t2.Attribute("SerieName").Value;
                                string serieId2 = t2.Attribute("SerieId").Value;
                                if ((serieName2 == serieName || serieName2 == serieName1) && compareId >= int.Parse(Id2))
                                {
                                    compareId = int.Parse(Id2);
                                    teamId_clubName[Id1] = clubName;
                                    teamId_clubId[Id1] = clubId;
                                    teamId_serieName[Id1] = serieName2;
                                    teamId_serieId[Id1] = serieId2;
                                    teamId_teamId[Id1] = Id1;
                                    t1.Attribute("SerieName").SetValue(serieName2);
                                    t1.Attribute("SerieId").SetValue(serieId2);
                                    string reeks = null;
                                    teamId_poule[Id1] = GetReeksNamePouleLetter(serieName1, ref reeks);
                                    found = true;
                                }
                            }
                            if (!found)
                            {
                                Console.WriteLine("Error: Still not found - team: {0} ({1})", name1, serieName1);
                            }
                        }
                        else
                        {
                            ToBeRemoved.Add(t1);
                        }
                    }
                }
                foreach (var t in ToBeRemoved)
                    t.Remove();
            }
            clubs = doc.Element("Registrations").Element("Clubs").Elements("Club");
            foreach (var club in clubs)
            {
                var constraints = club.Element("TeamConstraints").Elements("TeamConstraint");
                foreach (var con in constraints)
                {
                    var teamId1 = con.Attribute("Team1Id");
                    var teamId2 = con.Attribute("Team2Id");
                    if(teamId_teamId.ContainsKey(teamId1.Value))
                        teamId1.SetValue(teamId_teamId[teamId1.Value]);
                    if (teamId_teamId.ContainsKey(teamId2.Value))
                        teamId2.SetValue(teamId_teamId[teamId2.Value]);
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
                        ploegId = teamId_teamId[ploegId];
                        var rank = rangschikking.Element("volgorde").Value;
                        var punten = int.Parse(rangschikking.Element("puntentotaal").Value);
                        rank = rank.Replace(".", "");
                        var oldSerieName = teamId_serieName[ploegId];
                        var newSerieName = oldSerieName;
                        if(serieName_serieName.ContainsKey(oldSerieName))
                            newSerieName = serieName_serieName[oldSerieName];
//                        if (teamId_serieId.ContainsKey(newSerieName))
                        {
                            file.WriteLine("  <Ranking serieId='{0}' serieName='{1}' pouleName='' teamId='{2}' teamName='' ranking='{3}' score='{4}' /> ",
                                serieName_serieId[newSerieName],//    teamId_serieId[ploegId],
                                newSerieName,
                                ploegId,
                                rank,
                                punten);
                        }
  //                      else 
  //                      {
  //                          Console.WriteLine("Does not contain: '{0}'", ploegId);
  //                      }
                    }
                    else if (teamId_teamId.Count(t => t.Value == ploegId) == 1)
                    {
                        deletedTeams.Add(ploegId);
                    }
                }
                file.WriteLine("</Rankings>");
                file.Close();
            }

            foreach (var club in clubs)
            {
                var teams = club.Element("Teams").Elements("Team");
                foreach (var t1 in teams)
                {
                    if(deletedTeams.Contains(t1.Attribute("Id").Value))
                    {
                        XAttribute attr = new XAttribute("Deleted", "true");
                        t1.Add(attr);
                    }
                    var serieName = t1.Attribute("SerieName");
                    if (serieName_serieName.ContainsKey(serieName.Value))
                    {
                        var newSerieName = serieName_serieName[serieName.Value];
                        serieName.SetValue(newSerieName);
                        t1.Attribute("SerieId").SetValue(serieName_serieId[newSerieName]);
                    }
                }
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
                if (!teamId_teamId.ContainsKey(wedstrijd.Element("thuisploeg_id").Value))
                    continue;
                if (!teamId_teamId.ContainsKey(wedstrijd.Element("bezoekersploeg_id").Value))
                    continue;
                var thuisPloegId = teamId_teamId[wedstrijd.Element("thuisploeg_id").Value];
                var bezoekersPloegId = teamId_teamId[wedstrijd.Element("bezoekersploeg_id").Value];
                var thuisPloeg = wedstrijd.Element("thuisploeg").Value;
                var bezoekersPloeg = wedstrijd.Element("bezoekersploeg").Value;
                var poule = teamId_poule[wedstrijd.Element("bezoekersploeg_id").Value]; //poule from original ploegId
                var oldSerieName = teamId_serieName[thuisPloegId];
                var newSerieName = oldSerieName;
                if (serieName_serieName.ContainsKey(oldSerieName))
                    newSerieName = serieName_serieName[oldSerieName];
                var serieId1 = serieName_serieId[newSerieName];
                var sporthal = wedstrijd.Element("sporthal").Value.Replace(",", " ");
                var sporthalId = -1;
                DateTime date = DateTime.Parse(wedstrijd.Element("datum").Value);
                var aanvangsuur = wedstrijd.Element("aanvangsuur").Value;
                string thuisclubname = teamId_clubName[thuisPloegId];
                string thuisclubId = teamId_clubId[thuisPloegId];
                string bezoekersclubname = teamId_clubName[bezoekersPloegId];
                string bezoekersclubId = teamId_clubId[bezoekersPloegId];
                writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
                    newSerieName, serieId1, poule, -1, thuisPloeg, thuisPloegId, bezoekersPloeg, bezoekersPloegId, sporthal, sporthalId, date.ToShortDateString(), aanvangsuur, thuisclubname, thuisclubId, bezoekersclubname, bezoekersclubId);
            }
            writer.Close();

        }
        static string GetReeksNamePouleLetter(string reeks, ref string reeksName)
        {
            var last = reeks.Substring(reeks.Length-2, 2);
            var letter = reeks.Substring(reeks.Length-1, 1);
            if(last == " A") { reeksName = reeks.Substring(reeks.Length-1); return letter; }
            if(last == " B") { reeksName = reeks.Substring(reeks.Length-1); return letter; }
            if(last == " C") { reeksName = reeks.Substring(reeks.Length-1); return letter; }
            if(last == " D") { reeksName = reeks.Substring(reeks.Length-1); return letter; }
            if(last == " E") { reeksName = reeks.Substring(reeks.Length-1); return letter; }
            if(last == " F") { reeksName = reeks.Substring(reeks.Length-1); return letter; }
            if(last == " G") { reeksName = reeks.Substring(reeks.Length-1); return letter; }
            if(last == " H") { reeksName = reeks.Substring(reeks.Length-1); return letter; }
            if(last == " I") { reeksName = reeks.Substring(reeks.Length-1); return letter; }
            if(last == " J") { reeksName = reeks.Substring(reeks.Length-1); return letter; }
            if(last == " K") { reeksName = reeks.Substring(reeks.Length-1); return letter; }
            if(last == " L") { reeksName = reeks.Substring(reeks.Length-1); return letter; }
            if(last == " M") { reeksName = reeks.Substring(reeks.Length-1); return letter; }
            if(last == " N") { reeksName = reeks.Substring(reeks.Length-1); return letter; }
            if(last == " O") { reeksName = reeks.Substring(reeks.Length-1); return letter; }
            if(last == " P") { reeksName = reeks.Substring(reeks.Length-1); return letter; }
            if(last == " Q") { reeksName = reeks.Substring(reeks.Length-1); return letter; }
            if(last == " R") { reeksName = reeks.Substring(reeks.Length-1); return letter; }
            reeksName = reeks;
            return "A";
        }
        static bool EndsWithLetter(string serie)
        {
            if (serie.Substring(serie.Length-2) == " A") return true;
            if (serie.Substring(serie.Length-2) == " B") return true;
            if (serie.Substring(serie.Length-2) == " C") return true;
            if (serie.Substring(serie.Length-2) == " D") return true;
            if (serie.Substring(serie.Length-2) == " E") return true;
            if (serie.Substring(serie.Length-2) == " F") return true;
            if (serie.Substring(serie.Length-2) == " G") return true;
            if (serie.Substring(serie.Length-2) == " H") return true;
            if (serie.Substring(serie.Length-2) == " I") return true;
            if (serie.Substring(serie.Length-2) == " J") return true;
            if (serie.Substring(serie.Length-2) == " K") return true;
            if (serie.Substring(serie.Length-2) == " L") return true;
            if (serie.Substring(serie.Length-2) == " M") return true;
            if (serie.Substring(serie.Length-2) == " N") return true;
            if (serie.Substring(serie.Length-2) == " O") return true;
            if (serie.Substring(serie.Length-2) == " P") return true;
            if (serie.Substring(serie.Length-2) == " Q") return true;
            if (serie.Substring(serie.Length-2) == " R") return true;
            return false;
        }

        static bool AppendedWithLetter(string org, string str)
        {
            if (org + " A" == str) return true;
            if (org + " B" == str) return true;
            if (org + " C" == str) return true;
            if (org + " D" == str) return true;
            if (org + " E" == str) return true;
            if (org + " F" == str) return true;
            if (org + " G" == str) return true;
            if (org + " H" == str) return true;
            if (org + " I" == str) return true;
            if (org + " J" == str) return true;
            if (org + " K" == str) return true;
            if (org + " L" == str) return true;
            if (org + " M" == str) return true;
            if (org + " N" == str) return true;
            if (org + " O" == str) return true;
            if (org + " P" == str) return true;
            if (org + " Q" == str) return true;
            if (org + " R" == str) return true;
            if (org + " R2 A" == str) return true;
            if (org + " R2 B" == str) return true;
            if (org + " R2 C" == str) return true;
            if (org + " R2 D" == str) return true;
            if (org + " R2 E" == str) return true;
            if (org + " R2 F" == str) return true;
            if (org + " R2 G" == str) return true;
            if (org + " R2 H" == str) return true;
            if (org + " R2 I" == str) return true;
            if (org + " R2 J" == str) return true;
            if (org + " R2 K" == str) return true;
            if (org + " R2 L" == str) return true;
            if (org + " R2 M" == str) return true;
            if (org + " R2 N" == str) return true;
            if (org + " R2 O" == str) return true;
            if (org + " R2 P" == str) return true;
            if (org + " R2 Q" == str) return true;
            if (org + " R2 R" == str) return true;
            return false;
        }
    }
}
