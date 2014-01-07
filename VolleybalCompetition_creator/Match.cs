using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VolleybalCompetition_creator
{
    public class Match
    {
        public DateTime datetime { get; set; }
        public Team homeTeam { get { if (homeTeamIndex < poule.teams.Count) return poule.teams[homeTeamIndex]; else return null; } }
        public Team visitorTeam { get { if (visitorTeamIndex < poule.teams.Count) return poule.teams[visitorTeamIndex]; else return null; } }
        public int homeTeamIndex;
        public int visitorTeamIndex;
        Poule poule;
        Serie serie;
        public Match(DateTime datetime, Team homeTeam, Team visitorTeam, Serie serie, Poule poule)
        {
            this.datetime = datetime;
            this.homeTeamIndex = poule.teams.FindIndex(t => t == homeTeam);
            this.visitorTeamIndex = poule.teams.FindIndex(t => t == visitorTeam);
            this.serie = serie;
            this.poule = poule;
        }
    }
}
