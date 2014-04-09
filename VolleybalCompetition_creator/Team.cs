using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VolleybalCompetition_creator
{
    public class Team: ConstraintAdmin
    {
        public TeamGroups group { get; set; }
        public int Id { get; set; }
        public string name { get; set; }
        public Club club { get; set; }
        public Sporthal sporthal { get; set; }
        public Poule poule = null;
        public Serie serie = null;
        public string seriePouleName { get { return poule.serie.name + poule.name; } }
        public Time defaultTime;
        public List<Team> NotAtSameWeekend = new List<Team>();
        public DayOfWeek defaultDay = DayOfWeek.Monday; // initial value since monday is never the default
        public int Index 
        { 
            get 
            {
                if (poule == null) return 0;
                return 1+poule.teams.FindIndex(t => t == this); 
            } 
        }
        public int AvgDistance { 
            get
            {
                if (poule != null) return poule.CalculateDistances(this);
                else return 0;
            }
        }
        public Team(int Id, string name, Poule poule, Serie serie) 
        {
            this.Id = Id;
            this.name = name;
            this.poule = poule;
            this.serie = serie;
            this.sporthal = null;
            this.group = TeamGroups.NoGroup;
        }
        public bool IsMatch(Match match)
        {
            return match.homeTeam == this || match.visitorTeam == this;
        }
        public static Team CreateNullTeam(Poule poule)
        {
            Team team = new Team(0,"---",poule,poule.serie);
            team.defaultDay = DayOfWeek.Saturday;
            team.defaultTime = new Time(0,0);
            team.club = Club.CreateNullClub();
            return team;
        }

        public List<DateTime> plannedMatches = new List<DateTime>();

    }
}
