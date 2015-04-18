using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CompetitionCreator
{

    public class Week : ConstraintAdmin
    {
        public enum EvenOddEnum { Even, Odd};
        public EvenOddEnum EvenOdd {
            get
            {
                if (WeekNr % 2 == 0) return Week.EvenOddEnum.Even;
                else return Week.EvenOddEnum.Odd;
            }
        }
        private int Year;
        private int WeekNr;
        public Week(int year, int weekNr)
        {
            this.Year = year;
            this.WeekNr = weekNr;
        }
        public bool Even { get { return WeekNr % 2 == 0; } }
        public Week(string datestr)
        {
            DateTime date = DateTime.ParseExact(datestr, "yyyy-MM-dd", null);
            CreateWeek(date);
        }
        public Week(DateTime date)
        {
            CreateWeek(date);
        }
        private void CreateWeek(DateTime date)
        {
            Convert(date, ref Year, ref WeekNr);
        }
        public static bool operator <(Week w1, Week w2)
        {
            return w1.Year < w2.Year || (w1.Year == w2.Year && w1.WeekNr < w2.WeekNr);
        }
        public static bool operator >(Week w1, Week w2)
        {
            return w1.Year > w2.Year || (w1.Year == w2.Year && w1.WeekNr > w2.WeekNr);
        }
        public override string ToString()
        {
            return Saturday.ToShortDateString();
        }


        private const DayOfWeek firstDayInWeek = DayOfWeek.Monday;
        public DateTime FirstDayInWeek
        {
            get
            {
                
                Debug.Assert(WeekNr >= 1);
                DateTime jan1 = new DateTime(Year, 1, 1);

                int daysOffset = firstDayInWeek - jan1.DayOfWeek;
                if (daysOffset < 0) daysOffset += 7;
                
                DateTime firstDayInWeek1 = jan1.AddDays(daysOffset);
                Debug.Assert(firstDayInWeek1.DayOfWeek == firstDayInWeek);
                Debug.Assert(firstDayInWeek1.Year == Year);
                DateTime result = firstDayInWeek1.AddDays((WeekNr - 1) * 7);

                return result.Date;
            }
        }
        public DateTime Saturday
        {
            get
            {
                return FirstDayInWeek.AddDays(DayOfWeek.Saturday - firstDayInWeek).Date;
            }
        }
        public DateTime Sunday
        {
             get 
            {
                return FirstDayInWeek.AddDays(DayOfWeek.Sunday - firstDayInWeek).Date;
            }
        }
        public static void Convert(DateTime date, ref int Year, ref int WeekNr)
        {
            System.Globalization.CultureInfo cul = System.Globalization.CultureInfo.CurrentCulture;
            WeekNr = cul.Calendar.GetWeekOfYear(
                date,
                System.Globalization.CalendarWeekRule.FirstFullWeek,
                firstDayInWeek);
            Year = date.AddDays(firstDayInWeek - date.DayOfWeek).Year;
            //Year = date.Year;
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
                        firstDayInWeek);
                    int Year = day.AddDays(firstDayInWeek-day.DayOfWeek).Year;
                    DateTime calculatedDate = new Week(Year, Weeknr).Saturday;
                    if (calculatedDate.Year != day.Year ||
                        calculatedDate.DayOfYear != day.DayOfYear)
                    {
                        Console.WriteLine("Failed for date: {0} != {1} - {2}", day.ToString(), calculatedDate, Weeknr);
                    }
                }
            }
        }
        public static void Show()
        {
            for (int i = 0; i < 300; i++)
            {
                DateTime day = DateTime.Now.AddDays(i);
                //if (day.DayOfWeek == DayOfWeek.Saturday)
                {
                    System.Globalization.CultureInfo cul = System.Globalization.CultureInfo.CurrentCulture;
                    int Weeknr = cul.Calendar.GetWeekOfYear(
                        day,
                        System.Globalization.CalendarWeekRule.FirstFullWeek, firstDayInWeek);
                    Console.WriteLine("{0} - {1}", day.ToString(),Weeknr);
                    
                }
            }

        }

    }
}
