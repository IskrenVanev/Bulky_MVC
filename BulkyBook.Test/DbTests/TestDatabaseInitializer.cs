using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using Microsoft.Extensions.Configuration;

namespace BulkyBook.Test.DbTests
{
    public class TestDatabaseInitializer
    {
        public static DbContextOptions<ApplicationDbContext> CreateOptions()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "BulkyWeb")) // Path to your web project
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");

            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString) // Use your database provider
                .Options;
        }

        public static void SeedTestData(ApplicationDbContext dbContext)
        {
            // Copy the seeding logic from your ApplicationDbContext's OnModelCreating method
            // Make sure to adjust DbSet properties to match your test context's DbSet properties

            // Example: Seeding Categories
            if (!dbContext.Categories.Any())
            {
                dbContext.Categories.AddRange(
                    new Category { Id = 1, Name = "Action", DisplayOrder = 1 },
                    new Category { Id = 2, Name = "SciFi", DisplayOrder = 2 },
                    new Category { Id = 3, Name = "History", DisplayOrder = 3 });
            }

            // Example: Seeding Companies
            if (!dbContext.Companies.Any())
            {
                dbContext.Companies.AddRange(
                    new Company
                    {
                        Id = 1,
                        Name = "Tech Solution",
                        StreetAddress = "123 Tech St",
                        City = "Tech City",
                        PostalCode = "12121",
                        State = "MA",
                        PhoneNumber = "666666666"
                    }
                    // ... add other companies ...
                );
            }

            // Example: Seeding Products
            if (!dbContext.Products.Any())
            {
                dbContext.Products.AddRange(
                    new Product
                    {
                        Id = 1,
                        Title = "Fortune of Time",
                        Author = "Billy Spark",
                        Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                        ISBN = "SWD9999001",
                        ListPrice = 99,
                        Price = 90,
                        Price50 = 85,
                        Price100 = 80,
                        CategoryId = 1
                    }
                    // ... add other products ...
                );
            }

            dbContext.SaveChanges();
        }
    }
}
