using PCG.Library.Models.Generators;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PCG.Library.Utilities
{
    public static class BenchmarkUtilities
    {

        public static void StartBenchMark(int[][] nodesCount)
        {

        }

        public static List<BenchMarkResult> BasicDijkstraMethod(int nodesCount, int dimension, bool onlyS1EqualOne = false, int limit = 1000)
        {
            var diam = float.MaxValue;
            var avgDiam = float.MaxValue;
            //var lockEqual = new object();
            var indexer = new int[dimension];
            var optimalParams = new List<Tuple<int[], long>>();

            for (var i = 0; i < dimension; i++)
            {
                indexer[i] = i + 1;
            }

            var start = Stopwatch.StartNew();
            var finishedCollection = new ConcurrentQueue<Tuple<int[], float[], long>>();
            var srcMatrix = Enumerable.Repeat(new byte[nodesCount], nodesCount).ToArray();

            var lim = 0;

            var bench = new List<BenchMarkResult>();

            while (lim < limit && (onlyS1EqualOne && indexer[0] < 2 || !onlyS1EqualOne && indexer[0] + dimension <= nodesCount / 2 + 1))
            {
                BasicCirculantGenerator.SpecialIncrement(nodesCount, ref indexer, onlyS1EqualOne);

                var startA = Stopwatch.StartNew();
                var matrix = BasicCirculantGenerator.GenerateGraphMatrixByGeneratrix(indexer, ref srcMatrix);

                startA.Stop();

                var startT = Stopwatch.StartNew();
                var tempResult = OneThreadCirculantGenerator.SolveDiamAndAverDiam(nodesCount, matrix);
                var memory = Process.GetCurrentProcess().WorkingSet64;

                startT.Stop();

                bench.Add(new BenchMarkResult
                {
                    Memory = memory,
                    NodesCount = nodesCount,
                    Grade = dimension,
                    Total = startT.ElapsedTicks + startA.ElapsedTicks,
                    TimeSpans = new Tuple<string, long>[]
                    {
                        new Tuple<string, long>("Init elapsed time", startA.ElapsedTicks),
                        new Tuple<string, long>("Calculation", startT.ElapsedTicks),
                    }

                });

                lim++;
            };

            return bench;
        }

        /// <summary>
        /// Simulate Iterations
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="dimension"></param>
        /// <returns></returns>
        public static long GetIterations(int nodes, int dimension)
        {
            var indexer = new int[dimension];
            long iter = 1;

            for (var i = 0; i < dimension; i++)
            {
                indexer[i] = i + 1;
            }

            while (indexer[0] + dimension <= nodes / 2 + 1)
            {
                iter++;
                BasicCirculantGenerator.SpecialIncrement(nodes, ref indexer, false);
            }

            return iter;
        }

        public static List<BenchMarkResult> Method2(int nodesCount, int dimension, bool onlyS1EqualOne = false, int limit = 1000)
        {
            var diam = float.MaxValue;
            var avgDiam = float.MaxValue;
            //var lockEqual = new object();
            var indexer = new int[dimension];
            var optimalParams = new List<Tuple<int[], long>>();

            for (var i = 0; i < dimension; i++)
            {
                indexer[i] = i + 1;
            }

            var start = Stopwatch.StartNew();
            var finishedCollection = new ConcurrentQueue<Tuple<int[], float[], long>>();
            var srcMatrix = Enumerable.Repeat(new byte[nodesCount], nodesCount).ToArray();

            var lim = 0;

            var bench = new List<BenchMarkResult>();

            while (lim < limit && (onlyS1EqualOne && indexer[0] < 2 || !onlyS1EqualOne && indexer[0] + dimension <= nodesCount / 2 + 1))
            {
                BasicCirculantGenerator.SpecialIncrement(nodesCount, ref indexer, onlyS1EqualOne);

                var startT = Stopwatch.StartNew();
                var tempResult = OptimizedParallelCirculantGenerator.SolveDiamAndAverDiam(nodesCount, indexer);
                var memory = Process.GetCurrentProcess().WorkingSet64;

                startT.Stop();

                bench.Add(new BenchMarkResult
                {
                    Memory = memory,
                    NodesCount = nodesCount,
                    Grade = dimension,
                    Total = startT.ElapsedTicks,
                    TimeSpans = new Tuple<string, long>[]
                    {
                        new Tuple<string, long>("Calculation", startT.ElapsedTicks),
                    }

                });

                lim++;
            };

            return bench;
        }
    }

    public class BenchMarkResult
    {
        public Tuple<string, long>[] TimeSpans;
        public long Total;
        public long Memory;
        public int NodesCount;
        public int Grade { get; set; }
    }
}
