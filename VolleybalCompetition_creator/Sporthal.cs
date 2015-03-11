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
            if (sporthal != null)
            {
                if (distance.ContainsKey(sporthal.id)) return distance[sporthal.id];
                else throw new Exception(string.Format("No distance info available from {0} to {1}", name, sporthal.name));
            }
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
                return sporthall.name;
            }
        }
        public int id 
        { 
            get
            {
                return sporthall.id;
            } 
        }
        public double lat { get { return sporthall.lat; } }
        public double lng { get { return sporthall.lng; } }
        public List<DateTime> NotAvailable = new List<DateTime>();
        public override string ToString()
        {
            return name;
        }
        public SporthallClub(Sporthal sporthall)
        {
            this.sporthall = sporthall;

        }
    }
}
