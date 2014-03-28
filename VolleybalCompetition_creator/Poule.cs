using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VolleybalCompetition_creator
{
    public class Poule: ConstraintAdmin
    {
        public Serie serie = null;
        public string name { get; set; }
        public List<Team> teams = new List<Team>();
        public Poule(string name) { this.name = name; }
        public List<Weekend> weekends = new List<Weekend>();
        public void AddWeekend(int year, int weekNr)
        {
            int index = weekends.FindIndex(w => w.WeekNr == weekNr && w.Year == year);
            if (index < 0)
            {
                weekends.Add(new Weekend(year, weekNr));
            }
        }
        public int FindWeekendNrInSchema(int year, int weekNr)
        {
            int index = weekends.FindIndex(w => w.WeekNr == weekNr && w.Year == year);
            return index;
        }
        public List<Match> matches = new List<Match>();
        public void OptimizeWeekends(Klvv klvv, IProgress intf)
        {
            if (serie.optimizable)
            {
                if (conflict > 0)
                {
                    if (weekends.Count > 12)
                    {
                        List<Weekend> result = new List<Weekend>(weekends);
                        int minConflict = conflict;
                        for (int i = 0; i < weekends.Count / 2; i++)
                        {
                            intf.Progress(i, weekends.Count);
                            for (int j = 0; j < weekends.Count / 2; j++)
                            {
                                Swap(weekends, i, j);
                                klvv.Evaluate(this);
                                if (conflict < minConflict)
                                {
                                    result = new List<Weekend>(weekends);
                                    minConflict = conflict;
                                }
                                Swap(weekends, i, j);
                            }
                            weekends = new List<Weekend>(result);
                        }
                        for (int i = weekends.Count / 2; i < weekends.Count; i++)
                        {
                            intf.Progress(i, weekends.Count);
                            for (int j = weekends.Count / 2; j < weekends.Count; j++)
                            {
                                Swap(weekends, i, j);
                                klvv.Evaluate(this);
                                if (conflict < minConflict)
                                {
                                    result = new List<Weekend>(weekends);
                                    minConflict = conflict;
                                }
                                Swap(weekends, i, j);
                            }
                            weekends = new List<Weekend>(result);
                        }
                    }
                    else // full optimalisation
                    {
                        klvv.Evaluate(null);
                        int conflictBefore = conflict;
                        List<Weekend> firstHalf = new List<Weekend>();
                        List<Weekend> lastHalf = new List<Weekend>();
                        for (int i = 0; i < weekends.Count / 2; i++)
                        {
                            firstHalf.Add(weekends[i]);
                        }
                        for (int i = weekends.Count / 2; i < weekends.Count; i++)
                        {
                            lastHalf.Add(weekends[i]);
                        }
                        List<Weekend> result = new List<Weekend>();
                        List<Weekend> temp = new List<Weekend>(weekends);
                        weekends = new List<Weekend>();
                        // First optimize first half
                        try
                        {
                            List<Weekend> remaining = new List<Weekend>(firstHalf);
                            List<Weekend> best = null;
                            int minConflicts = int.MaxValue;
                            GenerateWeekendCombination(klvv, new List<Weekend>(), lastHalf, remaining, ref minConflicts, ref best, intf);
                            if (best != null) firstHalf = best;
                            remaining = new List<Weekend>(lastHalf);
                            best = null;
                            minConflicts = int.MaxValue;
                            GenerateWeekendCombination(klvv, firstHalf, new List<Weekend>(), remaining, ref minConflicts, ref best, intf);
                            if (best != null) lastHalf = best;
                        }
                        catch { }
                        weekends = firstHalf;
                        weekends.AddRange(lastHalf);
                        //if (best != null) weekends = best; else weekends = temp;


                    }
                    klvv.Evaluate(null);
                }
            }
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
        private void GenerateWeekendCombination(Klvv klvv, List<Weekend> firstHalf, List<Weekend> lastHalf, List<Weekend> remainingWeekends, ref int minConflicts, ref List<Weekend> best, IProgress intf)
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
                    GenerateWeekendCombination(klvv, firstHalf, lastHalf, remainingWeekends, ref minConflicts, ref best, intf);
                    remainingWeekends.Add(weekends[weekends.Count - 1]);
                    weekends.RemoveAt(weekends.Count - 1);
                }
            }
            else
            {
                weekends.InsertRange(0, firstHalf);
                weekends.AddRange(lastHalf);
                klvv.Evaluate(this);
                weekends.RemoveRange(0, firstHalf.Count);
                weekends.RemoveRange(weekends.Count - lastHalf.Count, lastHalf.Count);
                if (conflict < minConflicts)
                {
                    minConflicts = conflict;
                    Console.WriteLine("Min conflict: {0}", minConflicts);
                    best = new List<Weekend>(weekends);
                }
                if (intf.Cancelled())
                {
                    throw new Exception("Cancelled");
                }
            }

        }
        public void OptimizeTeam(Klvv klvv, IProgress intf,Team team)
        {
            if (serie.optimizable)
            {
                if (conflict > 0)
                {
                    List<Team> resultTeams = new List<Team>(teams);
                    List<Weekend> resultWeekends = new List<Weekend>(weekends);
                    int minConflict = conflict;
                    int teamindex = teams.FindIndex(t => t == team);
                    for (int i = 0; i < teams.Count; i++)
                    {
                        if (i != teamindex)
                        {
                            intf.Progress(i, teams.Count);
                            Swap(teams, i, teamindex);
                            klvv.Evaluate(this);
                            OptimizeWeekends(klvv, intf);
                            OptimizeHomeVisitor(klvv);
                            if (conflict < minConflict)
                            {
                                resultTeams = new List<Team>(teams);
                                resultWeekends = new List<Weekend>(weekends);
                                minConflict = conflict;
                            }
                            Swap(teams, i, teamindex); // swap back
                        }
                    }
                    teams = new List<Team>(resultTeams);
                    weekends = new List<Weekend>(resultWeekends);
                    klvv.Evaluate(this);
                    OptimizeHomeVisitor(klvv);
                    klvv.Evaluate(null);
                }
            }
        }

        public void OptimizeTeamAssignment(Klvv klvv, IProgress intf)
        {
            if (serie.optimizable)
            {
                if (conflict > 0)
                {
                    if (teams.Count <= 7)
                    {
                        klvv.Evaluate(null);
                        int conflictBefore = conflict;
                        /*int conflict = 0;
                        foreach (Team team in teams)
                        {
                            conflict += team.conflict;
                        }*/
                        // iterate on all solutions and evaluate?
                        int minConflicts = int.MaxValue;
                        List<Team> result = new List<Team>();
                        List<Team> temp = new List<Team>(teams);
                        teams = new List<Team>();
                        List<Team> best = null;
                        try
                        {
                            GenerateTeamCombination(klvv, temp, ref minConflicts, ref best, intf);
                        }
                        catch { }
                        if (best != null) teams = best; else teams = temp;
                    }
                    else // semi-optimisation.
                    {
                        List<Team> result = new List<Team>(teams);
                        int minConflict = conflict;
                        for (int i = 0; i < teams.Count; i++)
                        {
                            intf.Progress(i, teams.Count);
                            for (int j = 0; j < teams.Count; j++)
                            {
                                Swap(teams, i, j);
                                klvv.Evaluate(this);
                                if (conflict < minConflict)
                                {
                                    result = new List<Team>(teams);
                                    minConflict = conflict;
                                }
                                Swap(teams, i, j);
                            }
                            teams = new List<Team>(result);
                        }
                        klvv.Evaluate(null);
                    }
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
        private void GenerateTeamCombination(Klvv klvv, List<Team> remainingTeams, ref int minConflicts, ref List<Team> best, IProgress intf)
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
                    GenerateTeamCombination(klvv, remainingTeams, ref minConflicts, ref best,intf);
                    remainingTeams.Add(teams[teams.Count - 1]);
                    teams.RemoveAt(teams.Count - 1);
                }
            }
            else
            {
                klvv.Evaluate(this);
                if (conflict < minConflicts)
                {
                    minConflicts = conflict;
                    Console.WriteLine("Min conflict: {0}", minConflicts);
                    best = new List<Team>(teams);
                }
                if (intf.Cancelled())
                {
                    throw new Exception("Cancelled");
                }
            }

        }
        public void SwitchHomeTeamVisitorTeam(Match match1)
        {
            if (serie.optimizable)
            {
                Match match2 = null;
                foreach (Match m in matches)
                {
                    if (m.homeTeam == match1.visitorTeam && m.visitorTeam == match1.homeTeam) match2 = m;
                }
                // swap teams
                match1.homeTeamIndex = match2.homeTeamIndex;
                match1.visitorTeamIndex = match2.visitorTeamIndex;
                match2.homeTeamIndex = match1.visitorTeamIndex;
                match2.visitorTeamIndex = match1.homeTeamIndex;
            }
        }
        public void OptimizeHomeVisitor(Klvv klvv)
        {
            if (serie.optimizable)
            {
                foreach (Match match in matches)
                {
                    if (match.conflict > 0)
                    {
                        int before = conflict;
                        SwitchHomeTeamVisitorTeam(match);
                        klvv.Evaluate(this);
                        int after = conflict;
                        if (before <= after)
                        { // switch back
                            SwitchHomeTeamVisitorTeam(match);
                            klvv.Evaluate(this);
                        }
                    }
                }
                klvv.Evaluate(null);
            }
        }
    }
}
