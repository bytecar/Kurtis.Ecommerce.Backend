using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Kurtis.Common.Models;
using Kurtis.Common.Helpers;
using Kurtis.DAL;

namespace Kurtis.Tests.Utils
{
    public static class TestHelpers
    {
        /// <summary>
        /// Create an in-memory EF Core database context for isolated unit tests.
        /// </summary>
        public static KurtisDbContext CreateInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<KurtisDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new KurtisDbContext(options);
        }

        /// <summary>
        /// Seed the in-memory database with minimal valid data.
        /// </summary>
        public static void SeedSampleData(KurtisDbContext ctx)
        {
            var brand = new Brand { Label = "Brand1" };
            var category = new Category { Label = "Category1" };
            ctx.Brands.Add(brand);
            ctx.Categories.Add(category);
            ctx.SaveChanges();

            var product = new Product
            {
                Name = "Sample Product",
                Description = "A demo product used in tests.",
                Price = 999.99m,
                BrandId = brand.Id,
                CategoryId = category.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            ctx.Products.Add(product);
            ctx.SaveChanges();

            ctx.Inventories.Add(new Inventory
            {
                ProductId = product.Id,
                Size = "M",
                Quantity = 10,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            ctx.SaveChanges();
        }

        /// <summary>
        /// Generate a valid JWT token for API authorization headers in tests.
        /// </summary>
        public static string GenerateJwtForTest(string username = "tester", string role = "User")
        {
            return JwtTokenHelper.GenerateToken(username, role, 120);
        }

        /// <summary>
        /// Create a configured HttpClient from a WebApplicationFactory for integration tests.
        /// Automatically adds the JWT bearer token if provided.
        /// </summary>
        public static HttpClient CreateClient<TProgram>(WebApplicationFactory<TProgram> factory, string? jwtToken = null)
            where TProgram : class
        {
            var client = factory.CreateClient();
            if (!string.IsNullOrEmpty(jwtToken))
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
            return client;
        }

        /// <summary>
        /// Seed both DB and HttpClient for integration tests.
        /// </summary>
        public static async Task<(HttpClient client, KurtisDbContext db)> CreateClientWithSeedAsync<TProgram>(
            WebApplicationFactory<TProgram> factory)
            where TProgram : class
        {
            var db = CreateInMemoryDb();
            SeedSampleData(db);
            var jwt = GenerateJwtForTest();
            var client = CreateClient(factory, jwt);
            return (client, db);
        }
    }
}
