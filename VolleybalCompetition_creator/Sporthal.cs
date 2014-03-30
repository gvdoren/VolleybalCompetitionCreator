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
        public List<DateTime> NotAvailable = new List<DateTime>();
        public SortedList<int, int> distance = new SortedList<int, int>();
        public Sporthal(int id, string name,Club club)
        {
            this.name = name;
            this.id = id;
        }
        public override string ToString()
        {
            return name;
        }
        public int Distance(Sporthal sporthal)
        {
            if (distance.ContainsKey(sporthal.id)) return distance[sporthal.id];
            return 0;
        }
    }
}
