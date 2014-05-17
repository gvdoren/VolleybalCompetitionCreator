using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;

namespace VolleybalCompetition_creator
{
    public class AnoramaWeekend
    {
        public const int MaxReeksen = 10;
        public Weekend weekend;
        public bool[] reeksen = new bool[MaxReeksen];
        public AnoramaWeekend(Weekend we)
        {
            this.weekend = we;
            for (int i = 0; i < MaxReeksen; i++) reeksen[i] = true;
        }
    }

    public class Anorama
    {
        public string title;
        public List<AnoramaWeekend> weekends = new List<AnoramaWeekend>();
        public List<string> reeksen = new List<string>();
        public Anorama(int year)
        {
            try
            {
                ReadXML(year);
            }
            catch
            {
                CreateAnorama(year);
            }
        }
        public void CreateAnorama(int year)
        {
            weekends.Clear();
            reeksen.Clear();
            Weekend weekend = new Weekend(new DateTime(year, 9, 1));
            DateTime end = new DateTime(year + 1, 5, 1);
            while (weekend.Saturday < end)
            {
                weekends.Add(new AnoramaWeekend(weekend));
                weekend = new Weekend(weekend.Saturday.AddDays(7));
            }
            title = string.Format("Anorama Seizoen {0}-{1}", year, year + 1);
            CreateReeks("14");
            CreateReeks("12");
            CreateReeks("10");
            CreateReeks("8");
            CreateReeks("6");
            CreateReeks("4");
        }
        public List<Weekend> GetReeks(string name)
        {
            List<AnoramaWeekend> we = weekends.FindAll((w => w.reeksen[reeksen.FindIndex(n => n == name)] == true));
            return we.Select(w => w.weekend).ToList();
        }
        public void CreateReeks(string name)
        {
            reeksen.Add(name);
        }
        public void WriteXML(int year)
        {
            string MyDocuments = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            using (XmlWriter writer = XmlWriter.Create(string.Format("{0}\\Anorama{1}.xml", MyDocuments, year)))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Anorama");
                writer.WriteAttributeString("Title", title);
                writer.WriteStartElement("Reeksen");
                foreach (string reeks in reeksen)
                {
                    writer.WriteElementString("Reeks", reeks);
                }
                writer.WriteEndElement();
                writer.WriteStartElement("Weekends");
                foreach (AnoramaWeekend weekend in weekends)
                {
                    writer.WriteStartElement("Weekend");
                    writer.WriteAttributeString("Date", weekend.weekend.Saturday.ToShortDateString());
                    for (int i = 0; i < reeksen.Count; i++)
                    {
                        writer.WriteStartElement("Reeks");
                        writer.WriteAttributeString("name", reeksen[i]);
                        writer.WriteAttributeString("value", weekend.reeksen[i].ToString());
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

        }
        public void ReadXML(int year)
        {
            string MyDocuments = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            XElement Anorama = XElement.Load(string.Format("{0}\\Anorama{1}.xml",MyDocuments , year));
            title = Anorama.Attribute("Title").Value;
            IEnumerable<XElement> Reeksen = Anorama.Element("Reeksen").Elements("Reeks");
            foreach (XElement reeks in Reeksen)
            {
                reeksen.Add(reeks.Value);
            }
            IEnumerable<XElement> Weekends = Anorama.Element("Weekends").Elements("Weekend");
            foreach (XElement Weekend in Weekends)
            {
                DateTime dt = DateTime.Parse(Weekend.Attribute("Date").Value);
                AnoramaWeekend anoramaweekend = new AnoramaWeekend(new Weekend(dt));
                IEnumerable<XElement> Reeksen1 = Weekend.Elements("Reeks");
                foreach (XElement reeks1 in Reeksen1)
                {
                    string name = reeks1.Attribute("name").Value;
                    int index = this.reeksen.FindIndex(r => r == name);
                    anoramaweekend.reeksen[index] = bool.Parse(reeks1.Attribute("value").Value);
                }
                weekends.Add(anoramaweekend);
            }
        }
    }
}
