using PCG.Library.Models.GeneratorObjects;
using PCG.Library.Models.Validation;
using PCG.Library.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace PCG.Library.Models.Generators
{
    public class BasicCirculantGenerator
    {
        protected const float Eps = .005f;
        protected readonly object ForcedLock = new object();
        protected bool ForcedStop;
        protected LastState LastState = new LastState();
        protected bool IsFullLogging = false;
        protected bool IsFullReport = false;
        protected int ThreadCount = Environment.ProcessorCount / 2;
        private readonly BasicWorkInformation _report = new BasicWorkInformation();
        protected decimal TotalTime = 0;

        public void Start(GeneratorTask task)
        {
            if (string.IsNullOrEmpty(task.NodesDescription))
            {
                return;
            }

            ThreadCount = task.ThreadsCount;

            lock (ForcedLock)
            {
                LastState = SuperUtilities.RestoreLastState(ConfigurationManager.AppSettings["LastStatePath"]);
            }

            var nodes = task.Nodes;

            if (task.NodesDescription.Equals(LastState.NodesDescription))
            {
                var curN = nodes.SkipWhile(x => x != LastState.CurrentNodesCount).ToList();
                nodes = curN.Any() ? curN : nodes;
            }
            else
            {
                LastState = new LastState();
                LastState.NodesDescription = task.NodesDescription;
            }

            _report.ReportId = Guid.NewGuid();
            _report.ComputerInfo = SuperUtilities.GetProcessorInfo();
            _report.ThreadCount = task.ThreadsCount;
            _report.StudentFullName = task.StudentFullName;
            _report.ExtendedInfo = new ExtendedInformation { Descriptions = new List<ParametersDescription>() };

            foreach (var nodesCount in nodes)
            {
                Console.WriteLine($"Synthesis for nodes count: {nodesCount}");

                var paramDescription = new ParametersDescription
                {
                    OptimalCirculants = new List<CirculantParameters>()
                };

                var result = GenerateOptimalCirculants(nodesCount, task.Dimension);

                if (result == null || ForcedStop)
                {
                    // _report.TotalTime += result?.FirstOrDefault()?.TotalMilliseconds ?? TotalTime;

                    paramDescription.IsFinished = false;
                    _report.ExtendedInfo.Descriptions.Add(paramDescription);
                    paramDescription.LastState = LastState;

                    break;
                }

                paramDescription.IsFinished = true;
                paramDescription.OptimalCirculants.AddRange(result);
                _report.TotalTime += result?.FirstOrDefault()?.TotalMilliseconds ?? 0;
                _report.ExtendedInfo.Descriptions.Add(paramDescription);

                SuperUtilities.SaveResultsInFolder(result, task.OutputFolderPath);
            }

            ReportBuilder.SaveInfo(_report, Path.Combine(task.OutputFolderPath, $"{_report.ValidFullName}-{DateTime.Now:yyyyMMdd.hhmm}.bin"));

            Console.WriteLine($"Synthesis is ended, type stop (or another chars) for exit.");
            //Console.In.Close();
            //Console.ReadKey(true);
        }

        public void Stop()
        {
            ForcedStop = true;

            lock (ForcedLock)
            {
                SuperUtilities.SaveLastState(LastState, ConfigurationManager.AppSettings["LastStatePath"]);
            }
        }

        public static byte[][] GenerateGraphMatrixByGeneratrix(int[] config, ref byte[][] srcMatrix, bool needClone = true, int startGeneratrixIndex = -1, int endGeneratrixIndex = -1)
        {
            var resultMatrix = needClone ? srcMatrix.Select(x => x.ToArray()).ToArray() : srcMatrix;

            // второй шаг, заполнение по степени.
            var begin = startGeneratrixIndex != -1 && startGeneratrixIndex >= 1 && startGeneratrixIndex <= srcMatrix.Length ? startGeneratrixIndex : 0;
            var end = endGeneratrixIndex >= 1 && endGeneratrixIndex <= config.Length - 1 ? endGeneratrixIndex : config.Length - 1;

            if (startGeneratrixIndex >= 1 && startGeneratrixIndex <= config.Length - 1 && endGeneratrixIndex == -1)
            {
                end = begin;
            }

            for (var i = begin; i <= end; i++)
            {
                var hopeLength = config[i];

                // Обход всех звеньев, по "часовой стрелке" (сначала сверху-вниз, потом слева-направо)
                for (var j = 0; j < srcMatrix.Length; j++)
                {
                    var hopeLengthPlusJ = hopeLength + j;
                    var connectedNodeI = (hopeLengthPlusJ < srcMatrix.Length ? hopeLengthPlusJ : j) - 1;
                    var connectedNodeJ = hopeLengthPlusJ < srcMatrix.Length ? j : hopeLengthPlusJ - srcMatrix.Length;

                    resultMatrix[connectedNodeI + 1][connectedNodeJ] = 1;
                    resultMatrix[connectedNodeJ][connectedNodeI + 1] = 1;
                }
            }

            return resultMatrix;
        }

        protected virtual List<CirculantParameters> GenerateOptimalCirculants(int nodesCount, int grade, bool byAvgDiam = false, bool onlyS1EqualOne = false)
        {
            return null;
        }

        protected static bool Equals(int[] ix, int[] iy)
        {
            return !ix.Where((t, i) => t != iy[i]).Any();
        }

        /// <summary>
        ///    Increment for circular graphs
        /// </summary>
        /// <param name="nodesCount"></param>
        /// <param name="config"></param>
        /// <param name="onlyS1EqualOne"></param>
        public static void SpecialIncrement(int nodesCount, ref int[] config, bool onlyS1EqualOne)
        {
            var elderIncr = 1;

            for (var i = config.Length - 1; i >= (onlyS1EqualOne ? 2 : 1); i--)
            {
                if (config[i] >= nodesCount / 2 - elderIncr + 1)
                {
                    elderIncr++;
                }
                else
                {
                    config[i]++;

                    for (var j = i + 1; j <= config.Length - 1; j++)
                    {
                        config[j] = config[j - 1] + 1;
                    }

                    return;
                }
            }

            config[0]++;
        }

        /// <summary>
        /// Comparer for arrays - all elements are equals
        /// </summary>
        protected class ArrayComparer : IComparer<int[]>
        {
            public int Compare(int[] x, int[] y)
            {

                if (x == null)
                {
                    if (y == null)
                    {
                        return 0;
                    }

                    return -1;
                }


                if (y == null)
                {
                    return 1;
                }

                for (var i = 0; i < x.Length; i++)
                {
                    if (x[i] > y[i])
                    {
                        return 1;
                    }

                    if (x[i] < y[i])
                    {
                        return -1;
                    }
                }

                return 0;
            }
        }
    }
}
