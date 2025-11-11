
using Xunit;
using FluentAssertions;
using Moq;
using System.Data;
using Kurtis.DAL.Dapper.Infrastructure;
using Kurtis.DAL.Dapper.Repositories;
using System.Threading.Tasks;

namespace Kurtis.Tests.Dapper
{
    public class CatalogDapperSeededTests
    {
        [Fact]
        public async Task CatalogDapperRepository_Does_Not_Throw_When_Factory_Returns_Connection()
        {
            var mockFactory = new Mock<IDbConnectionFactory>();
            var mockConn = new Mock<IDbConnection>();
            mockFactory.Setup(f => f.CreateConnection()).Returns(mockConn.Object);
            var repo = new CatalogDapperRepository(mockFactory.Object);
            var ex = await Record.ExceptionAsync(() => repo.SearchProductsAsync(null));
            ex.Should().BeNull();
        }
    }
}
