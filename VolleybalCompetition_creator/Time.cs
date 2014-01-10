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
        public static bool operator <(Time t1, Time t2)
        {
            return t1.Hours < t2.Hours || (t1.Hours == t2.Hours && t1.Minutes < t2.Minutes);
        }
        public static bool operator >(Time t1, Time t2)
        {
            return t1.Hours > t2.Hours || (t1.Hours == t2.Hours && t1.Minutes > t2.Minutes);
        }
        public override string ToString()
        {
            return Hours.ToString() + ":" + Minutes.ToString("D2");
        }
    }
}
