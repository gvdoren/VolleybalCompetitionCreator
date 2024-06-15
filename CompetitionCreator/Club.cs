using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;


namespace CompetitionCreator
{
    //{"Id":"11","Name":"Avoc Achel","LogoId":"11"}
    public class Club : ConflictAdmin
    {
        public List<Team> teams = new List<Team>();
        public bool AddTeam(Team team)
        {
            if (teams.Contains(team) == false)
            {
                if (team.club != null) team.club.RemoveTeam(team);
                teams.Add(team);
                team.club = this;
                return true;
            }
            return false;

        }

        public List<Club> SharingSporthal = new List<Club>();
        public bool SharingSporthalBool { get { return SharingSporthal.Count() > 0; } }
        public List<Team> GetGroupX()
        {
            var X = teams.FindAll(t => t.group == TeamGroups.GroupX);
            foreach (var club in SharingSporthal)
            {
                X.AddRange(club.teams.FindAll(t => t.group == TeamGroups.GroupX));
            }
            return X;
        }
        public List<Team> GetGroupY()
        {
            var Y = teams.FindAll(t => t.group == TeamGroups.GroupY);
            foreach (var club in SharingSporthal)
            {
                Y.AddRange(club.teams.FindAll(t => t.group == TeamGroups.GroupY));
            }
            return Y;
        }

        public bool RemoveTeam(Team team)
        {
            if (teams.Contains(team) == true)
            {
                teams.Remove(team);
                team.club = null;
                return true;
            }
            return false;
        }
        public int percentage
        {

            get
            {
                int selectedMatches = 0;
                int conflicts = 0;
                List<Poule> poules = new List<Poule>();
                foreach (Team t in teams)
                {
                    if (t.poule != null && t.poule.evaluated && poules.Contains(t.poule) == false)
                    {
                        poules.Add(t.poule);
                        foreach (Match match in t.poule.matches)
                        {
                            if (match.RealMatch() && match.homeTeam.club == this)
                            {
                                selectedMatches++;
                                if (match.HasConflict())
                                    conflicts++;
                            }
                        }
                    }
                }
                if (selectedMatches == 0) return 0;
                return (int)Math.Round((double)conflicts * 100 / (double)selectedMatches);
            }
        }

        public int maxGroupSize()
        {
            int max = 0;
            foreach(var ab in ABGroups)
            {
                var size = ab.A.Count + ab.B.Count;
                if (size > max)
                    max = size;
            }
            return max;
        }

        public int sporthallCount()
        {
            return sporthalls.Count;
        }

        public int conflictMatches
        {
            get
            {
                List<Poule> poules = new List<Poule>();
                int c = 0;
                foreach (Team t in teams)
                {
                    if (t.poule != null && t.poule.evaluated && poules.Contains(t.poule) == false)
                    {
                        poules.Add(t.poule);
                        foreach (var m in t.poule.matches)
                        {
                            if (m.RealMatch() && m.homeTeam.club == this && m.HasConflict())
                                c++;
                        }
                    }
                }
                return c;
            }
        }

        public int EvaluatedTeamCount
        {
            get
            {
                return teams.Count(t => t.evaluated);
            }
        }
        public List<SporthallAvailability> sporthalls = new List<SporthallAvailability>();
        public Club groupingWithClub = null;
        public string FreeFormatConstraints = "";
        public int Id { get; set; }
        public string name { get; set; }
        public string Stamnumber { get; set; }
        public bool ConstraintNotAtTheSameTime = false;
        public bool GroupAllWeek = false;
        public bool GroupAllSporthalls = true;
        public bool GroupAllDay = true;
        public static Club CreateNullClub()
        {
            Club club = new Club(-2, "----", "");
            return club;
        }
        public Club(int id, string name, string stamNumber)
        {
            this.Id = id;
            this.name = name;
            this.Stamnumber = stamNumber;
        }
        public override string ToString()
        {
            return name;
        }

        public class AB
        {
            public List<Team> A = new List<Team>();
            public List<Team> B = new List<Team>();
            public bool[] AWeeks = new bool[53];
            public bool[] BWeeks = new bool[53];

            struct counters
            {
                public int countA;
                public int countB;
                public int nonChangeableA;
                public int nonChangeableB;
                public int sporthalNotAvailableA;
                public int sporthalNotAvailableB;
                public bool used;
                public int weeknr;

                public int score(bool A)
                {
                    if (!used) return int.MinValue;
                    int temp = (nonChangeableA - nonChangeableB) * 10 + countA - countB - (sporthalNotAvailableA - sporthalNotAvailableB) * 10;
                    return A ? temp : -temp;
                }
            };
            public void DetermineWeeks()
            {
                AWeeks = new bool[53];
                BWeeks = new bool[53];
                counters[] weekCounters = new counters[53];
                for (int i = 0; i < 53; i++)
                    weekCounters[i].weeknr = i;

                foreach (Team team in A)
                {
                    foreach (Match match in team.poule.matches.Where(m => m.RealMatch()))
                    {
                        weekCounters[match.Week.WeekNumber].used = true;
                        if (match.homeTeam == team)
                        {
                            weekCounters[match.Week.WeekNumber].countA++;
                            if (team.poule.imported)
                                weekCounters[match.Week.WeekNumber].nonChangeableA++;
                        }
                        if (team.sporthal.NotAvailable.Contains(match.datetime.Date))
                            weekCounters[match.Week.WeekNumber].sporthalNotAvailableA++;
                    }
                }
                foreach (Team team in B)
                {
                    foreach (Match match in team.poule.matches.Where(m => m.RealMatch()))
                    {
                        weekCounters[match.Week.WeekNumber].used = true;
                        if (match.homeTeam == team)
                        {
                            weekCounters[match.Week.WeekNumber].countB++;
                            if (team.poule.imported)
                                weekCounters[match.Week.WeekNumber].nonChangeableB++;
                        }
                        if (team.sporthal.NotAvailable.Contains(match.datetime.Date))
                            weekCounters[match.Week.WeekNumber].sporthalNotAvailableB++;
                    }
                }

                int voorkeur = 0;//  1 + (int)Math.Round((double)weeks.Count * 0.1); // bv 20 weken (2x10) dan 4 dagen extra, 12 (6) -> 3
                int turnA = 1;
                if (B.Count() == 0)
                    turnA += voorkeur; // A mag eerst x keer extra kiezen
                var weeks = weekCounters.Where(wc => wc.used).ToList();
                while (weeks.Count > 0)
                {
                    int score = int.MinValue;
                    counters week = new counters();
                    foreach (var kvp in weeks)
                    {
                        if (kvp.score(turnA > 0) > score)
                        {
                            score = kvp.score(turnA > 0);
                            week = kvp;
                        }
                    }
                    if (turnA > 0)
                    {
                        AWeeks[week.weeknr] = true;
                        turnA--;
                    }
                    else
                    {
                        BWeeks[week.weeknr] = true;
                        turnA++;
                    }
                    weeks.Remove(week);
                }
            }
        }

        public List<AB> ABGroups = new List<AB>();

        bool MatchTeamGroup(Team t1, Team t2)
        {
            var dayMatch = GroupAllWeek ? true : t1.defaultDay == t2.defaultDay;
            var sporthalMatch = GroupAllSporthalls ? true : t1.sporthal.id == t2.sporthal.id;
            var timeMatch = GroupAllDay ? true : Math.Abs(t1.defaultTime.Hours - t2.defaultTime.Hours) < 2; // Als ze minder dan 2 uur uit elkaar liggen zijn ze overlappend
            return dayMatch & sporthalMatch & timeMatch;
        }


//        public void PrintGroupsInfo(StreamWriter outputFile)
//        {
//            foreach (var ab in ABGroups)
//            {
//                foreach (var t in ab.A)
//                {
//                    outputFile.WriteLine(t.club.name + t.name + t.poule.name);
//                }
//                foreach (var t in ab.B)
//                {
//                    outputFile.WriteLine(t.club.name + t.name + t.poule.name);
//                }
//                foreach (var w in ab.AWeeks)
//                    outputFile.WriteLine(w.ToString() + w.WeekNr());
//                foreach (var w in ab.BWeeks)
//                    outputFile.WriteLine(w.ToString() + w.WeekNr());
//            }
//        }

        public int OptimizedTeamCount()
        {
            return teams.Where(t => t.serie.evaluated).Count();
        }

        public void CreateGroups(Model model)
        {
            ABGroups.Clear();
            var remainder = new List<Team>(teams.Where(t => t.evaluated && t.poule != null));
            while (remainder.Count() > 0)
            {
                var t1 = remainder.First();
                var ab = new AB();
                ab.A.Add(t1);
                ABGroups.Add(ab);
                var nextRemainder = new List<Team>();
                remainder.RemoveAt(0);
                foreach (var t2 in remainder)
                {
                    if (MatchTeamGroup(t1, t2))
                    {
                        if (t1.group == t2.group)
                            ab.A.Add(t2);
                        else
                            ab.B.Add(t2);
                    }
                    else
                        nextRemainder.Add(t2);
                }
                remainder = nextRemainder;
            }
            // Add teams from other clubs in case these should be grouped based on sharing sporthal
            foreach (var club in SharingSporthal)
            {
                foreach (var t2 in club.teams.Where(t => t.evaluated && t.poule != null))
                {
                    foreach (var ab in ABGroups)
                    {
                        var t1 = ab.A.First();
                        if (MatchTeamGroup(t1, t2))
                        {
                            if (t1.group == t2.group)
                                ab.A.Add(t2);
                            else
                                ab.B.Add(t2);
                        }
                    }
                }
            }
            foreach (var ab in ABGroups)
                ab.DetermineWeeks();
        }
    }
}
