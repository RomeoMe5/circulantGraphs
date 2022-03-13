using System.Collections.Generic;
using System.Xml.Serialization;

namespace PCG.Library.Models.Validation
{
    [XmlRoot("AdditionalInfo")]
    public class ExtendedInformation
    {
        [XmlAttribute("allIncluded")]
        public bool AllIncluded { get; set; }

        [XmlArray("ParametersDescriptions")]
        public List<ParametersDescription> Descriptions { get; set; }
    }
}