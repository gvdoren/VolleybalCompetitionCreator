using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VolleybalCompetition_creator
{
    public class Team
    {
        public string name {get;set;}
        public Club club { get; set; }
        public Poule poule = null;
        public string seriePouleName { get { return poule.serie.name + poule.name; } }
        public Time defaultTime;
        public string defaultDay;
        public Team(string name, Poule poule) 
        { 
            this.name = name;
            this.poule = poule;
        }
    }
}
