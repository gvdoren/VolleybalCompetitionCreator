using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompetitionCreator
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

    public class Period
    {
        public DateTime From;
        public DateTime Until;
        public Period(DateTime from, DateTime until)
        {
            From = from;
            Until = until;
        }
        public bool InPeriod(DateTime time)
        {
            return time >= From && time < Until;
        }
    }
    public class SporthallAvailability
    {
        public Sporthal sporthall;
        public Team team = null;
        public int teamId = -1;
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
        public List<Period> NotAvailable = new List<Period>();
        public List<Field> fields { get; set; }
        public override string ToString()
        {
            return name;
        }
        public SporthallAvailability(Sporthal sporthall)
        {
            this.sporthall = sporthall;
            this.fields = new List<Field>();
        }
    }
}
