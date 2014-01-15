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
        public int cost = 1;
        public int conflict = 0;
        public abstract string[] GetTextDescription();
        public abstract void Evaluate(Klvv klvv, Poule poule);
        public abstract bool IsMatchRelated(Match match);
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
        public List<Match> conflictMatches = new List<Match>();
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
                                conflict += cost;
                                match.AddConflict(this);
                                match.poule.AddConflict(this);
                                match.serie.AddConflict(this);
                                club.AddConflict(this);
                                match.homeTeam.AddConflict(this);
                                conflictMatches.Add(match);
                            }
                            if (match.Day == DayOfWeek.Sunday && (match.Time < Zonvroeg || match.Time > Zonlaat))
                            {
                                conflict += cost;
                                match.AddConflict(this);
                                match.poule.AddConflict(this);
                                match.serie.AddConflict(this);
                                club.AddConflict(this);
                                match.homeTeam.AddConflict(this);
                                conflictMatches.Add(match);
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
            if(conflict>0)
            {
                result.Add("Wedstrijden die hier niet aan voldoen:");
                foreach(Match match in conflictMatches)
                {
                    result.Add(match.datetime + "  " + match.serie.name + match.poule.name + " : " + match.homeTeam.name + " - " + match.visitorTeam.name);
                }
            } else {
                result.Add("Alle wedstrijden vallen binnen de tijden");
            }
            return result.ToArray();
        }
        public override bool IsMatchRelated(Match match)
        {
            return conflictMatches.Contains(match);
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
        public List<Match> conflictMatches = new List<Match>();
        public override void Evaluate(Klvv klvv, Poule poule)
        {
            if (poule != null && team1.poule != poule && team2.poule != poule) return;

            conflict = 0;
            conflictMatches.Clear();
            Poule poule1 = team1.poule;
            Poule poule2 = team2.poule;
            int i1 = 0;
            int i2 = 0;
            while(i1<poule1.matches.Count && i2<poule2.matches.Count)
            {
                Match m1 = poule1.matches[i1];
                Match m2 = poule2.matches[i2];
                if(team1.IsMatch(m1) == false)
                {
                    i1++;   
                } else if(team2.IsMatch(m2) == false)
                {
                    i2++;
                } else if(Overlap(m1, m2))
                {
                    conflictMatches.Add(m1);
                    conflictMatches.Add(m2);
                    i1++;
                    i2++;
                } else if(m1.datetime<m2.datetime) i1++; else i2++;
            }
            foreach (Match match in conflictMatches)
            {
                match.AddConflict(this);
                team1.AddConflict(this);
                team1.poule.AddConflict(this);
                team2.AddConflict(this);
                team2.poule.AddConflict(this);
                club.AddConflict(this);
            }
            conflict = conflictMatches.Count * cost;
        }
        private bool Overlap(Match m1, Match m2)
        {
            TimeSpan delta = m1.datetime - m2.datetime;
            int min = (int) delta.TotalMinutes;
            if (m1.homeTeam.club == m2.homeTeam.club) return Math.Abs(min) < 90;
            else return Math.Abs(min) < (90+90); // extra tijd voor verplaatsing
        }
        public override string[] GetTextDescription()
        {
            List<string> result = new List<string>();
            if (conflict > 0)
            {
                result.Add("Wedstrijden die hier niet aan voldoen:");
                foreach (Match match in conflictMatches)
                {
                    result.Add(match.datetime + "  " + match.serie.name + match.poule.name + " : " + match.homeTeam.name + " - " + match.visitorTeam.name);
                }
            }
            else
            {
                result.Add("Geen conflicten");
            }
            return result.ToArray();
        }
        public override bool IsMatchRelated(Match match)
        {
            return conflictMatches.Contains(match);
        }
    }

}
