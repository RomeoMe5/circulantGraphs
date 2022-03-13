using PCG.Library.Models.Validation;
using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace PCG.Library.Utilities
{
    public static class ReportBuilder
    {
        /// <summary>
        /// Base xml deserializer 
        /// </summary>
        /// <param name="xmlText"></param>
        /// <returns></returns>
        public static BasicWorkInformation GetBasicWorkInformation(string xmlText)
        {
            var xmlSerialize = new XmlSerializer(typeof(BasicWorkInformation));

            using (var rs = new StringReader(xmlText))
            {
                return (BasicWorkInformation)xmlSerialize.Deserialize(rs);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="filePath"></param>
        public static void SaveInfo(BasicWorkInformation state, string filePath)
        {
            var xmlSerialize = new XmlSerializer(typeof(BasicWorkInformation));
            var fileInfo = new FileInfo(filePath);

            if (!string.IsNullOrEmpty(fileInfo.DirectoryName) && !Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            var sb = new StringBuilder();

            using (var rs = new StringWriter(sb))
            {
                xmlSerialize.Serialize(rs, state);
            }

            File.WriteAllText(filePath, Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString())));
        }
    }
}
