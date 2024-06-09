using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompetitionCreator
{
    public class Poule: ConflictAdmin
    {
        public class SnapShot
        {
            public List<Team> resultTeams = new List<Team>();
            public List<MatchWeek> resultWeeks = new List<MatchWeek>();
            public List<Match> resultMatches = new List<Match>();
            public int TotalConflicts = -1;
        }

        Stack<SnapShot> snapShots = new Stack<SnapShot>();
        public SnapShot bestSnapShot = new SnapShot();

        // Temp lists for snapshot
        public bool imported = false;
        private int optimisationThreshold = 200;

        public bool OptimizeNumber(Model model)
        {
            return model.OptimizeNumber && imported == false && serie.optimize == true;
        }
        public bool OptimizeHomeVisit(Model model)
        {
            return model.OptimizeHomeVisit && imported == false && serie.optimize == true;
        }
        public bool OptimizeSchema(Model model)
        {
            return model.OptimizeSchema && imported == false && serie.optimize == true;
        }
        public bool Optimize(Model model)
        {
            return OptimizeNumber(model) || OptimizeHomeVisit(model) || OptimizeSchema(model);
        }
        public bool evaluated { get { return serie.evaluated && imported == false; } }
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
                    Error.AddManualError("Failed adding a team to a poule.", string.Format("{0} contains no null teams while the poule is not full", name));
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
        public List<MatchWeek> weeks = new List<MatchWeek>();
        public int CalculateDistances(Team t1)
        {
            int distance = 0;
            int extraTeam = 1;
            if (t1.sporthal != null)
            {
                foreach (Team t2 in teams)
                {
                    if(t2.RealTeam() && t1.sporthal != null) distance += t1.sporthal.sporthall.Distance(t2.sporthal.sporthall);
                    if (t2 == t1) extraTeam = 0; // zit al in de teamcount
                }
            }
            if (TeamCount > 1)
            {
                distance = (distance / (extraTeam+TeamCount - 1));
            }
            return distance;
        }
        public List<Match> matches = new List<Match>();
        public List<Match> CopyMatches(List<Match> matches_org)
        {
            List<Match> matches1 = new List<Match>();
            foreach (Match match in matches_org)
            {
                matches1.Add(new Match(match));
            }
            return matches1;
        }

        public List<MatchWeek> CopyWeeks(List<MatchWeek> weeks_org)
        {
            List<MatchWeek> weeks1 = new List<MatchWeek>();
            foreach (MatchWeek week in weeks_org)
            {
                weeks1.Add(new MatchWeek(week));
            }
            return weeks1;
        }

        public void OptimizeWeeks(Model model, IProgress intf, int optimizationLevel)
        {
            if (OptimizeNumber(model))
            {
                //if (conflict_cost > 0)
                {
                    int round = 0;
                    int totalIndex = 0;
                    do
                    {
                        List<MatchWeek> previousRoundList = weeks.Where(w => w.round < round).ToList();
                        List<MatchWeek> currentRoundList = weeks.Where(w => w.round == round).ToList();
                        List<MatchWeek> nextRoundList = weeks.Where(w => w.round > round).ToList();
                        if(currentRoundList.Count > 5)//12)
                        {
                            for (int i = 0; i < currentRoundList.Count; i++)
                            {
                                intf.Progress(totalIndex, weeks.Count);
                                for (int j = i + 1; j < currentRoundList.Count; j++)
                                {
                                    var startSnapShot = CreateSnapShot(model);
                                    Swap(currentRoundList, i, j);
                                    weeks = new List<MatchWeek>(previousRoundList);
                                    weeks.AddRange(currentRoundList);
                                    weeks.AddRange(nextRoundList);
                                    var result = SnapShotIfImproved(model,false);
                                    if (result == SnapshotStatus.Better || result == SnapshotStatus.Close)
                                    {
                                        if (optimizationLevel > 0) OptimizeHomeVisitor(model, optimizationLevel > 1);
                                    }
                                    RestoreSnapShot(startSnapShot);
                                    if (intf.Cancelled()) return;
                                }
                                if (intf.Cancelled()) break;
                            }
                        }
                        else // full optimalisation
                        {
                            var startSnapShot = CreateSnapShot(model);
                            try
                            {
                                List<MatchWeek> remaining = currentRoundList;
                                weeks = new List<MatchWeek>(previousRoundList);
                                GenerateWeekCombination(model, ref weeks, remaining, nextRoundList, intf);
                            }
                            finally 
                            { 
                                RestoreSnapShot(startSnapShot);
                            }
                        }
                        totalIndex += currentRoundList.Count;
                        round++;
                    } while (totalIndex < weeks.Count && round<4);
                }
            }
        }
        public enum SnapshotStatus { Worse, Close, Better };
        public SnapshotStatus SnapShotIfImproved(Model model, bool equalAllowed = true)
        {
            Changed();
            SnapshotStatus result = SnapshotStatus.Worse;
            var snapShot = CreateSnapShot(model);
            if (snapShot.TotalConflicts < bestSnapShot.TotalConflicts)
            {
                if (snapShot.TotalConflicts < bestSnapShot.TotalConflicts + optimisationThreshold)
                    result = SnapshotStatus.Close;
                if (snapShot.TotalConflicts < bestSnapShot.TotalConflicts || (equalAllowed && snapShot.TotalConflicts == bestSnapShot.TotalConflicts))
                { 
                    bestSnapShot = snapShot;
                    model.stateNotSaved = true;
                    result = SnapshotStatus.Better;
                }
            }
            return result;
        }
        public SnapShot CreateSnapShot(Model model)
        {
            model.Evaluate(this);
            SnapShot snapShot = new Poule.SnapShot();
            snapShot.TotalConflicts = model.TotalConflicts();
            snapShot.resultTeams = new List<Team>(teams);
            snapShot.resultWeeks = CopyWeeks(weeks);
            snapShot.resultMatches = CopyMatches(matches);
            return snapShot;
        }

        public void SetInitialSnapShot(Model model)
        {
            var sn = CreateSnapShot(model);
            model.Evaluate(this);
            sn.TotalConflicts = model.TotalConflicts();
            bestSnapShot = sn;
        }

        public void SetBestSnapShot(SnapShot snapShot)
        {
            SnapShot newSnapShot = new Poule.SnapShot();
            newSnapShot.TotalConflicts = snapShot.TotalConflicts;
            newSnapShot.resultTeams = new List<Team>(snapShot.resultTeams);
            newSnapShot.resultWeeks = CopyWeeks(snapShot.resultWeeks);
            newSnapShot.resultMatches = CopyMatches(snapShot.resultMatches);

            bestSnapShot = newSnapShot;
        }

        public void RestoreSnapShot(SnapShot snapShot)
        {
            Changed();
            teams = new List<Team>(snapShot.resultTeams);
            weeks = CopyWeeks(snapShot.resultWeeks);
            matches = CopyMatches(snapShot.resultMatches);
        }

        private void Swap(List<MatchWeek> list, int i, int j)
        {
            MatchWeek ti = list[i];
            MatchWeek tj = list[j];
            list.RemoveAt(i);
            list.Insert(i, tj);
            list.RemoveAt(j);
            list.Insert(j, ti);
        }
        private void GenerateWeekCombination(Model model, ref List<MatchWeek> weeks, List<MatchWeek> remainingWeeks, List<MatchWeek> lastWeeks, IProgress intf)
        {
            if (remainingWeeks.Count > 0)
            {
                for (int i = 0; i < remainingWeeks.Count; i++)
                {
                    weeks.Add(remainingWeeks[0]);
                    remainingWeeks.RemoveAt(0);
                    GenerateWeekCombination(model, ref weeks, remainingWeeks, lastWeeks, intf);
                    remainingWeeks.Add(weeks[weeks.Count - 1]);
                    weeks.RemoveAt(weeks.Count - 1);
                }
            }
            else
            {
                int index = weeks.Count;
                weeks.AddRange(lastWeeks);
                SnapShotIfImproved(model,false);
                weeks.RemoveRange(index, lastWeeks.Count);
                if (intf.Cancelled())
                {
                    //throw new Exception("Cancelled");
                }
            }

        }
        public void OptimizeTeams(Model model, IProgress intf, int optimizeLevel)
        {
            List<Team> teamList = null;
            teamList = new List<Team>();
            if (OptimizeNumber(model))
            {
                foreach (Team team in teams)
                {
                    if (team.conflict_cost > 0)
                    {
                        teamList.Add(team);
                    }
                }
            }
            teamList.Sort(delegate(Team t1, Team t2) { return t1.conflict_cost.CompareTo(t2.conflict_cost); });
            teamList.Reverse();
            foreach (Team team in teamList)
            {
                {
                    intf.SetText("Optimizing - " + team.poule.serie.name + team.poule.name + " - " + team.name);
                    OptimizeTeam(model, intf, team, optimizeLevel);
                    if (intf.Cancelled()) return;
                }
                Console.WriteLine(" - {1}:totalConflicts: {0}", model.TotalConflicts(), team.poule.fullName);
            }
        }

        public void OptimizeTeam(Model model, IProgress intf, Team team, int optimizationLevel)
        {
            if (OptimizeNumber(model))
            {
                //if (conflict_cost > 0)
                {
                    var startSnapShot = CreateSnapShot(model);
                    int teamindex = teams.FindIndex(t => t == team);
                    for (int i = 0; i < maxTeams; i++)
                    {
                        if (i != teamindex)
                        {
                            intf.Progress(i, teams.Count);
                            Swap(teams, i, teamindex);
                            // Voegt dit veel toe?
                            OptimizeWeeks(model, intf, optimizationLevel);
                            if (intf.Cancelled() == false) break;
                            if(optimizationLevel>0) OptimizeHomeVisitor(model);
                            //SnapShotIfImproved(model, ref minConflict);
                            Swap(teams, i, teamindex); // swap back
                        }
                        if (intf.Cancelled())
                        {
                            break;
                        }
                    }
                    RestoreSnapShot(startSnapShot);
                }
            }
        }

        public void OptimizeTeamAssignment(Model model, IProgress intf)
        {
            if (OptimizeNumber(model))
            {
                //if (conflict_cost > 0)
                {
                    var startSnapShot = CreateSnapShot(model);
                    if (maxTeams <= 6 && GlobalState.optimizeLevel > 0)
                    {
                        OptimizeFullTeamAssignment(model, intf);
                    }
                    else // semi-optimisation.
                    {
                        if (intf.Cancelled() == false) OptimizeNumberOnAnalysis(model, intf);
                        if (GlobalState.optimizeLevel > 0)
                        {
                            for (int i = 0; i < maxTeams; i++)
                            {
                                intf.Progress(i, maxTeams);
                                for (int j = 0; j < maxTeams; j++)
                                {
                                    List<Team> temp = new List<Team>(teams);
                                    if (j != i)
                                    {
                                        Swap(teams, i, j);
                                        SnapShotIfImproved(model, false);
                                        Swap(teams, i, j);
                                    }
                                    if (intf.Cancelled()) break;
                                }
                                if (intf.Cancelled()) break;
                            }
                        }
                    }
                    RestoreSnapShot(startSnapShot);
                }
            }
        }

        public int[,] AnalyzeTeamAssignment(Model model, IProgress intf)
        {
            var startSnapShot = CreateSnapShot(model);
            int[,] score = new int[maxTeams,maxTeams];
            List<Team> fixedOrderList = new List<Team>(teams);
            for(int k = 1;k<=maxTeams;k++)
            {
                teams.Add(teams[0]);
                teams.RemoveAt(0);
                /*
                for (int i = 0; i < maxTeams-1; i++)
                {
                    Swap(teams, i, i+1);
                }*/
                model.Evaluate(this);
                int index = 0;
                foreach (Team team in fixedOrderList)
                {
                    score[index, (index + maxTeams - k) % maxTeams] = team.conflict;
                    index++;
                }
            }
            RestoreSnapShot(startSnapShot);
            return score;
        }

        public int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }
        public void OptimizeNumberOnAnalysis(Model model, IProgress intf)
        {
            if (evaluated == false) // This optimization will not work otherwise since all own conflicts are zero
                return;
            var startSnapShot = CreateSnapShot(model);

            //var i = RandomNumber(0, maxTeams - 1);
            //var j = RandomNumber(0, maxTeams);
            //if (i == j) j++;
            //Swap(teams, i, j);
            int[,] score = AnalyzeTeamAssignment(model, intf);

            var algorithm = new GraphAlgorithms.HungarianAlgorithm(score);

            var result = algorithm.Run();

            //printArray(result);
            {
                var temp = teams;
                teams = new List<Team>();
                foreach (var index in result)
                {
                    teams.Add(temp[index]);
                }
                SnapShotIfImproved(model, false);
                teams = temp;
            }
            RestoreSnapShot(startSnapShot);
        }
        
        public void OptimizeFullTeamAssignment(Model model, IProgress intf)
        {
            if (OptimizeNumber(model))
            {
                //if (conflict_cost > 0)
                {
                    var startSnapShot = CreateSnapShot(model);
                    List<Team> temp = new List<Team>(teams);
                    teams = new List<Team>();
                    try
                    {
                        GenerateTeamCombination(model, temp, intf);
                    }
                    catch { }
                    RestoreSnapShot(startSnapShot);

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
        private void GenerateTeamCombination(Model model, List<Team> remainingTeams, IProgress intf)
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
                    GenerateTeamCombination(model, remainingTeams, intf);
                    remainingTeams.Add(teams[teams.Count - 1]);
                    teams.RemoveAt(teams.Count - 1);
                }
            }
            else
            {
                SnapShotIfImproved(model, false);
                if (intf.Cancelled())
                {
                    //throw new Exception("Cancelled");
                }
            }

        }
        public void SwitchHomeTeamVisitorTeam(Model model, Match match)
        {
            if (OptimizeHomeVisit(model))
            {
                int homeMatchesCounthome = 0;
                int homeMatchesCountvisitor = 0;
                int homeTeamIndex = match.homeTeamIndex;
                int visitorTeamIndex = match.visitorTeamIndex;

                List<Match> homeList = new List<Match>();  // Generic implementation taking into account 1 or 3 rounds
                List<Match> visitorList = new List<Match>();

                foreach (Match m in matches)
                {
                    if (m.homeTeam == match.homeTeam && m.visitorTeam == match.visitorTeam) homeList.Add(m);
                    if (m.homeTeam == match.visitorTeam && m.visitorTeam == match.homeTeam) visitorList.Add(m);
                    if (m.homeTeam == match.homeTeam) homeMatchesCounthome++;
                    if (m.homeTeam == match.visitorTeam) homeMatchesCountvisitor++;
                }
                foreach (Match m in homeList)
                {
                    m.homeTeamIndex = visitorTeamIndex;
                    m.visitorTeamIndex = homeTeamIndex;
                }
                foreach (Match m in visitorList)
                {
                    m.homeTeamIndex = homeTeamIndex;
                    m.visitorTeamIndex = visitorTeamIndex;
                }
            }
        }

 /*       public void SwitchHomeTeamVisitorTeam(Model model, Match match)
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
        */
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


        public bool OptimizeHomeVisitor(Model model, bool tryZeroCostMatches = true)
        {
            bool changed = false;
            int compareTo = 0;
            if (tryZeroCostMatches) compareTo = -1;
            if (OptimizeHomeVisit(model))
            {
                foreach (Match match in matches)
                {
                    if (match.conflict_cost > compareTo)
                    {
                        SwitchHomeTeamVisitorTeam(model, match);
                        if (SnapShotIfImproved(model,false) == SnapshotStatus.Worse)
                        { // switch back
                            SwitchHomeTeamVisitorTeam(model, match);
                        }
                        else 
                        {
                            changed = true;
                        }
                    }
                }
            }
            return changed;
        }
        public void OptimizeHomeVisitorReverse(Model model, bool equalAllowed = true)
        {
            int compareTo = 0;
            if (equalAllowed) compareTo = -1;
            if (OptimizeHomeVisit(model))
            {
                List<Match> reverseMatches = new List<Match>(matches);
                reverseMatches.Reverse();
                foreach (Match match in reverseMatches)
                {
                    if (match.conflict_cost > compareTo)
                    {
                        SwitchHomeTeamVisitorTeam(model, match);
                        if (SnapShotIfImproved(model, false) == SnapshotStatus.Worse)
                        { // switch back
                            SwitchHomeTeamVisitorTeam(model, match);
                        }
                    }
                }
            }
        }
        public bool OptimizeSchema6(Model model, IProgress intf, int optimizationLevel)
        {
            int count = 0;
            int round = 0;
            while(OptimizeSchema6Int(model, intf, optimizationLevel, ref round, ref count));
            return false;
        }
        public bool OptimizeSchema6Int(Model model, IProgress intf, int optimizationLevel, ref int roundref, ref int count)
        {
            if (maxTeams > 6) return false;
            while(roundref < 4)
            {
                int round = roundref;
                UInt32[] bitPatterns = new UInt32[(maxTeams * (maxTeams-1))/2];
                UInt32[] schema = new UInt32[(maxTeams * (maxTeams - 1)) / 2];
                List<Match> selectedMatches = matches.Where(m => m.Week.round == round).ToList();
                if (selectedMatches.Count == 0)
                    break;
                List<int> indexes = new List<int>();
                foreach (var m in selectedMatches)
                {
                    if (indexes.Contains(m.weekIndex) == false)
                    {
                        indexes.Add(m.weekIndex);
                    }
                }
                UInt32[] days = new UInt32[indexes.Count];
                for (int i = 0; i < days.Count(); i++) days[i] = 0;

                for (int i = 0; i < selectedMatches.Count; i++)
                {
                    bitPatterns[i] = (1u << selectedMatches[i].homeTeamIndex) | (1u << selectedMatches[i].visitorTeamIndex);
                }
                List<List<UInt32>> schemas = new List<List<uint>>();
                RecursiveFullSchema(model, ref days, ref bitPatterns, ref schema, ref schemas, selectedMatches.Count - 1);
                while(count < schemas.Count)
                {
                    var sch = schemas[count];
                    count++;
                    intf.Progress(count, schemas.Count * 2);
                    if (intf.Cancelled())
                        return false;

                    //List<Match> selectedMatches1 = matches.Where(m => m.Week.round == round).ToList();
                    for (int i = 0; i < selectedMatches.Count; i++)
                    {
                        Match m = selectedMatches[i];
                        m.weekIndex = indexes[(int)sch[i]];
                    }
                    var result = SnapShotIfImproved(model,false);
                    if (result == SnapshotStatus.Better || result == SnapshotStatus.Close) // kans nog aanwezig op goede score
                    {
                        if (optimizationLevel > 1)
                        {
                            //if (OptimizeHomeVisitor(model, false))
                              //  return true; // administration is not correct anymore, so jump out
                            if (OptimizeHomeVisitor(model, optimizationLevel > 1))
                                return true; // administration is not correct anymore, so jump out
                        }
                    }
                }
                count = 0;
                roundref++;
            }
            return false;
        }

        private void RecursiveFullSchema(Model model, ref UInt32[] days, ref UInt32[] bitPatterns, ref UInt32[] schema, ref List<List<UInt32>> schemas, int matchNumber)
        {
            if (matchNumber >= 0)
            {
                for (UInt32 d = 0; d < days.Count(); d++)
                {
                    if ((days[d] & bitPatterns[matchNumber]) == 0)
                    {
                        days[d] |= bitPatterns[matchNumber];
                        schema[matchNumber] = d;
                        RecursiveFullSchema(model, ref days, ref bitPatterns, ref schema, ref schemas, matchNumber - 1);
                        days[d] &= (~bitPatterns[matchNumber]);
                    }
                }
            }
            else
            {
                //List<UInt32> newSchema = new List<uint>();
                //for (int i = 0; i < 15; i++)
                //{
                //    newSchema.Add(schema[i]);
                //}
                //schemas.Add(newSchema);
                schemas.Add(schema.ToList());
            }
         }

        public bool OptimizeSchema(Model model, IProgress intf, int setSize, int optimizationLevel)
        {
            if (maxTeams > 6)
            {
                if (OptimizeNumber(model))
                {
                    List<Match>[] matchesPerWeek = new List<Match>[weeks.Count];
                    for (int index = 0; index < weeks.Count; index++)
                    {
                        matchesPerWeek[index] = matches.Where(m => m.weekIndex == index).ToList();
                    }
                    for (int index1 = 0; index1 < weeks.Count; index1++)
                    {
                        intf.Progress(index1, weeks.Count);
                        if (intf.Cancelled())
                        {
                            return false;
                        }
                        try
                        {
                            int round = weeks[index1].round;
                            List<Match> matchesDay = matchesPerWeek[index1];
                            Match[] matches1 = new Match[setSize];
                            int[] indices = new int[setSize];
                            for (int i = 0; i < setSize; i++)
                                indices[i] = i;
                            do
                            {
                                UInt32 indexes = 0;
                                for (int i = 0; i < setSize; i++)
                                {
                                    matches1[i] = matchesDay[indices[i]];
                                    indexes |= matches1[i].matchMask;
                                }
                                for (int index2 = index1 + 1; index2 < weeks.Count; index2++)
                                {
                                    if (weeks[index2].round == round)
                                    {
                                        List<Match> matches2 = new List<Match>();
                                        bool validWeek = true;
                                        foreach (Match m in matchesPerWeek[index2])
                                        {
                                            UInt32 matchMask = m.matchMask;
                                            if ((indexes & matchMask) == matchMask)
                                            {
                                                matches2.Add(m);
                                            }
                                            else if ((indexes & matchMask) != 0)
                                            { // Switching is not possible when a match is half overlapping
                                                validWeek = false;
                                                break;
                                            }
                                        }
                                        if (validWeek)
                                        {
                                            SwitchMatches(index1, matches1, index2, matches2);
                                            var result = SnapShotIfImproved(model, false);
                                            if (result == SnapshotStatus.Better || result == SnapshotStatus.Close)
                                            {
                                                if (OptimizeHomeVisitor(model, optimizationLevel > 1))
                                                    throw new Exception();  // admin is not correct any more
                                            }
                                            SwitchMatches(index2, matches1, index1, matches2);
                                        }
                                    }
                                }
                            } while(NextSetMatches(ref indices, setSize, matchesDay.Count-1));
                        }
                        catch
                        {
                            // Is changed, so recalculate
                            matchesPerWeek = new List<Match>[weeks.Count];
                            for (int index = 0; index < weeks.Count; index++)
                            {
                                matchesPerWeek[index] = matches.Where(m => m.weekIndex == index).ToList();
                            }
                        }
                    }
                }
            }
            return false;
        }

        bool NextSetMatches(ref int[] indices, int indicesCount, int maxIndex)
        {
            int current = indicesCount-1;
            indices[current]++;
            while(indices[current]>maxIndex)
            {
                indices[current] = 0;
                current--;
                if (current < 0)
                    return false;
                indices[current]++;
                for (int i = current + 1; i < indicesCount; i++)
                {
                    indices[i] = indices[i - 1] + 1;
                    if (indices[i] > maxIndex)
                        return false; 
                }
            }
            return true;
        }

        private void SwitchMatches(int index1, IEnumerable<Match> matches1, int index2, IEnumerable<Match> matches2)
        {
            foreach(Match m in matches1)
                m.weekIndex = index2;
            foreach(Match m in matches2)
                m.weekIndex = index1;
        }


        public bool OptimizeSchema3(Model model, IProgress intf, int optimizationLevel)
        {
            if (maxTeams > 6)
            {
                if (OptimizeNumber(model))
                {
                    int matchCount = 0;
                    for (int index = 0; index < weeks.Count; index++)
                    {
                        int round = weeks[index].round;
                        intf.Progress(index, weeks.Count);
                        if (intf.Cancelled())
                            return false;
                        try
                        {
                            List<Match> matchesDay = matches.Where(m => m.weekIndex == index).ToList();
                            for (int x = 0; x < matchesDay.Count - 1; x++)
                            {
                                for (int y = x + 1; y < matchesDay.Count; y++)
                                {
                                    for (int z = y + 1; z < matchesDay.Count; z++)
                                    {
                                        Match match1 = matchesDay[x];
                                        Match match2 = matchesDay[y];
                                        Match match3 = matchesDay[z];
                                        UInt32 indexes = 0;
                                        const UInt32 one = 1;
                                        indexes |= one << match1.homeTeamIndex;
                                        indexes |= one << match1.visitorTeamIndex;
                                        indexes |= one << match2.homeTeamIndex;
                                        indexes |= one << match2.visitorTeamIndex;
                                        indexes |= one << match3.homeTeamIndex;
                                        indexes |= one << match3.visitorTeamIndex;
                                        List<Match>[] threeMatches = new List<Match>[weeks.Count];
                                        for (int i = 0; i < weeks.Count; i++)
                                            threeMatches[i] = new List<Match>();
                                        int maxIndex = int.MinValue;
                                        int minIndex = int.MaxValue;
                                        foreach (Match m in matches)
                                        {
                                            if (m.weekIndex > index && (indexes & (one << m.homeTeamIndex)) != 0 && (indexes & (one << m.visitorTeamIndex)) != 0 && m.Week.round == round)
                                            {
                                                threeMatches[m.weekIndex].Add(m);
                                                maxIndex = Math.Max(maxIndex, m.weekIndex);
                                                minIndex = Math.Min(minIndex, m.weekIndex);
                                            }
                                        }
                                        for (int index2 = minIndex; index2 <= maxIndex; index2++)
                                        {
                                            if (threeMatches[index2].Count == 3)
                                            {
                                                Match match_1 = threeMatches[index2][0];
                                                Match match_2 = threeMatches[index2][1];
                                                Match match_3 = threeMatches[index2][2];
                                                match1.weekIndex = index2;
                                                match2.weekIndex = index2;
                                                match3.weekIndex = index2;
                                                match_1.weekIndex = index;
                                                match_2.weekIndex = index;
                                                match_3.weekIndex = index;
                                                var result = SnapShotIfImproved(model,false);
                                                if (result == SnapshotStatus.Better || result == SnapshotStatus.Close)
                                                {
                                                    if (OptimizeHomeVisitor(model, optimizationLevel > 1))
                                                        throw new Exception();  // admin is not correct any more
                                                }
                                                match_1.weekIndex = index2;
                                                match_2.weekIndex = index2;
                                                match_3.weekIndex = index2;
                                                match1.weekIndex = index;
                                                match2.weekIndex = index;
                                                match3.weekIndex = index;
                                                matchCount++;
                                            }

                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                            //continue with the next week.
                        }
                    }
                }
            }
            return false;
        }

        public void CreateMatchesFromSchemaFiles(Schema schema, Serie serie, Poule poule)
        {
            foreach(SchemaWeek week in schema.weeks.Values)
            {
                foreach(SchemaMatch match in week.matches)
                {
                    Match m = new Match(week.WeekNr, match.team1, match.team2, serie, poule);
                    weeks[week.WeekNr].round = week.round;
                    matches.Add(m);
                }
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
