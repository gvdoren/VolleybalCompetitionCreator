using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;


namespace CompetitionCreator
{
    //{"Id":"11","Name":"Avoc Achel","LogoId":"11"}
    public class Club: ConstraintAdmin
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
                foreach(Team t in teams)
                {
                    if(t.poule != null)
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
        public bool Dirty = true;
        public bool PerWeek = false;
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
    }
}
