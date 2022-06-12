using PCG.Library.Models.GeneratorObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PCG.Library.Models.Generators
{
    public class OneThreadCirculantGenerator : BasicCirculantGenerator
    {
        protected override List<CirculantParameters> GenerateOptimalCirculants(int nodesCount, int grade, bool byAvgDiam = false, bool onlyS1EqualOne = false)
        {
            return SingleSearchOptimalGraphs(nodesCount, grade, byAvgDiam, onlyS1EqualOne);
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

        public static float[] SolveDiamAndAverDiam(int length, byte[][] matrix)
        {
            var maxDiam = float.MinValue;
            float averDiam = 0;
            var allMoves = 0;

            // nodes_count было, стало 1
            for (var i = 0; i < 1; i++)
            {
                var d = DejkstraPath(i, matrix);
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

        private List<CirculantParameters> SingleSearchOptimalGraphs(int nodesCount, int grade, bool byAvgDiam = false, bool onlyS1EqualOne = false, int limit = -1)
        {
            var diam = float.MaxValue;
            var avgDiam = float.MaxValue;
            var indexer = new int[grade];
            var optimalParams = new List<Tuple<int[], long>>();

            for (var i = 0; i < grade; i++)
            {
                indexer[i] = i + 1;
            }

            //LastState.CurrentNodesCount = nodesCount;

            //if (LastState.NotCheckedConfig != null)
            //{
            //    indexer = LastState.NotCheckedConfig;
            //    optimalParams.AddRange(LastState.GoodConfigs.Select(x => new Tuple<int[], long>(x, 0)));
            //    diam = LastState.Diameter;
            //    avgDiam = LastState.AverageDiameter;
            //}
            //else
            //{
            //    var nodeDescription = LastState.NodesDescription;

            //    LastState = new LastState
            //    {
            //        NodesDescription = nodeDescription
            //    };
            //}

            var start = Stopwatch.StartNew();
            var srcMatrix = Enumerable.Repeat(new byte[nodesCount], nodesCount).ToArray();

            while (onlyS1EqualOne && indexer[0] < 2 || !onlyS1EqualOne && indexer[0] + grade <= nodesCount / 2 + 1)
            {
                var startA = Stopwatch.StartNew();
                var matrix = GenerateGraphMatrixByGeneratrix(indexer, ref srcMatrix);

                startA.Stop();

                var startT = Stopwatch.StartNew();
                var tempResult = SolveDiamAndAverDiam(nodesCount, matrix);

                var isBest = false;
                var isEqual = false;

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
                    optimalParams.Add(new Tuple<int[], long>(indexer.ToArray(), startT.ElapsedMilliseconds));
                }

                SpecialIncrement(nodesCount, ref indexer, onlyS1EqualOne);
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