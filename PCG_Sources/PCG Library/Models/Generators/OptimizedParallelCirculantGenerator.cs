using PCG.Library.Models.GeneratorObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PCG.Library.Models.Generators
{
    /// <summary>
    /// GCG algorithm - enchanced algorithm
    /// </summary>
    public class OptimizedParallelCirculantGenerator : BasicCirculantGenerator
    {
        protected override List<CirculantParameters> GenerateOptimalCirculants(int nodesCount, int grade, bool byAvgDiam = false, bool onlyS1EqualOne = false)
        {
            return ParallelSearchOptimalGraphs(nodesCount, grade, byAvgDiam, onlyS1EqualOne, ThreadCount);
        }

        private static int[] CirculantDejkstraPath(int nodeIndex, int length, int[] generators)
        {
            var d = new int[length]; // минимальное расстояние
            var v = new int[length]; // посещенные вершины

            int minIndex;

            for (var i = 0; i < length; i++)
            {
                d[i] = int.MaxValue;
                v[i] = 1;
            }

            d[nodeIndex] = 0;

            // Шаг алгоритма
            do
            {
                minIndex = int.MaxValue;
                var min = int.MaxValue;

                for (var i = 0; i < length; i++)
                {
                    // Если вершину ещё не обошли и вес меньше min
                    if (v[i] == 1 && d[i] < min)
                    {
                        // Переприсваиваем значения
                        min = d[i];
                        minIndex = i;
                    }
                }

                // Добавляем найденный минимальный вес к текущему весу вершины и сравниваем с текущим минимальным весом вершины
                if (minIndex != int.MaxValue)
                {
                    foreach (var x in generators)
                    {
                        var temp = min + 1;
                        var nextIndex = (minIndex + x) % length;

                        if (temp < d[nextIndex])
                        {
                            d[nextIndex] = temp;
                        }

                        var t = minIndex - x;
                        var reverseIndex = t >= 0 ? t : length + minIndex - x;

                        if (temp < d[reverseIndex])
                        {
                            d[reverseIndex] = temp;
                        }
                    }

                    v[minIndex] = 0;
                }
            } while (minIndex < int.MaxValue);

            return d;
        }

        public static float[] SolveDiamAndAverDiam(int length, int[] generators)
        {
            var maxDiam = float.MinValue;
            float averDiam = 0;
            var allMoves = 0;

            // nodes_count было, стало 1
            for (var i = 0; i < 1; i++)
            {
                var d = CirculantDejkstraPath(i, length, generators);
                allMoves += length - i - 1;

                for (var j = 1 + i; j < length; j++)
                {
                    if (maxDiam < d[j])
                    {
                        maxDiam = d[j];
                    }

                    averDiam += d[j];
                }
            }

            averDiam /= allMoves;
            return new float[2] { maxDiam, averDiam };
        }

        private List<CirculantParameters> ParallelSearchOptimalGraphs(int nodesCount, int dimension, bool byAvgDiam = false, bool onlyS1EqualOne = false, int threadCount = 0, int? limit = -1)
        {
            var diam = float.MaxValue;
            var avgDiam = float.MaxValue;
            var indexer = new int[dimension];
            var optimalParams = new List<Tuple<int[], long>>();

            for (var i = 0; i < dimension; i++)
            {
                indexer[i] = i + 1;
            }

            LastState.CurrentNodesCount = nodesCount;

            if (LastState.NotCheckedConfig != null)
            {
                indexer = LastState.NotCheckedConfig;
                optimalParams.AddRange(LastState.GoodConfigs.Select(x => new Tuple<int[], long>(x, 0)));
                diam = LastState.Diameter;
                avgDiam = LastState.AverageDiameter;
            }
            else
            {

                var nodeDescription = LastState.NodesDescription;

                LastState = new LastState
                {
                    NodesDescription = nodeDescription
                };
            }

            var start = Stopwatch.StartNew();
            var totalLazyIndexer = new List<Tuple<int[], bool>>();
            var finishedCollection = new ConcurrentQueue<Tuple<int[], float[], long>>();

            while (onlyS1EqualOne && indexer[0] < 2 || !onlyS1EqualOne && indexer[0] + dimension <= nodesCount / 2 + 1)
            {
                var i = 0;

                while ((onlyS1EqualOne && indexer[0] < 2 || !onlyS1EqualOne && indexer[0] + dimension <= nodesCount / 2 + 1) && i < 100)
                {
                    totalLazyIndexer.Add(new Tuple<int[], bool>(indexer.ToArray(), false));
                    SpecialIncrement(nodesCount, ref indexer, onlyS1EqualOne);
                    i++;
                }

                lock (ForcedLock)
                {
                    var result = Parallel.ForEach(totalLazyIndexer, new ParallelOptions { MaxDegreeOfParallelism = threadCount > 0 ? threadCount : int.MaxValue }, ints =>
                      {
                          if (ForcedStop)
                          {
                              return;
                          }

                          var startT = Stopwatch.StartNew();
                          var tempResult = SolveDiamAndAverDiam(nodesCount, ints.Item1);

                          startT.Stop();
                          
                          finishedCollection.Enqueue(new Tuple<int[], float[], long>(ints.Item1, tempResult, startT.ElapsedTicks));

                          var index = totalLazyIndexer.FindIndex(tuple => Equals(tuple.Item1, ints.Item1));

                          if (index > -1)
                          {
                              totalLazyIndexer[index] = new Tuple<int[], bool>(ints.Item1, true);
                          }
                      });

                    while (!result.IsCompleted)
                    {
                        OptimalCirculantsSelection(finishedCollection, optimalParams, byAvgDiam, ref diam, ref avgDiam);

                        if (ForcedStop)
                        {
                            OptimalCirculantsSelection(finishedCollection, optimalParams, byAvgDiam, ref diam, ref avgDiam);
                            return SaveLastState(dimension, start, totalLazyIndexer, indexer, optimalParams, avgDiam, diam, nodesCount);
                        }
                    }

                    if (ForcedStop)
                    {
                        OptimalCirculantsSelection(finishedCollection, optimalParams, byAvgDiam, ref diam, ref avgDiam);
                        return SaveLastState(dimension, start, totalLazyIndexer, indexer, optimalParams, avgDiam, diam, nodesCount);
                    }
                }

                OptimalCirculantsSelection(finishedCollection, optimalParams, byAvgDiam, ref diam, ref avgDiam);
                totalLazyIndexer.Clear();
            }

            OptimalCirculantsSelection(finishedCollection, optimalParams, byAvgDiam, ref diam, ref avgDiam);
            start.Stop();

            return optimalParams.Select(x => new CirculantParameters
            {
                AverageLength = avgDiam,
                Diameter = diam,
                Generators = x.Item1,
                NodesCount = nodesCount,
                Ticks = x.Item2,
                TotalMilliseconds = start.ElapsedMilliseconds
            }).ToList();
        }

        private void OptimalCirculantsSelection(ConcurrentQueue<Tuple<int[], float[], long>> finishedCollection, List<Tuple<int[], long>> optimalParams, bool byAvgDiam, ref float diam, ref float avgDiam)
        {
            while (finishedCollection.Any())
            {
                if (!finishedCollection.TryDequeue(out var info))
                {
                    continue;
                }

                var isBest = false;
                var isEqual = false;

                if (!byAvgDiam && diam >= info.Item2[0] || byAvgDiam && avgDiam >= info.Item2[1])
                {
                    isBest = avgDiam > info.Item2[1] || diam > info.Item2[0];
                    isEqual = !isBest && Math.Abs(diam - info.Item2[0]) < Eps && Math.Abs(avgDiam - info.Item2[1]) < Eps;
                }

                if (isBest)
                {
                    if (avgDiam > info.Item2[1] || diam > info.Item2[0])
                    {
                        optimalParams.Clear();

                        diam = info.Item2[0];
                        avgDiam = info.Item2[1];
                    }
                }

                if (isBest || isEqual)
                {
                    optimalParams.Add(new Tuple<int[], long>(info.Item1, info.Item3));
                }

                TotalTime += info.Item3;
            }
        }

        private List<CirculantParameters> SaveLastState(int grade, Stopwatch start, List<Tuple<int[], bool>> totalLazyIndexer, int[] indexer, List<Tuple<int[], long>> optimalParams, float avgDiam, float diam, int nodesCount)
        {
            start.Stop();

            LastState.NotCheckedConfig = totalLazyIndexer.Where(x => !x.Item2).OrderBy(x => x.Item1, new ArrayComparer()).FirstOrDefault()?.Item1 ?? indexer;
            LastState.GoodConfigs = optimalParams.Select(x => x.Item1).ToArray();
            LastState.AverageDiameter = avgDiam;
            LastState.Diameter = diam;
            LastState.Dimension = grade;
            LastState.CurrentNodesCount = nodesCount;

            TotalTime = start.ElapsedMilliseconds;

            return null;
        }

        public static List<CirculantParameters> SearchOptimalGraphs(int nodesCount, int grade, bool byAvgDiam = false, bool onlyS1EqualOne = false)
        {
            var diam = float.MaxValue;
            var avgDiam = float.MaxValue;

            //int* optimal_link = new int[dimension + 1];
            var indexer = new int[grade];
            var emptyMatrix = new byte[nodesCount][];
            emptyMatrix = emptyMatrix.Select(x => new byte[nodesCount]).ToArray();

            var optimalParams = new List<Tuple<int[], long>>();

            for (var i = 0; i < grade; i++)
            {
                indexer[i] = i + 1;
            }

            var start = new Stopwatch();
            start.Start();

            while (onlyS1EqualOne && indexer[0] < 2 || !onlyS1EqualOne && indexer[0] + grade <= nodesCount / 2 + 1)
            {
                var matrix = GenerateGraphMatrixByGeneratrix(indexer, ref emptyMatrix);

                var startT = new Stopwatch();
                startT.Start();

                var tempResult = (float[])null;  //SolveDiamAndAverDiam(matrix);

                startT.Stop();

                var isBest = false;
                var isEqual = false;

                if (!byAvgDiam && diam >= tempResult[0] || byAvgDiam && avgDiam >= tempResult[1])
                {
                    isBest = avgDiam > tempResult[1] || diam > tempResult[0];
                    isEqual = !isBest && Math.Abs(diam - tempResult[0]) < Eps && Math.Abs(avgDiam - tempResult[1]) < Eps;
                }

                if (isBest)
                {
                    optimalParams.Clear();

                    diam = tempResult[0];
                    avgDiam = tempResult[1];
                }

                if (isBest || isEqual)
                {
                    optimalParams.Add(new Tuple<int[], long>(indexer.ToArray(), 0 /*startT.ElapsedTicks*/));
                }

                SpecialIncrement(matrix.Length, ref indexer, onlyS1EqualOne);
            }

            start.Stop();

            return optimalParams.Select(x => new CirculantParameters
            {
                AverageLength = avgDiam,
                Diameter = diam,
                Generators = x.Item1,
                NodesCount = nodesCount,
                Ticks = x.Item2,
                TotalMilliseconds = start.ElapsedMilliseconds
            }).ToList();
        }
    }
}