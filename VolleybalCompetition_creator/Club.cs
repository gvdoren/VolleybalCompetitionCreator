﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;


namespace VolleybalCompetition_creator
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
        public bool RemoveTeam(Team team)
        {
            teams.Remove(team);
            team.club = null;
            return true;
        }
        public int percentage
        {
            get { if (teams.Count == 0) return 0;
                  return (conflict * 100) / teams.Count; }
        }
        public List<SporthallClub> sporthalls = new List<SporthallClub>();
        public Club groupingWithClub = null;
        public string FreeFormatConstraints = "";
        public int Id { get; set; }
        public string name { get; set; }
        public bool ConstraintAllInOneWeekend = false;
        public bool ConstraintNotAtTheSameTime = false;
        public bool Dirty = true;
        public static Club CreateNullClub()
        {
            Club club = new Club();
            club.name = "----";
            return club;
        }
        public Club()
        {
        }
        public override string ToString()
        {
            return name;
        }
    }
}
