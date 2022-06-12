using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PCG.Library.Utilities;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var nodes = new Tuple<int, int>[]
            {
            //    new Tuple<int, int>(1000, 100),
            //    new Tuple<int, int>(2500, 100),
            //    new Tuple<int, int>(5000, 50),
                new Tuple<int, int>(10000, 10),
                new Tuple<int, int>(20000, 10),
                new Tuple<int, int>(50000, 10),
                new Tuple<int, int>(100000, 10),
            };
            var grade = 3;

            foreach (var node in nodes)
            {
                var iters = BenchmarkUtilities.GetIterations(node.Item1, grade);


                var result2 = BenchmarkUtilities.Method2(node.Item1, grade, limit: node.Item2);

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var result1 = BenchmarkUtilities.BasicDijkstraMethod(node.Item1, grade, limit: node.Item2);

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }

        }
    }
}
