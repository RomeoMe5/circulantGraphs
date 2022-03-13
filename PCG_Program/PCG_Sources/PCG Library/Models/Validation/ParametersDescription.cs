using PCG.Library.Models.GeneratorObjects;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PCG.Library.Models.Validation
{
    [XmlRoot("ParametersDescription")]
    public class ParametersDescription
    {
        [XmlAttribute("isFinished")]
        public bool IsFinished { get; set; }

        [XmlElement]
        public LastState LastState { get; set; }

        [XmlArray("OptimalDescriptions")]
        public List<CirculantParameters> OptimalCirculants { get; set; }

        [XmlArray("AllDescriptions")]
        public List<CirculantParameters> AllCirculants { get; set; }
    }
}