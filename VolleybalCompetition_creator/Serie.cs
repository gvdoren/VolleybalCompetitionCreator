using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeifenLuo.WinFormsUI.Docking;

namespace VolleybalCompetition_creator
{
    public class Serie:ConstraintAdmin
    {
        public string name { get; set; }
        public Dictionary<string, Poule> poules = new Dictionary<string, Poule>();
        public Serie(string name) { this.name = name; }
        List<Constraint> conflictConstraints = new List<Constraint>();
    }
    

}
