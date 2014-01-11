using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VolleybalCompetition_creator
{
    public class Poule: ConstraintAdmin
    {
        public Serie serie = null;
        public string name { get; set; }
        public List<Team> teams = new List<Team>();
        public Poule(string name) { this.name = name; }
        public List<Weekend> weekends = new List<Weekend>();
        public List<Match> matches = new List<Match>();
    }
}
