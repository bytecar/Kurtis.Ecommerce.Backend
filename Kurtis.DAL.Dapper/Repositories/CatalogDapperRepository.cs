
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Kurtis.DAL.Dapper.Infrastructure;
using Kurtis.DAL.Dapper.Interfaces;
using System.Data;

namespace Kurtis.DAL.Dapper.Repositories
{
    public class CatalogDapperRepository : ICatalogDapperRepository
    {
        private readonly IDbConnectionFactory _factory;
        public CatalogDapperRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<IEnumerable<dynamic>> SearchProductsAsync(string? q, int page = 1, int pageSize = 20)
        {
            using var conn = _factory.CreateConnection();
            var sql = @"
                        SELECT p.Id, p.Name, p.Price, p.DiscountedPrice, p.AverageRating, p.RatingCount,
                               b.Label AS BrandLabel, c.Label AS CategoryLabel,
                               p.ImageUrlsJson
                        FROM dbo.Products p
                        LEFT JOIN dbo.Brands b ON p.BrandId = b.Id
                        LEFT JOIN dbo.Categories c ON p.CategoryId = c.Id
                        WHERE (@q IS NULL OR p.Name LIKE '%' + @q + '%' OR p.Description LIKE '%' + @q + '%')
                        ORDER BY p.CreatedAt DESC
                        OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;";
            return await conn.QueryAsync(sql, new { q, skip = (page - 1) * pageSize, take = pageSize });
        }

        public async Task<dynamic?> GetProductDetailsAsync(int productId)
        {
            using var conn = _factory.CreateConnection();
            var sql = @"
                        SELECT p.*, b.Label AS BrandLabel, c.Label AS CategoryLabel FROM dbo.Products p
                        LEFT JOIN dbo.Brands b ON p.BrandId = b.Id
                        LEFT JOIN dbo.Categories c ON p.CategoryId = c.Id
                        WHERE p.Id = @productId;
                        SELECT Size, Quantity FROM dbo.Inventories WHERE ProductId = @productId;
                        SELECT pc.CollectionId, col.[Name] FROM dbo.ProductCollections pc JOIN dbo.Collections col ON pc.CollectionId = col.Id WHERE pc.ProductId = @productId;
                        ";
            using var multi = await conn.QueryMultipleAsync(sql, new { productId });
            var product = await multi.ReadFirstOrDefaultAsync();
            var inventory = (await multi.ReadAsync()).AsList();
            var collections = (await multi.ReadAsync()).AsList();
            return new { product, inventory, collections };
        }
    }
}
