using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace CompetitionCreator
{
    class ImportExport
    {
        static public string BaseDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)+@"\CompetitionCreator";
        static void Error(XElement element, string str)
        {
            string filename = element.Document.BaseUri;
            IXmlLineInfo info = element;
            int lineNumber = info.LineNumber;
            int linePosition = info.LinePosition;
            string prefix = filename + "(" + lineNumber.ToString() + "," + linePosition.ToString() + ")\n\n Element '" + element.Name + "' ";
            CompetitionCreator.Error.AddManualError("Reading the input file failed. ", 
                                                    "The input file that is read is not conform the expected scheme. Possibly an old file is read that is not conform the current scheme any more. <br/>"+
                                                    prefix + str);
            MessageBox.Show("Error reading xml data", prefix + str);
            throw new Exception(prefix + str);
        }
        static void Error(XDocument element, string str)
        {
            string filename = element.Document.BaseUri;
            IXmlLineInfo info = element;
            int lineNumber = info.LineNumber;
            int linePosition = info.LinePosition;
            string prefix = filename + "(" + lineNumber.ToString() + "," + linePosition.ToString() + ")\n\n";
            CompetitionCreator.Error.AddManualError("Reading the input file failed. ", 
                                                    "The input file that is read is not conform the expected scheme. Possibly an old file is read that is not conform the current scheme any more.<br/>"+
                                                    prefix + str);
            MessageBox.Show("Error reading xml data", prefix + str);
            throw new Exception(prefix + str);
        }
        static void Error(XAttribute attribute, string str)
        {
            string filename = attribute.Document.BaseUri;
            IXmlLineInfo info = attribute;
            int lineNumber = info.LineNumber;
            int linePosition = info.LinePosition;
            string prefix = filename + "(" + lineNumber.ToString() + "," + linePosition.ToString() + ")\n\n Attribute '" + attribute.Name + "'";
            CompetitionCreator.Error.AddManualError("Reading the input file failed. ", 
                                                    "The input file that is read is not conform the expected scheme. Possibly an old file is read that is not conform the current scheme any more.<br/>" +
                                                    prefix + str);
            MessageBox.Show(prefix + str);
            throw new Exception(prefix + str);

        }
        public static bool BoolAttribute(XElement element, params String[] attributeStrings)
        {
            bool result = false;
            XAttribute attr = null;
            foreach (string attribute in attributeStrings)
            {
                attr = element.Attribute(attribute);
                if (attr != null) break;
            }
            if (attr == null)
            {
                Error(element, "misses the attribute: '" + attributeStrings[0].ToString() + "'");
            }
            bool success = bool.TryParse(attr.Value.ToString(), out result);
            if (success == false)
            {
                Error(attr, " is not an boolean (true/false)");
            }
            return result;
        }
        public static bool BoolOptionalAttribute(XElement element, bool def, params String[] attributeStrings)
        {
            bool result = def;
            XAttribute attr = null;
            foreach (string attribute in attributeStrings)
            {
                attr = element.Attribute(attribute);
                if (attr != null) break;
            }
            if (attr != null)
            {
                bool success = bool.TryParse(attr.Value.ToString(), out result);
                if (success == false)
                {
                    Error(attr, " is not an boolean (true/false)");
                }
            }
            return result;
        }
        static int IntegerOptionalAttribute(XElement element, int def, params String[] attributeStrings)
        {
            int result = def;
            XAttribute attr = null;
            foreach (string attribute in attributeStrings)
            {
                attr = element.Attribute(attribute);
                if (attr != null) break;
            }
            if (attr != null)
            {
                bool success = int.TryParse(attr.Value.ToString(), out result);
                if (success == false)
                {
                    Error(attr, "is not an integer (true/false)");
                }
            }
            return result;
        }
        public static int IntegerAttribute(XElement element, params String[] attributeStrings)
        {
            int result = 0;
            XAttribute attr = null;
            foreach (string attribute in attributeStrings)
            {
                attr = element.Attribute(attribute);
                if (attr != null) break;
            }
            if (attr == null)
            {
                Error(element, "misses the attribute: '" + attributeStrings[0].ToString() + "'");
            }
            bool success = int.TryParse(attr.Value.ToString(), out result);
            if (success == false)
            {
                Error(attr, " is not an integer");
            }
            return result;
        }
        public static T EnumAttribute<T>(XElement element, params String[] attributeStrings) where T : struct
        {
            T result;
            XAttribute attr = null;
            foreach (string attribute in attributeStrings)
            {
                attr = element.Attribute(attribute);
                if (attr != null) break;
            }
            if (attr == null)
            {
                Error(element, "misses the attribute: '" + attributeStrings[0].ToString() + "'");
            }

            bool success = Enum.TryParse(attr.Value.ToString(), out result);
            if (success == false)
            {
                Error(attr, "is not an integer");
            }
            return result;
        }
        public static T EnumElement<T>(XElement element, params String[] attributeStrings) where T : struct
        {
            T result;
            XElement attr = null;
            foreach (string attribute in attributeStrings)
            {
                attr = element.Element(attribute);
                if (attr != null) break;
            }
            if (attr == null)
            {
                Error(element, "misses the attribute: '" + attributeStrings[0].ToString() + "'");
            }

            bool success = Enum.TryParse(attr.Value.ToString(), out result);
            if (success == false)
            {
                Error(attr, "is not an integer");
            }
            return result;
        }
        public static DateTime DateAttribute(XElement element, params String[] attributeStrings)
        {
            DateTime result;
            XAttribute attr = null;
            foreach (string attribute in attributeStrings)
            {
                attr = element.Attribute(attribute);
                if (attr != null) break;
            }
            if (attr == null)
            {
                Error(element, "misses the attribute: '" + attributeStrings[0].ToString() + "'");
            }
            bool success = DateTime.TryParse(attr.Value.ToString(), out result);
            if (success == false)
            {
                Error(attr, "is not an date");
            }
            return result;
        }
        public static string StringAttribute(XElement element, params String[] attributeStrings)
        {
            XAttribute attr = null;
            foreach (string attribute in attributeStrings)
            {
                attr = element.Attribute(attribute);
                if (attr != null) break;
            }
            if (attr == null)
            {
                Error(element, "The following attribute is not available: " + attributeStrings[0].ToString());
            }
            return attr.Value.ToString();
        }
        public static string StringElement(XElement element, params String[] attributeStrings)
        {
            XElement attr = null;
            foreach (string attribute in attributeStrings)
            {
                attr = element.Element(attribute);
                if (attr != null) break;
            }
            if (attr == null)
            {
                Error(element, "The following element is not available: " + attributeStrings[0].ToString());
            }
            return attr.Value.ToString();
        }
        public static string StringOptionalAttribute(XElement element, string def, params String[] attributeStrings)
        {
            XAttribute attr = null;
            foreach (string attribute in attributeStrings)
            {
                attr = element.Attribute(attribute);
                if (attr != null) break;
            }
            if (attr == null)
            {
                return def;
            }
            return attr.Value.ToString();
        }
        public static XElement Element(XElement element, params String[] elementStrings)
        {
            XElement elem = null;
            foreach (string elementString in elementStrings)
            {
                elem = element.Element(elementString);
                if (elem != null) break;
            }
            if (elem == null)
            {
                Error(element, "The following element is not available: " + elementStrings[0].ToString());
            }
            return elem;
        }
        public static XElement OptionalElement(XElement element, params String[] elementStrings)
        {
            XElement elem = null;
            foreach (string elementString in elementStrings)
            {
                elem = element.Element(elementString);
                if (elem != null) break;
            }
            return elem;
        }

        public static XElement Element(XDocument document, params String[] elementStrings)
        {
            XElement elem = null;
            foreach (string elementString in elementStrings)
            {
                elem = document.Element(elementString);
                if (elem != null) break;
            }
            if (elem == null)
            {
                Error(document, "The following element is not available: " + elementStrings[0].ToString());
            }
            return elem;
        }
        public static bool BoolElement(XElement element, params String[] attributeStrings)
        {
            bool result = false;
            XElement attr = null;
            foreach (string attribute in attributeStrings)
            {
                attr = element.Element(attribute);
                if (attr != null) break;
            }
            if (attr == null)
            {
                Error(element, "misses the attribute: '" + attributeStrings[0].ToString() + "'");
            }
            bool success = bool.TryParse(attr.Value.ToString(), out result);
            if (success == false)
            {
                Error(attr, "is not an boolean (true/false)");
            }
            return result;
        }
        public static int IntegerElement(XElement element, params String[] attributeStrings)
        {
            int result = 0;
            XElement attr = null;
            foreach (string attribute in attributeStrings)
            {
                attr = element.Element(attribute);
                if (attr != null) break;
            }
            if (attr == null)
            {
                Error(element, "misses the attribute: '" + attributeStrings[0].ToString() + "'");
            }
            bool success = int.TryParse(attr.Value.ToString(), out result);
            if (success == false)
            {
                Error(attr, "is not an integer");
            }
            return result;
        }


        WebClient wc = null;
        public ImportExport()
        {
            wc = new WebClient();
            wc.Proxy = null;
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            HttpWebRequest.DefaultWebProxy = null;
            WebRequest.DefaultWebProxy = null;
        }
        public void OpenLastProject(Model model)
        {
            string lastOpenedProject = Properties.Settings.Default.LastOpenedProject;
            if (File.Exists(lastOpenedProject))
            {
                LoadFullCompetition(model, lastOpenedProject);
            }
        }

        public void WriteClubConstraints(Model model, string filename, bool unknown_also = true)
        {
            XmlWriter writer = XmlWriter.Create(filename);
            writer.WriteStartDocument();
            writer.WriteStartElement("Registrations");
            WriteClubConstraintsInt(model, writer, unknown_also);
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }
        static private void WriteClubConstraintsInt(Model model, XmlWriter writer, bool unknown_also)
        {
            writer.WriteStartElement("Clubs");
            foreach (Club club in model.clubs)
            {
                if (unknown_also || club.Id >= 0)
                {
                    writer.WriteStartElement("Club");
                    writer.WriteAttributeString("Id", club.Id.ToString());
                    writer.WriteAttributeString("Name", club.name);
                    if (club.Stamnumber != null) writer.WriteAttributeString("StamNumber", club.Stamnumber);
                    if (club.groupingWithClub != null) writer.WriteAttributeString("LinkedClub", club.groupingWithClub.Id.ToString());
                    writer.WriteElementString("FreeFormatConstraint", club.FreeFormatConstraints);
                    writer.WriteStartElement("Sporthalls");
                    foreach (SporthallAvailability sporthal in club.sporthalls)
                    {
                        if (unknown_also || sporthal.id >= 0)
                        {
                            writer.WriteStartElement("Sporthall");
                            writer.WriteAttributeString("Id", sporthal.id.ToString());
                            writer.WriteAttributeString("Name", sporthal.name);
                            writer.WriteAttributeString("Latitude", sporthal.lat.ToString(CultureInfo.InvariantCulture));
                            writer.WriteAttributeString("Longitude", sporthal.lng.ToString(CultureInfo.InvariantCulture));
                            if (sporthal.team != null) writer.WriteAttributeString("TeamId", sporthal.team.Id.ToString());
                            if(sporthal.fields.Count>0)
                            {
                                writer.WriteStartElement("Fields");
                                foreach (Field f in sporthal.fields)
                                {
                                    writer.WriteStartElement("Field");
                                    writer.WriteAttributeString("Name", f.Name);
                                    writer.WriteAttributeString("Id", f.Id.ToString());
                                    writer.WriteEndElement();
                                }
                                writer.WriteEndElement();
                            }
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
                            writer.WriteAttributeString("Name", team.name);
                            writer.WriteAttributeString("Id", team.Id.ToString());
                            writer.WriteAttributeString("StartTime", team.defaultTime.ToString());
                            writer.WriteAttributeString("Day", team.defaultDay.ToString());
                            writer.WriteAttributeString("SerieId", team.serie.id.ToString());
                            writer.WriteAttributeString("SerieName", team.serie.name);
                            if (team.serie.extraTimeBefore > 0.01) writer.WriteAttributeString("extraTimeBefore", team.serie.extraTimeBefore.ToString());
                            writer.WriteAttributeString("Group", groupLetter);
                            writer.WriteAttributeString("SporthallId", team.sporthal.id.ToString());
                            writer.WriteAttributeString("SporthallName", team.sporthal.name);
                            if (team.field != null)
                            {
                                writer.WriteAttributeString("FieldId", team.field.Id.ToString());
                                writer.WriteAttributeString("FieldName", team.field.Name);
                            }
                            if (team.fixedNumber >= 0) writer.WriteAttributeString("FixedNumber", team.fixedNumber.ToString());
                            if (team.deleted) writer.WriteAttributeString("Deleted", "true");
                            if (team.EvenOdd != Team.WeekRestrictionEnum.All) writer.WriteAttributeString("EvenOdd", team.EvenOdd.ToString());
                            writer.WriteEndElement();
                        }
                    }
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }
        public void WritePoules(Model model, XmlWriter writer, List<Serie> series)
        {
            writer.WriteStartElement("Poules");
            foreach (Poule poule in model.poules)
            {
                if (series.Contains(poule.serie))
                {
                    writer.WriteStartElement("Poule");
                    writer.WriteAttributeString("Name", poule.name);
                    writer.WriteAttributeString("SerieId", poule.serie.id.ToString());
                    writer.WriteAttributeString("SerieName", poule.serie.name);
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }
        public void WriteTeams(Model model, XmlWriter writer, List<Serie> series)
        {
            writer.WriteStartElement("Teams");
            foreach (Poule poule in model.poules)
            {
                if (series.Contains(poule.serie))
                {
                    foreach (Team team in poule.teams)
                    {
                        if (team.RealTeam())
                        {
                            writer.WriteStartElement("Team");
                            writer.WriteAttributeString("SerieName", team.poule.serie.name);
                            writer.WriteAttributeString("SerieId", team.poule.serie.id.ToString());
                            writer.WriteAttributeString("PouleName", team.poule.name);
                            writer.WriteAttributeString("Id", team.Id.ToString());
                            writer.WriteAttributeString("Name", team.name);
                            // Not the id and the name of the clubsporthal is used, but the original (duplication of sporthalls)
                            writer.WriteAttributeString("SporthallId", team.sporthal.sporthall.id.ToString());
                            writer.WriteAttributeString("SporthallName", team.sporthal.sporthall.name);
                            if (team.field != null)
                            {
                                writer.WriteAttributeString("FieldId", team.field.Id.ToString());
                                writer.WriteAttributeString("FieldName", team.field.Name);
                            }
                            writer.WriteAttributeString("ClubId", team.club.Id.ToString());
                            writer.WriteAttributeString("ClubName", team.club.name);
                            writer.WriteEndElement();
                        }
                    }
                }
            }
            writer.WriteEndElement();
        }
        public void WriteMatches(Model model, XmlWriter writer, List<Serie> series)
        {
            writer.WriteStartElement("Matches");
            foreach (Poule poule in model.poules)
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
                            writer.WriteAttributeString("PouleName", match.poule.name);
                            writer.WriteAttributeString("homeTeamName", match.homeTeam.name);
                            writer.WriteAttributeString("homeTeamId", match.homeTeam.Id.ToString());
                            writer.WriteAttributeString("visitorTeamName", match.visitorTeam.name);
                            writer.WriteAttributeString("visitorTeamId", match.visitorTeam.Id.ToString());
                            writer.WriteAttributeString("date", match.datetime.ToString("d/M/yyyy"));
                            writer.WriteAttributeString("time", match.datetime.ToString("H:m"));//.ToShortTimeString());
                            writer.WriteAttributeString("SporthallId", match.homeTeam.sporthal.id.ToString());
                            writer.WriteAttributeString("SporthallName", match.homeTeam.sporthal.name);
                            if (match.homeTeam.field != null)
                            {
                                writer.WriteAttributeString("FieldId", match.homeTeam.field.Id.ToString());
                                writer.WriteAttributeString("FieldName", match.homeTeam.field.Name);
                            }

                            writer.WriteEndElement();
                        }
                    }
                }
            }
        }
        public void WriteStatistics(Model model, string filename)
        {
            StreamWriter writer = new StreamWriter(filename);
            writer.WriteLine("Poules:");
            writer.WriteLine("Poule, Matches, Conflicts, Percentage");
            foreach (Poule poule in model.poules)
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
            foreach (Club club in model.clubs)
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
        public void WriteMatches(Model model, string filename, List<Serie> series)
        {
            StreamWriter writer = new StreamWriter(filename);
            writer.WriteLine("Serie,Poule,Speeldag,datum,Speeltijd,homeClub,homeTeam,visitorClub,visitorTeam");
            foreach (Poule poule in model.poules)
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
        public void WriteSeriePoules(Model model, string filename)
        {
            StreamWriter writer = new StreamWriter(filename);
            writer.WriteLine("Serie,Poule,Poule-team-number,Team,Team-Id,Club,Club-Id,Speeldag,Speeltijd,Groep");
            foreach (Team team in model.teams)
            {
                if (team.club.Id >= 0 && !team.deleted)
                {
                    string pouleName = "-";
                    if (team.poule != null)
                    {
                        pouleName = team.poule.name;
                    }

                    writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", team.serie.name, pouleName, team.Index, team.name, team.Id, team.club.name, team.club.Id, team.defaultDay.ToString(), team.defaultTime.ToString(), team.group.ToString());
                }
            }
            writer.Close();
        }
        public void WriteExportCompetitionXml(Model model, string filename, List<Serie> series)
        {
            XmlWriter writer = XmlWriter.Create(filename);
            writer.WriteStartDocument();
            writer.WriteStartElement("Competition");
            WritePoules(model, writer, series);
            WriteTeams(model, writer, series);
            WriteMatches(model, writer, series);
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }
        public void WriteConflictReportPerType(Model model, string filename)
        {
            StreamWriter writer = new StreamWriter(filename);
            string name = "";
            string title = "";
            model.constraints.Sort(delegate(Constraint c1, Constraint c2) { return c1.Title.CompareTo(c2.Title); });
            foreach (Constraint constraint in model.constraints)
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

        public void WriteConflictReportPerClub(Model model, string filename)
        {
            StreamWriter writer = new StreamWriter(filename);
            string name = "";
            string title = "";
            foreach (Club club in model.clubs)
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
        public void WriteCompetitionPerClub(Model model, string filename)
        {
            StreamWriter writer = new StreamWriter(filename);
            foreach (Club club in model.clubs)
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
                        writer.WriteLine("{5},{0},{1},{2},{3},{4},{7},{8},{6}", match.datetime, match.poule.fullName, match.homeTeam.name, match.visitorTeam.name, match.homeTeam.group.ToString(), constraint, conflicts, match.homeTeam.sporthal.name.Replace(",", " "), match.homeTeam.FieldName);
                    }
                }
            }
            writer.Close();
        }
        static public void WriteProject(Model model, string fileName, bool backup = false)
        {
            XmlWriter writer = null;
            if (backup)
            {
                string dir = BaseDirectory + "\\Backup\\";
                if (Directory.Exists(dir) == false)
                    Directory.CreateDirectory(dir);
                writer = XmlWriter.Create(dir+"backup.xml");
            }
            else
            {
                writer = XmlWriter.Create(fileName);
            }
            writer.WriteStartDocument();
            writer.WriteStartElement("Competition");
            writer.WriteStartElement("Settings");
            writer.WriteAttributeString("Version", model.version);
            writer.WriteAttributeString("Year", model.year.ToString());
            writer.WriteAttributeString("OptimizeNumber", model.OptimizeNumber.ToString());
            writer.WriteAttributeString("OptimizeHomeVisit", model.OptimizeHomeVisit.ToString());
            writer.WriteAttributeString("OptimizeSchema", model.OptimizeSchema.ToString());
            writer.WriteStartElement("Series");
            foreach (Serie serie in model.series)
            {
                writer.WriteStartElement("Serie");
                writer.WriteAttributeString("Name", serie.name);
                writer.WriteAttributeString("Id", serie.id.ToString());
                writer.WriteAttributeString("Evaluated", serie.evaluated.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
            WriteClubConstraintsInt(model, writer, true);
            writer.WriteStartElement("Poules");
            foreach (Poule poule in model.poules)
            {
                writer.WriteStartElement("Poule");
                writer.WriteAttributeString("Name", poule.name);
                writer.WriteAttributeString("SerieId", poule.serie.id.ToString());
                writer.WriteAttributeString("SerieName", poule.serie.name);
                if (poule.imported)
                {
                    writer.WriteAttributeString("Imported", "true");
                }
                writer.WriteAttributeString("MaxTeams", poule.maxTeams.ToString());
                writer.WriteStartElement("Teams");
                foreach (Team team in poule.teams)
                {
                    writer.WriteStartElement("Team");
                    writer.WriteAttributeString("Name", team.name);
                    writer.WriteAttributeString("Id", team.Id.ToString());
                    writer.WriteEndElement();

                }
                writer.WriteEndElement();
                writer.WriteStartElement("Weeks");
                foreach (MatchWeek week in poule.weeks)
                {
                    writer.WriteStartElement("Week");
                    int round = week.round + 1;
                    writer.WriteAttributeString("Round", round.ToString());
                    writer.WriteAttributeString("Date", week.Monday.Date.ToShortDateString());
                    if (week.dayOverruled) writer.WriteAttributeString("OverruledDay", "true");
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteStartElement("Matches");
                foreach (Match match in poule.matches)
                {
                    writer.WriteStartElement("Match");
                    writer.WriteAttributeString("homeTeam", match.homeTeamIndex.ToString());
                    writer.WriteAttributeString("visitorTeam", match.visitorTeamIndex.ToString());
                    writer.WriteAttributeString("week", match.weekIndex.ToString());
                    if(match.IsTimeOverruled())
                        writer.WriteAttributeString("time", match.Time.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteStartElement("Constraints");
            foreach (Constraint c in model.constraints)
            {
                DateConstraint con = c as DateConstraint;
                if (con != null)
                {
                    writer.WriteStartElement("DateConstraint");
                    writer.WriteAttributeString("TeamId", con.team.Id.ToString());
                    writer.WriteAttributeString("Date", con.date.ToShortDateString());
                    writer.WriteAttributeString("What", con.homeVisitNone.ToString());
                    writer.WriteAttributeString("Cost", con.cost.ToString());
                    writer.WriteEndElement();
                }
            }
            foreach (TeamConstraint tcon in model.teamConstraints)
            {
                if (tcon != null)
                {
                    writer.WriteStartElement("TeamConstraint");
                    writer.WriteAttributeString("Team1Id", tcon.team1Id.ToString());
                    writer.WriteAttributeString("Team2Id", tcon.team2Id.ToString());
                    writer.WriteAttributeString("What", tcon.what.ToString());
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
            if (backup == false)
            {
                model.stateNotSaved = false;
                model.savedFileName = fileName;
                Properties.Settings.Default.LastOpenedProject = fileName;
                Properties.Settings.Default.Save();
            }
            //model.Changed();
        }


        public void ImportRanking(Model model, XElement doc)
        {
            foreach (XElement ranking in doc.Elements("Ranking"))
            {
                int teamId = IntegerAttribute(ranking, "teamId");
                int serieId = IntegerAttribute(ranking, "serieId");
                string pouleName = StringAttribute(ranking, "pouleName");
                int score = IntegerAttribute(ranking, "score");
                int rank = IntegerAttribute(ranking, "ranking");
                foreach (Team t in model.teams)
                {
                    if (t.Id == teamId && t.serie.id == serieId/*&& t.poule!= null && t.poule.name == pouleName*/)
                    {
                        t.Ranking = rank.ToString("D2") + "." + (99 - score).ToString();
                    }
                }

            }
        }
        private void ImportPoules(Model model, XElement competition)
        {
            XElement poulesElement = Element(competition, "Poules");
            foreach (XElement poule in poulesElement.Elements("Poule"))
            {
                string PouleName = StringAttribute(poule, "Name");
                string SerieName = StringAttribute(poule, "SerieName");
                int SerieId = IntegerAttribute(poule, "SerieId", "SerieSortId");
                Serie serie = model.series.Find(s => s.id == SerieId);
                if (serie == null)
                {
                    serie = new Serie(SerieId, SerieName, model);
                    model.series.Add(serie);
                }
                Poule po = model.poules.Find(p => p.serie == serie && p.name == PouleName);
                int maxTeams = IntegerAttribute(poule, "MaxTeams");
                if (maxTeams == 0) maxTeams = 14;
                if (po == null)
                {
                    po = new Poule(PouleName, maxTeams, serie);
                    serie.poules.Add(po);
                    model.poules.Add(po);
                }
                else
                    return;


                po.imported = BoolOptionalAttribute(poule, false, "Imported", "SerieImported");

                int index = 0;
                foreach (XElement team in Element(poule, "Teams").Elements("Team"))
                {
                    int teamId = IntegerAttribute(team, "Id");
                    string teamName = StringAttribute(team, "Name");
                    Team te;
                    if (teamName == "----")
                    {
                        te = Team.CreateNullTeam(null, serie);
                    }
                    else
                    {
                        te = model.teams.Find(t => t.Id == teamId && t.name == teamName); // TODO: remove condition name == teamName. Now still required for national teams that are not numbered uniquely
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
                foreach (XElement week in Element(poule, "Weeks").Elements("Week"))
                {
                    DateTime date = DateAttribute(week, "Date").Date;
                    int round = IntegerOptionalAttribute(week, 1, "Round");
                    MatchWeek w = new MatchWeek(date);
                    w.round = round - 1;
                    w.dayOverruled = BoolOptionalAttribute(week, false, "OverruledDay");
                    po.weeks.Add(w);
                }
                // piece of code that can be removed after some time. This is to read in data without round info.
                int maxRound = po.weeks.Max(w => w.round);
                if (maxRound == 0)
                {
                    int index1 = 0;
                    foreach (MatchWeek week in po.weeks)
                    {
                        if (index1 < po.weeks.Count / 2)
                            week.round = 0;
                        else
                            week.round = 1;
                        index1++;
                    }
                }

                foreach (XElement match in Element(poule, "Matches").Elements("Match"))
                {
                    int weekIndex = IntegerAttribute(match, "week");
                    int homeTeam = IntegerAttribute(match, "homeTeam");
                    int visitorTeam = IntegerAttribute(match, "visitorTeam");
                    Match m = new Match(weekIndex, homeTeam, visitorTeam, serie, po);
                    string s = StringOptionalAttribute(match, null, "time");
                    if(s != null)
                        m.Time = new Time(DateTime.Parse(s));
                    po.matches.Add(m);
                }
            }
        }
        private void ImportTeamConstraints(Model model, XElement competition)
        {
            XElement teamConstraints = Element(competition, "Constraints");
            if (teamConstraints != null)
            {
                foreach (XElement con in teamConstraints.Elements("DateConstraint"))
                {
                    int teamId = IntegerAttribute(con, "TeamId");
                    DateTime date = DateAttribute(con, "Date").Date;
                    DateConstraint.HomeVisitNone what = EnumAttribute<DateConstraint.HomeVisitNone>(con, "What", "Type");
                    Team team = model.teams.Find(t => t.Id == teamId);
                    DateConstraint tc = new DateConstraint(team);
                    tc.homeVisitNone = what;
                    tc.date = date;
                    tc.cost = IntegerAttribute(con, "Cost");
                    model.constraints.Add(tc);
                }
                foreach (XElement con in teamConstraints.Elements("TeamConstraint"))
                {
                    int team1Id = IntegerAttribute(con, "Team1Id");
                    int team2Id = IntegerAttribute(con, "Team2Id");
                    TeamConstraint.What what = EnumAttribute<TeamConstraint.What>(con, "What");
                    var exists = model.teamConstraints.Find(c => c.team1Id == team1Id && c.team2Id == team2Id && c.what == what);
                    if (exists == null)
                    {
                        TeamConstraint tc = new TeamConstraint(model, team1Id, team2Id, what);
                        model.teamConstraints.Add(tc);
                    }
                }
            }
        }
        public Model LoadFullCompetitionIntern(string filename)
        {
            XDocument doc = XDocument.Load(filename, LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
            XElement competition = Element(doc, "Competition");
            XElement settings = Element(competition, "Settings");
            int year = IntegerAttribute(settings, "Year");
            Model modelnew = new Model(year);
            modelnew.OptimizeNumber = BoolOptionalAttribute(settings, true, "OptimizeNumber");
            modelnew.OptimizeHomeVisit = BoolOptionalAttribute(settings, false, "OptimizeHomeVisit");
            modelnew.OptimizeSchema = BoolOptionalAttribute(settings, false, "OptimizeSchema");

            ImportTeamSubscriptions(modelnew, competition);
            ImportPoules(modelnew, competition);
            ImportTeamConstraints(modelnew, competition);
            foreach (XElement serie in Element(settings, "Series").Elements("Serie"))
            {
                int serieId = IntegerAttribute(serie, "Id");
                Serie se = modelnew.series.Find(s => s.id == serieId);
                if (se != null)
                {
                    se.evaluated = BoolOptionalAttribute(serie, true, "Evaluated");
                }
            }
            modelnew.savedFileName = filename;
            modelnew.RenewConstraints();
            return modelnew;
        }
        public void LoadFullCompetition(Model model, string filename)
        {
            try
            {
                Model modelnew = LoadFullCompetitionIntern(filename);
                Properties.Settings.Default.LastOpenedProject = filename;
                Properties.Settings.Default.Save();
                model.Changed();
                // modelnew is distributed to the views
                model.Changed(modelnew);
                modelnew.Evaluate(null);
                modelnew.Changed();
            }
            catch (Exception exc)
            {
                CompetitionCreator.Error.AddManualError("Loading competition failed.", string.Format("Project '{0}' could not be opened. The XML might not be correct, or the syntax is of an previous version. \n Exception: {1}", filename, exc.ToString()));
                MessageBox.Show(string.Format("Project '{0}' could not be opened. The XML might not be correct, or the syntax is of an previous version. \n Exception: {1}", filename, exc.ToString()));
            }
        }

        public void ImportTeamSubscriptions(Model model, XElement doc)
        {
            XElement clubsEl = Element(doc, "Clubs");
            foreach (XElement club in clubsEl.Elements("Club"))
            {
                int clubId = IntegerAttribute(club, "Id");
                string clubName = StringAttribute(club, "Name");
                string clubStamNumber = StringOptionalAttribute(club, null, "StamNumber");
                Club cl = model.clubs.Find(c => c.Id == clubId);
                if (cl == null)
                {
                    cl = new Club(clubId, clubName, clubStamNumber);
                    model.clubs.Add(cl);
                }
                if (cl.Stamnumber == null && clubStamNumber != null)
                {
                    cl.Stamnumber = clubStamNumber;
                }
                XAttribute attr = club.Attribute("LinkedClub");
                if (attr != null)
                {
                    int groupClubId = int.Parse(attr.Value);
                    Club groupedClub = model.clubs.Find(c => c.Id == groupClubId);
                    if (groupedClub != null)
                    {
                        groupedClub.groupingWithClub = cl;
                        cl.groupingWithClub = groupedClub;
                    }

                }

                XElement freeformatconstraint = Element(club, "FreeFormatConstraint");

                cl.FreeFormatConstraints = freeformatconstraint.Value;

                foreach (var sporthal in Element(club, "Sporthalls").Elements("Sporthall"))
                {
                    string sporthallName = StringAttribute(sporthal, "Name");
                    int id = IntegerAttribute(sporthal, "Id");
                    string latAttrString = StringOptionalAttribute(sporthal, null, "Latitude", "latitude");
                    string lngAttrString = StringOptionalAttribute(sporthal, null, "Longitude", "longitude");
                    //VVB hack:
                    {
                        if (sporthallName == "-")
                        {
                            foreach (var sporthalAlt in Element(club, "Sporthalls").Elements("Sporthall"))
                            {
                                sporthallName = StringAttribute(sporthalAlt, "Name");
                                id = IntegerAttribute(sporthalAlt, "Id");
                                latAttrString = StringOptionalAttribute(sporthalAlt, null, "Latitude", "latitude");
                                lngAttrString = StringOptionalAttribute(sporthalAlt, null, "Longitude", "longitude");
                                if (sporthallName != "-")
                                    break;
                            }
                        }
                    }
                    int teamId = IntegerOptionalAttribute(sporthal, -1, "TeamId");
                    Sporthal sp1 = model.sporthalls.Find(s => s.id == id);
                    if (sp1 == null)
                    {
                        sp1 = new Sporthal(id, sporthallName);
                        model.sporthalls.Add(sp1);
                        //System.Windows.Forms.MessageBox.Show(string.Format("Sporthal id {0} not known known", id));
                    }
                    double sporthallLatitude;
                    double sporthallLongitude;
                    if (latAttrString != null && lngAttrString != null)
                    {
                        bool ok1 = double.TryParse(latAttrString, NumberStyles.Number, CultureInfo.InvariantCulture, out sporthallLatitude);
                        bool ok2 = double.TryParse(lngAttrString, NumberStyles.Number, CultureInfo.InvariantCulture, out sporthallLongitude);
                        if (ok1 && ok2)
                        {
                            sp1.lat = sporthallLatitude;
                            sp1.lng = sporthallLongitude;
                        }
                    }

                    SporthallAvailability sp = cl.sporthalls.Find(t => t.id == id && t.teamId == teamId);
                    if (sp == null)
                    {
                        sp = new SporthallAvailability(sp1);
                        sp.teamId = teamId;
                        cl.sporthalls.Add(sp);
                    }
                    XElement Fields = OptionalElement(sporthal, "Fields");
                    if(Fields != null)
                    {
                        foreach (var field in Fields.Elements("Field"))
                        {
                            string FieldName = StringAttribute(field, "Name");
                            int FieldId = IntegerAttribute(field, "Id");
                            Field f = new Field();
                            f.Name = FieldName;
                            f.Id = FieldId;
                            sp.fields.Add(f);
                        }
                    }

                    //clear original dates (for re-reading the subscriptions)
                    sp.NotAvailable.Clear();
                    if (OptionalElement(sporthal, "NotAvailables") != null)
                    {
                        foreach (var notAvailable in Element(sporthal, "NotAvailables").Elements("NotAvailable"))
                        {
                            string from = notAvailable.Attribute("from").Value;
                            string until = notAvailable.Attribute("until").Value;
                            DateTime dt = DateTime.Parse(from);
                            DateTime dtFrom = new DateTime(dt.Year, dt.Month, dt.Day);
                            DateTime dtUntil = DateTime.Parse(until);

                            while(dtFrom < dtUntil)
                            {
                               
                                if (sp.NotAvailable.Contains(dtFrom) == false) sp.NotAvailable.Add(dtFrom);
                                dtFrom = dtFrom.AddDays(1);
                            }
                        }
                    }
                    else if(OptionalElement(sporthal,"NotAvailable") != null)
                    {
                        foreach (var date in Element(sporthal, "NotAvailable").Elements("Date"))
                        {
                            DateTime dt = DateTime.Parse(date.Value.ToString());
                            if (sp.NotAvailable.Contains(dt) == false) sp.NotAvailable.Add(dt);
                        }
                    }
                    if (cl.sporthalls.Contains(sp) == false) cl.sporthalls.Add(sp);
                }

                foreach (var team in club.Element("Teams").Elements("Team"))
                {
                    int id = IntegerAttribute(team, "Id");
                    int serieId = IntegerAttribute(team, "SerieId", "SerieSortId");
                    string seriePrefix = StringAttribute(team, "SerieName");
                    Serie serie = model.series.Find(s => s.id == serieId);
                    if (serie == null)
                    {
                        serie = new Serie(serieId, seriePrefix, model);
                        model.series.Add(serie);
                    }
                    string teamName = StringAttribute(team, "Name");
                    // Search both on name & id, since the national Id's are not unique.
                    Team te = null;
                    if (id >= 0)
                    {
                        te = cl.teams.Find(t => t.Id == id && t.serie == serie);
                        cl.teams.RemoveAll(t => t.Id == id && t.serie != serie);
                    }
                    else te = cl.teams.Find(t => t.Id == id && t.name == teamName && t.serie == serie);
                    if (te == null)
                    {
                        te = new Team(id, teamName, null, serie, cl);
                        model.teams.Add(te);
                    }
                    else
                    {
                        // not removing poule. Used for re-reading subscriptions
                        //if (te.poule != null) te.poule.RemoveTeam(te);
                    }
                    // new team, or check on changed data (re-reading subscriptions)
                    string DayString = StringAttribute(team, "Day");
                    if (DayString == "7") DayString = "0";
                    DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), DayString);
                    Time time = new Time(DateAttribute(team, "StartTime"));
                    te.defaultDay = day;
                    te.defaultTime = time;
                    string teamGroup = StringOptionalAttribute(team, "-", "Group");
                    te.group = TeamGroups.NoGroup;
                    if (teamGroup != "-") te.group = TeamGroups.NoGroup;
                    if (teamGroup == "A") te.group = TeamGroups.GroupX;
                    if (teamGroup == "B") te.group = TeamGroups.GroupY;
                    if (teamGroup == "X") te.group = TeamGroups.GroupX;
                    if (teamGroup == "Y") te.group = TeamGroups.GroupY;

                    if (BoolOptionalAttribute(team, false, "Deleted")) te.DeleteTeam(model); // remains only in the club administration.
                    te.fixedNumber = IntegerOptionalAttribute(team, -1, "FixedNumber");
                    string extraTimeString = StringOptionalAttribute(team, null, "extraTimeBefore"); // bijvoorbeeld voor reserve wedstrijd 
                    if (extraTimeString != null)
                    {
                        double extraTime;
                        bool ok = double.TryParse(extraTimeString, NumberStyles.Number, CultureInfo.InvariantCulture, out extraTime);
                        if (ok) serie.extraTimeBefore = extraTime;
                    }
                    string EvenOdd = StringOptionalAttribute(team, null, "EvenOdd");
                    if (EvenOdd != null)
                    {
                        if (MySettings.Settings.TranslateEvenOddToGroupXY)
                        {
                            te.group = TeamGroups.NoGroup;
                            if (EvenOdd == "Even") te.group = TeamGroups.GroupX;
                            if (EvenOdd == "Odd") te.group = TeamGroups.GroupY;
                        }
                        else
                        {
                            te.EvenOdd = Team.WeekRestrictionEnum.All;
                            if (EvenOdd == "Even") te.EvenOdd = Team.WeekRestrictionEnum.Even;
                            if (EvenOdd == "Odd") te.EvenOdd = Team.WeekRestrictionEnum.Odd;
                        }
                    }
                    te.email = StringOptionalAttribute(team, "ContactEmail");
                    // VVB hack: can be removed.
                    /*
                    XElement constraints = OptionalElement(team, "Restrictions", "Constraints");
                    if (constraints != null)
                    {
                        // Create a seperate entity for the team
                        te.sporthal = new SporthallAvailability(sp2);
                        te.sporthal.teamId = te.Id;
                        te.club.sporthalls.Add(te.sporthal);
                        foreach (var constraint in constraints.Elements("Restriction"))
                        {
                            DateTime date = DateAttribute(constraint, "Date", "PlayDate");
                            te.sporthal.NotAvailable.Add(date);
                        }
                    }*/
                    int sporthalId = IntegerAttribute(team, "SporthallId");
                    //string sporthallName = StringAttribute(team, "SporthallName");

                    SporthallAvailability sporthall = cl.sporthalls.Find(s => s.sporthall.id == sporthalId && s.teamId == id);
                    if (sporthall == null)
                        sporthall = cl.sporthalls.Find(s => s.sporthall.id == sporthalId);
                    // VVB hack: we hebben een alternatieve sporthal genomen specifiek voor dit team (van een ander team)
                    if (sporthall == null)
                        sporthall = cl.sporthalls.Find(s => s.teamId == id);
                    if (sporthall == null)
                        sporthall = cl.sporthalls.Find(sp => sp.id == sporthalId);
                    if (sporthall == null)
                    {
                        sporthall = new SporthallAvailability(new Sporthal(sporthalId, "Unknown"));
                        cl.sporthalls.Add(sporthall);
                    }
                    te.sporthal = sporthall;
                    if (sporthall.teamId == te.Id)
                    {
                        sporthall.team = te;
                    }
                    string fieldName = StringOptionalAttribute(team, null, "FieldName");
                    if(fieldName != null)
                    {
                        int FieldId = IntegerAttribute(team, "FieldId");
                        te.field = new Field();
                        te.field.Id = FieldId;
                        te.field.Name = fieldName;
                    }
                }
            }
            // Read in the FixedConstraint, waar ze ook staan (VVB hack, omdat ze onder de clubs staan)
            var fixConstraints = doc.Descendants("FixedConstraint");
            fixConstraints = fixConstraints.Union(doc.Descendants("TeamConstraint"));
            foreach (XElement con in fixConstraints)
            {
                int team1Id = IntegerAttribute(con, "Team1Id", "SourceTeam");
                int team2Id = IntegerAttribute(con, "Team2Id", "TargetTeam");
                TeamConstraint.What what = TeamConstraint.What.HomeOnSameDay;
                string whatString = StringAttribute(con, "What", "Type", "ConstraintType");
                if (whatString == "zelfde weekend thuis") what = TeamConstraint.What.HomeInSameWeekend;
                if (whatString == "niet samen thuis weekend") what = TeamConstraint.What.HomeNotInSameWeekend;
                if (whatString == "zelfde dag thuis") what = TeamConstraint.What.HomeOnSameDay;
                if (whatString == "niet zelfde dag thuis") what = TeamConstraint.What.HomeNotOnSameDay;
                if (whatString == "HomeInSameWeekend") what = TeamConstraint.What.HomeInSameWeekend;
                if (whatString == "HomeNotInSameWeekend") what = TeamConstraint.What.HomeNotInSameWeekend;
                if (whatString == "HomeOnSameDay") what = TeamConstraint.What.HomeOnSameDay;
                if (whatString == "HomeNotOnSameDay") what = TeamConstraint.What.HomeNotOnSameDay;
                var exists = model.teamConstraints.Find(c => c.team1Id == team1Id && c.team2Id == team2Id && c.what == what);
                if (exists == null)
                {

                    TeamConstraint constraint = new TeamConstraint(model, team1Id, team2Id, what);
                    model.teamConstraints.Add(constraint);
                }
            }
            CalculateDistancesSporthalls(model);
        }

        public void ImportCSV(Model model, string fileName)
        {
            const int serieIndex = 0;
            const int serieIdIndex = 1;
            const int pouleIndex = 2;
            //const int pouleIdIndex = 3;
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
            List<Poule> importPoules = new List<Poule>();
            foreach (string line in lines)
            {
                ParameterLines.Add(line.Split(','));
            }
            foreach (string[] parameters in ParameterLines)
            {
                int SerieId = int.Parse(parameters[serieIdIndex]);
                Serie serie = model.series.Find(s => s.id == SerieId);
                if (serie == null)
                {
                    serie = new Serie(SerieId, parameters[serieIndex], model);
                    model.series.Add(serie);
                    //System.Windows.Forms.MessageBox.Show(string.Format("Serie id '{0}' is unknown", SerieId));
                }
                int homeClubId = int.Parse(parameters[homeClubIdIndex]);
                Club homeClub = model.clubs.Find(s => s.Id == homeClubId);
                if (homeClub == null)
                {
                    homeClub = new Club(homeClubId, parameters[homeClubIndex], null);
                    model.clubs.Add(homeClub);
                    //System.Windows.Forms.MessageBox.Show(string.Format("Club id '{0}' is unknown", homeClubId));
                }
                Club visitorClub = null;
                if (parameters[visitorClubIdIndex].Length > 0)
                {
                    int visitorClubId = int.Parse(parameters[visitorClubIdIndex]);
                    visitorClub = model.clubs.Find(s => s.Id == visitorClubId);
                    if (visitorClub == null)
                    {
                        visitorClub = new Club(visitorClubId, parameters[visitorClubIndex], null);
                        model.clubs.Add(visitorClub);
                        //System.Windows.Forms.MessageBox.Show(string.Format("Club id '{0}' is unknown", visitorClubId));
                    }
                }
                int sporthalId = int.Parse(parameters[sporthallIdIndex]);
                Sporthal sporthal = model.sporthalls.Find(s => s.id == sporthalId);
                if (sporthal == null)
                {
                    sporthal = new Sporthal(sporthalId, parameters[sporthallIndex]);
                    model.sporthalls.Add(sporthal);
                    //                    System.Windows.Forms.MessageBox.Show(string.Format("Sporthal id '{0}' is unknown", sporthalId));
                }
                // sporthal aan club toevoegen indien deze nog niet bestaat
                SporthallAvailability sporthallclub = homeClub.sporthalls.Find(sp => sp.sporthall == sporthal);
                if (sporthallclub == null)
                {
                    sporthallclub = new SporthallAvailability(sporthal);
                    homeClub.sporthalls.Add(sporthallclub);
                }
                // Calculate the size of the poule
                List<string> teamsList = new List<string>();
                List<string[]> SelectedLines = ParameterLines.FindAll(l => l[serieIndex] == parameters[serieIndex] && l[pouleIndex] == parameters[pouleIndex]);
                foreach (string[] parameterline in SelectedLines)
                {
                    if (teamsList.Contains(parameterline[homeTeamIndex]) == false) teamsList.Add(parameterline[homeTeamIndex]);
                    if (teamsList.Contains(parameterline[visitorTeamIndex]) == false) teamsList.Add(parameterline[visitorTeamIndex]);
                }
                int c = teamsList.Count;
                // poules toevoegen indien ze niet bestaan
                Poule poule = serie.poules.Find(s => s.name == parameters[pouleIndex]);
                if (poule != null && poule.maxTeams < c)
                {
                    model.poules.Remove(poule);
                    serie.poules.Remove(poule);
                    poule = null;
                }
                if (poule == null)
                {
                    poule = new Poule(parameters[pouleIndex], c, serie);
                    model.poules.Add(poule);
                    serie.poules.Add(poule);
                }
                else
                {
                    // matches will be added
                    poule.matches.Clear();
                    poule.weeks.Clear();
                }
                if (importPoules.Contains(poule) == false)
                {
                    importPoules.Add(poule);
                }

                poule.imported = true;
                if (parameters[homeTeamIndex].Length > 0)
                {
                    int teamId = int.Parse(parameters[homeTeamIdIndex]);
                    Team team = null;
                    if (teamId >= 0)
                    {
                        team = model.teams.Find(t => t.Id == teamId);
                    }
                    else
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
                        model.teams.Add(team);
                        //serie.AddTeam(team);
                        homeClub.AddTeam(team);
                    }
                    else 
                    {
                        team.serie = serie;
                    }
                    team.sporthal = homeClub.sporthalls[0]; // just first sporthal.
                    poule.AddTeam(team);
                }
                
                // visitor teams (sommige competities hebben geen heen, en terug reeks
                if (parameters[visitorTeamIndex].Length > 0 && visitorClub != null)
                {
                    int teamId = int.Parse(parameters[visitorTeamIdIndex]);
                    Team team = null;
                    if (teamId >= 0)
                    {
                        team = model.teams.Find(t => t.Id == teamId);
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
                        model.teams.Add(team);
                        //serie.AddTeam(team);
                        visitorClub.AddTeam(team);
                    }
                    else
                    {
                        team.serie = serie;
                    }
                    if (visitorClub.sporthalls.Count > 0) team.sporthal = visitorClub.sporthalls[0]; // just first sporthal.
                    poule.AddTeam(team);
                }
            }
            foreach (Poule poule in importPoules)
            {
                foreach (string[] parameters in ParameterLines)
                {
                    if (parameters[pouleIndex] == poule.name && poule.serie.id == int.Parse(parameters[serieIdIndex]))
                    {
                        MatchWeek we = new MatchWeek(DateTime.Parse(parameters[dateIndex] + " " + parameters[timeIndex]));
                        if (poule.weeks.Find(w => w == we) == null) poule.weeks.Add(we);
                    }
                }
                poule.weeks.Sort();

                foreach (string[] parameters in ParameterLines)
                {
                    if (parameters[pouleIndex] == poule.name && poule.serie.id == int.Parse(parameters[serieIdIndex]))
                    {
                        Serie serie = model.series.Find(s => s.id == int.Parse(parameters[serieIdIndex]));
                        //Poule poule = serie.poules.Find(s => s.name == parameters[pouleIndex]);
                        if (parameters[homeTeamIndex].Length > 0 && parameters[visitorTeamIndex].Length > 0)
                        {
                            int homeTeamIndex1 = int.Parse(parameters[homeTeamIdIndex]);
                            Team t2 = model.teams.Find(t1 => t1.Id == homeTeamIndex1);
                            int homePouleTeamIndex = poule.teams.FindIndex(t => t.Id == homeTeamIndex1);
                            int visitorTeamIndex1 = int.Parse(parameters[visitorTeamIdIndex]);
                            int visitorPouleTeamIndex = poule.teams.FindIndex(t => t.Id == visitorTeamIndex1);
                            MatchWeek we = new MatchWeek(DateTime.Parse(parameters[dateIndex] + " " + parameters[timeIndex]));
                            int index = poule.weeks.FindIndex(w => w == we);
                            Match m = new Match(index, homePouleTeamIndex, visitorPouleTeamIndex, serie, poule);
                            m.Time = new Time(DateTime.Parse(parameters[timeIndex]));
                            Time ti = m.Time;
                            poule.matches.Add(m);
                        }
                    }
                }
            }
            model.RenewConstraints();
            model.Evaluate(null);
            model.Changed();
        }

        public void CalculateDistancesSporthalls(Model model)
        {
            foreach (Sporthal sporthal1 in model.sporthalls)
            {
                if (sporthal1.lat != 0 && sporthal1.lng != 0)
                {
                    foreach (Sporthal sporthal2 in model.sporthalls)
                    {
                        if (sporthal2.lat != 0 && sporthal2.lng != 0)
                        {
                            double lat_km = Math.Abs(sporthal1.lat - sporthal2.lat) * 110.0;
                            double lng_km = Math.Abs(sporthal1.lng - sporthal2.lng) * Math.Cos((Math.PI / 180) * sporthal1.lat) * 111.0;
                            double estimated_distance_km = 1.2 * Math.Sqrt((lat_km * lat_km) + (lng_km * lng_km));
                            if (sporthal1.distance.ContainsKey(sporthal2.id))
                                sporthal1.distance.Remove(sporthal2.id);
                            sporthal1.distance.Add(sporthal2.id, (int)estimated_distance_km);
                        }
                    }

                }

            }
        }

        public void CompareRegistrations(Model model, string openFileName, string saveFileName)
        {
            StreamWriter writer = new StreamWriter(saveFileName);
            writer.WriteLine("Club,Serie,Team-Id,Team,Dag,Tijd,Groep,Commentaar");
            Model modelOld = LoadFullCompetitionIntern(openFileName);
            // start comparing both projects
            foreach (Club club in model.clubs)
            {
                Club clubOld = modelOld.clubs.Find(c => c.Id == club.Id);
                if (clubOld != null)
                {
                    if (club.name != "Nationaal") CompareClub(writer, modelOld, club, clubOld);
                }
                else
                {

                }
            }
            writer.Close();
        }
        public void CompareClub(StreamWriter writer, Model modelOld, Club club, Club clubOld)
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

        public void ConvertKLVVCompetitionToCSV(Model model, string filename)
        {
            List<KeyValuePair<string, string>> output = new List<KeyValuePair<string, string>>();
            Dictionary<int, int> registrationMapping = new Dictionary<int, int>();
            Dictionary<int, int> clubMapping = new Dictionary<int, int>();
            string url3 = "http://klvv.be/server/series.php?season=2015-2016&trophy=false";
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


            foreach (Club club in model.clubs)
            {
                if (club.Id >= 0 && club.Id < 1000000)
                {
                    string url = "http://klvv.be/server/clubmatches.php?clubId=" + club.Id + "&hidePast=false&hideOut=true";
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

                                DateTime datetime = UnixTimeStampToDateTime(double.Parse(datetimeStr) / 1000);

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
                                    sporthal = model.sporthalls.Find(s => s.id == sporthalId).name;
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
                                output.Add(new KeyValuePair<string, string>(serieName + pouleName, line));

                            }
                        }
                    }
                }
            }
            List<Selection> selection = new List<Selection>();
            List<string> selected = new List<string>();
            foreach (KeyValuePair<string, string> kvp in output)
            {
                if (selected.Contains(kvp.Key) == false)
                {

                    selected.Add(kvp.Key);
                    Selection sel = new Selection(kvp.Key);
                    sel.selected = true;
                    selection.Add(sel);
                }
            }
            SelectionDialog diag = new SelectionDialog(selection, true);
            diag.ShowDialog();
            selection = diag.Selections;
            StreamWriter writer = new StreamWriter(filename);
            foreach (KeyValuePair<string, string> kvp in output)
            {
                Selection s = selection.Find(sel => sel.label == kvp.Key);
                if (s != null) writer.WriteLine(kvp.Value);
            }
            writer.Close();
        }
        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public void ConvertVVBCompetitionToCSV(Model model, string filename)
        {
            StreamWriter writer = new StreamWriter(filename);
            //XDocument doc = XDocument.Load(@"http://vvb.volleyadmin.be/services/wedstrijden_xml.php", LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
            XDocument doc = XDocument.Load(@"http://www.volleyadmin2.be/services/wedstrijden_xml.php?province_id=11", LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
            XElement kalender = Element(doc, "kalender");
            // Create mapping to known series
            Dictionary<string, Serie> series = new Dictionary<string, Serie>();
            foreach (XElement wedstrijd in kalender.Elements("wedstrijd"))
            {
                string poule = StringElement(wedstrijd, "reeks");
                int thuisploegId = IntegerElement(wedstrijd, "thuisploeg_id");
                string stamnummer = StringElement(wedstrijd, "stamnummer_thuisclub");
                Team team = model.teams.Find(t => t.Id == thuisploegId && t.club.Stamnumber == stamnummer);
                if(team != null)
                {
                    if(series.ContainsKey(poule))
                    { // check to be sure
                        if(team.serie.id != series[poule].id)
                        {
                            CompetitionCreator.Error.AddManualError("Inconsistent data ",
                                        "Team id "+thuisploegId+" has a serie id "+team.serie.id+" that is different from another team in that poule ("+poule+"), other serie id: "+series[poule].id);
                            MessageBox.Show("Team id " + thuisploegId + " has a serie id " + team.serie.id + " that is different from another team in that poule (" + poule + "), other serie id: " + series[poule].id);

                        }
                    }
                    else
                    {
                        series.Add(poule, team.serie);
                    }
                }
            }
            foreach (XElement wedstrijd in kalender.Elements("wedstrijd"))
            {
                string reeks = "VVB";
                string reeksId = "1000000";
                string pouleId = "-1";
                string aanvangsuur = StringElement(wedstrijd, "aanvangsuur");
                string datum = StringElement(wedstrijd, "datum");
                string thuisploeg = StringElement(wedstrijd, "thuisploeg");
                string bezoekersploeg = StringElement(wedstrijd, "bezoekersploeg");
                string sporthal = StringElement(wedstrijd, "sporthal");
                string poule = StringElement(wedstrijd, "reeks");
                if(series.ContainsKey(poule))
                {
                    reeks = series[poule].name;
                    reeksId = series[poule].id.ToString();
                }
                int thuisploegId = IntegerElement(wedstrijd, "thuisploeg_id");
                int bezoekersploegId = IntegerElement(wedstrijd, "bezoekersploeg_id");
                string stamnummer_thuisclub = StringElement(wedstrijd, "stamnummer_thuisclub");
                string stamnummer_bezoekersclub = StringElement(wedstrijd, "stamnummer_bezoekersclub");
                string thuisclubname = "VVB";
                int thuisclubId = 1000000;
                string bezoekersclubname = "VVB";
                int bezoekersclubId = 1000000;
                Club thuisclub = model.clubs.Find(c => c.Stamnumber == stamnummer_thuisclub);
                if (thuisclub != null)
                {
                    thuisclubname = thuisclub.name;
                    thuisclubId = thuisclub.Id;
                }
                Club bezoekersclub = model.clubs.Find(c => c.Stamnumber == stamnummer_bezoekersclub);
                if (bezoekersclub != null)
                {
                    bezoekersclubname = bezoekersclub.name;
                    bezoekersclubId = bezoekersclub.Id;
                }
                DateTime date = DateTime.Parse(datum);
                sporthal = sporthal.Replace(",", " ");
                sporthal = sporthal.Replace(",", " ");
                int sporthalId = -1;
                writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
                    reeks, reeksId, poule, pouleId, thuisploeg, thuisploegId, bezoekersploeg, bezoekersploegId, sporthal, sporthalId, date.ToShortDateString(), aanvangsuur, thuisclubname, thuisclubId, bezoekersclubname, bezoekersclubId);
            }
            writer.Close();
        }



    }
}
