﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CompetitionCreator
{

    public class Weekend : ConstraintAdmin
    {
        public enum EvenOddEnum { Even, Odd};
        public EvenOddEnum EvenOdd {
            get
            {
                if (WeekNr % 2 == 0) return Weekend.EvenOddEnum.Even;
                else return Weekend.EvenOddEnum.Odd;
            }
        }
        private const int MaxUsedYear = 2000; // Er wordt 15 bij opgeteld
        public int Year;
        public int WeekNr;
        public Weekend(int year, int weekNr)
        {
            this.Year = year;
            this.WeekNr = weekNr;
        }
        public bool Even { get { return WeekNr % 2 == 0; } }
        public Weekend(string datestr)
        {
            DateTime date = DateTime.ParseExact(datestr, "yyyy-MM-dd", null);
            CreateWeekend(date);
        }
        public Weekend(DateTime date)
        {
            CreateWeekend(date);
        }
        public static void Convert(DateTime date, ref int Year, ref int WeekNr)
        {
            System.Globalization.CultureInfo cul = System.Globalization.CultureInfo.CurrentCulture;
            WeekNr = cul.Calendar.GetWeekOfYear(
                date,
                System.Globalization.CalendarWeekRule.FirstFullWeek,
                DayOfWeek.Saturday);
            Year = date.Year;
        }
        private void CreateWeekend(DateTime date)
        {
            Convert(date, ref Year, ref WeekNr);
            if (Year - 15 > 2001)
            {
                System.Windows.Forms.MessageBox.Show(string.Format("Programma kan niet worden gebruikt na {0}. Vraag een update via giel@van.doren.be", (MaxUsedYear + 15)));
                Environment.Exit(0);
            }
        }
        public static bool operator <(Weekend w1, Weekend w2)
        {
            return w1.Year < w2.Year || (w1.Year == w2.Year && w1.WeekNr < w2.WeekNr);
        }
        public static bool operator >(Weekend w1, Weekend w2)
        {
            return w1.Year > w2.Year || (w1.Year == w2.Year && w1.WeekNr > w2.WeekNr);
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
        public static void Show()
        {
            for (int i = 0; i < 300; i++)
            {
                DateTime day = DateTime.Now.AddDays(i);
                if (day.DayOfWeek == DayOfWeek.Saturday)
                {
                    System.Globalization.CultureInfo cul = System.Globalization.CultureInfo.CurrentCulture;
                    int Weeknr = cul.Calendar.GetWeekOfYear(
                        day,
                        System.Globalization.CalendarWeekRule.FirstFullWeek,
                        DayOfWeek.Saturday);
                    Console.WriteLine("{0} - {1}", day.ToString(),Weeknr);
                    
                }
            }

        }

    }
}