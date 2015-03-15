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
        public List<AnnoramaWeekend> weekends = new List<AnnoramaWeekend>();
    }
    
    public class AnnoramaWeekend
    {
        public Weekend weekend;
        public bool match;
        public AnnoramaWeekend(Weekend we, bool match)
        {
            weekend = we;
            this.match = match;
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
            Weekend weekend = new Weekend(start);
            DateTime current = start;
            while (weekend.Saturday < end)
            {
                reeks.weekends.Add(new AnnoramaWeekend(weekend, false));
                current = current.AddDays(7);
                weekend = new Weekend(current);
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
                    writer.WriteStartElement("Weekends");
                    foreach (AnnoramaWeekend weekend in reeks.weekends)
                    {
                        writer.WriteStartElement("Weekend");
                        writer.WriteAttributeString("Date", weekend.weekend.Saturday.ToShortDateString());
                        writer.WriteAttributeString("Match", weekend.match.ToString());
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
            title = Annorama.Attribute("Title").Value;
            start = DateTime.Parse(Annorama.Attribute("Start").Value.ToString());
            end = DateTime.Parse(Annorama.Attribute("End").Value.ToString());
            reeksen.Clear();
            IEnumerable<XElement> Reeksen = Annorama.Element("Reeksen").Elements("Reeks");
            int i = 0;
            foreach (XElement reeks in Reeksen)
            {
                string name = reeks.Attribute("Name").Value;
                int count = int.Parse(reeks.Attribute("Count").Value);
                AnnoramaReeks re = CreateReeks(name, count);
                IEnumerable<XElement> Weekends = reeks.Element("Weekends").Elements("Weekend");
                foreach (XElement Weekend in Weekends)
                {
                    DateTime dt = DateTime.Parse(Weekend.Attribute("Date").Value);
                    Weekend we = new Weekend(dt);
                    bool match = bool.Parse(Weekend.Attribute("Match").Value.ToString());
                    AnnoramaWeekend anWeekend = re.weekends.Find(w => w.weekend.Saturday == we.Saturday);
                    anWeekend.match = match;
                }
                i++;
            }
        }
    }
}
