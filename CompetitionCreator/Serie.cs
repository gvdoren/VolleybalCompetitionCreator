using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeifenLuo.WinFormsUI.Docking;

namespace CompetitionCreator
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
        Model klvv = null;
        public string name { get; set; }
        public int id { get; set; }
        public List<Poule> poules = new List<Poule>();
        public enum ImportanceLevels {  High, Medium, Low };
        ImportanceLevels _importance;
        public ImportanceLevels importance 
        { 
            get {
                return _importance;
            }
            set { _importance = value; klvv.Changed(); klvv.Evaluate(null); }
        }
        public bool Nationaal = false;
        
        public Serie(int id, string name, Model klvv) 
        { 
            this.klvv = klvv;
            this.id = id; 
            this.name = name;
            optimizableNumber = true;
            optimizableHomeVisit = false;
            optimizableWeekends = false;
            extraTimeBefore = 0;
            importance = ImportanceLevels.Medium;
            // geen reserve match
        }
        
        public List<Team> teams {
            get { return klvv.teams.FindAll(t => t.serie == this && t.deleted == false); }
        }
        //private bool _optimizable;
        //private bool _weekOptimizable;
        //private bool _homeVisitOptimizable;
        public bool optimizableNumber;// { get { return _optimizable & evaluated & imported == false; } set { _optimizable = value; } }
        public bool optimizableWeekends;// { get { return optimizable && _weekOptimizable; } set { _weekOptimizable = value; } }
        public bool optimizableHomeVisit;// { get { return optimizable && _homeVisitOptimizable; } set { _homeVisitOptimizable = value; } }
        public double extraTimeBefore { get; set; }
    }
    

}
