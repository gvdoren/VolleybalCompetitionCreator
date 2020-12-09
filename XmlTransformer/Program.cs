using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Globalization;
using SiteImporter;

namespace XmlTransformer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Competition creator - Data importer Vlaamse Volleybal Bond");
            Console.WriteLine(" Selecteer het nummer en druk op <return/enter>");
            int v = 0;
            string val;
            do
            {
                Console.WriteLine("Welke provincie:");
                int number = 0;
                foreach (var prov in SiteImporter.SiteImporter.provincies)
                {
                    Console.WriteLine("{0}. {1}", number, prov.Name);
                    number++;
                }
                val = Console.ReadLine();
            } while (!int.TryParse(val, out v) || v < 0 || v >= SiteImporter.SiteImporter.provincies.Length);
            ProvincieInfo provincie = SiteImporter.SiteImporter.provincies[v];
            SiteImporter.SiteImporter.ImportSite(provincie);
            Console.WriteLine("De bestanden zijn opgeslagen");
        }
    }
}
