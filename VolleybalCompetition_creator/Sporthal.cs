using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VolleybalCompetition_creator
{
    public class Sporthal
    {
        public string name { get; set; }
        public int id { get; set; }
        public Club club { get; set; }
        public List<DateTime> NotAvailable = new List<DateTime>();
        public Sporthal(int id, string name,Club club)
        {
            this.name = name;
            this.id = id;
            this.club = club;
        }
        public override string ToString()
        {
            return name;
        }
    }
}
