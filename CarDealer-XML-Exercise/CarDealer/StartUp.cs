using CarDealer.Data;
using CarDealer.DTOs.Export;
using CarDealer.DTOs.Import;
using CarDealer.Models;
using Castle.Core.Resource;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main()
        {
            CarDealerContext context = new CarDealerContext();
            string inputXml = File.ReadAllText("../../../Datasets/suppliers.xml");
            Console.WriteLine(GetSalesWithAppliedDiscount(context));
        }


        //9
        public static string ImportSuppliers(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<SupplierImportDto>), new XmlRootAttribute("Suppliers"));
            var suppliersDto = new List<SupplierImportDto>();

            using (var reader = new StringReader(inputXml))
            {
                suppliersDto = (List<SupplierImportDto>)xmlSerializer.Deserialize(reader);
            }
            var suppliers = new List<Supplier>();
            foreach (var s in suppliersDto)
            {
                Supplier supplier = new Supplier()
                {
                    Name = s.Name,
                    IsImporter = true
                };
                suppliers.Add(supplier);
            }
            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();
            return $"Successfully imported {suppliers.Count}";
        }

        //10
        public static string ImportParts(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<PartImportDto>), new XmlRootAttribute("Parts"));
            var partsDto = new List<PartImportDto>();
            using (var reader = new StringReader(inputXml))
            {
                partsDto = (List<PartImportDto>)xmlSerializer.Deserialize(reader);
            }
            var parts = new List<Part>();
            foreach (var p in partsDto)
            {
                if (!context.Suppliers.Any(s => s.Id == p.SupplierId))
                {
                    continue;
                }
                Part part = new Part()
                {
                    Name = p.Name,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    SupplierId = p.SupplierId
                };
                parts.Add(part);
            }
            context.Parts.AddRange(parts);
            context.SaveChanges();
            return $"Successfully imported {parts.Count}";
        }

        //11
        public static string ImportCars(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<CarsImportDto>), new XmlRootAttribute("Cars"));

            var carsDto = new List<CarsImportDto>();

            using (var reader = new StringReader(inputXml))
            {
                carsDto = (List<CarsImportDto>)xmlSerializer.Deserialize(reader);
            }
            var cars = new List<Car>();
            foreach (var c in carsDto)
            {
                List<Part> parts = new List<Part>();

                Car car = new Car()
                {
                    Make = c.Make,
                    Model = c.Model,
                    TraveledDistance = c.TraveledDistance
                };

                foreach (var p in c.Parts.Select(p => p.PartId).Distinct())
                {
                    Part part = context.Parts.Find(p);
                    if (part == null)
                    {
                        continue;
                    }
                    car.PartsCars.Add(new PartCar()
                    {
                        CarId = car.Id,
                        PartId = part.Id
                    });
                }
                cars.Add(car);
            }
            context.AddRange(cars);
            context.SaveChanges();
            return $"Successfully imported {cars.Count}";
        }

        //12
        public static string ImportCustomers(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<CustomerImportDto>), new XmlRootAttribute("Customers"));
            var customersDto = new List<CustomerImportDto>();
            using (var reader = new StringReader(inputXml))
            {
                customersDto = (List<CustomerImportDto>)xmlSerializer.Deserialize(reader);
            }
            var customers = new List<Customer>();
            foreach (var c in customersDto)
            {
                customers.Add(new Customer()
                {
                    Name = c.Name,
                    BirthDate = c.BirthDate,
                    IsYoungDriver = c.IsYoungDriver,
                });
            }
            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Count}";
        }
        //13
        public static string ImportSales(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<SalesImportDto>), new XmlRootAttribute("Sales"));

            List<SalesImportDto> salesDto = new();

            using (var reader = new StringReader(inputXml))
            {
                salesDto = (List<SalesImportDto>)xmlSerializer.Deserialize(reader);
            }
            var sales = new List<Sale>();
            foreach (var s in salesDto)
            {
                Car car = context.Cars.Find(s.CarId);
                if (car is null)
                {
                    continue;
                }
                Sale sale = new Sale()
                {
                    Discount = s.Discount,
                    Car = car,
                    CustomerId = s.CustomerId,
                };
                sales.Add(sale);
            }
            context.Sales.AddRange(sales);
            context.SaveChanges();
            return $"Successfully imported {sales.Count}";
        }

        //14
        public static string GetCarsWithDistance(CarDealerContext context)
        {
            var cars = context.Cars
                .Where(c => c.TraveledDistance > 2_000_000)
                .OrderBy(c => c.Make)
                .ThenBy(c => c.Model)
                .Take(10)
                .ToList();
            List<CarsWithDistanceExportDto> carsDto = new();
            foreach (var c in cars)
            {
                carsDto.Add(new CarsWithDistanceExportDto()
                {
                    Make = c.Make,
                    Model = c.Model,
                    TraveledDistance = c.TraveledDistance,
                });
            }
            return SerializeToXml(carsDto, "cars");
        }

        //15
        public static string GetCarsFromMakeBmw(CarDealerContext context)
        {
            var cars = context.Cars
                .Where(c => c.Make.ToLower() == "bmw")
                .OrderBy(c => c.Model)
                .ThenByDescending(c => c.TraveledDistance)
                .ToList();
            var carsDto = new List<CarsFromMakeExportDto>();
            foreach (var c in cars)
            {
                carsDto.Add(new()
                {
                    Id = c.Id,
                    Model = c.Model,
                    TraveledDistance = c.TraveledDistance
                });
            }
            return SerializeToXml(carsDto, "cars");
        }

        //16
        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var suppliers = context.Suppliers
                .Where(s => s.IsImporter == false)
                .Select(s => new LocalSupplierExportDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    PartsCount = s.Parts.Count
                })
                .ToList();
            var suppliersDto = new List<LocalSupplierExportDto>();
            foreach (var s in suppliers)
            {
                suppliersDto.Add(new LocalSupplierExportDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    PartsCount = s.PartsCount
                });
            }
            return SerializeToXml(suppliersDto, "suppliers");
        }
        //17
        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var cars = context.Cars
                .Select(c => new
                {
                    Make = c.Make,
                    Model = c.Model,
                    TraveledDistance = c.TraveledDistance,
                    Parts = c.PartsCars.Select(p => new
                    {
                        Name = p.Part.Name,
                        Price = p.Part.Price
                    })
                    .OrderByDescending(p => p.Price)
                    .ToList(),
                })
                .OrderByDescending(c => c.TraveledDistance)
                .ThenBy(c => c.Model)
                .Take(5)
                .ToList();

            var carsDto = new List<CarsWithPartsExportDto>();

            foreach (var c in cars)
            {
                List<PartsExportDto> parts = new();
                foreach (var p in c.Parts)
                {
                    parts.Add(new PartsExportDto
                    {
                        Name = p.Name,
                        Price = Math.Round(p.Price, 2)
                    });
                }
                carsDto.Add(new CarsWithPartsExportDto
                {
                    Make = c.Make,
                    Model = c.Model,
                    TraveledDistance = c.TraveledDistance,
                    Parts = parts.ToArray()
                });
            }
            return SerializeToXml(carsDto, "cars");
        }
        //18
        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var customers = context.Customers
                .Where(c => c.Sales.Any())
                .Select(c => new
                {
                    FullName = c.Name,
                    BoughtCars = c.Sales.Count,
                    SpentMoney =
                     c.IsYoungDriver == false
                    ? c.Sales.SelectMany(c => c.Car.PartsCars).Sum(pc => (double)pc.Part.Price)
                    : c.Sales.SelectMany(c => c.Car.PartsCars).Sum(pc => Math.Round((double)pc.Part.Price * 0.95, 2))
                })
                .ToList()
                .OrderByDescending(c => c.SpentMoney)
                .ToList();

            var customersDto = new List<TotalCustomerSalesExportDto>();
            foreach (var c in customers)
            {
                customersDto.Add(new TotalCustomerSalesExportDto()
                {
                    FullName = c.FullName,
                    BoughtCars = c.BoughtCars,
                    SpentMoney = c.SpentMoney.ToString("f2")
                });
            }
            return SerializeToXml(customersDto, "customers");
        }

        //19
        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var sales = context.Sales
                .Select(s => new SalesWithDiscountExportDto()
                {
                    Car = new CarInfoDto
                    {
                        Make = s.Car.Make,
                        Model = s.Car.Model,
                        TraveledDistance = s.Car.TraveledDistance
                    },
                    Discount = (int)s.Discount,
                    CustomerName = s.Customer.Name,
                    Price = s.Car.PartsCars.Sum(p => p.Part.Price),
                    PriceWithDiscount =
                        Math.Round((double)(s.Car.PartsCars
                            .Sum(p => p.Part.Price) * (1 - (s.Discount / 100))), 4)
                })
                .ToList();
            return SerializeToXml(sales, "sales");
        }

        //Serialize Method for List Of Type
        private static string SerializeToXml<T>(List<T> dto, string xmlRoot)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<T>), new XmlRootAttribute(xmlRoot));

            var namespaces = new XmlSerializerNamespaces();

            namespaces.Add("", "");

            using (var writer = new StringWriter())
            {
                xmlSerializer.Serialize(writer, dto, namespaces);
                string result = writer.ToString();
                return result;
            }
        }
    }
}