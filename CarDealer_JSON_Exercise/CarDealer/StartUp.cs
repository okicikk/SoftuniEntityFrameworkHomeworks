using CarDealer.Data;
using CarDealer.DTOs.Import;
using CarDealer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main()
        {
            var carDealer = new CarDealerContext();
            //string json = File.ReadAllText("../../../Datasets/sales.json");
            Console.WriteLine(GetSalesWithAppliedDiscount(carDealer));
        }
        //9
        public static string ImportSuppliers(CarDealerContext context, string inputJson)
        {
            var suppliers = JsonConvert.DeserializeObject<List<Supplier>>(inputJson);
            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();
            return $"Successfully imported {suppliers.Count}."; ;
        }

        //10
        public static string ImportParts(CarDealerContext context, string inputJson)
        {
            var parts = JsonConvert.DeserializeObject<List<Part>>(inputJson);

            parts.RemoveAll(p => !context.Suppliers.Any(s => s.Id == p.SupplierId));

            context.Parts.AddRange(parts);
            context.SaveChanges();

            return $"Successfully imported {parts.Count}.";
        }

        //11
        public static string ImportCars(CarDealerContext context, string inputJson)
        {
            var carsToBeAdded = new List<Car>();
            var partCarsToBeAdded = new List<PartCar>();

            var carsDto = JsonConvert.DeserializeObject<List<CarDto>>(inputJson);
            foreach (var c in carsDto)
            {
                Car car = new Car()
                {
                    Make = c.Make,
                    Model = c.Model,
                    TraveledDistance = c.TraveledDistance,
                };
                foreach (var partId in c.PartsId.Distinct())
                {
                    Part part = context.Parts.Find(partId);
                    if (part != null)
                    {
                        PartCar partCar = new PartCar()
                        {
                            Car = car,
                            Part = part
                        };
                        car.PartsCars.Add(partCar);
                    }
                }
                carsToBeAdded.Add(car);
            }

            context.Cars.AddRange(carsToBeAdded);

            context.SaveChanges();

            return $"Successfully imported {carsDto.Count}.";
        }
        //12
        public static string ImportCustomers(CarDealerContext context, string inputJson)
        {

            var customers = JsonConvert.DeserializeObject<List<Customer>>(inputJson);

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Count}.";
        }
        //13 
        public static string ImportSales(CarDealerContext context, string inputJson)
        {
            var salesToBeAdded = new HashSet<Sale>();


            var salesDto = JsonConvert.DeserializeObject<List<ImportSalesDto>>(inputJson);
            foreach (var s in salesDto)
            {
                Sale sale = new Sale()
                {
                    Discount = s.Discount,
                    Car = context.Cars.Find(s.CarId),
                    Customer = context.Customers.Find(s.CustomerId)
                };
                salesToBeAdded.Add(sale);
            }

            context.Sales.AddRange(salesToBeAdded);
            context.SaveChanges();
            return $"Successfully imported {salesDto.Count}.";
        }
        //14
        public static string GetOrderedCustomers(CarDealerContext context)
        {
            var orderedCustomers = JsonConvert
                .SerializeObject(context.Customers
                .OrderBy(c => c.BirthDate)
                .ThenBy(c => c.IsYoungDriver)
                .Select(c => new
                {
                    Name = c.Name,
                    BirthDate = $"{c.BirthDate:dd/MM/yyyy}",
                    IsYoungDriver = c.IsYoungDriver,
                })
                .ToList()
                , SerializeSettings());

            return orderedCustomers;
        }

        //15
        public static string GetCarsFromMakeToyota(CarDealerContext context)
        {
            var carsFromToyota = context.Cars
                .Select(c => new
                {
                    Id = c.Id,
                    Make = c.Make,
                    Model = c.Model,
                    TraveledDistance = c.TraveledDistance
                })
                .Where(c => c.Make == "Toyota")
                .OrderBy(c => c.Model)
                .ThenByDescending(c => c.TraveledDistance)
                .ToList();
            return JsonConvert.SerializeObject(carsFromToyota, SerializeSettings());
        }

        //16
        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var suppliers = context.Suppliers
                .Where(s => s.IsImporter == false)
                .Select(s => new
                {
                    Id = s.Id,
                    Name = s.Name,
                    PartsCount = s.Parts.Count
                })
                .ToList();

            return JsonConvert.SerializeObject(suppliers, SerializeSettings());
        }

        //17
        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var carsWithParts = context.Cars
                .Select(c => new
                {
                    car = new
                    {
                        Make = c.Make,
                        Model = c.Model,
                        TraveledDistance = c.TraveledDistance,
                    },
                    parts = c.PartsCars
                    .Select(pc => new
                    {
                        Name = pc.Part.Name,
                        Price = pc.Part.Price.ToString("f2")
                    }).ToList()
                })
                .ToList();

            return JsonConvert.SerializeObject(carsWithParts, SerializeSettings());
        }

        //18
        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var customers = context.Customers
                .Where(c => c.Sales.Any())
                .Select(c => new
                {
                    fullName = c.Name,
                    boughtCars = c.Sales.Count,
                    spentMoney = c.Sales.SelectMany(s=>s.Car.PartsCars).Sum(pc=>pc.Part.Price)
                })
                .OrderByDescending(c=>c.spentMoney)
                .ThenByDescending(c => c.boughtCars)
                .ToList();
            return JsonConvert.SerializeObject(customers, SerializeSettings());
        }

        //19
        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var sales = context.Sales
                .Select(s => new
                {
                    car = new
                    {
                        Make = s.Car.Make,
                        Model = s.Car.Model,
                        TraveledDistance = s.Car.TraveledDistance
                    },

                    customerName = s.Customer.Name,
                    discount = s.Discount.ToString("f2"),
                    price = s.Car.PartsCars.Sum(pc=>pc.Part.Price).ToString("f2"),
                    priceWithDiscount = $"{Math.Round(s.Car.PartsCars.Sum(pc => pc.Part.Price) * ((100 - s.Discount) / 100),2):f2}"
                })
                .Take(10)
                .ToList();
            return JsonConvert.SerializeObject(sales, SerializeSettings());
        }
        private static JsonSerializerSettings SerializeSettings()
        {
            return new JsonSerializerSettings
            {

                Formatting = Formatting.Indented,
            };
        }
    }
}