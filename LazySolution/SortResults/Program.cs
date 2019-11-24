using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;

namespace SortResults
{
    public class Graph
    {
        public int NodesCount;
        public int[] Generators;
        public int Diameter;
        public double AverageLength;
        public int LinksCount;
        public long Time;

        public override int GetHashCode()
        {
            return NodesCount.GetHashCode() + Generators.Sum().GetHashCode() + Diameter.GetHashCode() + AverageLength.GetHashCode();
        }

        public override string ToString()
        {
            return $"\"C({NodesCount}; {string.Join(", ", Generators)})\";{Diameter};{AverageLength};{LinksCount};{Time}";
        }
    }

    public static class Program
    {
        public static string InputDirectory => ConfigurationManager.AppSettings["InputGraphs"];
        public static string OutputRootDirectory => ConfigurationManager.AppSettings["OutputRoot"];

        public static Dictionary<int, ConcurrentDictionary<int, Graph>> SOneGraphs = new Dictionary<int, ConcurrentDictionary<int, Graph>>();
        public static Dictionary<int, ConcurrentDictionary<int, Graph>> OptimalGraphs = new Dictionary<int, ConcurrentDictionary<int, Graph>>();
        public static ConcurrentDictionary<int, Graph> DdGraphs = new ConcurrentDictionary<int, Graph>();
        public static object SOneLock = new object();
        public static object OptimalLock = new object();

        public static void Main(string[] args)
        {
            var log = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);

            if (!Directory.Exists(InputDirectory))
            {
                log.Warn("Папки с данными не существует. Завершение работы приложения.");
                Console.ReadKey(true);
                return;
            }

            while (!Parallel.ForEach(Directory.GetFiles(InputDirectory), AnalyzeFile).IsCompleted)
            {

            }

            try
            {
                if (Directory.Exists(OutputRootDirectory))
                {
                    var rootOutput = new DirectoryInfo(OutputRootDirectory);
                    rootOutput.Empty();
                    rootOutput.Delete(true);
                }

                var directoryInfo = Directory.CreateDirectory(OutputRootDirectory);

                foreach (var sOneGraph in SOneGraphs)
                {
                    DirectoryInfo sub = null;

                    var gradeRootDirectory = $"grade {sOneGraph.Key.ToString()}";

                    if (directoryInfo.GetDirectories().Any(x => x.Name.Equals(gradeRootDirectory)))
                    {
                        sub = directoryInfo.GetDirectories().First(x => x.Name.Equals(gradeRootDirectory));
                    }
                    else
                    {
                        sub = directoryInfo.CreateSubdirectory(gradeRootDirectory);
                    }

                    var a = 2;
                    var lastSub = $"C(n; 1, {string.Join(", ", sOneGraph.Value.First().Value.Generators.Skip(1).Select(x => $"s{a++}"))})";

                    if (!sub?.GetDirectories().Any(x => x.Name.Equals(lastSub)) ?? false)
                    {
                        sub = sub.CreateSubdirectory(lastSub);
                    }

                    if (sub == null)
                    {
                        throw new Exception($"Не удалось создать папку: {gradeRootDirectory}");
                    }

                    var groupsByNodeCount = sOneGraph.Value.GroupBy(x => x.Value.NodesCount).ToList();
                    groupsByNodeCount.Sort((first, second) => first.Key - second.Key);

                    var minNodes = groupsByNodeCount.Min(x => x.Key);
                    var maxNodes = groupsByNodeCount.Max(x => x.Key);

                    foreach (var group in groupsByNodeCount)
                    {

                        File.AppendAllLines(Path.Combine(sub.FullName, $"all_ring_gr{sOneGraph.Key}_n{minNodes}-{maxNodes}.csv"), new[] { group.First().Value.ToString() });
                        a = 1;

                        var sort = group.ToList();
                        sort.Sort((first, second) =>
                        {
                            for (int i = 0; i < first.Value.Generators.Length; i++)
                            {
                                if (first.Value.Generators[i] > second.Value.Generators[i] || first.Value.Generators[i] < second.Value.Generators[i])
                                {
                                    return first.Value.Generators[i] - second.Value.Generators[i];
                                }
                            }

                            return 0;
                        });

                        File.AppendAllLines(Path.Combine(sub.FullName, $"C({group.First().Value.NodesCount}; {string.Join(", ", sOneGraph.Value.First().Value.Generators.Select(x => $"s{a++}")).Substring(1)}).csv"), sort.Select(x => x.Value.ToString()));
                    }
                }

                foreach (var optimalGraph in OptimalGraphs)
                {
                    DirectoryInfo sub = null;

                    var gradeRootDirectory = $"grade {optimalGraph.Key.ToString()}";

                    if (directoryInfo.GetDirectories().Any(x => x.Name.Equals(gradeRootDirectory)))
                    {
                        sub = directoryInfo.GetDirectories().First(x => x.Name.Equals(gradeRootDirectory));
                    }
                    else
                    {
                        sub = directoryInfo.CreateSubdirectory(gradeRootDirectory);
                    }

                    var a = 1;
                    var lastSub = $"C(n; {string.Join(", ", optimalGraph.Value.First().Value.Generators.Select(x => $"s{a++}"))})";

                    if (!sub?.GetDirectories().Any(x => x.Name.Equals(lastSub)) ?? false)
                    {
                        sub = sub.CreateSubdirectory(lastSub);
                    }
                    else
                    {
                        sub = directoryInfo.GetDirectories().First(x => x.Name.Equals(lastSub));
                    }

                    if (sub == null)
                    {
                        throw new Exception($"Не удалось создать папку: {gradeRootDirectory}");
                    }

                    var groupsByNodeCount = optimalGraph.Value.GroupBy(x => x.Value.NodesCount).ToList();
                    groupsByNodeCount.Sort((pairs, grouping) => pairs.Key - grouping.Key);

                    var minNodes = groupsByNodeCount.Min(x => x.Key);
                    var maxNodes = groupsByNodeCount.Max(x => x.Key);

                    foreach (var group in groupsByNodeCount)
                    {
                        var minDiam = group.Min(x => x.Value.Diameter);
                        var filteredGroup = group.Where(x => x.Value.Diameter <= minDiam).ToList();
                        var minAvg = filteredGroup.Min(x => x.Value.AverageLength);
                        filteredGroup = filteredGroup.Where(x => x.Value.AverageLength <= minAvg).ToList();
                        filteredGroup.Sort((first, second) =>
                        {
                            for (int i = 0; i < first.Value.Generators.Length; i++)
                            {
                                if (first.Value.Generators[i] > second.Value.Generators[i] || first.Value.Generators[i] < second.Value.Generators[i])
                                {
                                    return first.Value.Generators[i] - second.Value.Generators[i];
                                }
                            }

                            return 0;
                        });


                        File.AppendAllLines(Path.Combine(sub.FullName, $"all_optCirc_gr{optimalGraph.Key}_n{minNodes}-{maxNodes}.csv"), new[] { filteredGroup.First().Value.ToString() });
                        a = 1;
                        File.AppendAllLines(Path.Combine(sub.FullName, $"C({group.First().Value.NodesCount}; {string.Join(", ", filteredGroup.First().Value.Generators.Select(x => $"s{a++}"))}).csv"), filteredGroup.Select(x => x.Value.ToString()));
                    }
                }

                // grade 2
                if (DdGraphs != null && DdGraphs.Any())
                {
                    DirectoryInfo sub = null;

                    var gradeRootDirectory = "grade 2";

                    sub = directoryInfo.GetDirectories().Any(x => x.Name.Equals(gradeRootDirectory)) ? directoryInfo.GetDirectories().First(x => x.Name.Equals(gradeRootDirectory)) : directoryInfo.CreateSubdirectory(gradeRootDirectory);

                    var lastSub = $"C(n; D, D-1)";

                    if (!sub?.GetDirectories().Any(x => x.Name.Equals(lastSub)) ?? false)
                    {
                        sub = sub.CreateSubdirectory(lastSub);
                    }
                    else
                    {
                        sub = directoryInfo.GetDirectories().First(x => x.Name.Equals(lastSub));
                    }

                    if (sub == null)
                    {
                        throw new Exception($"Не удалось создать папку: {gradeRootDirectory}");
                    }

                    var groupsByNodeCount = DdGraphs.GroupBy(x => x.Value.NodesCount).ToList();
                    groupsByNodeCount.Sort((pairs, grouping) => pairs.Key - grouping.Key);

                    var minNodes = groupsByNodeCount.Min(x => x.Key);
                    var maxNodes = groupsByNodeCount.Max(x => x.Key);

                    foreach (var group in groupsByNodeCount)
                    {
                        var minDiam = group.Min(x => x.Value.Diameter);
                        var filteredGroup = group.Where(x => x.Value.Diameter <= minDiam).ToList();
                        var minAvg = filteredGroup.Min(x => x.Value.AverageLength);
                        filteredGroup = filteredGroup.Where(x => x.Value.AverageLength <= minAvg).ToList();
                        filteredGroup.Sort((first, second) =>
                        {
                            for (int i = 0; i < first.Value.Generators.Length; i++)
                            {
                                if (first.Value.Generators[i] > second.Value.Generators[i] || first.Value.Generators[i] < second.Value.Generators[i])
                                {
                                    return first.Value.Generators[i] - second.Value.Generators[i];
                                }
                            }

                            return 0;
                        });

                        File.AppendAllLines(Path.Combine(sub.FullName, $"all_optCirc_gr{2}_n{minNodes}-{maxNodes}.csv"), new[] { group.First().Value.ToString() });
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error("Не удалось удалить папку с предыдущими результатами. Отмена операции.", ex);
                Console.ReadKey(true);

                return;
            }

            Console.ReadKey(true);
        }

        private static void AnalyzeFile(string filePath)
        {
            var log = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);

            log.Info($"Обработка файла: {filePath}");

            var allStrings = File.ReadAllLines(filePath);

            if (allStrings.FirstOrDefault()?.Contains("Кол-во") ?? false)
            {
                allStrings = allStrings.Skip(1).ToArray();
            }

            while (!Parallel.ForEach(allStrings, graphResult =>
            {
                try
                {
                    var splitted = graphResult.Split(';');

                    int nodeCounts = 0;
                    nodeCounts = Convert.ToInt32(splitted[0]);

                    int[] generatrixes;
                    generatrixes = AnalyzeCirculantParamString(splitted[1]);
                    int diam;
                    diam = Convert.ToInt32(splitted[2]);
                    double avgL;
                    avgL = double.Parse(splitted[3], new NumberFormatInfo { NumberDecimalSeparator = "." });

                    long time;
                    try
                    {
                        time = Convert.ToInt64(splitted[4]);
                    }
                    catch (Exception e)
                    {
                        log.Debug($"Не удалось проанализировать строку {graphResult}. Время.", e);
                        return;
                    }
                    int linksCount = -1;

                    try
                    {
                        linksCount = Convert.ToInt32(splitted[5]);
                    }
                    catch (Exception e)
                    {
                        log.Debug($"Не удалось проанализировать строку {graphResult}. Кол-во узлов.", e);
                    }

                    AddToDictionary(new Graph
                    {
                        NodesCount = nodeCounts,
                        AverageLength = avgL,
                        Diameter = diam,
                        Generators = generatrixes,
                        LinksCount = linksCount == -1 ? nodeCounts * generatrixes.Length : linksCount,
                        Time = time
                    });
                }
                catch (Exception ex)
                {
                    log.Error($"Не удалось проанализировать строку (файл: {filePath}) {graphResult}", ex);
                }
            }).IsCompleted) { }

            log.Info($"Анализ файла {filePath} закончен.");
        }

        private static void AddToDictionary(Graph graph)
        {
            var grade = graph.Generators.Length;

            if (graph.Generators.First() == 1)
            {
                if (!SOneGraphs.ContainsKey(grade))
                {
                    lock (SOneLock)
                    {
                        if (!SOneGraphs.ContainsKey(grade))
                        {
                            SOneGraphs.Add(grade, new ConcurrentDictionary<int, Graph>());
                        }
                    }
                }

                var gradeCol = SOneGraphs[grade];
                gradeCol.AddOrUpdate(graph.GetHashCode(), graph, (i, g) => SOneGraphs[grade][i].Time > g.Time ? g : graph);
            }

            {
                if (!OptimalGraphs.ContainsKey(grade))
                {
                    lock (OptimalLock)
                    {
                        if (!OptimalGraphs.ContainsKey(grade))
                        {
                            OptimalGraphs.Add(grade, new ConcurrentDictionary<int, Graph>());
                        }
                    }
                }

                var gradeCol = OptimalGraphs[grade];
                gradeCol.AddOrUpdate(graph.GetHashCode(), graph, (i, g) => OptimalGraphs[grade][i].Time > g.Time ? g : graph);
            }

            if (grade == 2)
            {
                var d = Convert.ToInt32(Math.Round((Math.Pow(2 * graph.NodesCount - 1, .5) - 1) / 2d));

                if (graph.Generators.First() == d)
                {
                    DdGraphs.AddOrUpdate(graph.GetHashCode(), graph, (i, g) => DdGraphs[i].Time > g.Time ? g : graph);
                }
            }
        }


        private static int[] AnalyzeCirculantParamString(string description)
        {
            var pattern = @"C\((\d+)(((\;?\,)\s*\d+)+)\)";
            var regex = new Regex(pattern);
            Match match = regex.Match(description);

            var nodesCount = 0;
            var generatrixes = new List<int>();
            var generatrixesString = string.Empty;

            while (match.Success)
            {
                generatrixesString = match.Groups[2].Value;
                match = match.NextMatch();
            }

            if (!string.IsNullOrEmpty(generatrixesString))
            {
                generatrixes.AddRange(generatrixesString.Replace(" ", string.Empty).Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse));
                generatrixes.Sort();
            }

            return generatrixes.ToArray();
        }

        public static void Empty(this DirectoryInfo directory)
        {
            foreach (var file in directory.GetFiles())
            {
                file.Delete();
            }

            foreach (var subDirectory in directory.GetDirectories())
            {
                subDirectory.Delete(true);
            }
        }
    }
}
