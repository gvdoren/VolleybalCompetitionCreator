﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CompetitionCreator
{
    public class SchemaMatch
    {
        public int team1;
        public int team2;
        public SchemaMatch(int team1, int team2)
        {
            this.team1 = team1;
            this.team2 = team2;
        }
    }
    public class SchemaWeek
    {
        public SchemaWeek(int nr) { WeekNr = nr; }
        public int WeekNr;
        public int round;
        public List<SchemaMatch> matches = new List<SchemaMatch>();
    }
    public class Schema
    {
        public SortedDictionary<int, SchemaWeek> weeks = new SortedDictionary<int, SchemaWeek>();
        public int teamCount = 0;
        public string name;
        SchemaWeek week(int i) { return weeks[i]; }
        public void Read(string fileName)
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(fileName);
                char[] delimiters = new char[] { ',', ';' };
                foreach (string line in lines)
                {
                    string[] var = line.Split(delimiters);
                    if (var.Length == 3)
                    {
                        int weekNr = int.Parse(var[0]) - 1; // internal administration starts at 0, schema files start at 1
                        if (weeks.ContainsKey(weekNr) == false)
                        {
                            weeks.Add(weekNr, new SchemaWeek(weekNr));
                        }
                        int team1 = int.Parse(var[1]) - 1;
                        int team2 = int.Parse(var[2]) - 1;
                        weeks[weekNr].matches.Add(new SchemaMatch(team1, team2));
                    }
                    else if(var.Length == 4) //including round
                    {
                        int round = int.Parse(var[0]) - 1;
                        int weekNr = int.Parse(var[1]) - 1; // internal administration starts at 0, schema files start at 1
                        if (weeks.ContainsKey(weekNr) == false)
                        {
                            weeks.Add(weekNr, new SchemaWeek(weekNr));
                        }
                        int team1 = int.Parse(var[2]) - 1;
                        int team2 = int.Parse(var[3]) - 1;
                        weeks[weekNr].matches.Add(new SchemaMatch(team1, team2));
                        weeks[weekNr].round = round;
                    }
                }
                teamCount = weeks[0].matches.Count*2;
                FileInfo fi = new FileInfo(fileName);
                name = fi.Name;
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Error in schema :"+ fileName);
            }

        }

    }
}
