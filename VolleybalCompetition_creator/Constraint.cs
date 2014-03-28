using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeifenLuo.WinFormsUI.Docking;

namespace VolleybalCompetition_creator
{
    public enum TeamGroups {GroupA, GroupB, NoGroup};

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
        public Sporthal sporthal;
        public ConstraintZaal(Sporthal sporthal)
        {
            name = "Sporthal niet beschikbaar";
            this.sporthal = sporthal;
            club = sporthal.club;
            VisitorAlso = false;
            cost = 1;
        }
        public override void Evaluate(Klvv klvv, Poule p)
        {
            conflict = 0;
            conflictMatches.Clear();
            bool optimize = false;
            foreach (Sporthal sporthal in club.sporthalls)
            {
                if (sporthal.NotAvailable.Count > 0) optimize = true;
            }
            if (optimize)
            {
                foreach (Team team in club.teams)
                {
                    Poule poule = team.poule;
                    if (poule.serie.constraintsHold)
                    {
                        if (p == null || poule == p)
                        {
                            foreach (Match match in poule.matches)
                            {
                                DateTime dt = match.datetime.Date;
                                if (match.homeTeam.sporthal != null && match.homeTeam.sporthal == sporthal && match.homeTeam.sporthal.NotAvailable.Contains(dt) == true)
                                {
                                    this.AddConflictMatch(match);
                                }
                            }
                        }
                    }
                }
            }
        }
        public override string[] GetTextDescription()
        {
            List<string> result = new List<string>();
            result.Add(sporthal.name + "is niet beschikbaar op: ");
            foreach (DateTime dt in sporthal.NotAvailable)
            {
                result.Add(string.Format("{0} - {1}",dt.DayOfWeek, dt.Date));
            }
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
            conflict = 0;
            conflictMatches.Clear();
            if (poule.serie.constraintsHold)
            {
                if (p == null || p == poule)
                {
                    for (int m1 = 0; m1 < poule.matches.Count; m1++)
                    {
                        for (int m2 = m1 + 1; m2 < poule.matches.Count; m2++)
                        {
                            Match match1 = poule.matches[m1];
                            Match match2 = poule.matches[m2];
                            if (match1.homeTeam == match2.visitorTeam && match1.visitorTeam == match2.homeTeam)
                            {
                                if ((match2.datetime - match1.datetime).TotalDays < poule.teams.Count*4)
                                {
                                    AddConflictMatch(match1, match2);
                                }
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
            name = "Teams in verschillende groupen spelen op dezelfde dag";
        }
        public Team team1;
        public override void Evaluate(Klvv klvv, Poule poule)
        {
            conflict = 0;
            conflictMatches.Clear();
            if (poule != null && team1.poule != poule) return;
            Poule poule1 = team1.poule;
            if (poule1.serie.constraintsHold)
            {
                foreach (Team team2 in team1.club.teams)
                {
                    if (team2.poule.serie.constraintsHold)
                    {
                        if ((team1.group == TeamGroups.GroupA && team2.group == TeamGroups.GroupB) ||
                           (team1.group == TeamGroups.GroupB && team2.group == TeamGroups.GroupA)
                            )
                        {
                            Poule poule2 = team2.poule;
                            int i1 = 0;
                            int i2 = 0;
                            while (i1 < poule1.matches.Count && i2 < poule2.matches.Count)
                            {
                                Match m1 = poule1.matches[i1];
                                Match m2 = poule2.matches[i2];
                                if (m1.datetime.Date < m2.datetime.Date)
                                {
                                    i1++;
                                }
                                else if (m2.datetime.Date < m1.datetime.Date)
                                {
                                    i2++;
                                }
                                else
                                {
                                    if (m1.homeTeam == team1 && m2.homeTeam == team2)
                                    {
                                        this.AddConflictMatch(m1, m2);
                                    }
                                    i1++;
                                    i2++;
                                }
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
    class ConstraintNotAllInHomeWeekend : Constraint
    {
        public ConstraintNotAllInHomeWeekend(Club club)
        {
            this.club = club;
            name = "Niet iedereen in zelfde thuis-weekend";
            VisitorAlso = false;
        }
        public override void Evaluate(Klvv klvv, Poule poule)
        {
            // TODO: skip if poule is not related to club.
            conflict = 0;
            conflictMatches.Clear();
            bool A = false;
            bool B = false;
            TeamGroups targetGroup = TeamGroups.NoGroup;
            bool optimize = (poule == null);
            foreach (Team team in club.teams)
            {
                if (team.group == TeamGroups.GroupA) A = true;
                if (team.group == TeamGroups.GroupB) B = true;
                if (team.poule == poule) optimize = true;
            }
            if (optimize)
            {
                if (A == true && B == false)
                {
                    targetGroup = TeamGroups.GroupA;
                }
                if (A == false && B == true)
                {
                    targetGroup = TeamGroups.GroupB;
                }
                if (targetGroup != TeamGroups.NoGroup)
                {
                    SortedList<DateTime, List<Match>> CountPerWeekend = new SortedList<DateTime, List<Match>>();
                    foreach (Team team in club.teams)
                    {
                        if (team.poule.serie.constraintsHold)
                        {
                            if (team.group == targetGroup)
                            {
                                Poule po = team.poule;
                                foreach (Match match in po.matches)
                                {
                                    DateTime we = match.Weekend.Saturday;
                                    if (CountPerWeekend.ContainsKey(we) == false) CountPerWeekend.Add(we, new List<Match>());
                                    if (match.homeTeam.club == club) CountPerWeekend[we].Add(match);
                                }
                            }
                        }
                    }
                    List<List<Match>> sortedCounts = CountPerWeekend.Values.ToList();
                    sortedCounts.Sort(delegate(List<Match> l1, List<Match> l2) 
                    {
                        // ensure that the series that cannot be optimized weight more in selecting the weekends
                        int l1count = l1.Count;
                        foreach (Match match in l1)
                        {
                            if (match.poule.serie.optimizable == false) l1count += 10;
                        }
                        int l2count = l2.Count;
                        foreach (Match match in l2)
                        {
                            if (match.poule.serie.optimizable == false) l2count += 10;
                        }
                        return l1count.CompareTo(l2count); 
                    });
                    for (int i = 0; i < sortedCounts.Count / 2; i++)
                    {
                        foreach (Match match in sortedCounts[i])
                        {
                            this.AddConflictMatch(match);
                        }
                        if (sortedCounts[i].Count > 0) this.conflict++;
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
