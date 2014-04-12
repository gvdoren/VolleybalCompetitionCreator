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
        public ConstraintZaal(Sporthal sporthal,Club club)
        {
            name = "Sporthal niet beschikbaar";
            this.sporthal = sporthal;
            this.club = club;
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
                                    match.visitorTeam.name != "---")
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
                            if (match1.homeTeam == match2.visitorTeam && 
                                match1.visitorTeam == match2.homeTeam &&
                                match1.homeTeam.name != "---" &&
                                match1.visitorTeam.name != "---")
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
            List<Team> listTeams = new List<Team>(team1.club.teams);
            if (club.groupingWithClub != null) 
                listTeams.AddRange(club.groupingWithClub.teams);
            conflict = 0;
            conflictMatches.Clear();
            if (poule != null && team1.poule != poule) return;
            Poule poule1 = team1.poule;
            if (poule1 != null)
            {
                if (poule1.serie.constraintsHold)
                {
                    List<Match> team1Matches = team1.poule.matches.FindAll(m => m.homeTeam == team1);
                    team1Matches.Sort(delegate(Match m1, Match m2) { return m1.datetime.CompareTo(m2.datetime); });
                    foreach (Team team2 in listTeams)
                    {
                    
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
                                            if (m1.visitorTeam.name != "---" && m2.visitorTeam.name != "---")
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
    class SporthalSharedOnSameDay : Constraint
    {
        Sporthal sporthal;
        public SporthalSharedOnSameDay(Sporthal sporthal)
        {
            this.sporthal = sporthal;
            name = "Sporthal gedeeld door twee clubs op dezelfde dag";
            VisitorAlso = false;
        }
        public override void Evaluate(Klvv klvv, Poule poule)
        {
            // TODO: skip if poule is not related to club.
            conflict = 0;
            conflictMatches.Clear();
            if (sporthal.clubs.Count == 2)
            {
                if ((sporthal.clubs[0].name == "DV Hasselt" || sporthal.clubs[1].name == "DV Hasselt") &&
                    (sporthal.clubs[0].name == "VTI Blok Hasselt" || sporthal.clubs[1].name == "VTI Blok Hasselt"))
                {
                    Dictionary<Club, SortedList<DateTime, List<Match>>> CountPerWeekend = new Dictionary<Club, SortedList<DateTime, List<Match>>>();
                    Dictionary<Club, List<List<Match>>> sortedCounts = new Dictionary<Club, List<List<Match>>>();
                    foreach (Club cl in sporthal.clubs)
                    {
                        CountPerWeekend[cl] = new SortedList<DateTime, List<Match>>();
                        foreach (Team team in cl.teams)
                        {
                            if (team.poule != null && team.poule.serie.constraintsHold)
                            {
                                Poule po = team.poule;
                                foreach (Match match in po.matches)
                                {
                                    if (match.homeTeam.sporthal == sporthal)
                                    {
                                        DateTime we = match.Weekend.Saturday;
                                        if (CountPerWeekend[cl].ContainsKey(we) == false) CountPerWeekend[cl].Add(we, new List<Match>());
                                        if (match.homeTeam.club == cl) CountPerWeekend[cl][we].Add(match);
                                    }
                                }
                            }
                        }
                        sortedCounts[cl] = CountPerWeekend[cl].Values.ToList();
                        sortedCounts[cl].Sort(delegate(List<Match> l1, List<Match> l2)
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
                            return l2count.CompareTo(l1count);
                        });
                    }
                    // verdelen van de weekenden
                    List<DateTime> usedDates = new List<DateTime>();
                    List<KeyValuePair<DateTime, Club>> clubweekends = new List<KeyValuePair<DateTime, Club>>();
                    bool cont = true;
                    do
                    {
                        cont = false;
                        foreach (Club cl1 in sporthal.clubs)
                        {
                            foreach (List<Match> l1 in sortedCounts[cl1])
                            {
                                if (usedDates.Contains(l1[0].Weekend.Saturday) == false)
                                {
                                    clubweekends.Add(new KeyValuePair<DateTime, Club>(l1[0].Weekend.Saturday, cl1));
                                    usedDates.Add(l1[0].Weekend.Saturday);
                                    cont = true;
                                    break;
                                }
                            }
                        }
                    } while (cont == true);

                    foreach (Club cl in sporthal.clubs)
                    {
                        foreach (Team team in cl.teams)
                        {
                            if (team.poule != null && team.poule.serie.constraintsHold)
                            {
                                Poule po = team.poule;
                                foreach (Match match in po.matches)
                                {
                                    if (match.homeTeam.club == cl)
                                    {
                                        if (match.homeTeam.sporthal == sporthal && clubweekends.Contains(new KeyValuePair<DateTime, Club>(match.Weekend.Saturday, cl)) == false)
                                        {
                                            AddConflictMatch(match);
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

}
