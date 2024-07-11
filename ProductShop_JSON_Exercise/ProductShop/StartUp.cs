using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using ProductShop.Data;
using ProductShop.DTOs.Export;
using ProductShop.Models;
using System.Collections.Generic;
using System.Net.Cache;
using System.Reflection.Metadata;
using System.Text.Json.Nodes;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main()
        {
            ProductShopContext productShopContext = new ProductShopContext();
            //string userText = File.ReadAllText("../../../Datasets/categories-products.json");
            Console.WriteLine(GetUsersWithProducts(productShopContext));
        }
        //1
        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            var model = JsonConvert.DeserializeObject<List<User>>(inputJson);
            context.Users.AddRange(model);
            context.SaveChanges();
            return $"Successfully imported {model.Count}";
        }

        //2 
        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            var products = JsonConvert.DeserializeObject<List<Product>>(inputJson);
            context.Products.AddRange(products);
            context.SaveChanges();
            return $"Successfully imported {products.Count}";
        }
        //3
        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            var categories = JsonConvert.DeserializeObject<List<Category>>(inputJson);
            categories.RemoveAll(x => x.Name is null);
            context.Categories.AddRange(categories);
            context.SaveChanges();
            return $"Successfully imported {categories.Count}";
        }

        //4
        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            var categoryProducts = JsonConvert.DeserializeObject<List<CategoryProduct>>(inputJson);
            context.CategoriesProducts.AddRange(categoryProducts);
            context.SaveChanges();
            return $"Successfully imported {categoryProducts.Count}";
        }
        //5
        public static string GetProductsInRange(ProductShopContext context)
        {
            var productsJson = JsonConvert
                .SerializeObject(context.Products
                .Select(p => new
                {
                    Name = p.Name,
                    Price = p.Price,
                    Seller = p.Seller.FirstName + ' ' + p.Seller.LastName,
                })
                .Where(x => x.Price >= 500 && x.Price <= 1000)
                .OrderBy(x => x.Price)
                , SerializeObjectOptions());

            return productsJson;
        }
        //6
        public static string GetSoldProducts(ProductShopContext context)
        {
            var options = SerializeObjectOptions();
            var soldProducts = context.Users
                .Where(u => u.ProductsSold.Any(p => p.Buyer != null))
                .Select(u => new
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    SoldProducts = u.ProductsSold
                    .Where(p => p.Buyer != null)
                    .Select(p => new
                    {
                        Name = p.Name,
                        Price = p.Price,
                        BuyerFirstName = p.Buyer.FirstName,
                        BuyerLastName = p.Buyer.LastName
                    })
                    .ToList()
                })
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToList();


            return JsonConvert.SerializeObject(soldProducts, options);
        }

        //7
        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var options = SerializeObjectOptions();
            var categories = context.Categories
                .Include(c => c.CategoriesProducts)
                .ThenInclude(cp => cp.Product)
                .AsEnumerable()
                .Select(c => new CategoryByProductsSoldDTO
                {
                    Category = c.Name,
                    ProductsCount = c.CategoriesProducts.Count,
                    AveragePrice = $"{c.CategoriesProducts.Average(x => x.Product.Price):f2}",
                    TotalRevenue = $"{c.CategoriesProducts.Sum(x => x.Product.Price):f2}"
                })
                .OrderByDescending(c => c.ProductsCount)
                .ToList();

            return JsonConvert.SerializeObject(categories, options);
        }

        //8
        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var usersWithProducts = context.Users
                .Where(u => u.ProductsSold.Any(p => p.Buyer != null))
                .OrderByDescending(u => u.ProductsSold.Count(p => p.Buyer != null))
                .Select(u => new
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Age = u.Age,
                    SoldProducts = u.ProductsSold
                    .Where(p => p.Buyer != null)
                    .Select(p => new
                    {
                        Name = p.Name,
                        Price = p.Price
                    }).ToList()
                }).ToList();
            var result = new
            {
                UsersCount = usersWithProducts.Count,
                Users = usersWithProducts.Select(u => new
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Age = u.Age,
                    SoldProducts = new
                    {
                        Count = u.SoldProducts.Count,
                        Products = u.SoldProducts
                    }
                })
            };
            return JsonConvert.SerializeObject(result, SerializeObjectOptions());
        }

        private static JsonSerializerSettings SerializeObjectOptions()
        {
            return new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };
        }
    }
}