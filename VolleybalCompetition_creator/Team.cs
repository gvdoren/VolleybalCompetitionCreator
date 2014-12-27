using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VolleybalCompetition_creator
{
    public class Team: ConstraintAdmin
    {
        public enum WeekendRestrictionEnum { Even, Odd, All };
        public WeekendRestrictionEnum EvenOdd = WeekendRestrictionEnum.All;
        public TeamGroups group { get; set; }
        public int Id { get; set; }
        public string name { get; set; }
        public Club club { get; set; }
        public string Ranking { get; set;  }
        public SporthallClub sporthal { get; set; }
        public Poule poule = null;
        public Serie serie = null;
        public string seriePouleName { get { return serie.name + ((poule!= null)?poule.name:"-"); } }
        public Time defaultTime;
        public bool Optimizable { get { return FixedSchema == false; } }
        public int FixedSchemaNumber = 0;
        public bool FixedSchema { get { return FixedSchemaNumber > 0; } }
        public string email;
        public string NotAtSameTimeId
        {
            get
            {
                if (NotAtSameTime != null) return NotAtSameTime.serieTeamName;
                else return "";
            }
            set
            {
                int id;
                bool success = int.TryParse(value, out id);
                if (success)
                {
                    NotAtSameTime = club.teams.Find(t => t.Id == id);
                }
                else
                {
                    NotAtSameTime = null;
                }
            }
        }
        public override void AddConflict(Constraint constraint)
        {
            // only for real teams.
            if(RealTeam()) base.AddConflict(constraint);
        }
        public Team NotAtSameTime = null;
        public string serieTeamName { get { return serie.name + " - " + name; } }
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
        public Team(int Id, string name, Poule poule, Serie serie, Club club) 
        {
            //this.NotAtSameTime = this;
            this.Id = Id;
            this.name = name;
            if (poule != null) poule.AddTeam(this);
            else this.poule = null;
            this.serie = serie;
            this.sporthal = null;
            this.group = TeamGroups.NoGroup;
            club.AddTeam(this);
        }
        public int percentage
        {
            get
            {
                if (poule == null) return 0;
                int maxConflicts = ((poule.teams.Count - 1) / 3);
                return (conflict * 100) / maxConflicts;
            }
        }
        public bool IsMatch(Match match)
        {
            return match.homeTeam == this || match.visitorTeam == this;
        }
        public static Team CreateNullTeam(Poule poule, Serie serie)
        {
            Team team = new Team(0,"----",poule,serie, Club.CreateNullClub());
            team.defaultDay = DayOfWeek.Saturday;
            team.defaultTime = new Time(0,0);
            
            return team;
        }
        public bool RealTeam()
        {
            return name != "----";
        }
        public void RemoveTeam(Klvv klvv)
        {
            if (club != null)  club.RemoveTeam(this);
            if (poule != null) poule.RemoveTeam(this);
            if (serie != null) serie.RemoveTeam(this);
            klvv.RemoveTeam(this);
        }
        public List<DateTime> plannedMatches = new List<DateTime>();
       
    }

}
