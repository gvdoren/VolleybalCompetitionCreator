using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace VolleybalCompetition_creator
{
    public class Weekend : ConstraintAdmin
    {
        public int Year { get; set; }
        public int WeekNr { get; set; }
        public Weekend(int year, int weekNr)
        {
            this.Year = year;
            this.WeekNr = weekNr;
        }
        public Weekend(string datestr)
        {
            DateTime date = DateTime.ParseExact(datestr, "yyyy-MM-dd", null);
            System.Globalization.CultureInfo cul = System.Globalization.CultureInfo.CurrentCulture;
            this.WeekNr = cul.Calendar.GetWeekOfYear(
                date,
                System.Globalization.CalendarWeekRule.FirstFullWeek,
                DayOfWeek.Saturday);
            this.Year = date.Year;
            //date = DateTime.Parse(datestr);
        }
        public static bool operator <(Weekend w1, Weekend w2)
        {
            return w1.Year < w2.Year || (w1.Year == w2.Year && w1.WeekNr < w2.WeekNr);
        }
        public static bool operator >(Weekend w1, Weekend w2)
        {
            return w1.Year > w2.Year || (w1.Year == w2.Year && w1.WeekNr > w2.WeekNr);
        }
        public Weekend(DateTime date)
        {
            System.Globalization.CultureInfo cul = System.Globalization.CultureInfo.CurrentCulture;
            this.WeekNr = cul.Calendar.GetWeekOfYear(
                date,
                System.Globalization.CalendarWeekRule.FirstFullWeek,
                DayOfWeek.Saturday);
            this.Year = date.Year;
            //date = DateTime.Parse(datestr);
        }
        public override string ToString()
        {
            return Saturday.ToShortDateString();
        }
        public DateTime Saturday
        {
            get
            {
                Debug.Assert(WeekNr >= 1);
                DateTime jan1 = new DateTime(Year, 1, 1);
                int daysOffset = DayOfWeek.Saturday - jan1.DayOfWeek;
                if (daysOffset < 0) daysOffset += 7;
                DateTime firstSaturday = jan1.AddDays(daysOffset);
                Debug.Assert(firstSaturday.DayOfWeek == DayOfWeek.Saturday);
                Debug.Assert(firstSaturday.Year == Year);
                DateTime result = firstSaturday.AddDays((WeekNr - 1) * 7);

                return result;
            }
        }
        public DateTime Sunday
        {
            get
            {
                return Saturday.AddDays(1);
            }
        }
        // Test-code to check whether the week-conversion is correct.
        public static void Test()
        {
            for (int i = 0; i < 10000; i++)
            {
                DateTime day = DateTime.Now.AddDays(i);
                if (day.DayOfWeek == DayOfWeek.Saturday)
                {
                    System.Globalization.CultureInfo cul = System.Globalization.CultureInfo.CurrentCulture;
                    int Weeknr = cul.Calendar.GetWeekOfYear(
                        day,
                        System.Globalization.CalendarWeekRule.FirstFullWeek,
                        DayOfWeek.Saturday);
                    int Year = day.Year;
                    DateTime calculatedDate = new Weekend(Year, Weeknr).Saturday;
                    if (calculatedDate.Year != Year ||
                        calculatedDate.DayOfYear != day.DayOfYear)
                    {
                        Console.WriteLine("Failed for date: {0}", day.ToString());
                    }
                }
            }
        }

    }
}
