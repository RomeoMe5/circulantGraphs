using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PCG.Library.Models.Generators.Dejkstra
{
    public class ParallelCirculantGenerator : BasicCirculantGenerator
    {
        private const float Eps = .005f;
        private readonly object _forcedLock = new object();
        private bool _forcedStop;
        private LastState _lastState = new LastState();

        protected override List<CirculantParameters> GenerateOptimalCirculants(int nodesCount, int grade, bool byAvgDiam = false, bool onlyS1EqualOne = false)
        {
            return ParallelSearchOptimalGraphs(nodesCount, grade, byAvgDiam, onlyS1EqualOne);
        }

        private static int[] DejkstraPath(int nodeIndex, byte[][] a)
        {
            var d = new int[a.Length]; // минимальное расстояние
            var v = new int[a.Length]; // посещенные вершины

            int minindex;

            for (var i = 0; i < a.Length; i++)
            {
                d[i] = int.MaxValue;
                v[i] = 1;
            }

            d[nodeIndex] = 0;

            // Шаг алгоритма
            do
            {
                minindex = int.MaxValue;
                var min = int.MaxValue;

                for (var i = 0; i < a.Length; i++)
                {
                    // Если вершину ещё не обошли и вес меньше min
                    if (v[i] == 1 && d[i] < min)
                    {
                        // Переприсваиваем значения
                        min = d[i];
                        minindex = i;
                    }
                }

                // Добавляем найденный минимальный вес к текущему весу вершины и сравниваем с текущим минимальным весом вершины
                if (minindex != int.MaxValue)
                {
                    for (var i = 0; i < a.Length; i++)
                    {
                        if (a[minindex][i] > 0)
                        {
                            var temp = min + a[minindex][i];

                            if (temp < d[i])
                            {
                                d[i] = temp;
                            }
                        }
                    }

                    v[minindex] = 0;
                }
            } while (minindex < int.MaxValue);

            return d;
        }

        private static int[] CirculantDejkstraPath(int nodeIndex, int length, int[] generatrixes)
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
                    for (var i = 0; i < generatrixes.Length; i++)
                    {
                        var temp = min + 1;
                        if (temp < d[minIndex + generatrixes[i]])
                        {
                            d[i] = temp;
                        }
                    }

                    v[minIndex] = 0;
                }
            } while (minIndex < int.MaxValue);

            return d;
        }

        private static float[] SolveDiamAndAverDiam(byte[][] a)
        {
            var maxDiam = float.MinValue;
            float averDiam = 0;

            var allMoves = 0; // = nodes_count*(nodes_count-1)/2

            // nodes_count было, стало 1
            for (var i = 0; i < 1; i++)
            {
                var d = DejkstraPath(i, a);
                allMoves += a.Length - i - 1;

                for (var j = 1 + i; j < a.Length; j++)
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

        private List<CirculantParameters> ParallelSearchOptimalGraphs(int nodesCount, int grade, bool byAvgDiam = false, bool onlyS1EqualOne = false, int threadCount = 0)
        {
            var diam = float.MaxValue;
            var avgDiam = float.MaxValue;
            var lockEqual = new object();

            var indexer = new int[grade];
            var emptyMatrix = new byte[nodesCount][];
            emptyMatrix = emptyMatrix.Select(x => new byte[nodesCount]).ToArray();

            var optimalParams = new List<Tuple<int[], long>>();

            for (var i = 0; i < grade; i++)
            {
                indexer[i] = i + 1;
            }

            if (_lastState.NotCheckedConfig != null)
            {
                indexer = _lastState.NotCheckedConfig;
                optimalParams.AddRange(_lastState.GoodConfigs.Select(x => new Tuple<int[], long>(x, 0)));
                diam = _lastState.Diameter;
                avgDiam = _lastState.AverageDiameter;
            }
            else
            {
                _lastState = new LastState();
            }

            var start = new Stopwatch();
            start.Start();

            var totalLazyIndexer = new List<ValueTuple<int[], bool>>();

            while (onlyS1EqualOne && indexer[0] < 2 || !onlyS1EqualOne && indexer[0] + grade <= nodesCount / 2 + 1)
            {
                var i = 0;

                while ((onlyS1EqualOne && indexer[0] < 2 || !onlyS1EqualOne && indexer[0] + grade <= nodesCount / 2 + 1) && i < 100)
                {
                    totalLazyIndexer.Add(new ValueTuple<int[], bool>(indexer.ToArray(), false));
                    SpecialIncrement(emptyMatrix.Length, ref indexer, onlyS1EqualOne);
                    i++;
                }

                lock (_forcedLock)
                {
                    var result = Parallel.ForEach(totalLazyIndexer, new ParallelOptions { MaxDegreeOfParallelism = threadCount > 0 ? threadCount : int.MaxValue }, ints =>
                      {
                          if (_forcedStop)
                          {
                              return;
                          }

                          var matrix = GenerateGraphMatrixByGeneratrix(ints.Item1, ref emptyMatrix);

                          //var startT = new Stopwatch();
                          //startT.Start();

                          var tempResult = SolveDiamAndAverDiam(matrix);

                          //startT.Stop();

                          var isBest = false;
                          var isEqual = false;

                          lock (lockEqual)
                          {
                              if (!byAvgDiam && diam >= tempResult[0] || byAvgDiam && avgDiam >= tempResult[1])
                              {
                                  isBest = avgDiam > tempResult[1] || diam > tempResult[0];
                                  isEqual = !isBest && Math.Abs(diam - tempResult[0]) < Eps && Math.Abs(avgDiam - tempResult[1]) < Eps;
                              }

                              if (isBest)
                              {
                                  if (avgDiam > tempResult[1] || diam > tempResult[0])
                                  {
                                      optimalParams.Clear();

                                      diam = tempResult[0];
                                      avgDiam = tempResult[1];
                                  }
                              }

                              if (isBest || isEqual)
                              {
                                  optimalParams.Add(new Tuple<int[], long>(ints.Item1, 0));
                              }
                          }

                          var foundIndexerIndex = totalLazyIndexer.FindIndex(tuple => Equals(tuple.Item1, ints.Item1));

                          if (foundIndexerIndex > -1)
                          {
                              totalLazyIndexer[foundIndexerIndex] = new ValueTuple<int[], bool>(ints.Item1, true);
                          }
                      });

                    while (!result.IsCompleted)
                    {
                        if (_forcedStop)
                        {
                            start.Stop();

                            _lastState.NotCheckedConfig = totalLazyIndexer.Where(x => !x.Item2).OrderBy(x => x.Item1, new ArrayComparer()).FirstOrDefault().Item1;
                            _lastState.GoodConfigs = optimalParams.Select(x => x.Item1).ToArray();
                            _lastState.AverageDiameter = avgDiam;
                            _lastState.Diameter = diam;
                            _lastState.Grade = grade;

                            return null;
                        }
                    }

                    if (_forcedStop)
                    {
                        start.Stop();

                        _lastState.NotCheckedConfig = totalLazyIndexer.Where(x => !x.Item2).OrderBy(x => x.Item1, new ArrayComparer()).FirstOrDefault().Item1;
                        _lastState.GoodConfigs = optimalParams.Select(x => x.Item1).ToArray();
                        _lastState.AverageDiameter = avgDiam;
                        _lastState.Diameter = diam;
                        _lastState.Grade = grade;

                        return null;
                    }
                }

                totalLazyIndexer.Clear();
            }

            start.Stop();

            return optimalParams.Select(x => new CirculantParameters
            {
                AverageLength = avgDiam,
                Diameter = diam,
                Generatrixes = x.Item1,
                NodesCount = nodesCount,
                Ticks = x.Item2,
                TotalMilliseconds = start.ElapsedMilliseconds
            }).ToList();
        }

        public static List<CirculantParameters> SearchOptimalGraphs(int nodesCount, int grade, bool byAvgDiam = false, bool onlyS1EqualOne = false)
        {
            var diam = float.MaxValue;
            var avgDiam = float.MaxValue;
            
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

                //var startT = new Stopwatch();
                //startT.Start();

                var tempResult = SolveDiamAndAverDiam(matrix);

                //startT.Stop();

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
                Generatrixes = x.Item1,
                NodesCount = nodesCount,
                Ticks = x.Item2,
                TotalMilliseconds = start.ElapsedMilliseconds
            }).ToList();
        }
    }
}