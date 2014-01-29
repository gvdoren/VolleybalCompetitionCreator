using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VolleybalCompetition_creator
{
    public class Team: ConstraintAdmin
    {
        public string name {get;set;}
        public Club club { get; set; }
        public Poule poule = null;
        public string seriePouleName { get { return poule.serie.name + poule.name; } }
        public Time defaultTime;
        public DayOfWeek defaultDay = DayOfWeek.Monday; // initial value since monday is never the default
        public int Index { get { return 1+poule.teams.FindIndex(t => t == this); } }
        public Team(string name, Poule poule) 
        { 
            this.name = name;
            this.poule = poule;
        }
        public bool IsMatch(Match match)
        {
            return match.homeTeam == this || match.visitorTeam == this;
        }
        public static Team CreateNullTeam(Poule poule)
        {
            Team team = new Team("---",poule);
            team.defaultDay = DayOfWeek.Saturday;
            team.defaultTime = new Time(0,0);
            team.club = Club.CreateNullClub();
            return team;
        }

    }
}
