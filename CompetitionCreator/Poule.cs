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
        public bool optimizableMatch { get { return serie != null && serie.optimizableMatch && imported == false; } }

        public bool evaluated { get { return imported == false; } }
        List<Team> resultTeams = new List<Team>();
        List<Weekend> resultWeekendsFirst = new List<Weekend>();
        List<Weekend> resultWeekendsSecond = new List<Weekend>();
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
        public List<Weekend> weekendsFirst = new List<Weekend>();
        public List<Weekend> weekendsSecond = new List<Weekend>();
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
        
        public int FindWeekendNrInSchema(int year, int weekNr)
        {
            int index = weekendsFirst.FindIndex(w => w.WeekNr == weekNr && w.Year == year);
            if(index<0) index = weekendsSecond.FindIndex(w => w.WeekNr == weekNr && w.Year == year);
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

        public void CalculateRelatedConstraints(Model model)
        {
            relatedConstraints = new List<Constraint>();
            //foreach (Constraint con in model.constraints)
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
            foreach (Constraint con in model.constraints)
            {
                if (clubs.Contains(con.club) || 
                    con.poule == this || 
                    teams.Contains(con.team)  
                    ) relatedConstraints.Add(con);
            }

        }

        public void OptimizeWeekends(Model model, IProgress intf)
        {
            MakeDirty();
            if (optimizableWeekends)
            {
                //SnapShot(model);
                if (conflict_cost > 0)
                {
                    if (weekendsFirst.Count > 5)//12)
                    {
                        List<Weekend> result = new List<Weekend>(weekendsFirst);
                        for (int i = 0; i < weekendsFirst.Count; i++)
                        {
                            intf.Progress(i, (weekendsFirst.Count + weekendsSecond.Count));
                            for (int j = i + 1; j < weekendsFirst.Count; j++)
                            {
                                Swap(weekendsFirst, i, j);
                                SnapShotIfImproved(model);
                                Swap(weekendsFirst, i, j);
                            }
                            weekendsFirst = resultWeekendsFirst;
                        }
                        result = new List<Weekend>(weekendsSecond);
                        for (int i = 0; i < weekendsSecond.Count; i++)
                        {
                            intf.Progress(i, (weekendsFirst.Count + weekendsSecond.Count));
                            for (int j = i + 1; j < weekendsSecond.Count; j++)
                            {
                                Swap(weekendsSecond, i, j);
                                SnapShotIfImproved(model);
                                Swap(weekendsSecond, i, j);
                            }
                            weekendsSecond = resultWeekendsSecond;
                        }
                    }
                    else // full optimalisation
                    {
                        // First optimize first half
                        try
                        {
                            List<Weekend> remaining = weekendsFirst;
                            weekendsFirst = new List<Weekend>();
                            GenerateWeekendCombination(model, ref weekendsFirst, remaining, intf);
                            weekendsFirst = resultWeekendsFirst;
                            
                            remaining = weekendsSecond;
                            weekendsSecond = new List<Weekend>();
                            GenerateWeekendCombination(model, ref weekendsSecond, remaining, intf);
                            weekendsSecond = resultWeekendsSecond;
                        }
                        catch { }
                    }
                }
            }
        }
        public bool SnapShotIfImproved(Model model, bool equalAllowed = true)
        {
            bool snapshot = false;
            model.EvaluateRelatedConstraints(this);
            int total = model.TotalConflicts();
            if (total < model.LastTotalConflicts || (equalAllowed && total == model.LastTotalConflicts))
            {
                snapshot = true;
                model.LastTotalConflicts = total;
            }

            if(snapshot)
            {
                resultTeams = new List<Team>(teams);
                resultWeekendsFirst = new List<Weekend>(weekendsFirst);
                resultWeekendsSecond = new List<Weekend>(weekendsSecond);
                resultMatches = CopyMatches();
                minConflicts = conflict_cost;
                model.stateNotSaved = true; 
            }
            return snapshot;
        }
        bool snapShotTaken = false;
        public void SnapShot(Model model)
        {
            if (snapShotTaken == true)
            {
                snapShotTaken = true;
            }
            snapShotTaken = true;
            model.Evaluate(null);
            resultTeams = new List<Team>(teams);
            resultWeekendsFirst = new List<Weekend>(weekendsFirst);
            resultWeekendsSecond = new List<Weekend>(weekendsSecond);
            resultMatches = CopyMatches();
            minConflicts = conflict_cost;
        }
        public void CopyAndClearSnapShot(Model model)
        {
            teams = resultTeams;
            weekendsFirst = resultWeekendsFirst;
            weekendsSecond = resultWeekendsSecond;
            matches = resultMatches;
            snapShotTaken = false;
            model.Evaluate(null);
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
        private void GenerateWeekendCombination(Model model, ref List<Weekend> firstHalf, List<Weekend> remainingWeekends, IProgress intf)
        {
            if (remainingWeekends.Count > 0)
            {
                for (int i = 0; i < remainingWeekends.Count; i++)
                {
                    if (firstHalf.Count == 0)
                    {
                        intf.Progress(i, remainingWeekends.Count - 1);
                    }
                    firstHalf.Add(remainingWeekends[0]);
                    remainingWeekends.RemoveAt(0);
                    GenerateWeekendCombination(model, ref firstHalf, remainingWeekends, intf);
                    remainingWeekends.Add(firstHalf[firstHalf.Count - 1]);
                    firstHalf.RemoveAt(firstHalf.Count - 1);
                }
            }
            else
            {
                SnapShotIfImproved(model);
                if (intf.Cancelled())
                {
                    throw new Exception("Cancelled");
                }
            }

        }
        public void OptimizeTeam(Model model, IProgress intf,Team team)
        {
            MakeDirty();
            if (serie.optimizableNumber)
            {
                if (conflict_cost > 0)
                {
                    SnapShot(model);
                    int teamindex = teams.FindIndex(t => t == team);
                    for (int i = 0; i < maxTeams; i++)
                    {
                        if (i != teamindex)
                        {
                            intf.Progress(i, teams.Count);
                            Swap(teams, i, teamindex);
                            OptimizeWeekends(model, intf);
                            OptimizeHomeVisitor(model);
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
                if (conflict_cost > 0)
                {
                    if (maxTeams <= 6)
                    {
                        OptimizeFullTeamAssignment(model, intf);
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
                                    SnapShotIfImproved(model);
                                    Swap(teams, i, j);
                                }
                            }
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
            }
            return score;
        }
        public void OptimizeIndividualMatches(Model model)
        {
            MakeDirty();
            if (optimizableMatch)
            {
                // Only meaningfull when having spare weekends
                if (weekendsFirst.Count + weekendsSecond.Count > 2 * (TeamCount - 1))
                {
                    foreach (Match m in matches)
                    {
                        m.OptimizeIndividual(model);
                    }
                    matches = resultMatches;
                }
            }
        }

        public void OptimizeFullTeamAssignment(Model model, IProgress intf)
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
                SnapShotIfImproved(model);
                if (intf.Cancelled())
                {
                    throw new Exception("Cancelled");
                }
            }

        }
        public void SwitchHomeTeamVisitorTeam(Model model, Match match)
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


        public void OptimizeHomeVisitor(Model model)
        {
            MakeDirty();
            if (optimizableHomeVisit)
            {
                foreach (Match match in matches)
                {
                    if (match.conflict_cost > 0)
                    {
                        SwitchHomeTeamVisitorTeam(model, match);
                        if (SnapShotIfImproved(model) == false)
                        { // switch back
                            SwitchHomeTeamVisitorTeam(model, match);
                        }
                    }
                }
                matches = resultMatches;
            }
        }
        public void OptimizeHomeVisitorReverse(Model model)
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
                        SwitchHomeTeamVisitorTeam(model, match);
                        if (SnapShotIfImproved(model) == false)
                        { // switch back
                            SwitchHomeTeamVisitorTeam(model, match);
                        }
                    }
                }
                matches = resultMatches;
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
