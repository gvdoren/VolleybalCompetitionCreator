﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeifenLuo.WinFormsUI.Docking;

namespace VolleybalCompetition_creator
{
    public class Serie:ConstraintAdmin
    {
        private bool reserve = false;
        public string name { get; set; }
        public int id { get; set; }
        public Dictionary<string, Poule> poules = new Dictionary<string, Poule>();
        public bool Volwassenen { get { return name == "1D" || name == "2D" || name == "3D" || name == "4D" || name == "1H" || name == "2H" || name == "3H" || name == "2H_ZR"; } }
        public bool Gewestelijk { get { return false == (name == "Nationaal" || name == "1D" || name == "2D" || name == "3D" || name == "4D" || name == "1H" || name == "2H" || name == "3H" || name == "2H_ZR" || name == "PSD"); } }
        public Serie(int id, string name) 
        { 
            this.id = id; 
            this.name = name; 
            optimizable = true; 
            constraintsHold = true;
            weekOrderChangeAllowed = Gewestelijk;
            homeVisitChangeAllowed = Gewestelijk;
            reserve = (name == "1H" || name == "2H" || name == "1D" || name == "2D" || name == "Nationaal");
        }
        public List<Team> teams = new List<Team>();
        public bool optimizable { get; set; }
        public bool constraintsHold { get; set; }
        public bool weekOrderChangeAllowed { get; set; }
        public bool homeVisitChangeAllowed { get; set; }
        public bool Reserve()
        {
            return reserve;
        }
    }
    

}
