using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeifenLuo.WinFormsUI.Docking;

namespace VolleybalCompetition_creator
{
    public class ConstraintAdmin
    {
        List<Constraint> conflictConstraints = new List<Constraint>();
        public int conflict { get; set; }
        public void ClearConflicts()
        {
            conflictConstraints.Clear();
            conflict = 0;
        }
        public void AddConflict(Constraint constraint)
        {
            conflictConstraints.Add(constraint);
            conflict += constraint.cost;
        }
    }

    public abstract class Constraint
    {
        List<Club> clubs = new List<Club>();
        List<Poule> poules = new List<Poule>();
        List<Team> teams = new List<Team>();
        public Club club { get; set; }
        public string name { get; set; }
        public abstract void Evaluate(Klvv klvv);
        public int cost = 1;
        public int conflict = 0;
    }
    class ConstraintZaal : Constraint
    {
        public ConstraintZaal()
        {
            name = "Beschikbaarheid zaal";
        }
        public Time Zatvroeg = new Time(23, 59);
        public Time Zatlaat = new Time(0,0);
        public Time Zonvroeg = new Time(23, 59);
        public Time Zonlaat = new Time(0,0);

        public override void Evaluate(Klvv klvv)
        {
            conflict = 0;
            foreach (Poule poule in klvv.poules)
            {
                foreach (Match match in poule.matches)
                {
                    if (match.homeTeam != null && match.homeTeam.club == club)
                    {
                        if (match.Day == DayOfWeek.Saturday && (match.Time < Zatvroeg || match.Time > Zatlaat))
                        {
                            conflict += cost;
                            match.AddConflict(this);
                            match.poule.AddConflict(this);
                            match.serie.AddConflict(this);
                            club.AddConflict(this);
                            match.homeTeam.AddConflict(this);
                        }
                        if (match.Day == DayOfWeek.Sunday && (match.Time < Zonvroeg || match.Time > Zonlaat))
                        {
                            conflict += cost;
                            match.AddConflict(this);
                            match.poule.AddConflict(this);
                            match.serie.AddConflict(this);
                            club.AddConflict(this);
                            match.homeTeam.AddConflict(this);
                        }
                    }
                }
            }
        }
    }
}
