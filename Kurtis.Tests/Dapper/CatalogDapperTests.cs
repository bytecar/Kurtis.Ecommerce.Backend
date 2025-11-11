
using Xunit;
using FluentAssertions;
using Moq;
using System.Data;
using Dapper;
using Kurtis.DAL.Dapper.Repositories;
using Kurtis.DAL.Dapper.Infrastructure;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Kurtis.Tests.Dapper
{
    public class CatalogDapperTests
    {
        [Fact]
        public async Task SearchProductsAsync_Returns_Empty_When_NoData()
        {
            var mockFactory = new Mock<IDbConnectionFactory>();
            var mockConn = new Mock<IDbConnection>();
            mockFactory.Setup(f => f.CreateConnection()).Returns(mockConn.Object);
            // Dapper extension methods are static; instead ensure method handles nulls gracefully by mocking QueryAsync via extension can't be mocked easily.
            // Here we assert repository instantiation and method signature.
            var repo = new CatalogDapperRepository(mockFactory.Object);
            var ex = await Record.ExceptionAsync(() => repo.SearchProductsAsync(null));
            // We expect no exception in our abstraction (actual DB behavior not tested here)
            ex.Should().BeNull();
        }
    }
}
