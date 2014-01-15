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
                DateTime date= SaturdayInWeek(Year, Week);
                date = date.AddDays(homeTeam.defaultDay == DayOfWeek.Saturday ? 0 : 1);
                date = date.AddHours(homeTeam.defaultTime.Hours);
                date = date.AddMinutes(homeTeam.defaultTime.Minutes);
                return date;
            }
        }
        public int Year { get; set; }
        public int Week { get; set; }
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
        public int homeTeamIndex;
        public int visitorTeamIndex;
        public Poule poule;
        public Serie serie;
        public Match(DateTime datetime, Team homeTeam, Team visitorTeam, Serie serie, Poule poule)
        {
            System.Globalization.CultureInfo cul = System.Globalization.CultureInfo.CurrentCulture;
            this.Week = cul.Calendar.GetWeekOfYear(
                datetime,
                System.Globalization.CalendarWeekRule.FirstFullWeek,
                DayOfWeek.Saturday);
            this.Year = datetime.Year;
            this.Time = new Time(datetime);
            this.homeTeamIndex = poule.teams.FindIndex(t => t == homeTeam);
            this.visitorTeamIndex = poule.teams.FindIndex(t => t == visitorTeam);
            this.serie = serie;
            this.poule = poule;
        }
        static DateTime SaturdayInWeek(int year, int weekNum)
        {
            Debug.Assert(weekNum >= 1);
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Saturday - jan1.DayOfWeek;
            if (daysOffset < 0) daysOffset += 7;
            DateTime firstSaturday = jan1.AddDays(daysOffset);
            Debug.Assert(firstSaturday.DayOfWeek == DayOfWeek.Saturday);
            Debug.Assert(firstSaturday.Year == year);
            DateTime result = firstSaturday.AddDays((weekNum-1) * 7);

            return result;
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
                    DateTime calculatedDate = SaturdayInWeek(Year, Weeknr);
                    if (calculatedDate.Year != Year ||
                        calculatedDate.DayOfYear != day.DayOfYear)
                    {
                        Console.WriteLine("Failed for date: {0}", day.ToString());
                    }
                }
            }
        }
    }
}
