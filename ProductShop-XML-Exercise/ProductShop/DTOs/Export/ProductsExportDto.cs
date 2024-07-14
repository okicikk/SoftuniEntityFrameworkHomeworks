using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ProductShop.DTOs.Export
{
    [XmlType("Product")]
    public class ProductsExportDto
    {
        [XmlElement("name")]
        public string Name { get; set; }
        [XmlElement("price")]
        public string Price { get; set; }
        [XmlElement("buyer")]
        public string? Buyer { get; set; }
    }
}
