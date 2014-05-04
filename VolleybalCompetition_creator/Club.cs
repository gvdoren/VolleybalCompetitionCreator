using System;
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
