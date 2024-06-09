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
                int c = 0;
                foreach (Team t in teams)
                {
                    if (t.poule != null)
                    {
                        c += t.poule.teams.Count - 1;
                    }
                }
                if (c == 0) return 0;
                return (conflict * 100) / c;

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
            public List<MatchWeek> AWeeks = new List<MatchWeek>();
            public List<MatchWeek> BWeeks = new List<MatchWeek>();

            struct counters
            {
                public counters(int A, int B)
                {
                    countA = A;
                    countB = B;
                    nonChangeableA = 0;
                    nonChangeableB = 0;
                    sporthalNotAvailableA = 0;
                    sporthalNotAvailableB = 0;
                }
                public counters(counters c)
                {
                    countA = c.countA;
                    countB = c.countB;
                    nonChangeableA = c.nonChangeableA;
                    nonChangeableB = c.nonChangeableB;
                    sporthalNotAvailableA = c.sporthalNotAvailableA;
                    sporthalNotAvailableB = c.sporthalNotAvailableB;
                }
                public int countA;
                public int countB;
                public int nonChangeableA;
                public int nonChangeableB;
                public int sporthalNotAvailableA;
                public int sporthalNotAvailableB;

                public int score(bool A)
                {
                    int temp = (nonChangeableA - nonChangeableB) * 10 + countA - countB - (sporthalNotAvailableA - sporthalNotAvailableB) * 10;
                    return A ? temp : -temp;
                }
            };
            public void DetermineWeeks()
            {
                AWeeks = new List<MatchWeek>();
                BWeeks = new List<MatchWeek>();
                SortedDictionary<MatchWeek, counters> weeks = new SortedDictionary<MatchWeek, counters>();
                foreach (Team team in A)
                {
                    foreach (Match match in team.poule.matches.Where(m => m.RealMatch() && m.homeTeam == team))
                    {
                        if (!weeks.ContainsKey(match.Week))
                            weeks.Add(match.Week, new counters());
                        var c = new counters(weeks[match.Week]);
                        c.countA++;
                        if (team.poule.imported)
                            c.nonChangeableA++;
                        if (team.sporthal.NotAvailable.Contains(match.datetime.Date))
                            c.sporthalNotAvailableA++;
                        weeks[match.Week] = c;
                    }
                }
                foreach (Team team in B)
                {
                    foreach (Match match in team.poule.matches.Where(m => m.RealMatch() && m.homeTeam == team))
                    {
                        if (!weeks.ContainsKey(match.Week))
                            weeks.Add(match.Week, new counters());
                        var c = new counters(weeks[match.Week]);
                        c.countB++;
                        if (team.poule.imported)
                            c.nonChangeableB++;
                        if (team.sporthal.NotAvailable.Contains(match.datetime.Date))
                            c.sporthalNotAvailableB++;
                        weeks[match.Week] = c;
                    }
                }

                int turnA = 1;
                int voorkeur = 1 + (int)Math.Round((double)weeks.Count * 0.1); // bv 20 weken (2x10) dan 4 dagen extra, 12 (6) -> 3
                if (B.Count() == 0)
                    turnA += voorkeur; // A mag eerst x keer extra kiezen

                while (weeks.Count > 0)
                {
                    int score = int.MinValue;
                    MatchWeek week = null;
                    foreach (var kvp in weeks)
                    {
                        if (kvp.Value.score(turnA>0) > score)
                        {
                            score = kvp.Value.score(turnA>0);
                            week = kvp.Key;
                        }
                    }
                    if (turnA > 0)
                    {
                        AWeeks.Add(week);
                        turnA--;
                    }
                    else
                    {
                        BWeeks.Add(week);
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
            var sporthalMatch = GroupAllSporthalls ? true : t1.sporthal == t2.sporthal;
            var timeMatch = GroupAllDay ? true : Math.Abs(t1.defaultTime.Hours - t2.defaultTime.Hours) < 2; // Als ze minder dan 2 uur uit elkaar liggen zijn ze overlappend
            return dayMatch & sporthalMatch & timeMatch;
        }


        public void PrintGroupsInfo(StreamWriter outputFile)
        {
            foreach (var ab in ABGroups)
            {
                foreach (var t in ab.A)
                {
                    outputFile.WriteLine(t.club.name + t.name + t.poule.name);
                }
                foreach (var t in ab.B)
                {
                    outputFile.WriteLine(t.club.name + t.name + t.poule.name);
                }
                foreach (var w in ab.AWeeks)
                    outputFile.WriteLine(w.ToString() + w.WeekNr());
                foreach (var w in ab.BWeeks)
                    outputFile.WriteLine(w.ToString() + w.WeekNr());
            }
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
