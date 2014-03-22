using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeifenLuo.WinFormsUI.Docking;

namespace VolleybalCompetition_creator
{
    public class ConstraintAdmin
    {
        public List<Constraint> conflictConstraints = new List<Constraint>();
        public int conflict { get; set; }
        public void ClearConflicts()
        {
            conflictConstraints.Clear();
            conflict = 0;
        }
        public void AddConflict(Constraint constraint)
        {
            if (conflictConstraints.Contains(constraint) == false)
            {
                conflictConstraints.Add(constraint);
            }
            conflict += constraint.cost;
        }
    }

    public abstract class Constraint
    {
        List<Club> clubs = new List<Club>();
        List<Poule> poules = new List<Poule>();
        List<Team> teams = new List<Team>();
        public bool VisitorAlso = true;
        public Club club { get; set; }
        public string name { get; set; }
        public int cost = 1;
        public int conflict = 0;
        public abstract string[] GetTextDescription();
        public abstract void Evaluate(Klvv klvv, Poule poule);
        public virtual string Title { get { return name + " - " + club.name; ;} }
        public List<Match> conflictMatches = new List<Match>();
        protected void AddConflictMatch(params Match[] matches) 
        {
            for ( int i = 0 ; i < matches.Length ; i++ )
            {
                conflictMatches.Add(matches[i]);
                conflict += cost;
                matches[i].AddConflict(this);
                matches[i].Weekend.AddConflict(this);
                matches[i].poule.AddConflict(this);
                matches[i].poule.serie.AddConflict(this);
                matches[i].homeTeam.AddConflict(this);
                if (VisitorAlso) matches[i].visitorTeam.AddConflict(this);
                matches[i].homeTeam.club.AddConflict(this);
                if (matches[i].homeTeam.club != matches[i].visitorTeam.club && VisitorAlso) matches[i].visitorTeam.club.AddConflict(this);
            }
        }
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
        public override void Evaluate(Klvv klvv, Poule p)
        {
            conflict = 0;
            conflictMatches.Clear();
            foreach (Poule poule in klvv.poules)
            {
                if (p == null || poule == p)
                {
                    foreach (Match match in poule.matches)
                    {
                        if (match.homeTeam.club == club)
                        {
                            if (match.Day == DayOfWeek.Saturday && (match.Time < Zatvroeg || match.Time > Zatlaat))
                            {
                                this.AddConflictMatch(match);
                            }
                            if (match.Day == DayOfWeek.Sunday && (match.Time < Zonvroeg || match.Time > Zonlaat))
                            {
                                this.AddConflictMatch(match);
                            }
                        }
                    }
                }
            }
        }
        public override string[] GetTextDescription()
        {
            List<string> result = new List<string>();
            if (Zatvroeg < Zatlaat) result.Add("Zaterdag: " + Zatvroeg + " - " + Zatlaat);
            if (Zonvroeg < Zonlaat) result.Add("Zondag:   " + Zonvroeg + " - " + Zonlaat);
            return result.ToArray();
        }
    }
    class ConstraintNotAtTheSameTime : Constraint
    {
        public ConstraintNotAtTheSameTime()
        {
            name = "Teams spelen tegelijk";
        }
        public Team team1;
        public Team team2;
        public override void Evaluate(Klvv klvv, Poule poule)
        {
            if (poule != null && team1.poule != poule && team2.poule != poule) return;

            conflict = 0;
            conflictMatches.Clear();
            Poule poule1 = team1.poule;
            Poule poule2 = team2.poule;
            int i1 = 0;
            int i2 = 0;
            while (i1 < poule1.matches.Count && i2 < poule2.matches.Count)
            {
                Match m1 = poule1.matches[i1];
                Match m2 = poule2.matches[i2];
                if (team1.IsMatch(m1) == false)
                {
                    i1++;
                }
                else if (team2.IsMatch(m2) == false)
                {
                    i2++;
                }
                else if (Overlap(m1, m2))
                {
                    this.AddConflictMatch(m1,m2);
                    i1++;
                    i2++;
                }
                else if (m1.datetime < m2.datetime) i1++; else i2++;
            }
        }
        private bool Overlap(Match m1, Match m2)
        {
            TimeSpan delta = m1.datetime - m2.datetime;
            int min = (int)delta.TotalMinutes;
            if (m1.homeTeam.club == m2.homeTeam.club) return Math.Abs(min) < 90;
            else return Math.Abs(min) < (90 + 90); // extra tijd voor verplaatsing
        }
        public override string[] GetTextDescription()
        {
            List<string> result = new List<string>();
            return result.ToArray();
        }
    }
    class ConstraintSchemaTooClose : Constraint
    {
        Poule poule;
        public override string Title
        {
            get
            {
                return name +" - "+ poule.name+poule.serie.name;
            }
        }
        public ConstraintSchemaTooClose(Poule poule)
        {
            name = "Heen en Terug te dicht bij elkaar";
            this.poule = poule;
        }
        public override void Evaluate(Klvv klvv, Poule p)
        {
            if (p == null || p == poule)
            {
                conflict = 0;
                conflictMatches.Clear();
                for (int m1 = 0; m1 < poule.matches.Count; m1++)
                {
                    for (int m2 = m1 + 1; m2 < poule.matches.Count; m2++)
                    {
                        Match match1 = poule.matches[m1];
                        Match match2 = poule.matches[m2];
                        if (match1.homeTeam == match2.visitorTeam && match1.visitorTeam == match2.homeTeam)
                        {
                            if ((match2.datetime - match1.datetime).TotalDays < 10)
                            {
                                AddConflictMatch(match1, match2);
                            }
                        }
                    }
                }
            }
        }
        public override string[] GetTextDescription()
        {
            List<string> result = new List<string>();
            return result.ToArray();
        }
    }
    class ConstraintNotAtWeekendHome : Constraint
    {
        public ConstraintNotAtWeekendHome(Team t)
        {
            team1 = t;
            club = team1.club;
            VisitorAlso = false;
            name = "Teams spelen in hetzelfde weekend";
        }
        public Team team1;
        public override void Evaluate(Klvv klvv, Poule poule)
        {
            if (poule != null && team1.poule != poule) return;
            conflict = 0;
            conflictMatches.Clear();
            if (team1.club.ConstraintNotAtTheSameTime == false) return;
            Poule poule1 = team1.poule;
            foreach (Team team2 in team1.NotAtSameWeekend)
            {
                Poule poule2 = team2.poule;
                int i1 = 0;
                int i2 = 0;
                while (i1 < poule1.matches.Count && i2 < poule2.matches.Count)
                {
                    Match m1 = poule1.matches[i1];
                    Match m2 = poule2.matches[i2];
                    if (m1.Weekend<m2.Weekend)
                    {
                        i1++;
                    }
                    else if (m2.Weekend<m1.Weekend)
                    {
                        i2++;
                    }
                    else 
                    {
                        if(m1.homeTeam == team1 && m2.homeTeam == team2)
                        {
                            this.AddConflictMatch(m1, m2);
                        }
                        i1++;
                        i2++;
                    }
                }
            }
        }
        public override string[] GetTextDescription()
        {
            List<string> result = new List<string>();
            return result.ToArray();
        }
    }

}
