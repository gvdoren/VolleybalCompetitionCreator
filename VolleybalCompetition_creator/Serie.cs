using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeifenLuo.WinFormsUI.Docking;

namespace VolleybalCompetition_creator
{
    public class Serie:ConstraintAdmin
    {
        private bool reserve = false;
        public bool imported
        {
            get
            {
                foreach (Poule poule in poules.Values)
                {
                    if (poule.imported == true) return true;
                }
                return false;
            }
        }
        public string name { get; set; }
        public int id { get; set; }
        public Dictionary<string, Poule> poules = new Dictionary<string, Poule>();
        public bool Nationaal = false;
        public bool Provinciaal { get { return name == "1D" || name == "2D" || name == "3D" || name == "4D" || name == "1H" || name == "2H" || name == "3H" || name == "2H_ZR" || name == "PSD"; } }
        public bool Gewestelijk { get { return Nationaal == false && Provinciaal == false; } }
        public Serie(int id, string name) 
        { 
            this.id = id; 
            this.name = name;
            _optimizable = true; 
            constraintsHold = true;
            weekOrderChangeAllowed = Gewestelijk;
            homeVisitChangeAllowed = Gewestelijk;
            reserve = (name == "1H" || name == "2H" || name == "1D" || name == "2D" || Nationaal);
        }
        public List<Team> teams = new List<Team>();
        public bool AddTeam(Team team)
        {
            if (teams.Contains(team) == false)
            {
                if (team.serie != null) team.serie.RemoveTeam(team);
                teams.Add(team);
                team.serie = this;
                return true;
            }
            return false;
        }
        public bool RemoveTeam(Team team)
        {
            teams.Remove(team);
            team.serie = null;
            return true;
        }
        private bool _optimizable;
        public bool optimizable { get { return _optimizable & constraintsHold & imported == false; } set { _optimizable = value; } }
        public bool constraintsHold { get; set; }
        public bool weekOrderChangeAllowed { get; set; }
        public bool homeVisitChangeAllowed { get; set; }
        public bool Reserve()
        {
            return reserve;
        }
    }
    

}
