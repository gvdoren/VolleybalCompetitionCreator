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
        Model model = null;
        public string name { get; set; }
        public int id { get; set; }
        public bool evaluated { get; set; }
        public List<Poule> poules = new List<Poule>();
        public bool Nationaal = false;
        
        public Serie(int id, string name, Model model) 
        { 
            this.model = model;
            this.id = id; 
            this.name = name;
            extraTimeBefore = 0;
            evaluated = true;
            // geen reserve match
        }
        
        public List<Team> teams {
            get { return model.teams.FindAll(t => t.serie == this && t.deleted == false); }
        }
        public double extraTimeBefore { get; set; }
    }
    

}
