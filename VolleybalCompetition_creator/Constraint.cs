using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VolleybalCompetition_creator
{
    public class Constraint
    {
        List<Club> clubs = new List<Club>();
        List<Poule> poules = new List<Poule>();
        List<Team> teams = new List<Team>();
    }
    class ConstraintZaal : Constraint
    {
        public Club club;
        public Time Zatvroeg = new Time(23, 59);
        public Time Zatlaat = new Time(0,0);
        public Time Zonvroeg = new Time(23, 59);
        public Time Zonlaat = new Time(0,0);
    }
}
