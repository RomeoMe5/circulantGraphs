using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PCG.Library.Models.Generators.Optimus
{
    public class ParallelCirculantGenerator : BasicCirculantGenerator
    {
        protected override List<CirculantParameters> GenerateOptimalCirculants(int nodesCount, int grade, bool byAvgDiam = false, bool onlyS1EqualOne = false)
        {
            return ParallelSearchOptimalGraphs(nodesCount, grade, byAvgDiam, onlyS1EqualOne);
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
                    foreach (var x in generatrixes)
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

        private static float[] SolveDiamAndAverDiam(int length, int[] generatrixes)
        {
            var maxDiam = float.MinValue;
            float averDiam = 0;

            var allMoves = 0; // = nodes_count*(nodes_count-1)/2

            // Вывод матрицы расстояний - раскомментировать для дебагал
            // std::wcout << L"Вывод расстояний:" << std::endl;

            // nodes_count было, стало 1
            for (var i = 0; i < 1; i++)
            {
                var d = CirculantDejkstraPath(i, length, generatrixes);
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

        private List<CirculantParameters> ParallelSearchOptimalGraphs(int nodesCount, int grade, bool byAvgDiam = false, bool onlyS1EqualOne = false, int threadCount = 0)
        {
            var diam = float.MaxValue;
            var avgDiam = float.MaxValue;
            var lockEqual = new object();
            var indexer = new int[grade];
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

                var nodeDescription = _lastState.NodesDescription;

                _lastState = new LastState
                {
                    NodesDescription = nodeDescription
                };
            }

            var start = Stopwatch.StartNew();
            var totalLazyIndexer = new List<ValueTuple<int[], bool>>();

            while (onlyS1EqualOne && indexer[0] < 2 || !onlyS1EqualOne && indexer[0] + grade <= nodesCount / 2 + 1)
            {
                var i = 0;

                while ((onlyS1EqualOne && indexer[0] < 2 || !onlyS1EqualOne && indexer[0] + grade <= nodesCount / 2 + 1) && i < 100)
                {
                    totalLazyIndexer.Add(new ValueTuple<int[], bool>(indexer.ToArray(), false));
                    SpecialIncrement(nodesCount, ref indexer, onlyS1EqualOne);
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

                          var startT = Stopwatch.StartNew();
                          var tempResult = SolveDiamAndAverDiam(nodesCount, ints.Item1);

                          startT.Stop();

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
                                  optimalParams.Add(new Tuple<int[], long>(ints.Item1, startT.ElapsedMilliseconds));
                              }
                          }

                          var ololo = totalLazyIndexer.FindIndex(tuple => Equals(tuple.Item1, ints.Item1));

                          if (ololo > -1)
                          {
                              totalLazyIndexer[ololo] = new ValueTuple<int[], bool>(ints.Item1, true);
                          }
                      });

                    while (!result.IsCompleted)
                    {
                        if (_forcedStop)
                        {
                            return SaveLastState(grade, start, totalLazyIndexer, indexer, optimalParams, avgDiam, diam);
                        }
                    }

                    if (_forcedStop)
                    {
                        return SaveLastState(grade, start, totalLazyIndexer, indexer, optimalParams, avgDiam, diam);
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

        private List<CirculantParameters> SaveLastState(int grade, Stopwatch start, List<(int[], bool)> totalLazyIndexer, int[] indexer, List<Tuple<int[], long>> optimalParams, float avgDiam, float diam)
        {
            start.Stop();

            _lastState.NotCheckedConfig = totalLazyIndexer.Where(x => !x.Item2).OrderBy(x => x.Item1, new ArrayComparer()).FirstOrDefault().Item1 ?? indexer;
            _lastState.GoodConfigs = optimalParams.Select(x => x.Item1).ToArray();
            _lastState.AverageDiameter = avgDiam;
            _lastState.Diameter = diam;
            _lastState.Grade = grade;

            return null;
        }

        public static List<CirculantParameters> SearchOptimalGraphs(int nodesCount, int grade, bool byAvgDiam = false, bool onlyS1EqualOne = false)
        {
            var diam = float.MaxValue;
            var avgDiam = float.MaxValue;

            //int* optimal_link = new int[grade + 1];
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

                var tempResult = (float[])null;  //SolveDiamAndAverDiam(matrix);

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