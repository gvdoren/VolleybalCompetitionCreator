using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VolleybalCompetition_creator
{
    public class Time
    {
        public int Hours;
        public int Minutes;
        public string Value
        {
            get { return ToString(); }
            set {
                Hours = 0;
            }

        }
        public Time(Time time)
        {
            this.Hours = time.Hours;
            this.Minutes = time.Minutes;
        }
        public Time(int hours, int minutes)
        {
            this.Hours = hours;
            this.Minutes = minutes;
        }
        public Time(DateTime datetime)
        {
            this.Hours = datetime.Hour;
            this.Minutes = datetime.Minute;
        }
        public void AddMinutes(int minutes)
        {
            Minutes += minutes;
            while (Minutes >= 60)
            {
                Hours++;
                Minutes -= 60;
            }
            while (Hours >= 24) Hours -= 24;
        }
        public static bool operator <(Time t1, Time t2)
        {
            return t1.Hours < t2.Hours || (t1.Hours == t2.Hours && t1.Minutes < t2.Minutes);
        }
        public static bool operator >(Time t1, Time t2)
        {
            return t1.Hours > t2.Hours || (t1.Hours == t2.Hours && t1.Minutes > t2.Minutes);
        }
        public static bool operator ==(Time t1, Time t2)
        {
            return t1.Hours == t2.Hours && t1.Minutes == t2.Minutes;
        }
        public static bool operator !=(Time t1, Time t2)
        {
            return ((Object)t1) == null || ((Object)t2) == null || t1.Hours != t2.Hours || t1.Minutes != t2.Minutes;
        }
        public override int GetHashCode()
        {
            return (Hours * 60) + Minutes;
        }
        public override bool Equals(object obj)
        {
            Time t = obj as Time;
            if (t != null)
            {
                return (this == t);
            }
            else
            {
                return false;
            }
        }
        public override string ToString()
        {
            return Hours.ToString() + ":" + Minutes.ToString("D2");
        }
    }
}
