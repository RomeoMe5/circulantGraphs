using Microsoft.Win32;
using PCG.Library.Models.Validation;
using PCG.Library.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace PCG.Validation
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var file = ReadFileFromFolder();

            if (!string.IsNullOrEmpty(file.Item1))
            {
                string xml = null;

                try
                {
                    var bytes = Convert.FromBase64String(file.Item1);

                    if (bytes.Any())
                    {
                        xml = Encoding.UTF8.GetString(bytes);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cant convert from base64. Error message: {0}", ex.Message);
                }

                if (!string.IsNullOrEmpty(xml))
                {
                    var basicInfo = ReportBuilder.GetBasicWorkInformation(xml);

                    if (basicInfo != null)
                    {
                        var folderName = $"Output\\{basicInfo.ValidFullName}";

                        var dirInfo = new DirectoryInfo(folderName);

                        if (!dirInfo.Exists)
                        {
                            dirInfo.Create();
                        }

                        var localPath = Path.Combine(folderName, "total.csv");

                        if (!File.Exists(localPath))
                        {
                            File.AppendAllText(localPath, BasicWorkInformation.HeadersString + Environment.NewLine, new UTF8Encoding(true));
                        }

                        foreach (var desc in basicInfo.ExtendedInfo.Descriptions)
                        {
                            if (desc.IsFinished)
                            {
                                SuperUtilities.SaveResultsInFolder(desc.OptimalCirculants, Path.Combine(folderName, "finish"));
                            }
                            else
                            {
                                SuperUtilities.SaveLastState(desc.LastState, Path.Combine(folderName, "not-finish", $"laststate-{desc.LastState.CurrentNodesCount}.xml"));
                                SuperUtilities.SaveResultsInFolder(desc.OptimalCirculants, Path.Combine(folderName, "not-finish"));
                            }
                        }

                        File.AppendAllText(localPath, basicInfo.ToCsvString(file.Item2), new UTF8Encoding(true));
                    }
                }
            }

            Console.WriteLine("Application end. Press any button for exit");
            Console.ReadKey(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        private static void DecryptFile(string filePath)
        {
            var allText = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;

            if (string.IsNullOrEmpty(allText))
            {
                Console.WriteLine("Can`t read text file.");
                return;
            }

            byte[] bytes;

            try
            {
                bytes = Convert.FromBase64String(allText);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удалось конвертировать в массив байтов, т.к. формат текста не в BASE64. Текст ошибки: {0}", ex.Message);
                return;
            }

            allText = Encoding.UTF8.GetString(bytes);
        }

        private static string RemoveInvalidChars(string original)
        {
            foreach (var ch in Path.GetInvalidPathChars())
            {
                original = original.Replace(ch.ToString(), "");
            }

            return original;
        }

        /// <summary>
        /// Open files and try to read string 
        /// </summary>
        /// <returns></returns>
        private static Tuple<string, string> ReadFileFromFolder()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = false,
                DefaultExt = ".bin"
            };

            if (dialog.ShowDialog() ?? false)
            {
                if (dialog.CheckFileExists)
                {
                    var fileName = dialog.FileName;

                    using (var reader = new StreamReader(dialog.OpenFile()))
                    {
                        var base64 = reader.ReadToEnd();

                        return new Tuple<string, string>(base64, fileName);
                    }
                }
            }

            Console.WriteLine("Can`t read file.");

            return new Tuple<string, string>(string.Empty, string.Empty);
        }
    }
}
