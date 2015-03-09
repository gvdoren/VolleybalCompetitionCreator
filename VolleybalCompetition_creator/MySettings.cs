using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.ComponentModel;

namespace VolleybalCompetition_creator
{
    public class IntCountSettings
    {
        public int x1 { get; set; }
        [DisplayName("xxx")]
        public int x2 { get; set; }
        [DisplayName("yyy")]
        public int x3 { get; set; }
    }

    public class MySettings
    {
        MySettings()
        {
            // Use the DefaultValue propety of each property to actually set it, via reflection.
            foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(this))
            {
                DefaultValueAttribute attr = (DefaultValueAttribute)prop.Attributes[typeof(DefaultValueAttribute)];
                if (attr != null)
                {
                    prop.SetValue(this, attr.Value);
                }
            }
        }
        string filename;
        public void Save(string filenameNew = null)
        {
            if (filenameNew != null) filename = filenameNew;
            XmlSerializer serializer = new XmlSerializer(typeof(MySettings));
            TextWriter writer = new StreamWriter(filename);
            serializer.Serialize(writer, this);
            writer.Close();
        }
        public static MySettings Load(string filename)
        {
            if (File.Exists(filename))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(MySettings));
                TextReader reader = new StreamReader(filename);
                object obj = deserializer.Deserialize(reader);
                MySettings XmlData = (MySettings)obj;
                XmlData.filename = filename;
                reader.Close();
                return XmlData;
            }
            else
            {
                MySettings mySettings = new MySettings();
                mySettings.filename = filename;
                return mySettings;
            } 
        }
        static MySettings _settings = null;
        public static MySettings Settings { 
            get{
                if(_settings == null)
                {
                    string BaseDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)+@"\CompetitionCreator";
                    _settings = Load(BaseDirectory+@"\settings.xml");
                }
                return _settings; 
            }
        }

        [Category("Constraints")]
        [DisplayName("Constraint.Different.Groups.On.Same.Day.Cost")]
        [DefaultValue(40)]
        public int DifferentGroupsOnSameDayCost { get; set; }
        [Category("Constraints")]
        [DisplayName("Constraint.Different.Groups.On.Same.Day.Gewestelijk.Cost")]
        [DefaultValue(20)]
        public int DifferentGroupsOnSameDayGewestelijkCost { get; set; }
        [Category("Constraints")]
        [DisplayName("Constraint.Different.Groups.On.Same.Day.Overlapping.Extra.Cost")]
        [DefaultValue(2)]
        public int DifferentGroupsOnSameDayOverlappingExtraCost { get; set; }
        [Category("Constraints")]
        [DisplayName("Constraint.Sporthal.NotAvailable.Cost")]
        [DefaultValue(40)]
        public int SporthalNotAvailableCost { get; set; }
        [Category("Constraints")]
        [DisplayName("Constraint.Sporthal.NotAvailable.Cost.Gewestelijk.Cost")]
        [DefaultValue(20)]
        public int SporthalNotAvailableGewestelijkCost { get; set; }
        [Category("Constraints")]
        [DisplayName("Constraint.Match.Too.Close.Costs")]
        [DefaultValue(1000)]
        public int MatchTooCloseCost { get; set; }
        [Category("Constraints")]
        [DisplayName("Constraint.HomeMatches.Too.Many.After.Each.Other")]
        [DefaultValue(4)]
        public int MatchTooManyAfterEachOther { get; set; }
        [Category("Constraints")]
        [DisplayName("Constraint.HomeMatches.Too.Many.After.Each.Other.Cost")]
        [DefaultValue(200)]
        public int MatchTooManyAfterEachOtherCost { get; set; }
        [Category("Constraints")]
        [DisplayName("Constraint.Two.Teams.of.same.club.in.Poule.Cost")]
        [DefaultValue(1000)]
        public int TwoPoulesOfSameClubInPouleCost { get; set; }
        [Category("Constraints")]
        [DisplayName("Constraint.Not.All.In.Same.Weekend.Cost")]
        [DefaultValue(10)]
        public int NotAllInSameWeekendCost { get; set; }
        [Category("Constraints")]
        [DisplayName("Constraint.Play.At.Same.Time.Cost")]
        [DefaultValue(1)]
        public int PlayAtSameTime { get; set; }
        [Category("Constraints")]
        [DisplayName("Constraint.Play.At.Same.Time.Normal.Length.Match")]
        [DefaultValue(2.0)]
        public double NormalLengthMatch { get; set; }
        [Category("Constraints")]
        [DisplayName("Constraint.Play.At.Same.Time.Travelling.Time")]
        [DefaultValue(1.5)]
        public double TravelingTime { get; set; }
        [Category("Constraints")]
        [DisplayName("Constraint.Play.At.Same.Time.Extra.Time.PreMatch")]
        [DefaultValue(2.0)]
        public double ReserveMatchExtraTime { get; set; }
        [Category("Constraints")]
        [DisplayName("Constraint.Too.Many.Conflicts.Club.Cost")]
        [DefaultValue(80)]
        public int TooManyConflictsPerClubCosts { get; set; }
        [Category("Constraints")]
        [DisplayName("Constraint.Too.Many.Conflicts.Team.Cost")]
        [DefaultValue(80)]
        public int TooManyConflictsPerTeamCosts { get; set; }
        [Category("Constraints")]
        [DisplayName("Constraint.Custom.Team.Constraint.Default.Cost")]
        [DefaultValue(1)]
        public int DefaultCustomTeamConstraintCost { get; set; }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public IntCountSettings countSettings { get; set; }

        [Category("Other")]
        [DisplayName("Constraint.Inconsistent.Other")]
        [DefaultValue(10000)]
        public int InconsistentCost { get; set; }
        [Category("Other")]
        [DisplayName("Constraint.No.Poule.Assigned")]
        [DefaultValue(10000)]
        public int NoPouleAssignedCost { get; set; }

    }

}
