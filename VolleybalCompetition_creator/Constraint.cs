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
            conflict ++;
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
        public virtual string Title 
        { 
            get 
            {
                if (club != null) return name + " - " + club.name;
                else return name;
            } 
        }
        public List<Match> conflictMatches = new List<Match>();
        protected void AddConflictMatch(params Match[] matches) 
        {

            for (int i = 0; i < matches.Length; i++)
            {
                if (matches[i].serie.optimizable) conflict += cost;
                else conflict += 10;
                if (conflictMatches.Contains(matches[i]) == false)
                {
                    conflictMatches.Add(matches[i]);
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
        public void Sort()
        {
            conflictMatches.Sort(delegate(Match m1, Match m2) { return m1.datetime.CompareTo(m2.datetime); });
        }
    }
    class ConstraintZaal : Constraint
    {
        public Sporthal sporthal;
        public ConstraintZaal(Sporthal sporthal,Club club)
        {
            name = "Sporthal niet beschikbaar";
            this.sporthal = sporthal;
            this.club = club;
            VisitorAlso = false;
            cost = 20;
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
                    if (poule != null && poule.serie.constraintsHold)
                    {
                        if (p == null || poule == p)
                        {
                            foreach (Match match in poule.matches)
                            {
                                DateTime dt = match.datetime.Date;
                                if (match.homeTeam.sporthal != null && 
                                    match.homeTeam.sporthal == sporthal && 
                                    match.homeTeam.sporthal.NotAvailable.Contains(dt) == true &&
                                    match.RealMatch())
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
        public Poule poule;
        public override string Title
        {
            get
            {
                return name + " - " + poule.name + poule.serie.name;
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
                        Match match1 = poule.matches[m1];
                        for (int m2 = m1 + 1; m2 < poule.matches.Count; m2++)
                        {
                            Match match2 = poule.matches[m2];
                            if (match1.homeTeam == match2.visitorTeam &&
                                match1.visitorTeam == match2.homeTeam &&
                                match1.RealMatch())
                            {
                                if ((match2.datetime - match1.datetime).TotalDays < poule.teams.Count * 4)
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
    class ConstraintPouleInconsistent : Constraint
    {
        public Poule poule;
        public override string Title
        {
            get
            {
                return name + " - " + poule.name + poule.serie.name;
            }
        }
        public ConstraintPouleInconsistent(Poule poule)
        {
            name = "Internal test - poule consistency";
            this.poule = poule;
        }
        public override void Evaluate(Klvv klvv, Poule p)
        {
            conflict = 0;
            conflictMatches.Clear();
            if (poule.serie.constraintsHold && poule.serie.name != "Nationaal")
            {
                if (p == null || p == poule)
                {
                    SortedList<int, int> HomeMatches = new SortedList<int, int>();
                    SortedList<int, int> VisitorMatches = new SortedList<int, int>();
                    foreach (Team t in poule.teams)
                    {
                        HomeMatches.Add(t.Id, 0);
                        VisitorMatches.Add(t.Id, 0);
                    }
                    for (int m1 = 0; m1 < poule.matches.Count; m1++)
                    {
                        Match match1 = poule.matches[m1];
                        if (match1.RealMatch())
                        {
                            HomeMatches[match1.homeTeam.Id]++;
                            VisitorMatches[match1.visitorTeam.Id]++;
                        }
                    }
                    foreach (Team t in poule.teams)
                    {
                        if (HomeMatches[t.Id] != poule.teams.Count - 1)
                        {
                            foreach (Match match in poule.matches)
                            {
                                if(match.homeTeam == t) AddConflictMatch(match);
                            }
                        } 
                        if (VisitorMatches[t.Id] != poule.teams.Count - 1)
                        {
                            foreach (Match match in poule.matches)
                            {
                                if (match.visitorTeam == t) AddConflictMatch(match);
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
        public ConstraintNotAtWeekendHome(Club cl)
        {
            club = cl;
            VisitorAlso = false;
            name = "Teams in verschillende groupen spelen op dezelfde dag";
        }
        public override void Evaluate(Klvv klvv, Poule poule)
        {
            List<Team> listTeams = new List<Team>(club.teams);
            if (club.groupingWithClub != null) 
                listTeams.AddRange(club.groupingWithClub.teams);
            conflict = 0;
            conflictMatches.Clear();
            for(int i=0;i<listTeams.Count;i++)
            {
                Team team1 = listTeams[i];
                Poule poule1 = team1.poule;
                if (poule1 != null)
                {
                    if (poule == null || poule == poule1)
                    {
                        if (poule1.serie.constraintsHold)
                        {
                            List<Match> team1Matches = team1.poule.matches.FindAll(m => m.homeTeam == team1);
                            team1Matches.Sort(delegate(Match m1, Match m2) { return m1.datetime.CompareTo(m2.datetime); });
                            for(int j=i+1;j<listTeams.Count;j++)
                            {
                                Team team2 = listTeams[j];
                                if (team2.poule != null)
                                {
                                    if (team2.poule.serie.constraintsHold)
                                    {
                                        if ((team1.group == TeamGroups.GroupA && team2.group == TeamGroups.GroupB) ||
                                           (team1.group == TeamGroups.GroupB && team2.group == TeamGroups.GroupA)
                                            )
                                        {
                                            List<Match> team2Matches = team2.poule.matches.FindAll(m => m.homeTeam == team2);
                                            team2Matches.Sort(delegate(Match m1, Match m2) { return m1.datetime.CompareTo(m2.datetime); });

                                            Poule poule2 = team2.poule;
                                            int i1 = 0;
                                            int i2 = 0;


                                            while (i1 < team1Matches.Count && i2 < team2Matches.Count)
                                            {
                                                Match m1 = team1Matches[i1];
                                                Match m2 = team2Matches[i2];
                                                if (m1.datetime.Date < m2.datetime.Date || m1.visitorTeam.club == m1.homeTeam.club)
                                                {
                                                    i1++;
                                                }
                                                else if (m2.datetime.Date < m1.datetime.Date || m2.visitorTeam.club == m2.homeTeam.club)
                                                {
                                                    i2++;
                                                }
                                                else
                                                {
                                                    if (m1.RealMatch() && m2.RealMatch())
                                                    {
                                                        DateTime st1 = m1.datetime;
                                                        DateTime en1 = st1.AddHours(2.5);
                                                        if(m1.serie.Reserve())
                                                        {
                                                            st1 = st1.AddHours(-2);
                                                        }
                                                        DateTime st2 = m2.datetime;
                                                        DateTime en2 = st2.AddHours(2.5);
                                                        if (m2.serie.Reserve())
                                                        {
                                                            st2 = st2.AddHours(-2);
                                                        }
                                                        if (m1.homeTeam.club != m2.homeTeam.club || (st1<= en2 && en1 >= st2))
                                                        {
                                                            this.AddConflictMatch(m1, m2);
                                                        }
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
    class ConstraintNotAtWeekendHome1 : Constraint
    {
        public ConstraintNotAtWeekendHome1(Club cl)
        {
            club = cl;
            VisitorAlso = false;
            name = "Teams in verschillende groupen spelen op dezelfde dag";
        }
        public override void Evaluate(Klvv klvv, Poule poule)
        {
            List<Match> matchList = null;
            SortedList<DateTime, List<Match>> A = new SortedList<DateTime, List<Match>>();
            matchList = new List<Match>();
            bool optimize = (poule == null);
            foreach (Team team in club.teams)
            {
                if (team.poule != null)
                {
                    if (poule == team.poule) optimize = true;
                    foreach (Match match in team.poule.matches)
                    {
                        if (match.homeTeam.club == club && match.serie.constraintsHold)
                        {
                            if (matchList.Contains(match) == false) matchList.Add(match);
                            if (A.ContainsKey(match.Weekend.Saturday) == false) A.Add(match.Weekend.Saturday, new List<Match>());
                            if (match.homeTeam.group == TeamGroups.GroupA) A[match.Weekend.Saturday].Add(match);
                        }
                    }
                }
            }
            if (optimize)
            {
                conflict = 0;
                conflictMatches.Clear();
            
                List<List<Match>> sortedCounts = A.Values.ToList();
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
                sortedCounts.Reverse();
                // A-weekends
                List<DateTime> AWeekends = new List<DateTime>();
                for (int i = 0; i < sortedCounts.Count / 2; i++)
                {
                    if (sortedCounts[i].Count > 0)
                    {
                        AWeekends.Add(sortedCounts[i][0].Weekend.Saturday);
                    }
                }
                foreach (Match match in matchList)
                {
                    if (AWeekends.Contains(match.Weekend.Saturday))
                    {
                        if (match.homeTeam.group == TeamGroups.GroupB)
                        {
                            AddConflictMatch(match);
                        }
                    }
                    else
                    {
                        if (match.homeTeam.group == TeamGroups.GroupA)
                        {
                            AddConflictMatch(match);
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
            List<Team> listTeams = new List<Team>(club.teams);
            if (club.groupingWithClub != null) listTeams.AddRange(club.groupingWithClub.teams);
            
            // TODO: skip if poule is not related to club.
            conflict = 0;
            conflictMatches.Clear();
            bool A = false;
            bool B = false;
            TeamGroups targetGroup = TeamGroups.NoGroup;
            bool optimize = (poule == null);
            foreach (Team team in listTeams)
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
                    foreach (Team team in listTeams)
                    {
                        if (team.poule != null && team.poule.serie.constraintsHold)
                        {
                            if (team.group == targetGroup)
                            {
                                Poule po = team.poule;
                                foreach (Match match in po.matches)
                                {
                                    if (match.RealMatch())
                                    {
                                        DateTime we = match.Weekend.Saturday;
                                        if (CountPerWeekend.ContainsKey(we) == false) CountPerWeekend.Add(we, new List<Match>());
                                        if (match.homeTeam.club == club) CountPerWeekend[we].Add(match);
                                    }
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
