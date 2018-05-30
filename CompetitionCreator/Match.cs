using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CompetitionCreator
{
    public class Match : ConstraintAdmin
    {
        public string ConflictString
        {
            get
            {
                string str = "";
                foreach(Constraint con in conflictConstraints)
                {
                    str += con.Title+" ";
                }
                return str;
            }
        }
        public int weekIndex = -1;
        public DateTime datetime
        {
            get
            {
                DateTime date = Week.PlayTime(homeTeam.defaultDay);
                date = date.AddHours(homeTeam.defaultTime.Hours);
                date = date.AddMinutes(homeTeam.defaultTime.Minutes);
                return date;
            }
        }
        public bool Overlapp(Match m)
        {
            if (RealMatch() && m.RealMatch())
            {
                double delta = 1.99; // normale lengte wedstrijd
                if (homeTeam.club != m.homeTeam.club) delta += 1.5; // extra reistijd
                DateTime st1 = datetime;
                DateTime en1 = st1.AddHours(delta);
                st1 = st1.AddHours(-serie.extraTimeBefore); // reserve wedstrijd
                DateTime st2 = m.datetime;
                DateTime en2 = st2.AddHours(delta);
                st2 = st2.AddHours(-m.serie.extraTimeBefore); // reserve wedstrijd
                if (st1 <= en2 && en1 >= st2)
                {
                    return true;
                }
            }
            return false;
        }
        private DayOfWeek Day { get { return datetime.DayOfWeek; } }
        public string DayString
        {
            get
            {
                switch (Day)
                {
                    case DayOfWeek.Monday:
                        return "Mon";
                    case DayOfWeek.Tuesday:
                        return "Tue";
                    case DayOfWeek.Wednesday:
                        return "Wed";
                    case DayOfWeek.Thursday:
                        return "Thu";
                    case DayOfWeek.Friday:
                        return "Fri";
                    case DayOfWeek.Saturday:
                        return "Sat";
                    case DayOfWeek.Sunday:
                        return "Sun";
                    default:
                        return "--";
                }
            }
        }

        private Time time = null;
        public bool IsTimeOverruled()
        {
            return time != null;
        }
        public Time Time
        {
            get
            {
                if (time != null)
                    return time;
                else
                    return homeTeam.defaultTime;
            }
            set
            {
                if (value == homeTeam.defaultTime)
                    time = null;
                else
                    time = value;
            }
        }
        public Team homeTeam { get { return poule.teams[homeTeamIndex]; } }
        public Team visitorTeam { get { return poule.teams[visitorTeamIndex]; } }
        public MatchWeek Week
        {
            get
            {
                // correct for individual corrections
                int index = weekIndex;
                return poule.weeks[index];
            }
        }

        public int homeTeamIndex;
        public int visitorTeamIndex;
        public UInt32 homeTeamMask { get { const UInt32 one = 1; return one << homeTeamIndex; } }
        public UInt32 visitorTeamMask { get { const UInt32 one = 1; return one << visitorTeamIndex; } }
        public UInt32 matchMask { get { return homeTeamMask | visitorTeamMask; } }
        public Poule poule;
        public Serie serie;
        public bool RealMatch()
        {
            return homeTeam.RealTeam() && visitorTeam.RealTeam();
        }
        public Match(int weekIndex, int homeTeam, int visitorTeam, Serie serie, Poule poule)
        {
            this.weekIndex = weekIndex;
            this.homeTeamIndex = homeTeam;
            this.visitorTeamIndex = visitorTeam;
            this.serie = serie;
            this.poule = poule;
        }
        public Match(Match match)
        {
            this.weekIndex = match.weekIndex;
            this.homeTeamIndex = match.homeTeamIndex;
            this.visitorTeamIndex = match.visitorTeamIndex;
            this.serie = match.serie;
            this.poule = match.poule;
            this.conflict = match.conflict;
            this.conflict_cost = match.conflict_cost;
            this.time = match.time;
        }
    }
}
