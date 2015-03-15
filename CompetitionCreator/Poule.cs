using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompetitionCreator
{
    public class Poule: ConstraintAdmin
    {
        // Temp lists for snapshot
        public bool imported = false;

        public bool optimizable { get { return serie != null && serie.optimizableNumber && imported == false; } }
        public bool optimizableWeekends { get { return serie != null && serie.optimizableWeekends && imported == false; } }
        public bool optimizableHomeVisit { get { return serie != null && serie.optimizableHomeVisit && imported == false; } }

        public bool evaluated { get { return imported == false; } }
        List<Team> resultTeams = new List<Team>();
        List<Weekend> resultWeekends = new List<Weekend>();
        List<Match> resultMatches = new List<Match>();
        public List<Constraint> relatedConstraints = new List<Constraint>();
        int minConflicts = 0;
        private int _maxTeams;
        public int maxTeams { get { return _maxTeams;}}
        public Serie serie = null;
        public string name { get; set; }
        public List<Team> teams = new List<Team>();
        public bool AddTeam(Team team, int index = -1)
        {
            if(TeamCount< maxTeams && teams.Contains(team) == false)
            {
                if(index<0) index = teams.FindIndex(t => t.RealTeam() == false);
                if (index >= 0)
                {
                    if (team.poule != null) team.poule.RemoveTeam(team);
                    teams[index] = team;
                    team.poule = this;
                    UpdateTeamCount();
                    return true;
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Inconsistency");
                }
            }
            return false;
        }
        public bool RemoveTeam(Team team)
        {
            int index = teams.FindIndex(t => t == team);
            if (index >=0 )
            {
                Team dummy = Team.CreateNullTeam(null, serie);
                dummy.poule = this;
                teams[index] = dummy;
                team.poule = null;
                UpdateTeamCount();
                return true;
            }
            return false;
        }
        public int TeamCount = 0;
        private void UpdateTeamCount()
        {
            TeamCount = 0;
            foreach (Team team in teams)
            {
                if(team.RealTeam()) TeamCount++;
            }
        }
        public Poule(string name, int maxTeams, Serie serie) 
        {
            /*while (maxTeams != 4 &&
                  maxTeams != 6 &&
                  maxTeams != 8 &&
                  maxTeams != 10 &&
                  maxTeams != 12 &&
                  maxTeams != 14) maxTeams++;
            */
            this.serie = serie;
            this._maxTeams = maxTeams;
            for (int i = 0; i < maxTeams; i++)
            {
                Team t = Team.CreateNullTeam(null,serie);
                teams.Add(t);
                t.poule = this;  // initially, this cannot be done by CreateNullTeam
            }
            this.name = name; 
        }
        public string fullName { get { return serie.name + name; ;} }
        public List<Weekend> weekends = new List<Weekend>();
        public int AddWeekend(DateTime date)
        {
            int year = 0;
            int weekNr = 0;
            Weekend.Convert(date, ref year, ref weekNr);
            int index = weekends.FindIndex(w => w.WeekNr == weekNr && w.Year == year);
            if (index < 0)
            {
                index = weekends.Count;
                weekends.Add(new Weekend(year, weekNr));
            }
            return index;
        }
        public int CalculateDistances(Team t1)
        {
            int distance = 0;
            if (t1.sporthal != null)
            {
                foreach (Team t2 in teams)
                {
                    if(t2.RealTeam() && t1.sporthal != null) distance += t1.sporthal.sporthall.Distance(t2.sporthal.sporthall);
                }
            }
            if (TeamCount > 1)
            {
                distance = (distance / (TeamCount - 1));
            }
            return distance;
        }
        
        public int FindWeekendNrInSchema(int year, int weekNr)
        {
            int index = weekends.FindIndex(w => w.WeekNr == weekNr && w.Year == year);
            return index;
        }
        public List<Match> matches = new List<Match>();
        public List<Match> CopyMatches()
        {
            List<Match> matches1 = new List<Match>();
            foreach (Match match in matches)
            {
                matches1.Add(new Match(match));
            }
            return matches1;
        }
        public void MakeDirty()
        {
            foreach (Team team in teams)
            {
                if (team.club != null) team.club.Dirty = true;
            }
        }

        public void CalculateRelatedConstraints(Model klvv)
        {
            relatedConstraints = new List<Constraint>();
            //foreach (Constraint con in klvv.constraints)
            //{
            //    if (con.poule == this) relatedConstraints.Add(con);
            //}
            List<Club> clubs = new List<Club>();
            
            foreach (Team team in teams)
            {
                if (clubs.Contains(team.club) == false) clubs.Add(team.club);
                // related club (via the grouping (sharing sporthall))
                if (team.club.groupingWithClub != null)
                {
                    Club relatedClub = team.club.groupingWithClub;
                    if (clubs.Contains(relatedClub) == false) clubs.Add(relatedClub);
                }
            }
            foreach (Constraint con in klvv.constraints)
            {
                if (clubs.Contains(con.club) || 
                    con.poule == this || 
                    teams.Contains(con.team)  
                    ) relatedConstraints.Add(con);
            }

        }

        public void OptimizeWeekends(Model klvv, IProgress intf)
        {
            MakeDirty();
            if (optimizableWeekends)
            {
                if (conflict_cost > 0)
                {
                    if (weekends.Count > 10)//12)
                    {
                        List<Weekend> result = new List<Weekend>(weekends);
                        for (int i = 0; i < weekends.Count / 2; i++)
                        {
                            intf.Progress(i, weekends.Count);
                            for (int j = i+1; j < weekends.Count / 2; j++)
                            {
                                Swap(weekends, i, j);
                                SnapShotIfImproved(klvv);
                                Swap(weekends, i, j);
                            }
                            weekends = resultWeekends;
                        }
                        for (int i = weekends.Count / 2; i < weekends.Count; i++)
                        {
                            intf.Progress(i, weekends.Count);
                            for (int j = i+1; j < weekends.Count; j++)
                            {
                                Swap(weekends, i, j);
                                SnapShotIfImproved(klvv);
                                Swap(weekends, i, j);
                            }
                            weekends = resultWeekends;
                        }
                    }
                    else // full optimalisation
                    {
                        // First optimize first half
                        try
                        {

                            List<Weekend> firstHalf = new List<Weekend>();
                            List<Weekend> lastHalf = new List<Weekend>();
                            for (int i = 0; i < weekends.Count / 2; i++) firstHalf.Add(weekends[i]);
                            for (int i = weekends.Count / 2; i < weekends.Count; i++) lastHalf.Add(weekends[i]);
                            List<Weekend> remaining = new List<Weekend>(firstHalf);
                            weekends = new List<Weekend>();
                            GenerateWeekendCombination(klvv, new List<Weekend>(), lastHalf, remaining, intf);
                            

                            firstHalf.Clear();
                            lastHalf.Clear();
                            for (int i = 0; i < weekends.Count / 2; i++) firstHalf.Add(weekends[i]);
                            for (int i = weekends.Count / 2; i < weekends.Count; i++) lastHalf.Add(weekends[i]);
                            remaining = new List<Weekend>(lastHalf);
                            weekends = new List<Weekend>(); 
                            GenerateWeekendCombination(klvv, firstHalf, new List<Weekend>(), remaining, intf);
                        }
                        catch { }
                        weekends = resultWeekends;
                    }
                    klvv.Evaluate(null);
                }
            }
        }
        private bool SnapShotIfImproved(Model klvv)
        {
            bool snapshot = false;
            klvv.EvaluateRelatedConstraints(this);
            int total = klvv.TotalConflicts();
            if (total <= klvv.LastTotalConflicts)
            {
                snapshot = true;
                klvv.LastTotalConflicts = total;
            }

            if(snapshot)
            {
                resultTeams = new List<Team>(teams);
                resultWeekends = new List<Weekend>(weekends);
                resultMatches = CopyMatches();
                minConflicts = conflict_cost;
                klvv.stateNotSaved = true; 
            }
            return snapshot;
        }
        public void SnapShot(Model klvv)
        {
            klvv.Evaluate(null);
            resultTeams = new List<Team>(teams);
            resultWeekends = new List<Weekend>(weekends);
            resultMatches = CopyMatches();
            minConflicts = conflict_cost;
        }

        private void Swap(List<Weekend> list, int i, int j)
        {
            Weekend ti = list[i];
            Weekend tj = list[j];
            list.RemoveAt(i);
            list.Insert(i, tj);
            list.RemoveAt(j);
            list.Insert(j, ti);
        }
        private void GenerateWeekendCombination(Model klvv, List<Weekend> firstHalf, List<Weekend> lastHalf, List<Weekend> remainingWeekends, IProgress intf)
        {
            if (remainingWeekends.Count > 0)
            {
                for (int i = 0; i < remainingWeekends.Count; i++)
                {
                    if (weekends.Count == 0)
                    {
                        intf.Progress(i, remainingWeekends.Count - 1);
                    }
                    weekends.Add(remainingWeekends[0]);
                    remainingWeekends.RemoveAt(0);
                    GenerateWeekendCombination(klvv, firstHalf, lastHalf, remainingWeekends, intf);
                    remainingWeekends.Add(weekends[weekends.Count - 1]);
                    weekends.RemoveAt(weekends.Count - 1);
                }
            }
            else
            {
                weekends.InsertRange(0, firstHalf);
                weekends.AddRange(lastHalf);
                SnapShotIfImproved(klvv);
                weekends.RemoveRange(0, firstHalf.Count);
                weekends.RemoveRange(weekends.Count - lastHalf.Count, lastHalf.Count);
                if (intf.Cancelled())
                {
                    throw new Exception("Cancelled");
                }
            }

        }
        public void OptimizeTeam(Model klvv, IProgress intf,Team team)
        {
            MakeDirty();
            if (serie.optimizableNumber)
            {
                if (conflict_cost > 0)
                {
                    int teamindex = teams.FindIndex(t => t == team);
                    for (int i = 0; i < maxTeams; i++)
                    {
                        if (i != teamindex)
                        {
                            intf.Progress(i, teams.Count);
                            Swap(teams, i, teamindex);
                            OptimizeWeekends(klvv, intf);
                            OptimizeHomeVisitor(klvv);
                            //SnapShotIfImproved(klvv, ref minConflict);
                            Swap(teams, i, teamindex); // swap back
                        }
                        if (intf.Cancelled())
                        {
                            break;
                        }
                    }
                    teams = resultTeams;
                    weekends =resultWeekends;
                    matches = resultMatches;
                    klvv.Evaluate(null);
                }
            }
        }

        public void OptimizeTeamAssignment(Model klvv, IProgress intf)
        {
            MakeDirty();
            if (optimizable)
            {
                if (conflict_cost > 0)
                {
                    if (maxTeams <= 6)
                    {
                        OptimizeFullTeamAssignment(klvv, intf);
                    }
                    else // semi-optimisation.
                    {
                        for (int i = 0; i < maxTeams; i++)
                        {
                            intf.Progress(i, maxTeams);
                            for (int j = 0; j < maxTeams; j++)
                            {
                                if (j != i)
                                {
                                    Swap(teams, i, j);
                                    SnapShotIfImproved(klvv);
                                    Swap(teams, i, j);
                                }
                            }
                        }
                    }
                    teams = resultTeams;
                    klvv.Evaluate(null);
                }
            }
        }

        public int[,] AnalyzeTeamAssignment(Model klvv, IProgress intf)
        {
            int[,] score = new int[maxTeams+1,maxTeams+1];
            List<Team> fixedOrderList = new List<Team>(teams);
            MakeDirty();
            if (optimizable)
            {
                for(int k = 1;k<=maxTeams;k++)
                {
                    teams.Add(teams[0]);
                    teams.RemoveAt(0);
                    /*
                    for (int i = 0; i < maxTeams-1; i++)
                    {
                        Swap(teams, i, i+1);
                    }*/
                    klvv.EvaluateRelatedConstraints(this);
                    int index = 0;
                    foreach (Team team in fixedOrderList)
                    {
                        score[index, (index + maxTeams - k) % maxTeams] = team.conflict;
                        index++;
                    }
                }
                Console.Write("{0,30}", "Teams:");
                for (int k = 1; k <= maxTeams; k++)
                {
                    Console.Write("{0,4}", "p-"+k.ToString());
                }
                Console.WriteLine();
                int index1 = 0;
                foreach (Team team in fixedOrderList)
                {
                    Console.Write("{0,30}", team.name);
                    for (int k = 0; k < maxTeams; k++)
                    {
                        Console.Write("{0,4}", score[index1, k]);
                    }
                    Console.WriteLine();
                    index1++;
                }
                teams = resultTeams;
                klvv.Evaluate(null);
            }
            return score;
        }
        public void AnalyzeAndOptimizeWeekends(Model klvv, IProgress intf)
        {
            SnapShot(klvv);
            //int best_score = 1000;
            int[,] table = AnalyzeWeekends(klvv, intf);
            /*
            Team[] used = new Team[maxTeams];
            for (int i = 0; i < maxTeams; i++) used[i] = null;
            AnalyzeAndOptimizeTeamAssignmentRecursive(klvv, intf, table, used, 0, ref best_score, new List<Team>(teams), 0);
            teams = resultTeams;
            */
            klvv.Evaluate(null);
        }
        public int[,] AnalyzeWeekends(Model klvv, IProgress intf)
        {
            const int maxWeekends = 10;
            if (weekends.Count != maxWeekends) throw (new Exception("Only usable for poule with 10 weekends"));
            int[,] score = new int[maxWeekends + 1, 5 + 1];
            List<Weekend> fixedOrderList = new List<Weekend>(weekends);
            MakeDirty();
            if (optimizableWeekends)
            {
                for (int k = 0; k < 5; k++)
                {
                    weekends.Insert(5, weekends[0]);
                    weekends.RemoveAt(0);
                    weekends.Add(weekends[5]);
                    weekends.RemoveAt(5);
                    //klvv.EvaluateRelatedConstraints(this);
                    klvv.Evaluate(null);
                    for(int index=0; index<maxWeekends; index++)
                    {
                        score[index, (index + 4 - k) % 5] = weekends[index].conflict;
                    }
                }
                Console.Write("{0,30}", "Weekends:");
                for (int k = 1; k <= 5; k++)
                {
                    Console.Write("{0,4}", "p-" + k.ToString());
                }
                Console.WriteLine();
                for (int index = 0; index < maxWeekends; index++)
                {
                    Console.Write("{0,30}", "Weekend "+(index+1).ToString());
                    for (int k = 0; k < 5; k++)
                    {
                        Console.Write("{0,4}", score[index, k]);
                    }
                    Console.WriteLine();
                }
                weekends = resultWeekends;
                klvv.Evaluate(null);
            }
            return score;
        }

        public void AnalyzeAndOptimizeTeamAssignment(Model klvv, IProgress intf)
        {
            SnapShot(klvv);
            int best_score = 1000;
            int[,] table = AnalyzeTeamAssignment(klvv, intf);
            Team[] used = new Team[maxTeams];
            for(int i = 0;i<maxTeams;i++) used[i] = null;
            AnalyzeAndOptimizeTeamAssignmentRecursive(klvv, intf, table, used, 0, ref best_score, new List<Team>(teams), 0);
            teams = resultTeams;
            klvv.Evaluate(null);
        }
        public void AnalyzeAndOptimizeTeamAssignmentRecursive(Model klvv, IProgress intf, int[,] table, Team[] used, int team, ref int best_score,List<Team> newTeamList, int score)
        {
            if (score < best_score && intf.Cancelled() == false)
            {
                if (team < maxTeams)
                {
                    for (int i = 0; i < maxTeams; i++)
                    {
                        if (team == 0) intf.Progress(i, maxTeams);
                        if (used[i] == null)
                        {
                            used[i] = teams[team];
                            AnalyzeAndOptimizeTeamAssignmentRecursive(klvv, intf, table, used, team + 1, ref best_score, new List<Team>(teams), score + table[team, i]);
                            used[i] = null;
                        }
                    }
                }
                else
                { // improved best-score
                    Console.WriteLine("score: {0}", score);
                    best_score = score;
                    List<Team> temp = teams;
                    teams = new List<Team>(used);
                    SnapShotIfImproved(klvv);
                    teams = temp;
                }
            }
        }
        public void OptimizeFullTeamAssignment(Model klvv, IProgress intf)
        {
            MakeDirty();
            if (optimizable)
            {
                if (conflict_cost > 0)
                {
                    List<Team> temp = new List<Team>(teams);
                    teams = new List<Team>();
                    try
                    {
                        GenerateTeamCombination(klvv, temp, intf);
                    }
                    catch { }
                    teams = resultTeams;

                }
            }
        }
        private void Swap(List<Team> list, int i, int j)
        {
            Team ti = list[i];
            Team tj = list[j];
            list.RemoveAt(i);
            list.Insert(i,tj);
            list.RemoveAt(j);
            list.Insert(j,ti);
        }
        private void GenerateTeamCombination(Model klvv, List<Team> remainingTeams, IProgress intf)
        {
            if (remainingTeams.Count > 0)
            {
                for (int i = 0; i < remainingTeams.Count; i++)
                {
                    if (teams.Count == 0)
                    {
                        intf.Progress(i,remainingTeams.Count-1);
                    }
                    teams.Add(remainingTeams[0]);
                    remainingTeams.RemoveAt(0);
                    GenerateTeamCombination(klvv, remainingTeams, intf);
                    remainingTeams.Add(teams[teams.Count - 1]);
                    teams.RemoveAt(teams.Count - 1);
                }
            }
            else
            {
                SnapShotIfImproved(klvv);
                if (intf.Cancelled())
                {
                    throw new Exception("Cancelled");
                }
            }

        }
        public void SwitchHomeTeamVisitorTeam(Model klvv, Match match)
        {
            if (optimizableHomeVisit)
            {
                Match match2 = null;
                Match match1 = null;
                foreach (Match m in matches)
                {
                    if (m.homeTeam == match.visitorTeam && m.visitorTeam == match.homeTeam) match2 = m;
                    if (m.homeTeam == match.homeTeam && m.visitorTeam == match.visitorTeam) match1 = m;
                }
                if (match2 != null && match1 != null)
                {
                    // swap teams
                    match1.homeTeamIndex = match2.homeTeamIndex;
                    match1.visitorTeamIndex = match2.visitorTeamIndex;
                    match2.homeTeamIndex = match1.visitorTeamIndex;
                    match2.visitorTeamIndex = match1.homeTeamIndex;
                }
            }
        }

        public Team BestFit(string name)
        {
            Team result = null;
            int best = 4; // minimum number of letters that need to be equal
            foreach (Team t in teams)
            {
                int score = 0;
                for (int i = 0; i < Math.Min(name.Length, t.name.Length); i++)
                {
                    if (name[i] != t.name[i]) break;
                    score = i + 1;
                }
                if (score > best)
                {
                    best = score;
                    result = t;
                }
            }
            return result;
        }


        public void OptimizeHomeVisitor(Model klvv)
        {
            MakeDirty();
            if (optimizableHomeVisit)
            {
                foreach (Match match in matches)
                {
                    if (match.conflict_cost > 0)
                    {
                        SwitchHomeTeamVisitorTeam(klvv, match);
                        if (SnapShotIfImproved(klvv) == false)
                        { // switch back
                            SwitchHomeTeamVisitorTeam(klvv, match);
                        }
                    }
                }
                matches = resultMatches;
                klvv.Evaluate(null);
            }
        }
        public void OptimizeHomeVisitorReverse(Model klvv)
        {
            MakeDirty();
            if (optimizableHomeVisit)
            {
                List<Match> reverseMatches = new List<Match>(matches);
                reverseMatches.Reverse();
                foreach (Match match in reverseMatches)
                {
                    if (match.conflict_cost > 0)
                    {
                        SwitchHomeTeamVisitorTeam(klvv, match);
                        if (SnapShotIfImproved(klvv) == false)
                        { // switch back
                            SwitchHomeTeamVisitorTeam(klvv, match);
                        }
                    }
                }
                matches = resultMatches;
                klvv.Evaluate(null);
            }
        }
        public void CreateMatchesFromSchemaFiles()
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(System.Windows.Forms.Application.StartupPath + @"/InputData/" + string.Format("Schema{0}.csv", maxTeams));
                int reqMatches = maxTeams * (maxTeams - 1);
                if (lines.Length != reqMatches)
                {
                    System.Windows.Forms.MessageBox.Show(string.Format("Schema{0}.csv does not have the correct number of lines. {1} iso {2}", maxTeams, lines.Length,reqMatches));
                }
                char[] delimiters = new char[] { ',', ';' };
                foreach (string line in lines)
                {
                    string[] var = line.Split(delimiters);
                    int day = int.Parse(var[0])-1; // internal administration starts at 0, schema files start at 1
                    int team1 = int.Parse(var[1])-1;
                    int team2 = int.Parse(var[2])-1;
                    Match m2 = new Match(day, team1, team2, serie, this);
                    matches.Add(m2);
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show(string.Format("No schema available for {0} teams", maxTeams));
            }
        }

        public void CreateMatches()
        {
            matches.Clear();
            List<int> allTeams = new List<int>();
            for (int i = 0; i < maxTeams; i++)
            {
                allTeams.Add(i);
            }
            int numDays = allTeams.Count() - 1;
            int halfsize = allTeams.Count() / 2;

            List<int> temp = new List<int>();
            List<int> teams = new List<int>();

            teams.AddRange(allTeams);
            temp.AddRange(allTeams);
            teams.RemoveAt(0);

            int teamSize = teams.Count;

            for (int day = 0; day < numDays * 2; day++)
            {
                //Calculate1stRound(day);
                if (day % 2 == 0)
                {
                    Console.Write(String.Format("\n\nDay {0}\n", (day + 1)));

                    int teamIdx = day % teamSize;

                    Console.Write(String.Format("{0} vs {1}\n", teams[teamIdx], temp[0]));
                    Match m1 = new Match(day, teams[teamIdx], temp[0], serie, this);
                    matches.Add(m1);


                    for (int idx = 0; idx < halfsize; idx++)
                    {
                        int firstTeam = (day + idx) % teamSize;
                        int secondTeam = ((day + teamSize) - idx) % teamSize;

                        if (firstTeam != secondTeam)
                        {
                            Console.Write(String.Format("{0} vs {1}\n", teams[firstTeam], teams[secondTeam]));
                            Match m2 = new Match(day, teams[firstTeam], teams[secondTeam], serie, this);
                            matches.Add(m2);
                        }
                    }
                }

                //Calculate2ndRound(day);
                if (day % 2 != 0)
                {
                    int teamIdx = day % teamSize;

                    Console.Write(String.Format("\n\nDay {0}\n", (day + 1)));

                    Console.Write(String.Format("{0} vs {1}\n", temp[0], teams[teamIdx]));
                    Match m1 = new Match(day, temp[0],teams[teamIdx], serie, this);
                    matches.Add(m1);

                    for (int idx = 0; idx < halfsize; idx++)
                    {
                        int firstTeam = (day + idx) % teamSize;
                        int secondTeam = ((day + teamSize) - idx) % teamSize;

                        if (firstTeam != secondTeam)
                        {
                            Console.Write(String.Format("{0} vs {1}\n", teams[secondTeam], teams[firstTeam]));
                            Match m2 = new Match(day, teams[secondTeam], teams[firstTeam], serie, this);
                            matches.Add(m2);
                        }
                    }
                }
            }
        }
    }
}
