using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VolleybalCompetition_creator
{
    public class Poule: ConstraintAdmin
    {
        // Temp lists for snapshot
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
        public void AddWeekend(int year, int weekNr)
        {
            int index = weekends.FindIndex(w => w.WeekNr == weekNr && w.Year == year);
            if (index < 0)
            {
                weekends.Add(new Weekend(year, weekNr));
            }
        }
        public int CalculateDistances(Team t1)
        {
            int distance = 0;
            if (t1.sporthal != null)
            {
                foreach (Team t2 in teams)
                {
                    if(t2.RealTeam()) distance += t1.sporthal.sporthall.Distance(t2.sporthal.sporthall);
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

        public void CalculateRelatedConstraints(Klvv klvv)
        {
            relatedConstraints = new List<Constraint>();
            foreach (Constraint con in klvv.constraints)
            {
                if (con.poule == this) relatedConstraints.Add(con);
            }
            List<Club> clubs = new List<Club>();
            
            foreach (Team team in teams)
            {
                if (clubs.Contains(team.club) == false) clubs.Add(team.club);
            }
            foreach (Constraint con in klvv.constraints)
            {
                if (clubs.Contains(con.club) || con.poule == this || (con as ConstraintTeams)!= null) relatedConstraints.Add(con);
            }

        }

        public void OptimizeWeekends(Klvv klvv, IProgress intf)
        {
            MakeDirty();
            if (serie.optimizable && serie.weekOrderChangeAllowed)
            {
                if (conflict_cost > 0)
                {
                    if (weekends.Count > 1)//12)
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
        private bool SnapShotIfImproved(Klvv klvv)
        {
            bool snapshot = false;
            if (klvv.slow)
            {
                klvv.Evaluate(null);
                int total = klvv.TotalConflicts();
                if (total < klvv.LastTotalConflicts)
                {
                    snapshot = true;
                    klvv.LastTotalConflicts = total;
                }
            }
            else
            {
                klvv.EvaluateRelatedConstraints(this);
                int total = klvv.TotalConflicts();
                if (total < klvv.LastTotalConflicts)
                {
                    snapshot = true;
                    klvv.LastTotalConflicts = total;
                }

            }
            if(snapshot)
            {
                resultTeams = new List<Team>(teams);
                resultWeekends = new List<Weekend>(weekends);
                resultMatches = CopyMatches();
                minConflicts = conflict_cost; 
            }
            return snapshot;
        }
        public void SnapShot(Klvv klvv)
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
        private void GenerateWeekendCombination(Klvv klvv, List<Weekend> firstHalf, List<Weekend> lastHalf, List<Weekend> remainingWeekends, IProgress intf)
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
        public void OptimizeTeam(Klvv klvv, IProgress intf,Team team)
        {
            MakeDirty();
            if (serie.optimizable)
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

        public void OptimizeTeamAssignment(Klvv klvv, IProgress intf)
        {
            MakeDirty();
            if (serie.optimizable)
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
        public int[,] AnalyzeTeamAssignment(Klvv klvv, IProgress intf)
        {
            int[,] score = new int[maxTeams+1,maxTeams+1];
            List<Team> fixedOrderList = new List<Team>(teams);
            MakeDirty();
            if (serie.optimizable)
            {
                for(int k = 1;k<=maxTeams;k++)
                {
                    intf.Progress(k-1, maxTeams);
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

        public void AnalyzeAndOptimizeTeamAssignment(Klvv klvv, IProgress intf, Team team)
        {
            int[,] score = AnalyzeTeamAssignment(klvv, intf);
            bool[] used = new bool[maxTeams];
            for(int i = 0;i<maxTeams;i++) used[i] = false;
            foreach (Team t in teams)
            {
                int index = teams.FindIndex(te => te == t);
                AnalyzeAndOptimizeTeamAssignmentRecursive(klvv, intf, score, used, index, t, 0, new List<Team>(teams), klvv.LastTotalConflicts);
            }
        }
        public void AnalyzeAndOptimizeTeamAssignmentRecursive(Klvv klvv, IProgress intf, int[,] score, bool[] used, int startPos, Team team, int delta,List<Team> newTeamList,int minTotalConflict )
        {
            //Console.WriteLine("Team: {0}", team.name);
            int index = teams.FindIndex(t => t == team);
            List<int> possibilities = new List<int>();
            for (int i = 0; i < maxTeams; i++)
            {
                if (used[i] == false) possibilities.Add(i);
            }
            possibilities.Sort(delegate(int i1, int i2) { return score[index,i1].CompareTo(score[index,i2]); });
            int delta1 = 0;
            foreach (int i in possibilities)
            {
                //Console.WriteLine("  - pos: {0}", i);
                delta1 = score[index, i] - score[index, index];
                if (delta + delta1 < 0)
                {
                    used[i] = true;
                    newTeamList[i] = team;
                    if (i == startPos)
                    {
                        List<Team> temp = new List<Team>(teams);
                        teams = newTeamList;
                        klvv.EvaluateRelatedConstraints(this);
                        if (klvv.TotalConflicts() <= minTotalConflict)
                        {
                            Console.WriteLine(" **** Final score: {0}", delta + delta1);
                            int index1 = 0;
                            int total = 0;
                            foreach (Team t in newTeamList)
                            {
                                int sc = score[teams.FindIndex(te => te == t), index1];
                                Console.WriteLine(" - {0} : {1}", t.name, sc);
                                total += sc;
                                index1++;
                            }
                            Console.WriteLine("   Total: {0} - {1}", total, klvv.TotalConflicts());
                            
                        }
                        teams = temp;
                    }
                    else
                    {
                        AnalyzeAndOptimizeTeamAssignmentRecursive(klvv, intf, score, used, startPos, teams[i], delta + delta1,newTeamList, minTotalConflict);
                        
                    }
                    newTeamList[i] = teams[i];
                    used[i] = false;
                }
            }

        }
        public void OptimizeFullTeamAssignment(Klvv klvv, IProgress intf)
        {
            MakeDirty();
            if (serie.optimizable)
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
        private void GenerateTeamCombination(Klvv klvv, List<Team> remainingTeams, IProgress intf)
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
        public void SwitchHomeTeamVisitorTeam(Klvv klvv, Match match)
        {
            if (serie.optimizable && serie.homeVisitChangeAllowed && (klvv.fixedSchema == false || match.Optimizable))
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
        public void OptimizeHomeVisitor(Klvv klvv)
        {
            MakeDirty();
            if (serie.optimizable && serie.homeVisitChangeAllowed)
            {
                foreach (Match match in matches)
                {
                    if (match.conflict_cost > 0)
                    {
                        SwitchHomeTeamVisitorTeam(klvv,match);
                        if(SnapShotIfImproved(klvv) == false)
                        { // switch back
                            SwitchHomeTeamVisitorTeam(klvv,match);
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
