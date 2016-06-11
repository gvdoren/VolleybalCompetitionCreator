using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CompetitionCreator
{

    public class MatchWeek : ConstraintAdmin, IComparable<MatchWeek>
    {
        public int round = -1;
        public bool dayOverruled = false;
        public DayOfWeek OverruledDay = DayOfWeek.Wednesday;
        private DateTime date;
        DateTime[] cachedDates = new DateTime[8];
        bool[] cachedDatesB = new bool[8];
        public DateTime PlayTime(DayOfWeek day)
        {
            if (cachedDatesB[(int)day])
                return cachedDates[(int)day];
            else
            {
                cachedDates[(int)day] = PlayTimeInt(day);
                cachedDatesB[(int)day] = true;
                return cachedDates[(int)day];
            }
        }
        public DateTime PlayTimeInt(DayOfWeek day)
        {
            if (dayOverruled) return Monday.AddDays(OverruledDay - DayOfWeek.Monday).Date;
            if (day == 0) day += 7;
            return Monday.AddDays(day - DayOfWeek.Monday).Date;
        }
        public MatchWeek(string datestr)
        {
            date = DateTime.ParseExact(datestr, "yyyy-MM-dd", null).Date;
            CalculateMonday();
        }
        public MatchWeek(DateTime date)
        {
            this.date = date.Date;
            CalculateMonday();
        }
        public MatchWeek(MatchWeek week)
        {
            this.round = week.round;
            this.dayOverruled = week.dayOverruled;
            this.date = week.date;
            CalculateMonday();
        }
        public static bool operator <(MatchWeek w1, MatchWeek w2)
        {
            return w1.Monday < w2.Monday || (w1.Monday == w2.Monday && w1.dayOverruled && w2.dayOverruled == false);
        }
        public static bool operator >(MatchWeek w1, MatchWeek w2)
        {
            return w2.Monday < w1.Monday || (w2.Monday == w1.Monday && w2.dayOverruled && w1.dayOverruled == false);
        }
        public override string ToString()
        {
            if (dayOverruled)
            {
                return PlayTime(OverruledDay) + "* - " + PlayTime(OverruledDay);
            } else
            return Monday.ToShortDateString() + "-" + Monday.AddDays(6).ToShortDateString();
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
            return x.Monday == y.Monday && x.dayOverruled == y.dayOverruled && y.OverruledDay == x.OverruledDay;
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
        //private const DayOfWeek Monday = DayOfWeek.Monday;
        private DateTime _Monday;
        public DateTime Monday { get { return _Monday; } }
        private DateTime _Tuesday;
        public DateTime Tuesday { get { return _Tuesday; } }
        private DateTime _Wednesday;
        public DateTime Wednesday { get { return _Wednesday; } }
        private DateTime _Thursday;
        public DateTime Thursday { get { return _Thursday; } }
        private DateTime _Friday;
        public DateTime Friday { get { return _Friday; } }
        private DateTime _Saturday;
        public DateTime Saturday { get { return _Saturday; } }
        private DateTime _Sunday;
        public DateTime Sunday { get { return _Sunday; } }
        private void CalculateMonday()
        {
            int daysOffset = date.DayOfWeek - DayOfWeek.Monday;
            if (daysOffset < 0) daysOffset += 7;

            _Monday = date.AddDays(-daysOffset);
            _Tuesday = Monday.AddDays(1);
            _Wednesday = Monday.AddDays(2);
            _Thursday = Monday.AddDays(3);
            _Friday = Monday.AddDays(4);
            _Saturday = _Monday.AddDays(5);
            _Sunday = _Monday.AddDays(6);

            foreach(DayOfWeek dow in Enum.GetValues(typeof(DayOfWeek)))
            {
                cachedDatesB[(int)dow] = false;
            }
        }
        public int WeekNr()
        {
            int WeekNr = 0;
            System.Globalization.CultureInfo cul = System.Globalization.CultureInfo.CurrentCulture;
            WeekNr = cul.Calendar.GetWeekOfYear(
                date,
                System.Globalization.CalendarWeekRule.FirstFullWeek,
                DayOfWeek.Monday);
            return WeekNr;
        }
    }
}
