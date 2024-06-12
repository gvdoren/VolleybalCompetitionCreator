using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CompetitionCreator
{

    public class MatchWeek : ConflictAdmin, IComparable<MatchWeek>
    {
        public int round = -1;
        private DateTime date;
        int weeknr;

        public DateTime PlayTime(DayOfWeek day)
        {
            if (day == 0) day += 7;
            return Monday.AddDays(day - DayOfWeek.Monday).Date;
        }
        public MatchWeek(string datestr)
        {
            CalculateMonday(ImportExport.ParseDateTime(datestr).Date);
        }
        public MatchWeek(DateTime date)
        {
            CalculateMonday(date);
        }
        public MatchWeek(MatchWeek week)
        {
            this.round = week.round;
            this.date = week.date;
            this.weeknr = week.weeknr;
        }
        public static bool operator <(MatchWeek w1, MatchWeek w2)
        {
            return w1.Monday < w2.Monday;
        }
        public static bool operator >(MatchWeek w1, MatchWeek w2)
        {
            return w2.Monday < w1.Monday;
        }
        public override string ToString()
        {
            return Monday.ToString("yyyy-MM-dd") + "-" + Monday.AddDays(6).ToString("yyyy-MM-dd");
        }
        public string Start
        {
            get 
            {
                return Saturday.ToString("dd-MM");
            }
        }
        public string End
        {
            get
            {
                return Sunday.ToString("dd-MM");
            }
        }
        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            MatchWeek p = (MatchWeek)obj;
            return p == this;
        }
        public override int GetHashCode()
        {
            return (int)date.Ticks;
        }
        public static bool operator ==(MatchWeek x, MatchWeek y)
        {
            Object xo = (Object)x;
            Object yo = (Object)y;
            if (xo == null && yo != null) return false;
            if (xo != null && yo == null) return false;
            if (xo == null && yo == null) return true;
            return x.Monday == y.Monday;
        }
        public static bool operator !=(MatchWeek x, MatchWeek y)
        {
            return !(x == y);
        }
        public int CompareTo(MatchWeek other)
        {
            if(this < other) return -1;
            else if (this == other) return 0;
            else return 1;
        }
        public DateTime Monday { get { return date; } }
        public DateTime Tuesday { get { return date.AddDays(1); } }
        public DateTime Wednesday { get { return date.AddDays(2); } }
        public DateTime Thursday { get { return date.AddDays(3); } }
        public DateTime Friday { get { return date.AddDays(4); } }
        public DateTime Saturday { get { return date.AddDays(5); } }
        public DateTime Sunday { get { return date.AddDays(6); } }
        private void CalculateMonday(DateTime d)
        {
            int daysOffset = d.DayOfWeek - DayOfWeek.Monday;
            if (daysOffset < 0) daysOffset += 7;

            date = d.AddDays(-daysOffset);
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            // DateTime time = date;
            // DayOfWeek day = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            // if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            // {
            //     time = time.AddDays(3);
            // }
            // 
            // // Return the week of our adjusted day
            // return System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            weeknr = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date.AddDays(3), System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
        public int WeekNumber { get { return weeknr; } }
        public int WeekNr()
        {
            return weeknr;
        }
    }
}
