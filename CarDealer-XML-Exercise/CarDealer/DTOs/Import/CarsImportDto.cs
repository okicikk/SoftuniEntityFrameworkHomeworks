using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CarDealer.DTOs.Import
{
    [XmlType("Car")]
    public class CarsImportDto
    {
        [XmlElement("make")]
        public string Make { get; set; }
        [XmlElement("model")]
        public string Model { get; set; }
        [XmlElement("traveledDistance")]
        public long TraveledDistance { get; set; }
        [XmlArray("parts")]
        [XmlArrayItem("partId")]
        public PartIdDto[] Parts { get; set; }
    }

    public class PartIdDto
    {
        [XmlAttribute("id")]
        public int PartId { get; set; }
    }
}
