using Microsoft.AspNetCore.Mvc;
using Kurtis.Common.Models;
using Kurtis.DAL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Kurtis.DAL;
using NuGet.Protocol;
using Kurtis.Common.DTOs;

namespace Kurtis.Api.Catalog.Controllers
{
    /// <summary>All endpoints pertaining to Products</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController(IProductRepository _repo, KurtisDbContext _db, ILogger<ProductsController> _logger) : ControllerBase
    {
        /// <summary>Search/list products with filters</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? q,
            [FromQuery] int? brandId,
            [FromQuery] int? categoryId,
            [FromQuery] string? gender,
            [FromQuery] bool? featured,
            [FromQuery] int minPrice = 0,
            [FromQuery] int maxPrice = int.MaxValue,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _db.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(p => p.Name.Contains(q) || p.Description.Contains(q));

            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId);

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            if (!string.IsNullOrWhiteSpace(gender))
                query = query.Where(p => p.Gender == gender);

            if (featured.HasValue)
                query = query.Where(p => p.Featured == featured);

            query = query.Where(p => p.Price >= minPrice && p.Price <= maxPrice);

            var total = await query.CountAsync();
            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { data = products, total, page, pageSize });
        }

        /// <summary>Get featured products</summary>
        [HttpGet("featured")]
        public async Task<IActionResult> GetFeatured([FromQuery] int limit = 10)
        {
            var products = await _db.Products
                .Where(p => p.Featured)
                .OrderByDescending(p => p.CreatedAt)
                .Take(limit)
                .ToListAsync();

            return Ok(products);
        }

        /// <summary>Get trending/new products</summary>
        [HttpGet("trending")]
        public async Task<IActionResult> GetTrending([FromQuery] int limit = 10)
        {
            var products = await _db.Products
                .Where(p => p.IsNew)
                .OrderByDescending(p => p.AverageRating)
                .ThenByDescending(p => p.RatingCount)
                .Take(limit)
                .ToListAsync();

            return Ok(products);
        }

        /// <summary>Get product with inventory and reviews</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var product = await _repo.GetWithDetailsAsync(id);
            if (product == null)
                return NotFound(new { error = "Product not found" });

            var inventory = await _db.Set<Inventory>()
                .Where(i => i.ProductId == id)
                .ToListAsync();

            var reviews = await _db.Set<Review>()
                .Where(r => r.ProductId == id)
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .ToListAsync();

            return Ok(new { product, inventory, recentReviews = reviews });
        }

        /// <summary>Get new arrivals</summary>
        [HttpGet("new")]
        public async Task<IActionResult> GetNew([FromQuery] int limit = 10)
        {
            var products = await _db.Products
                .Where(p => p.IsNew)
                .OrderByDescending(p => p.CreatedAt)
                .Take(limit)
                .ToListAsync();

            return Ok(products);
        }

        /// <summary>Create product (admin only)</summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Price <= 0)
                return BadRequest(new { error = "Invalid product data" });

            var brand = await _db.Brands.FindAsync(dto.BrandId);
            var category = await _db.Categories.FindAsync(dto.CategoryId);

            if (brand == null || category == null)
                return BadRequest(new { error = "Invalid brand or category" });

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                DiscountedPrice = dto.DiscountedPrice ?? 0,
                BrandId = dto.BrandId,
                CategoryId = dto.CategoryId,
                Gender = dto.Gender,
                SizesJson = dto.Sizes.ToJson(),
                ImageUrlsJson = dto.ImageUrls.ToJson(),
                Featured = dto.Featured,
                IsNew = dto.IsNew,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();
            _logger.LogInformation($"Product {product.Id} created: {product.Name}");

            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }

        /// <summary>Update product (admin only)</summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDTO dto)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { error = "Product not found" });

            if (!string.IsNullOrWhiteSpace(dto.Name))
                product.Name = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.Description))
                product.Description = dto.Description;

            if (dto.Price > 0)
                product.Price = dto.Price;

            if (dto.DiscountedPrice > 0)
                product.DiscountedPrice = dto.DiscountedPrice;

            if (dto.BrandId > 0)
                product.BrandId = dto.BrandId;

            if (dto.CategoryId > 0)
                product.CategoryId = dto.CategoryId;

            if (dto.Featured.HasValue)
                product.Featured = dto.Featured.Value;

            if (dto.ImageUrls != null && dto.ImageUrls.Any())
                product.ImageUrlsJson = dto.ImageUrls.ToJson();

            if (dto.Sizes != null && dto.Sizes.Any())
                product.SizesJson = dto.Sizes.ToJson();

            product.UpdatedAt = DateTime.UtcNow;
            _db.Products.Update(product);
            await _db.SaveChangesAsync();
            _logger.LogInformation($"Product {id} updated");

            return Ok(product);
        }

        /// <summary>Bulk feature/unfeature products (admin)</summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("bulk-feature")]
        public async Task<IActionResult> BulkFeature([FromBody] BulkFeatureDTO dto)
        {
            var products = await _db.Products
                .Where(p => dto.ProductIds.Contains(p.Id))
                .ToListAsync();

            foreach (var product in products)
                product.Featured = dto.Featured;

            await _db.SaveChangesAsync();
            _logger.LogInformation($"Bulk feature update: {products.Count} products");

            return Ok(new { updated = products.Count });
        }

        /// <summary>Delete product (admin only)</summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            var inventory = await _db.Set<Inventory>().Where(i => i.ProductId == id).ToListAsync();
            _db.Set<Inventory>().RemoveRange(inventory);

            var reviews = await _db.Set<Review>().Where(r => r.ProductId == id).ToListAsync();
            _db.Set<Review>().RemoveRange(reviews);

            var wishlists = await _db.Set<Wishlist>().Where(w => w.ProductId == id).ToListAsync();
            _db.Set<Wishlist>().RemoveRange(wishlists);

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            _logger.LogInformation($"Product {id} deleted with related data");

            return NoContent();
        }
    }    
}
