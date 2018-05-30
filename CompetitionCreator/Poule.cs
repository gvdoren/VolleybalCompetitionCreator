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
        private int optimisationThreshold = 200;
        private int optimisationConflictThreshold = 2;

        public bool optimizable { get { return serie != null && serie.optimizableNumber && imported == false; } }
        public bool optimizableWeeks { get { return serie != null && serie.optimizableWeeks && imported == false; } }
        public bool optimizableHomeVisit { get { return serie != null && serie.optimizableHomeVisit && imported == false; } }
        public bool optimizableMatch { get { return serie != null && serie.optimizableMatch && imported == false; } }

        public bool evaluated { get { return serie.evaluated && imported == false; } }
        List<Team> resultTeams = new List<Team>();
        List<MatchWeek> resultWeeks = new List<MatchWeek>();
        List<Match> resultMatches = new List<Match>();
        List<Match> bla = new List<Match>();
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
        public void MakeDirty()
        {
            foreach (Team team in teams)
            {
                if (team.club != null) team.club.Dirty = true;
            }
        }

        public void CalculateRelatedConstraints(Model model)
        {
            relatedConstraints = new List<Constraint>();
            List<Club> clubs = new List<Club>();
            List<Poule> poules = new List<Poule>();
            poules.Add(this);
            foreach (Team team in teams)
            {
                if (clubs.Contains(team.club) == false) clubs.Add(team.club);
            }
            foreach (Constraint con in model.constraints)
            {
                if (con.RelatedTo(clubs) ||
                    con.RelatedTo(poules)||
                    con.RelatedTo(teams))
                {
                    relatedConstraints.Add(con);
                }
            }

            // Dan krijg je de teams+clubs die via een constraint afhankelijkheid beinvloed worden
            List<Team> extraTeams = new List<Team>();
            List<Club> extraClubs = new List<Club>();
            List<Poule> extraPoules = new List<Poule>();
            foreach (Constraint con in relatedConstraints)
            {
                List<Team> teams1 = con.RelatedTeams();
                foreach (Team t in teams1)
                {
                    if (teams.Contains(t) == false) extraTeams.Add(t);
                }
                List<Club> clubs1 = con.RelatedClubs();
                foreach (Club c in clubs1)
                {
                    if (clubs.Contains(c) == false) extraClubs.Add(c);
                }
                // Zijn related poules relevant????
            }
            foreach (Constraint con in model.constraints)
            {
                if (con.RelatedTo(extraClubs) ||
                    con.RelatedTo(extraPoules) ||
                    con.RelatedTo(extraTeams))
                {
                    if (relatedConstraints.Contains(con) == false)
                    {
                        relatedConstraints.Add(con);
                    }
                }
            }

        }

        public void OptimizeWeeks(Model model, IProgress intf, int optimizationLevel)
        {
            MakeDirty();
            if (optimizableWeeks)
            {
                //SnapShot(model);
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
                                    Swap(currentRoundList, i, j);
                                    weeks = new List<MatchWeek>(previousRoundList);
                                    weeks.AddRange(currentRoundList);
                                    weeks.AddRange(nextRoundList);
                                    SnapShotIfImproved(model,false);
                                    if (TotalRelatedConflictsLast < TotalRelatedConflictsSnapshot + optimisationThreshold)
                                    {
                                        if (optimizationLevel > 0) OptimizeHomeVisitor(model, optimizationLevel > 1);
                                    }
                                    Swap(currentRoundList, i, j); 
                                    if (intf.Cancelled()) return;
                                    matches = CopyMatches(resultMatches);
                                }
                                weeks = resultWeeks;
                                if (intf.Cancelled()) break;
                            }
                        }
                        else // full optimalisation
                        {
                            try
                            {
                                List<MatchWeek> remaining = currentRoundList;
                                weeks = new List<MatchWeek>(previousRoundList);
                                GenerateWeekCombination(model, ref weeks, remaining, nextRoundList, intf);
                                weeks = resultWeeks;
                            }
                            catch { }
                        }
                        totalIndex += currentRoundList.Count;
                        round++;
                    } while (totalIndex < weeks.Count && round<4);
                }
            }
        }
        private int TotalRelatedConflictsSnapshot = 0;
        private int TotalRelatedConflictsLast = 0;
        public bool SnapShotIfImproved(Model model, bool equalAllowed = true)
        {
            if (snapShotTaken == false)
            {
                snapShotTaken = false;
            }
            model.EvaluateRelatedConstraints(this);
            TotalRelatedConflictsLast = model.TotalRelatedConflicts(this);
            if (TotalRelatedConflictsLast <= TotalRelatedConflictsSnapshot)
            {
                // check based on a full check:
                model.Evaluate(this);
                int total = model.TotalConflicts();
                if (total < model.TotalConflictsSnapshot || (equalAllowed && total == model.TotalConflictsSnapshot))
                {
                    model.TotalConflictsSnapshot = total;
                    model.stateNotSaved = true;
                    SnapShot(model);
                    return true;
                }
            }
            return false;
        }
        bool snapShotTaken = false;
        public void SnapShot(Model model)
        {
            if (snapShotTaken == true)
            {
                snapShotTaken = true;
            }
            snapShotTaken = true;
            //model.Evaluate(null);
            model.EvaluateRelatedConstraints(this);
            TotalRelatedConflictsSnapshot = model.TotalRelatedConflicts(this);
            resultTeams = new List<Team>(teams);
            resultWeeks = new List<MatchWeek>(weeks);
            resultMatches = CopyMatches(matches);
            minConflicts = conflict_cost;
        }
        public void CopyAndClearSnapShot(Model model)
        {
            if (snapShotTaken == false)
            {
                snapShotTaken = false;
            }
            snapShotTaken = false;
            teams = resultTeams;
            weeks = resultWeeks;
            matches = CopyMatches(resultMatches);
            snapShotTaken = false;
            model.Evaluate(null);
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
            if (optimizable)
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
            MakeDirty();
            if (serie.optimizableNumber)
            {
                //if (conflict_cost > 0)
                {
                    SnapShot(model);
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
                    CopyAndClearSnapShot(model);
                }
            }
        }

        public void OptimizeTeamAssignment(Model model, IProgress intf)
        {
            MakeDirty();
            if (optimizable)
            {
                //if (conflict_cost > 0)
                {
                    if (maxTeams <= 6)
                    {
                        OptimizeFullTeamAssignment(model, intf);
                    }
                    else // semi-optimisation.
                    {
                        if (intf.Cancelled() == false) OptimizeNumberOnAnalysis(model, intf);
                        for (int i = 0; i < maxTeams; i++)
                        {
                            intf.Progress(i, maxTeams);
                            for (int j = 0; j < maxTeams; j++)
                            {
                                if (j != i)
                                {
                                    Swap(teams, i, j);
                                    SnapShotIfImproved(model,false);
                                    Swap(teams, i, j);
                                }
                                if (intf.Cancelled()) break;
                            }
                            if (intf.Cancelled()) break;
                        }
                    }
                    teams = resultTeams;
                }
            }
        }

        public int[,] AnalyzeTeamAssignment(Model model, IProgress intf)
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
                    model.EvaluateRelatedConstraints(this);
                    int index = 0;
                    foreach (Team team in fixedOrderList)
                    {
                        score[index, (index + maxTeams - k) % maxTeams] = team.conflict;
                        index++;
                    }
                }
 /*               Console.Write("{0,30}", "Teams:");
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
  * */
                teams = resultTeams;
            }
            return score;
        }

        public void OptimizeNumberOnAnalysis(Model model, IProgress intf)
        {
            if (evaluated == false) // This optimization will not work otherwise since all own conflicts are zero
                return;
            SnapShot(model);
            int[,] score = AnalyzeTeamAssignment(model, intf);
            // Welk team eerst indelen: diegene die in het totaal het meeste conflicten heeft (of het verschil min/max grootste)
            List<Team> fixedOrderList = new List<Team>(teams);
            int[] sum = new int[fixedOrderList.Count];
            for (int index = 0; index < fixedOrderList.Count; index++)
            {
                sum[index] = 0;
                for (int index1 = 0; index1 < maxTeams; index1++)
                {
                    sum[index] += score[index, index1];
                }
            }
            fixedOrderList.Sort((Team t1, Team t2) => { 
                int i1 = teams.FindIndex(t => t==t1);
                int i2 = teams.FindIndex(t => t==t2);
                return sum[i2].CompareTo(sum[i1]);
            });
            // Op basis van huidige ordering
            int minScore = 0;
            for (int index = 0; index < fixedOrderList.Count; index++)
            {
                minScore += score[index, index];
            }

            // recalculate on new ordering
            teams = fixedOrderList;
            score = AnalyzeTeamAssignment(model, intf);

            // zo snel mogelijk de search afkappen (of toch een threshold toestaan)
            // score
            // teamOrder
            // weekOrder per team
            int[] teamIndex = new int[fixedOrderList.Count];
            for (int i = 0; i < fixedOrderList.Count; i++) teamIndex[i] = -1;
            bool[] selectedWeek1 = new bool[fixedOrderList.Count + 1];
            for (int i = 0; i <= fixedOrderList.Count; i++) selectedWeek1[i] = false;
            selectedWeek1[0] = true;
            teamIndex[0] = 0;
            int currentTeamNumber = 0;
            int totalScore = score[0,0];

            int cycle = 0;
            List<List<Team>>[] options = new List<List<Team>>[1000];
            for (int i = 0; i < 1000; i++) options[i] = new List<List<Team>>();
            while (true)
            {
                if (cycle++ % 1000 == 0)
                {
                    if (intf.Cancelled())
                        return;
                    intf.Progress(teamIndex[0], maxTeams);
                }
                if (currentTeamNumber >= maxTeams)
                {
                    // calculate current schema
                    if (totalScore < minScore + optimisationConflictThreshold)
                    {
                        Team[] reordered = new Team[fixedOrderList.Count];
                        for (int index = 0; index < fixedOrderList.Count; index++)
                        {
                            reordered[teamIndex[index]] = fixedOrderList[index];
                        }

                        //teams = reordered.ToList();

                        if (options[totalScore].Count < 1000)
                        {
                            options[totalScore].Add(reordered.ToList());
                        }

//                        SnapShot(model);
//                        if (SnapShotIfImproved(model, true))
                        
                        {
                            minScore = Math.Min(totalScore,minScore);
                        }
 
                    }
                    // prepare for next
                    currentTeamNumber--;

                    // next loop always ends, since last is always free
                    if (teamIndex[currentTeamNumber] >= 0)
                    {
                        selectedWeek1[teamIndex[currentTeamNumber]] = false;
                        totalScore -= score[currentTeamNumber, teamIndex[currentTeamNumber]];
                    }
                    do
                    {
                        teamIndex[currentTeamNumber]++;
                    } while (selectedWeek1[teamIndex[currentTeamNumber]] == true);
                    if (teamIndex[currentTeamNumber] < maxTeams)
                    {
                        totalScore += score[currentTeamNumber, teamIndex[currentTeamNumber]];
                        selectedWeek1[teamIndex[currentTeamNumber]] = true;
                    }
                }
                else
                {
                    if (teamIndex[currentTeamNumber] < maxTeams)
                    {
                        if (totalScore < minScore + optimisationConflictThreshold)
                            currentTeamNumber++;
                        if (currentTeamNumber < maxTeams)
                        {
                            // next loop always ends, since last is always free
                            if (teamIndex[currentTeamNumber] >= 0)
                            {
                                selectedWeek1[teamIndex[currentTeamNumber]] = false;
                                totalScore -= score[currentTeamNumber, teamIndex[currentTeamNumber]];
                            }
                            do
                            {
                                teamIndex[currentTeamNumber]++;
                            } while (selectedWeek1[teamIndex[currentTeamNumber]] == true);
                            if (teamIndex[currentTeamNumber] < maxTeams)
                            {
                                totalScore += score[currentTeamNumber, teamIndex[currentTeamNumber]];
                                selectedWeek1[teamIndex[currentTeamNumber]] = true;
                            }
                        }
                    }
                    else
                    {
                        selectedWeek1[teamIndex[currentTeamNumber]] = false;
                        totalScore -= score[currentTeamNumber, teamIndex[currentTeamNumber]];
                        teamIndex[currentTeamNumber] = -1;
                        currentTeamNumber--;
                        if (currentTeamNumber < 0) // alle zijn maximaal geweest, einde lus
                            break;

                        // next loop always ends, since last is always free
                        if (teamIndex[currentTeamNumber] >= 0)
                        {
                            selectedWeek1[teamIndex[currentTeamNumber]] = false;
                            totalScore -= score[currentTeamNumber, teamIndex[currentTeamNumber]];
                        }
                        do
                        {
                            teamIndex[currentTeamNumber]++;
                        } while (selectedWeek1[teamIndex[currentTeamNumber]] == true);
                        if (teamIndex[currentTeamNumber] < maxTeams)
                        {
                            totalScore += score[currentTeamNumber, teamIndex[currentTeamNumber]];
                            selectedWeek1[teamIndex[currentTeamNumber]] = true;
                        }
                    }
                }
            }
            int count = 0;
            for (int j = 0; j < 100; j++)
            {
                foreach (List<Team> list in options[j])
                {
                    if (count > 1000)
                        break;
                    teams = list;
                    SnapShotIfImproved(model, false);
                    //SnapShot(model);
                    //teams = resultTeams;
                    //return;
                    count++;
                    teams = fixedOrderList;
                }
            }
            teams = resultTeams;
        }
        
        public void OptimizeFullTeamAssignment(Model model, IProgress intf)
        {
            MakeDirty();
            if (optimizable)
            {
                //if (conflict_cost > 0)
                {
                    List<Team> temp = new List<Team>(teams);
                    teams = new List<Team>();
                    try
                    {
                        GenerateTeamCombination(model, temp, intf);
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
            if (optimizableHomeVisit)
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
            MakeDirty();
            if (optimizableHomeVisit)
            {
                foreach (Match match in matches)
                {
                    if (match.conflict_cost > compareTo)
                    {
                        SwitchHomeTeamVisitorTeam(model, match);
                        if (SnapShotIfImproved(model,false) == false)
                        { // switch back
                            SwitchHomeTeamVisitorTeam(model, match);
                        }
                        else 
                        {
                            changed = true;
                        }
                    }
                }
                if(changed) 
                    matches = CopyMatches(resultMatches);
            }
            return changed;
        }
        public void OptimizeHomeVisitorReverse(Model model, bool equalAllowed = true)
        {
            int compareTo = 0;
            if (equalAllowed) compareTo = -1;
            MakeDirty();
            if (optimizableHomeVisit)
            {
                List<Match> reverseMatches = new List<Match>(matches);
                reverseMatches.Reverse();
                foreach (Match match in reverseMatches)
                {
                    if (match.conflict_cost > compareTo)
                    {
                        SwitchHomeTeamVisitorTeam(model, match);
                        if (SnapShotIfImproved(model, false) == false)
                        { // switch back
                            SwitchHomeTeamVisitorTeam(model, match);
                        }
                    }
                }
                matches = CopyMatches(resultMatches);
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
            if (maxTeams != 6) return false;
            while(roundref < 2)
            {
                int round = roundref;
                UInt32[] days = new UInt32[5];
                for (int i = 0; i < 5; i++) days[i] = 0;
                UInt32[] bitPatterns = new UInt32[15];
                UInt32[] schema = new UInt32[15];
                List<Match> selectedMatches = matches.Where(m => m.Week.round == round).ToList();
                for (int i = 0; i < selectedMatches.Count; i++)
                {
                    bitPatterns[i] = (1u << selectedMatches[i].homeTeamIndex) | (1u << selectedMatches[i].visitorTeamIndex);
                }
                List<List<UInt32>> schemas = new List<List<uint>>();
                RecursiveFullSchema(model, ref days, ref bitPatterns, ref schema, ref schemas, selectedMatches.Count - 1);
                Console.WriteLine("Counted {0} schemas", schemas.Count);
                List<int> indexes = new List<int>();
                foreach (var m in selectedMatches)
                {
                    if (indexes.Contains(m.weekIndex) == false)
                    {
                        indexes.Add(m.weekIndex);
                    }
                }
                while(count < schemas.Count)
                {
                    var sch = schemas[count];
                    count++;
                    intf.Progress(count, schemas.Count * 2);
                    if (intf.Cancelled())
                    {
                        matches = CopyMatches(resultMatches);
                        return false;
                    }

                    //List<Match> selectedMatches1 = matches.Where(m => m.Week.round == round).ToList();
                    for (int i = 0; i < selectedMatches.Count; i++)
                    {
                        Match m = selectedMatches[i];
                        m.weekIndex = indexes[(int)sch[i]];
                    }
                    SnapShotIfImproved(model,false);
                    if (TotalRelatedConflictsLast < TotalRelatedConflictsSnapshot + optimisationThreshold) // kans nog aanwezig op goede score
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
                for (UInt32 d = 0; d < 5; d++)
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
                List<UInt32> newSchema = new List<uint>();
                for (int i = 0; i < 15; i++)
                {
                    newSchema.Add(schema[i]);
                }
                schemas.Add(newSchema);
            }
         }

        public bool OptimizeSchema(Model model, IProgress intf, int setSize, int optimizationLevel)
        {
            if (maxTeams > 6)
            {
                MakeDirty();
                if (optimizableWeeks)
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
                            matches = CopyMatches(resultMatches);
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
                                            int tempTotalConflicts = model.TotalConflictsSnapshot;
                                            SwitchMatches(index1, matches1, index2, matches2);
                                            SnapShotIfImproved(model, false);
                                            if (TotalRelatedConflictsLast < TotalRelatedConflictsSnapshot + optimisationThreshold)
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
                MakeDirty();
                if (optimizableWeeks)
                {
                    int matchCount = 0;
                    for (int index = 0; index < weeks.Count; index++)
                    {
                        int round = weeks[index].round;
                        intf.Progress(index, weeks.Count);
                        if (intf.Cancelled())
                        {
                            matches = CopyMatches(resultMatches);
                            return false;
                        }
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
                                                int tempTotalConflicts = model.TotalConflictsSnapshot;
                                                match1.weekIndex = index2;
                                                match2.weekIndex = index2;
                                                match3.weekIndex = index2;
                                                match_1.weekIndex = index;
                                                match_2.weekIndex = index;
                                                match_3.weekIndex = index;
                                                SnapShotIfImproved(model,false);
                                                if (TotalRelatedConflictsLast < TotalRelatedConflictsSnapshot + optimisationThreshold)
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
