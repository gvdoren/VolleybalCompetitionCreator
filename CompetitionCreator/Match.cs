using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CompetitionCreator
{
    public class Match: ConstraintAdmin
    {
        private DateTime _datetime = new DateTime(0);
        private bool specialDate { get { return _datetime.Ticks != 0; } }
        public int weekIndex = -1;
        public DateTime datetime 
        {
            get
            {
                if (specialDate) return _datetime;
                DateTime date= Weekend.Saturday;
                date = date.AddDays(homeTeam.defaultDay == DayOfWeek.Saturday ? 0 : 1);
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
        public DayOfWeek Day { get { return datetime.DayOfWeek; } }
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
                
        private Time time;
        public Time Time { 
            get 
            {
                if (specialDate) return time;
                return homeTeam.defaultTime; 
            }
            set 
            {
                time = value;
            } 
        }
        public Team homeTeam { get { return poule.teams[homeTeamIndex]; } }
        public Team visitorTeam { get { return poule.teams[visitorTeamIndex]; } }
        public Weekend Weekend
        {
            get
            {
                return poule.weekends[weekIndex];
            }
        }

        public int homeTeamIndex;
        public int visitorTeamIndex;
        public Poule poule;
        public Serie serie;
        public bool RealMatch()
        {
            return homeTeam.RealTeam() && visitorTeam.RealTeam();
        }
        public Match(DateTime datetime, Team homeTeam, Team visitorTeam, Serie serie, Poule poule)
        {
            weekIndex = poule.AddWeekend(datetime);
            this._datetime = datetime;
            this.Time = new Time(datetime);
            this.homeTeamIndex = poule.teams.FindIndex(t => t == homeTeam);
            this.visitorTeamIndex = poule.teams.FindIndex(t => t == visitorTeam);
            if (homeTeamIndex < 0)
            {
                Console.WriteLine("Negative");
            }
            if (visitorTeamIndex < 0)
            {
                Console.WriteLine("Negative");
            }
            this.serie = serie;
            this.poule = poule;
            
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
            this._datetime = match._datetime;
        }
    }
}
