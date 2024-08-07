﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CarDealer.DTOs.Export
{
    [XmlType("sale")]
    public class SalesWithDiscountExportDto
    {
        [XmlElement("car")]
        public CarInfoDto Car { get; set; }
        [XmlElement("discount")]
        public decimal Discount { get; set; }
        [XmlElement("customer-name")]
        public string CustomerName { get; set; }
        [XmlElement("price")]
        public decimal Price { get; set; }
        [XmlElement("price-with-discount")]
        public double PriceWithDiscount { get; set; }
    }
    public class CarInfoDto
    {
        [XmlAttribute("make")]
        public string Make { get; set; }
        [XmlAttribute("model")]
        public string Model { get; set; }
        [XmlAttribute("traveled-distance")]
        public long TraveledDistance { get; set; }
    }
}
