using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VolleybalCompetition_creator
{
    public class Sporthal
    {
        public string name { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public int id { get; set; }
        public SortedList<int, int> distance = new SortedList<int, int>();
        public Sporthal(int id, string name)
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
            if (sporthal != null && distance.ContainsKey(sporthal.id)) return distance[sporthal.id];
            return 0;
        }
    }

    public class SporthallClub
    {
        public Sporthal sporthall;
        public string name 
        { 
            get
            {
                if (duplicate) return sporthall.name + "_2";
                else return sporthall.name;
            }
        }
        public int id 
        { 
            get
            {
                if (duplicate) return sporthall.id + 1000000;
                else return sporthall.id;
            } 
        }
        public double lat { get { return sporthall.lat; } }
        public double lng { get { return sporthall.lng; } }
        public List<DateTime> NotAvailable = new List<DateTime>();
        public bool duplicate { get; set; }
        public override string ToString()
        {
            return name;
        }
        public SporthallClub(Sporthal sporthall, bool duplicate = false)
        {
            this.sporthall = sporthall;
            this.duplicate = duplicate;

        }
    }
}
