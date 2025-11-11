
using Xunit;
using FluentAssertions;
using Kurtis.Common.Models;
using System.Text.Json;

namespace Kurtis.Tests.Models
{
    public class ModelSerializationTests
    {
        [Fact]
        public void Product_Serializes_And_Deserializes()
        {
            var p = new Product { Id=1, Name = "T1", Price = 100, CreatedAt=System.DateTime.UtcNow, UpdatedAt=System.DateTime.UtcNow };
            var s = JsonSerializer.Serialize(p);
            var d = JsonSerializer.Deserialize<Product>(s);
            d.Should().NotBeNull();
            d!.Name.Should().Be("T1");
        }

        [Fact]
        public void Inventory_Serializes()
        {
            var i = new Inventory { Id=1, ProductId=1, Size="M", Quantity=10, CreatedAt=System.DateTime.UtcNow, UpdatedAt=System.DateTime.UtcNow };
            var s = JsonSerializer.Serialize(i);
            s.Should().Contain("Quantity");
        }
    }
}
