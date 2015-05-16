using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;


namespace CompetitionCreator
{
    //bla
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
        public List<Team> GetGroupX()
        {
            return teams.FindAll(t => t.group == TeamGroups.GroupX);
        }
        public List<Team> GetGroupY()
        {
            return teams.FindAll(t => t.group == TeamGroups.GroupY);
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
                if (EvaluatedTeamCount == 0) return 0;
                return (conflict * 100) / EvaluatedTeamCount; 
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
