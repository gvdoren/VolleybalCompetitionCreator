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
        public List<Match> matches = new List<Match>();
        public bool OptimizeTeamAssignment(Klvv klvv, IProgress intf)
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
                        GenerateCombination(klvv, temp, ref minConflicts, ref best, intf);
                    }
                    catch { }
                    if (best != null) teams = best; else teams = temp;
                    OptimizeHomeVisitor(klvv);
                    return minConflicts < conflictBefore;
                }
            }
            return false;
        }
        private void GenerateCombination(Klvv klvv, List<Team> remainingTeams, ref int minConflicts, ref List<Team> best, IProgress intf)
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
                    GenerateCombination(klvv, remainingTeams, ref minConflicts, ref best,intf);
                    remainingTeams.Add(teams[teams.Count - 1]);
                    teams.RemoveAt(teams.Count - 1);
                }
            }
            else
            {
                klvv.Evaluate(this);
                int conflict = 0;
                foreach (Team team in teams)
                {
                    conflict += team.conflict;
                }
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
        public void OptimizeHomeVisitor(Klvv klvv)
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
            klvv.Changed();
        }
    }
}
