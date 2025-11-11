
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
    public class InventoryEdgeTests
    {
        private DbContextOptions<KurtisDbContext> Options() => new DbContextOptionsBuilder<KurtisDbContext>()
            .UseInMemoryDatabase(System.Guid.NewGuid().ToString()).Options;

        [Fact]
        public async Task Decrement_ReturnsFalse_When_NoItem()
        {
            var opts = Options();
            using var ctx = new KurtisDbContext(opts);
            var repo = new InventoryRepository(ctx);
            var result = await repo.DecrementStockAsync(999, "M", 1);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task Decrement_Concurrency_Simulated()
        {
            var opts = Options();
            using var ctx1 = new KurtisDbContext(opts);
            using var ctx2 = new KurtisDbContext(opts);
            ctx1.Brands.Add(new Brand { Label = "B" }); ctx1.Categories.Add(new Category { Label = "C" });
            await ctx1.SaveChangesAsync();
            var p = new Product { Name = "Pc", Price = 10, BrandId = 1, CategoryId = 1, CreatedAt=System.DateTime.UtcNow, UpdatedAt=System.DateTime.UtcNow };
            ctx1.Products.Add(p); await ctx1.SaveChangesAsync();
            ctx1.Inventories.Add(new Inventory { ProductId = p.Id, Size = "M", Quantity = 5, CreatedAt=System.DateTime.UtcNow, UpdatedAt=System.DateTime.UtcNow });
            await ctx1.SaveChangesAsync();

            var repo1 = new InventoryRepository(ctx1);
            var repo2 = new InventoryRepository(ctx2);

            // repo1 decrements 3
            var ok1 = await repo1.DecrementStockAsync(p.Id, "M", 3);
            // repo2 tries to decrement 3 (should fail because quantity now 2)
            var ok2 = await repo2.DecrementStockAsync(p.Id, "M", 3);
            ok1.Should().BeTrue();
            ok2.Should().BeFalse();
        }
    }
}
