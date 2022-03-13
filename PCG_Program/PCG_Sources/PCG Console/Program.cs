using PCG.Library.Models.Generators;
using PCG.Library.Utilities;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using PCG.Library.Models.GeneratorObjects;

namespace PCG_Console
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            var yourTask = SuperUtilities.LoadTask(new FileInfo(ConfigurationManager.AppSettings["FirstTaskPath"]).FullName);
            var circulantGenerator = new OptimizedParallelCirculantGenerator();

            Console.WriteLine("The application is started");

            var task = Task.Run(() =>
            {
                circulantGenerator.Start(yourTask);
            });

            var stopped = false;

            do
            {
                if (stopped)
                {
                    continue;
                }

                Console.WriteLine("Type 'stop' for stop operation.");
                var line = Console.ReadLine();

                if (!(line?.Equals("stop", StringComparison.InvariantCultureIgnoreCase) ?? false))
                {
                    continue;
                }

                Console.WriteLine("Program is stopping, please wait.");

                circulantGenerator.Stop();
                stopped = true;
                //thread?.Abort();
            } while (!(task.IsCompleted || task.IsCanceled || task.IsFaulted));
        }
    }
}
