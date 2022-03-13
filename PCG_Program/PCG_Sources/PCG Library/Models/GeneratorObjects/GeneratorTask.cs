using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace PCG.Library.Models.GeneratorObjects
{
    [XmlRoot("Task", Namespace = "", IsNullable = false)]
    public class GeneratorTask
    {
        private int _threadCount;
        //[XmlAttribute("path")]
        //public string Path { get; set; }

        [XmlAttribute("nodesDescription")]
        public string NodesDescription { get; set; }

        [XmlAttribute("dimension")]
        public int Dimension { get; set; }

        [XmlAttribute("threadsCount")]
        public int ThreadsCount
        {
            get => _threadCount > 0 ? _threadCount : Environment.ProcessorCount / 2;
            set => _threadCount = value > 0 ? value : Environment.ProcessorCount / 2;
        }

        [XmlAttribute("nextTask")]
        public string NextTask { get; set; }

        [XmlAttribute("outputFolderPath")]
        public string OutputFolderPath { get; set; }

        [XmlAttribute("fullName")]
        public string StudentFullName { get; set; }

        [XmlAttribute("isFullLogging")]
        public bool IsFullLogging { get; set; }

        [XmlAttribute("isFullReport")]
        public bool IsFullReport { get; set; }

        /// <summary>
        ///     Enumeration of nodes counts, which are used for circulant graphs synthesis
        /// </summary>
        [XmlIgnore]
        public List<int> Nodes
        {
            get
            {
                if (string.IsNullOrEmpty(NodesDescription))
                {
                    return new List<int>();
                }

                var splitSentences = NodesDescription.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Where(y => char.IsDigit(y) || y == '-').ToArray()).Select(x => new string(x)).ToArray();
                var list = new List<int>();

                foreach (var splitSentence in splitSentences)
                {
                    int number;

                    if (!splitSentence.Contains('-'))
                    {
                        var s = splitSentence;

                        if (int.TryParse(s, out number))
                        {
                            list.Add(number);
                        }
                    }
                    else
                    {
                        var totals = Convert.ToString(splitSentence).Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries).Where(x => int.TryParse(x, out number)).Select(int.Parse).ToArray();

                        for (var index = 0; index < totals.Length - 1; index++)
                        {
                            list.AddRange(Enumerable.Range(Math.Min(totals[index], totals[index + 1]), Math.Abs(totals[index] - totals[index + 1]) + 1));
                        }
                    }
                }

                return list.Distinct().OrderBy(x => x).ToList();
            }
        }
    }
}