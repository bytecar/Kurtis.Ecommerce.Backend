
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Kurtis.DAL;
using Kurtis.Common.Models;
using Kurtis.DAL.Repositories;
using System.Threading.Tasks;
using System.Linq;

namespace Kurtis.Tests.DAL
{
    public class DalSeededTests
    {
        private DbContextOptions<KurtisDbContext> Options() => new DbContextOptionsBuilder<KurtisDbContext>()
            .UseInMemoryDatabase(System.Guid.NewGuid().ToString()).Options;

        [Fact]
        public async Task Seed_Brands_Categories_Products_And_Inventory_Workflow()
        {
            var opts = Options();
            using var ctx = new KurtisDbContext(opts);
            // seed brands and categories
            ctx.Brands.AddRange(new Brand { Label = "B1" }, new Brand { Label = "B2" });
            ctx.Categories.AddRange(new Category { Label = "C1" }, new Category { Label = "C2" });
            await ctx.SaveChangesAsync();
            // add products
            var p1 = new Product { Name = "ProdA", Price = 100, BrandId = 1, CategoryId = 1, CreatedAt=System.DateTime.UtcNow, UpdatedAt=System.DateTime.UtcNow };
            var p2 = new Product { Name = "ProdB", Price = 200, BrandId = 2, CategoryId = 2, CreatedAt=System.DateTime.UtcNow, UpdatedAt=System.DateTime.UtcNow };
            ctx.Products.AddRange(p1, p2);
            await ctx.SaveChangesAsync();
            // inventories
            ctx.Inventories.AddRange(new Inventory { ProductId = p1.Id, Size = "M", Quantity = 10, CreatedAt=System.DateTime.UtcNow, UpdatedAt=System.DateTime.UtcNow },
                                     new Inventory { ProductId = p2.Id, Size = "L", Quantity = 5, CreatedAt=System.DateTime.UtcNow, UpdatedAt=System.DateTime.UtcNow });
            await ctx.SaveChangesAsync();

            // verify
            ctx.Brands.Count().Should().Be(2);
            ctx.Categories.Count().Should().Be(2);
            ctx.Products.Count().Should().Be(2);
            ctx.Inventories.Count().Should().Be(2);

            var repo = new ProductRepository(ctx);
            var results = (await repo.SearchAsync("Prod",1,10)).ToList();
            results.Count.Should().Be(2);
        }
    }
}
