using Microsoft.Win32;
using PCG.Library.Models.GeneratorObjects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace PCG.Library.Utilities
{
    public static class SuperUtilities
    {
        private static readonly IFormatProvider NumberFormat = new NumberFormatInfo
        {
            NumberDecimalSeparator = "."
        };

        /// <summary>
        ///     Loading "Task" for setting searching critertia of optimal circulant topologies
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static GeneratorTask LoadTask(string filePath)
        {
            var xmlSerialize = new XmlSerializer(typeof(GeneratorTask));

            using (var rs = new StreamReader(filePath))
            {
                return (GeneratorTask)xmlSerialize.Deserialize(rs);
            }
        }

        /// <summary>
        ///     Save "Task" in file.xml
        /// </summary>
        /// <param name="task"></param>
        /// <param name="filePath"></param>
        public static void SaveTask(GeneratorTask task, string filePath)
        {
            var xmlSerialize = new XmlSerializer(typeof(GeneratorTask));

            var fileInfo = new FileInfo(filePath);

            if (!string.IsNullOrEmpty(fileInfo.DirectoryName) && !Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            using (var rs = new StreamWriter(filePath))
            {
                xmlSerialize.Serialize(rs, task);
            }
        }

        /// <summary>
        ///     Descriptions
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static LastState RestoreLastState(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new LastState();
            }

            var xmlSerialize = new XmlSerializer(typeof(LastState));

            using (var rs = new StreamReader(filePath))
            {
                return (LastState)xmlSerialize.Deserialize(rs);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="state"></param>
        /// <param name="filePath"></param>
        public static void SaveLastState(LastState state, string filePath)
        {
            var xmlSerialize = new XmlSerializer(typeof(LastState));
            var fileInfo = new FileInfo(filePath);

            if (!string.IsNullOrEmpty(fileInfo.DirectoryName) && !Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            using (var rs = new StreamWriter(filePath))
            {
                xmlSerialize.Serialize(rs, state);
            }
        }

        public static void SaveRecordInFolder(IEnumerable<CirculantParameters> pars, string outputFolder)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="pars"></param>
        /// <param name="outputFolder"></param>
        /// <param name="inMyFormat"></param>
        public static void SaveResultsInFolder(IEnumerable<CirculantParameters> pars, string outputFolder, bool inMyFormat = true)
        {
            var groupsByGrade = pars.GroupBy(x => x.Generators.Length);

            Directory.CreateDirectory(outputFolder);
            const string headerString = "Nodes count;Graph`s descriptions;Diameter;Average length;Time (ms);Connections count";

            foreach (var g in groupsByGrade)
            {
                var groupsByNodes = g.GroupBy(y => y.NodesCount);

                foreach (var gn in groupsByNodes)
                {
                    var fileName = $"{gn.First().Generators.Length}-{gn.First().NodesCount}.csv";

                    var l = pars.Select(x => $"{x.NodesCount};C({x.NodesCount}, {string.Join(", ", x.Generators)});{x.Diameter.ToString(NumberFormat)};{x.AverageLength.ToString(NumberFormat)};{x.TotalMilliseconds};{x.NodesCount * g.Key}").ToList();
                    l.Insert(0, headerString);

                    File.WriteAllLines(Path.Combine(outputFolder, fileName), l);
                }
            }
        }

        public static string GetProcessorInfo()
        {
            //This registry entry contains entry for processor info.
            var processorName = Registry.LocalMachine.OpenSubKey(@"Hardware\Description\System\CentralProcessor\0", RegistryKeyPermissionCheck.ReadSubTree);

            return processorName?.GetValue("ProcessorNameString") != null ? processorName.GetValue("ProcessorNameString").ToString() : string.Empty;
        }
    }
}