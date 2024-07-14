using AutoMapper.Configuration.Annotations;
using ProductShop.Data;
using ProductShop.DTOs.Export;
using ProductShop.DTOs.Import;
using ProductShop.Models;
using System.Runtime.ExceptionServices;
using System.Xml.Serialization;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main()
        {
            ProductShopContext context = new ProductShopContext();
            //string input = File.ReadAllText("../../../Datasets/categories-products.xml");
            Console.WriteLine(GetProductsInRange(context));
        }
        //1
        public static string ImportUsers(ProductShopContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<UserImportDto>), new XmlRootAttribute("Users"));

            List<UserImportDto> usersDto = new();
            using (var reader = new StringReader(inputXml))
            {
                usersDto = (List<UserImportDto>)xmlSerializer.Deserialize(reader);
            }

            List<User> users = new();
            foreach (var u in usersDto)
            {
                User user = new User()
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Age = u.Age,
                };
                users.Add(user);
            }

            context.Users.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Count}";
        }
        //2
        public static string ImportProducts(ProductShopContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ProductsImportDto>), new XmlRootAttribute("Products"));

            List<ProductsImportDto> productsDto = new List<ProductsImportDto>();

            using (StringReader reader = new StringReader(inputXml))
            {
                productsDto = (List<ProductsImportDto>)xmlSerializer.Deserialize(reader);
            }
            var products = new HashSet<Product>();
            foreach (var p in productsDto)
            {
                Product product = new Product()
                {
                    Name = p.Name,
                    Price = p.Price,
                    SellerId = p.SellerId,
                    BuyerId = p.BuyerId,
                };
                if (!context.Users.Any(u => u.Id == product.BuyerId))
                {
                    product.BuyerId = default;
                }
                products.Add(product);
            }

            context.Products.AddRange(products);
            context.SaveChanges();
            return $"Successfully imported {products.Count}";
        }
        //3
        public static string ImportCategories(ProductShopContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<CategoryImportDto>), new XmlRootAttribute("Categories"));

            var categoryDto = new List<CategoryImportDto>();

            using (var reader = new StringReader(inputXml))
            {
                categoryDto = (List<CategoryImportDto>)xmlSerializer.Deserialize(reader);
            }
            var categories = new HashSet<Category>();
            foreach (var c in categoryDto)
            {
                if (c.Name is null)
                {
                    continue;
                }
                var category = new Category()
                {
                    Name = c.Name,
                };
                categories.Add(category);
            }
            context.Categories.AddRange(categories);
            context.SaveChanges();
            return $"Successfully imported {categories.Count}";
        }
        //4
        public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
        {
            XmlSerializer xmlSerializer =
                new XmlSerializer(typeof(List<CategoryProductImportDto>), new XmlRootAttribute("CategoryProducts"));

            var categoryProductsDto = new List<CategoryProductImportDto>();

            using (var reader = new StringReader(inputXml))
            {
                categoryProductsDto = (List<CategoryProductImportDto>)xmlSerializer.Deserialize(reader);
            }
            var categoriesProducts = new HashSet<CategoryProduct>();
            foreach (var cp in categoryProductsDto)
            {
                if ((!context.Products.Any(p => p.Id == cp.ProductId)) || (!context.Categories.Any(c => c.Id == cp.CategoryId)))
                {
                    continue;
                }
                CategoryProduct categoryProduct = new CategoryProduct()
                {
                    CategoryId = cp.CategoryId,
                    ProductId = cp.ProductId,
                };
                categoriesProducts.Add(categoryProduct);
            }
            context.CategoryProducts.AddRange(categoriesProducts);
            context.SaveChanges();
            return $"Successfully imported {categoriesProducts.Count}";
        }
        //5 Judge does not like this solution
        public static string GetProductsInRange(ProductShopContext context)
        {
            var productsInRange = context.Products
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .OrderBy(p => p.Price)
                .Select(p => new
                {
                    Name = p.Name,
                    Price = p.Price,
                    Buyer = p.Buyer,
                })
                .Take(10)
                .ToList();

            var productsDto = new List<ProductsExportDto>();

            foreach (var product in productsInRange)
            {
                ProductsExportDto productsExportDto = new ProductsExportDto()
                {
                    Name = product.Name,
                    Price = Math.Round(product.Price, 2).ToString("0.##"),
                    Buyer = product.Buyer == null
                    ? null
                    : product.Buyer.FirstName + " " + product.Buyer.LastName
                };
                productsDto.Add(productsExportDto);
            }
            return SerializeToXml(productsDto, "Products");
        }
        //6
        public static string GetSoldProducts(ProductShopContext context)
        {
            var soldProducts = context.Users
                .Where(u => u.ProductsSold.Any())
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Select(u => new
                {
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    soldProducts = u.ProductsSold.Select(sp => new Product
                    {
                        Name = sp.Name,
                        Price = sp.Price
                    }).ToList()
                })
                .Take(5)
                .ToList();
            var soldProductsDto = soldProducts.Select(sp => new SoldProductsExportDto()
            {
                FirstName = sp.firstName,
                LastName = sp.lastName,
                SoldProducts = sp.soldProducts.Select(s => new SoldProductInfoDto
                {
                    Name = s.Name,
                    Price = s.Price
                }).ToArray()
            }).ToList();
            return SerializeToXml(soldProductsDto, "Users");
        }
        //7
        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context.Categories
                .OrderByDescending(c => c.CategoryProducts.Select(cp => cp.Product).Count())
                .Select(c => new
                {
                    Name = c.Name,
                    Count = c.CategoryProducts.Count,
                    AveragePrice = c.CategoryProducts.Average(cp => cp.Product.Price),
                    TotalRevenue = Math.Round(c.CategoryProducts.Sum(cp => cp.Product.Price), 2)
                })
                .ToList();
            var categoriesDto = categories.Select(c => new CategoriesByProductsCountDto
            {
                Name = c.Name,
                Count = c.Count,
                AveragePrice = c.AveragePrice,
                TotalRevenue = c.TotalRevenue
            })
                .OrderByDescending(c => c.Count)
                .ThenBy(c => c.TotalRevenue)
                .ToList();

            return SerializeToXml(categoriesDto, "Categories");
        }

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = context.Users
                .Where(u => u.ProductsSold.Any())
                .OrderByDescending(u => u.ProductsSold.Count)
                .Select(u => new
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Age = u.Age,
                    SoldProducts = new 
                    {
                        Count = u.ProductsSold.Count,
                        Products = u.ProductsSold.Select(ps => new ProductsDto
                        {
                            Name = ps.Name,
                            Price = ps.Price
                        })
                        .OrderByDescending(p=>p.Price)
                        .ToList()
                    }
                })
                .Take(10)
                .ToList();
            var usersDto = new List<UsersWithProductsDto>();
            foreach (var u in users)
            {
                UsersWithProductsDto userDto = new()
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Age = u.Age,
                    SoldProducts = new()
                    {
                        Count = u.SoldProducts.Count,
                        Products = u.SoldProducts.Products.Select(p => new ProductsDto 
                        {
                            Name = p.Name,
                            Price = Math.Round(p.Price,2)
                        }).ToArray()
                    }
                };
                usersDto.Add(userDto);
            }

            var usersAndCountDto = new UserArrayDto
            {
                Count = context.Users.Where(u=>u.ProductsSold.Any()).Count(),
                Users = usersDto.ToArray()
            };

            return SerializeToXml(usersAndCountDto, "Users");
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
        //Serialize Method for Type
        private static string SerializeToXml<T>(T dto, string xmlRoot)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T), new XmlRootAttribute(xmlRoot));

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