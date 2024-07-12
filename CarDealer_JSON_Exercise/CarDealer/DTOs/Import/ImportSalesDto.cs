using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarDealer.DTOs.Import
{
    public class ImportSalesDto
    {
        public int CarId { get; set; }
        public int CustomerId { get; set; }
        public int Discount { get; set; }

    }
}
