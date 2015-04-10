using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;

namespace CompetitionCreator
{
    public class AnnoramaReeks
    {
        public string Name;
        public int Count;
        public List<AnnoramaWeek> weeks = new List<AnnoramaWeek>();
    }
    
    public class AnnoramaWeek
    {
        public Week week;
        public bool match;
        public List<int> weekNrs = new List<int>();
        public AnnoramaWeek(Week we, bool match)
        {
            week = we;
            this.match = match;
        }
        public string weekNrString(int max)
        {
            if (weekNrs.Count == 0)
            {
                if (match) return "x";
                else return "-";
            }
            else if (weekNrs.Count == 1)
            {
                string reserve = "";
                if (weekNrs[0] > max) reserve = " R";
                return weekNrs[0].ToString() + reserve;
            }
            else
            {
                string reserve = "";
                if (weekNrs[0] > max) reserve = " R";
                string returnString = weekNrs[0].ToString() + reserve;
                for (int i = 1; i < weekNrs.Count; i++)
                {
                    reserve = "";
                    if (weekNrs[i] > max) reserve = " R";
                    returnString += ", " + weekNrs[i].ToString() + reserve;
                }
                return returnString;
            }
        }
    }

    public class Annorama
    {
        public string title;
        public List<AnnoramaReeks> reeksen = new List<AnnoramaReeks>();
        public DateTime start;
        public DateTime end;
        public Annorama(int year)
        {
            reeksen.Clear();
            start = new DateTime(year, 9, 1);
            end = new DateTime(year + 1, 5, 1);
            title = string.Format("Annorama Seizoen {0}-{1}", year, year + 1);
            try
            {
                ReadXML(year);
            }
            catch
            {
            }
        }
        public AnnoramaReeks GetReeks(string name)
        {
            return reeksen.Find(w => w.Name == name);
        }
        public AnnoramaReeks CreateReeks(string name, int count)
        {
            AnnoramaReeks reeks = new AnnoramaReeks();
            reeks.Name = name;
            reeks.Count = count;
            Week week = new Week(start);
            DateTime current = start;
            while (week.FirstDayInWeek < end)
            {
                reeks.weeks.Add(new AnnoramaWeek(week, false));
                current = current.AddDays(7);
                week = new Week(current);
            }
            reeksen.Add(reeks);
            return reeks;
        }
        public void WriteXML(int year)
        {
            string BaseDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\CompetitionCreator";
            using (XmlWriter writer = XmlWriter.Create(string.Format("{0}\\Annorama{1}.xml", BaseDirectory, year)))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Annorama");
                writer.WriteAttributeString("Title", title);
                writer.WriteAttributeString("Start", start.ToShortDateString());
                writer.WriteAttributeString("End", end.ToShortDateString());
                writer.WriteStartElement("Reeksen");
                int i=0;
                foreach (AnnoramaReeks reeks in reeksen)
                {
                    writer.WriteStartElement("Reeks");
                    writer.WriteAttributeString("Name", reeks.Name);
                    writer.WriteAttributeString("Count", reeks.Count.ToString());
                    writer.WriteStartElement("Weeks");
                    foreach (AnnoramaWeek week in reeks.weeks)
                    {
                        writer.WriteStartElement("Week");
                        writer.WriteAttributeString("Date", week.week.Saturday.ToShortDateString());
                        writer.WriteAttributeString("Match", week.match.ToString());
                        writer.WriteStartElement("WeekNumbers");
                        foreach (int weeknr in week.weekNrs)
                        {
                            writer.WriteElementString("Number", weeknr.ToString());
                        }
                        writer.WriteEndElement();
                        writer.WriteEndElement();

                    }
                    i++;
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }
        public void ReadXML(int year)
        {
            string BaseDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\CompetitionCreator";
            XElement Annorama = XElement.Load(string.Format("{0}\\Annorama{1}.xml", BaseDirectory, year));
            title = ImportExport.StringAttribute(Annorama,"Title");
            start = ImportExport.DateAttribute(Annorama, "Start");
            end = ImportExport.DateAttribute(Annorama, "End");
            reeksen.Clear();
            IEnumerable<XElement> Reeksen = ImportExport.Element(Annorama, "Reeksen").Elements("Reeks");
            int i = 0;
            foreach (XElement reeks in Reeksen)
            {
                string name = ImportExport.StringAttribute(reeks, "Name");
                int count = ImportExport.IntegerAttribute(reeks, "Count");
                AnnoramaReeks re = CreateReeks(name, count);
                IEnumerable<XElement> Weeks = ImportExport.Element(reeks,"Weeks").Elements("Week");
                foreach (XElement week in Weeks)
                {
                    DateTime dt = ImportExport.DateAttribute(week, "Date");
                    Week we = new Week(dt);
                    bool match = ImportExport.BoolAttribute(week, "Match");
                    AnnoramaWeek anWeek = re.weeks.Find(w => w.week == we);
                    anWeek.match = match;
                    foreach (XElement weeknr in week.Element("WeekNumbers").Elements("Number"))
                    {
                        anWeek.weekNrs.Add(int.Parse(weeknr.Value.ToString()));
                    }
                }
                i++;
            }
        }
    }
}
