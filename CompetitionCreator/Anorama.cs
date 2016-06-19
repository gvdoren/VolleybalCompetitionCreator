using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;

namespace CompetitionCreator
{
    public class YearPlan
    {
        public string Name;
        public int Count;
        public List<YearPlanWeek> weeks = new List<YearPlanWeek>();
    }
    
    public class YearPlanWeek
    {
        public MatchWeek week;
        public int weekNr = -1;
        public string Representation
        {
            get
            {
                if (week.dayOverruled)
                {
                    return week.PlayTime(week.OverruledDay).ToString("dd-MM-yyyy") + "("+ week.OverruledDay.ToString() +")";
                }
                else
                {
                    return week.Monday.ToString("dd-MM-yyyy") + " - " + week.Sunday.ToString("dd-MM-yyyy");
                }
            }
        }
        public YearPlanWeek(MatchWeek we)
        {
            week = new MatchWeek(we);
        }
        public string weekNrString(int max)
        {
            if (weekNr < 0) return "-";
            else
            {
                string reserve = "";
                if (weekNr > max) reserve = " R-"+week.round;
                return weekNr.ToString() + reserve;
            }
        }
    }

    public class YearPlans
    {
        public string title;
        public List<YearPlan> reeksen = new List<YearPlan>();
        public DateTime start;
        public DateTime end;
        public YearPlans(int year)
        {
            reeksen.Clear();
            start = new DateTime(year, 9, 1);
            end = new DateTime(year + 1, 5, 1);
            title = string.Format("Year plan: {0}-{1}", year, year + 1);
        }
        public YearPlan GetReeks(string name)
        {
            return reeksen.Find(w => w.Name == name);
        }
        public YearPlan CreateYearPlan(string name, int count)
        {
            YearPlan reeks = new YearPlan();
            reeks.Name = name;
            reeks.Count = count;
       
            return reeks;
        }
        public void WriteXML()
        {
            int year = start.Year;
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
                foreach (YearPlan reeks in reeksen)
                {
                    writer.WriteStartElement("Reeks");
                    writer.WriteAttributeString("Name", reeks.Name);
                    writer.WriteAttributeString("Count", reeks.Count.ToString());
                    writer.WriteStartElement("Weeks");
                    foreach (YearPlanWeek week in reeks.weeks)
                    {
                        if (week.weekNr >= 0)
                        {
                            writer.WriteStartElement("Week");
                            writer.WriteAttributeString("Date", week.week.Saturday.ToShortDateString());
                            if (week.week.dayOverruled) writer.WriteAttributeString("OverruledDay", week.week.OverruledDay.ToString());
                            if (week.week.round >= 0) writer.WriteAttributeString("Round", week.week.round.ToString());
                            writer.WriteAttributeString("WeekNumber", week.weekNr.ToString());
                            writer.WriteEndElement();
                        }
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
        public void ReadXML()
        {
            try
            {
                int year = start.Year;
                string BaseDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\CompetitionCreator";
                XElement yearPlan = XElement.Load(string.Format("{0}\\Annorama{1}.xml", BaseDirectory, year));
                title = ImportExport.StringAttribute(yearPlan, "Title");
                start = ImportExport.DateAttribute(yearPlan, "Start");
                end = ImportExport.DateAttribute(yearPlan, "End");
                reeksen.Clear();
                IEnumerable<XElement> Reeksen = ImportExport.Element(yearPlan, "Reeksen").Elements("Reeks");
                int i = 0;
                foreach (XElement reeks in Reeksen)
                {
                    string name = ImportExport.StringAttribute(reeks, "Name");
                    int count = ImportExport.IntegerAttribute(reeks, "Count");
                    YearPlan re = CreateYearPlan(name, count);
                    IEnumerable<XElement> Weeks = ImportExport.Element(reeks, "Weeks").Elements("Week");
                    foreach (XElement week in Weeks)
                    {
                        DateTime dt = ImportExport.DateAttribute(week, "Date");
                        MatchWeek we = new MatchWeek(dt);
                        string roundString = ImportExport.StringOptionalAttribute(week, null, "Round");
                        if (roundString != null)
                            we.round = int.Parse(roundString);
                        YearPlanWeek anWeek = new YearPlanWeek(we);
                        re.weeks.Add(anWeek);

                        string overruledDayString = ImportExport.StringOptionalAttribute(week, "No", "OverruledDay");
                        if (overruledDayString == "No")
                        {
                            anWeek.week.dayOverruled = false;
                        }
                        else
                        {
                            anWeek.week.dayOverruled = true;
                            if (overruledDayString == DayOfWeek.Monday.ToString()) anWeek.week.OverruledDay = DayOfWeek.Monday;
                            if (overruledDayString == DayOfWeek.Tuesday.ToString()) anWeek.week.OverruledDay = DayOfWeek.Tuesday;
                            if (overruledDayString == DayOfWeek.Wednesday.ToString()) anWeek.week.OverruledDay = DayOfWeek.Wednesday;
                            if (overruledDayString == DayOfWeek.Thursday.ToString()) anWeek.week.OverruledDay = DayOfWeek.Thursday;
                            if (overruledDayString == DayOfWeek.Friday.ToString()) anWeek.week.OverruledDay = DayOfWeek.Friday;
                            if (overruledDayString == DayOfWeek.Saturday.ToString()) anWeek.week.OverruledDay = DayOfWeek.Saturday;
                            if (overruledDayString == DayOfWeek.Sunday.ToString()) anWeek.week.OverruledDay = DayOfWeek.Sunday;
                        }
                        anWeek.weekNr = ImportExport.IntegerAttribute(week, "WeekNumber");
                    }
                    re.weeks.Sort((w1, w2) => { return w1.week.CompareTo(w2.week); });
                    this.reeksen.Add(re);
                    i++;
                }
            }
            catch { }
        }
    }
}
