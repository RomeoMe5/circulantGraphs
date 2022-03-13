using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PCG_Repeater
{
    public static class Program
    {
        // public static string 

        public static void Main(string[] args)
        {

        }

        private static string NextStep => ConfigurationManager.AppSettings[""];
    }

    public class Settings
    {
        [XmlAttribute]
        public static int NodesMax;

        [XmlAttribute]
        public static int NodesMin;

        [XmlAttribute]
        public int MaxGrade { get; set; }

        [XmlAttribute]
        public int MinGrade { get; set; }
    }
}
