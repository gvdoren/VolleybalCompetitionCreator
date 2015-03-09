using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeifenLuo.WinFormsUI.Docking;

namespace VolleybalCompetition_creator
{
    public class Serie:ConstraintAdmin
    {
        public bool imported
        {
            get
            {
                foreach (Poule poule in poules)
                {
                    if (poule.imported == true) return true;
                }
                return false;
            }
        }
        Klvv klvv = null;
        public string name { get; set; }
        public int id { get; set; }
        public List<Poule> poules = new List<Poule>();
        public bool Nationaal = false;
        public bool Provinciaal { get { return name == "1D" || name == "2D" || name == "3D" || name == "4D" || name == "1H" || name == "2H" || name == "3H" || name == "2H_ZR" || name == "PSD"; } }
        public bool Gewestelijk { get { return Nationaal == false && Provinciaal == false; } }
        
        public Serie(int id, string name, Klvv klvv) 
        { 
            this.klvv = klvv;
            this.id = id; 
            this.name = name;
            optimizable = true; 
            evaluated = true;
            extraTimeBefore = 0; // geen reserve match
            weekOrderChangeAllowed = Gewestelijk;
            homeVisitChangeAllowed = Gewestelijk;
        }
        
        public List<Team> teams {
            get { return klvv.teams.FindAll(t => t.serie == this && t.deleted == false); }
        }
        private bool _optimizable;
        private bool _weekOptimizable;
        private bool _homeVisitOptimizable;
        public bool optimizable { get { return _optimizable & evaluated & imported == false; } set { _optimizable = value; } }
        public bool weekOrderChangeAllowed { get { return optimizable && _weekOptimizable; } set { _weekOptimizable = value; } }
        public bool homeVisitChangeAllowed { get { return optimizable && _homeVisitOptimizable; } set { _homeVisitOptimizable = value; } }
        public bool evaluated { get; set; }
        public double extraTimeBefore { get; set; }
    }
    

}
