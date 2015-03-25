using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CompetitionCreator
{
    public class Match: ConstraintAdmin
    {
        public int weekIndex = -1;
        private int weekIndexIndividual = -1;
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
                // correct for individual corrections
                int index = weekIndex;
                if (weekIndexIndividual >= 0) index = weekIndexIndividual;

                if (index < poule.weekendsFirst.Count) return poule.weekendsFirst[index];
                else return poule.weekendsSecond[index - poule.weekendsFirst.Count];
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
            this.weekIndexIndividual = match.weekIndexIndividual;
            this.homeTeamIndex = match.homeTeamIndex;
            this.visitorTeamIndex = match.visitorTeamIndex;
            this.serie = match.serie;
            this.poule = match.poule;
            this.conflict = match.conflict;
            this.conflict_cost = match.conflict_cost;
            this.time = match.time;
        }
        public void OptimizeIndividual(Model model)
        {
            if (conflict_cost == 0 && weekIndexIndividual < 0) return;
            if (weekIndexIndividual>=0)
            { // try to get it back in the standard weekend 
                int temp = weekIndexIndividual;
                weekIndexIndividual = -1;
                if (poule.SnapShotIfImproved(model) == false)
                {
                    weekIndexIndividual = temp;
                }
            }
            List<Weekend> weekends = null;
            int extraIndex = 0;
            if (weekIndex < poule.weekendsFirst.Count)
            {
                extraIndex = 0;
                weekends = poule.weekendsFirst;
            } else 
            {
                extraIndex = poule.weekendsFirst.Count;
                weekends = poule.weekendsSecond;
            }
            List<Weekend> weekendsCopy = new List<Weekend>(weekends);
            // Find an alternative weekend
            foreach(Match m in poule.matches)
            {
                if (m.RealMatch())
                {
                    if (m.homeTeamIndex == homeTeamIndex || m.visitorTeamIndex == homeTeamIndex ||
                       m.homeTeamIndex == visitorTeamIndex || m.visitorTeamIndex == visitorTeamIndex)
                    {
                        weekendsCopy.Remove(m.Weekend);
                    }
                }
            }
            bool improved = false;
            foreach (Weekend we in weekendsCopy)
            {
                int temp1 = weekIndexIndividual;
                weekIndexIndividual = extraIndex + weekends.FindIndex(w => w.Saturday == we.Saturday);
                if (poule.SnapShotIfImproved(model, false) == false)
                {
                    weekIndexIndividual = temp1;
                }
                else
                {
                    weekIndexIndividual = weekIndexIndividual;
                }
            }
        }
    }
}
