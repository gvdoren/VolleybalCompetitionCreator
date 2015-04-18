using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CompetitionCreator
{

    public class Week : ConstraintAdmin, IComparable<Week>
    {
        public bool dayOverruled = false;
        public DayOfWeek OverruledDay = DayOfWeek.Wednesday;
        private DateTime date;
        public DateTime PlayTime(DayOfWeek day)
        {
            if (dayOverruled) return FirstDayInWeek.AddDays(OverruledDay - firstDayInWeek).Date;
            if (day == 0) day += 7;
            return FirstDayInWeek.AddDays(day - firstDayInWeek).Date;
        }
        public Week(string datestr)
        {
            date = DateTime.ParseExact(datestr, "yyyy-MM-dd", null).Date;
            CalculateFirstDayInWeek();
        }
        public Week(DateTime date)
        {
            this.date = date.Date;
            CalculateFirstDayInWeek();
        }
        public Week(Week week)
        {
            this.dayOverruled = week.dayOverruled;
            this.date = week.date;
            CalculateFirstDayInWeek();
        }
        public static bool operator <(Week w1, Week w2)
        {
            return w1.FirstDayInWeek < w2.FirstDayInWeek || (w1.FirstDayInWeek == w2.FirstDayInWeek && w1.dayOverruled && w2.dayOverruled == false);
        }
        public static bool operator >(Week w1, Week w2)
        {
            return w2.FirstDayInWeek < w1.FirstDayInWeek || (w2.FirstDayInWeek == w1.FirstDayInWeek && w2.dayOverruled && w1.dayOverruled == false);
        }
        public override string ToString()
        {
            if (dayOverruled)
            {
                return PlayTime(OverruledDay) + "* - " + PlayTime(OverruledDay);
            } else
            return FirstDayInWeek.ToShortDateString() + "-" + FirstDayInWeek.AddDays(6).ToShortDateString();
        }
        public string Start
        {
            get 
            {
                if (dayOverruled) return PlayTime(OverruledDay).ToString("dd-MM");
                else return Saturday.ToString("dd-MM");
            }
        }
        public string End
        {
            get
            {
                if (dayOverruled) return "*"+OverruledDay.ToString();
                else return Sunday.ToString("dd-MM");
            }
        }
        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            Week p = (Week)obj;
            return (FirstDayInWeek == p.FirstDayInWeek && dayOverruled == p.dayOverruled);
        }
        public override int GetHashCode()
        {
            return (int)date.Ticks;
        }
        public static bool operator ==(Week x, Week y)
        {
            Object xo = (Object)x;
            Object yo = (Object)y;
            if (xo == null && yo != null) return false;
            if (xo != null && yo == null) return false;
            if (xo == null && yo == null) return true;
            return x.FirstDayInWeek == y.FirstDayInWeek && x.dayOverruled == y.dayOverruled;
        }
        public static bool operator !=(Week x, Week y)
        {
            return !(x == y);
        }
        public int CompareTo(Week other)
        {
            if(this < other) return -1;
            else if (this == other) return 0;
            else return 1;
        }
        private const DayOfWeek firstDayInWeek = DayOfWeek.Monday;
        private DateTime _FirstDayInWeek;
        public DateTime FirstDayInWeek { get { return _FirstDayInWeek; } }
        private void CalculateFirstDayInWeek()
        {
            int daysOffset = date.DayOfWeek - firstDayInWeek;
            if (daysOffset < 0) daysOffset += 7;

            _FirstDayInWeek = date.AddDays(-daysOffset);
            _Saturday = _FirstDayInWeek.AddDays(5);
            _Sunday = _FirstDayInWeek.AddDays(6);
        }
        private DateTime _Saturday;
        public DateTime Saturday
        {
            get
            {
                return _Saturday;
            }
        }
        private DateTime _Sunday;
        public DateTime Sunday
        {
             get 
            {
                return _Sunday;
            }
        }
        public int WeekNr()
        {
            int WeekNr = 0;
            System.Globalization.CultureInfo cul = System.Globalization.CultureInfo.CurrentCulture;
            WeekNr = cul.Calendar.GetWeekOfYear(
                date,
                System.Globalization.CalendarWeekRule.FirstFullWeek,
                firstDayInWeek);
            return WeekNr;
        }
    }
}
