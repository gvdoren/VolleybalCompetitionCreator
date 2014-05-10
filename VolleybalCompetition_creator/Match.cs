using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace VolleybalCompetition_creator
{
    public class Match: ConstraintAdmin
    {
        public DateTime datetime 
        {
            get
            {
                DateTime date= Weekend.Saturday;
                date = date.AddDays(homeTeam.defaultDay == DayOfWeek.Saturday ? 0 : 1);
                date = date.AddHours(homeTeam.defaultTime.Hours);
                date = date.AddMinutes(homeTeam.defaultTime.Minutes);
                return date;
            }
        }
        private int tempYear;
        private int tempWeek;
        public int Year { get { return poule.weekends[weekIndex].Year; }}
        public int Week { get { return poule.weekends[weekIndex].WeekNr; }}
        public DayOfWeek Day { get { return datetime.DayOfWeek; } }
        public string DayString
        {
            get
            {
                if (Day == DayOfWeek.Saturday) return "Zat";
                if (Day == DayOfWeek.Sunday) return "Zon";
                return "--";
            }
        }
                
        private Time time;
        public Time Time { 
            get 
            {
                return homeTeam.defaultTime; 
            }
            set 
            {
                time = value;
            } 
        }
        public Team homeTeam { get { if (homeTeamIndex < poule.teams.Count) return poule.teams[homeTeamIndex]; else return Team.CreateNullTeam(poule); } }
        public Team visitorTeam { get { if (visitorTeamIndex < poule.teams.Count) return poule.teams[visitorTeamIndex]; else return Team.CreateNullTeam(poule); } }
        public Weekend Weekend { get { return poule.weekends[weekIndex]; } }
        public bool Optimizable
        {
            get
            {
                bool result = true;
                if (homeTeam != null && homeTeam.Optimizable == false) result = false;
                if (visitorTeam != null && visitorTeam.Optimizable == false) result = false;
                return result;
            }
        }

        public int homeTeamIndex;
        public int visitorTeamIndex;
        public int weekIndex = -1;
        public Poule poule;
        public Serie serie;
        public bool RealMatch()
        {
            return homeTeam.name != "---" && visitorTeam.name != "---";
        }
        public Match(DateTime datetime, Team homeTeam, Team visitorTeam, Serie serie, Poule poule)
        {
            System.Globalization.CultureInfo cul = System.Globalization.CultureInfo.CurrentCulture;
            this.tempWeek = cul.Calendar.GetWeekOfYear(
                datetime,
                System.Globalization.CalendarWeekRule.FirstFullWeek,
                DayOfWeek.Saturday);
            this.tempYear = datetime.Year;
            this.Time = new Time(datetime);
            this.homeTeamIndex = poule.teams.FindIndex(t => t == homeTeam);
            this.visitorTeamIndex = poule.teams.FindIndex(t => t == visitorTeam);
            if (visitorTeamIndex < 0)
            {
                Console.WriteLine("Negative");
            }
            this.serie = serie;
            this.poule = poule;
            poule.AddWeekend(tempYear, tempWeek);
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
            this.tempWeek = match.tempWeek;
            this.tempYear = match.tempYear;
            this.time = match.time;
        }

        
        
        public void SetWeekIndex()
        {
            if(weekIndex<0) weekIndex = poule.FindWeekendNrInSchema(tempYear, tempWeek);
        }
    }
}
