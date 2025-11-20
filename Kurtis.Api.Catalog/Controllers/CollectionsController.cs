using Microsoft.AspNetCore.Mvc;
using Kurtis.Common.Models;
using Kurtis.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Kurtis.Api.Catalog.Controllers
{
    /// <summary>All endpoints pertaining to Collections</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CollectionsController(KurtisDbContext db, ILogger<CollectionsController> logger) : ControllerBase
    {

        /// <summary>Get all collections</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool? active = true, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var query = db.Collections.AsQueryable();

            if (active.HasValue)
                query = query.Where(c => c.Active == active);

            var total = await query.CountAsync();
            var collections = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { data = collections, total, page, pageSize });
        }

        /// <summary>Get collection by ID with products</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var collection = await db.Collections.FindAsync(id);
            if (collection == null)
                return NotFound(new { error = "Collection not found" });

            var total = await db.ProductCollections
                .Where(pc => pc.CollectionId == id)
                .CountAsync();

            var products = await db.ProductCollections
                .Where(pc => pc.CollectionId == id)
                .Include(pc => pc.Product)
                .ThenInclude(p => p!.Brand)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(pc => pc.Product)
                .ToListAsync();

            return Ok(new { collection, products, total, page, pageSize });
        }

        /// <summary>Create collection (admin)</summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCollectionDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { error = "Collection name required" });

            var collection = new Collection
            {
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                Active = true,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.Collections.Add(collection);
            await db.SaveChangesAsync();
            logger.LogInformation($"Collection {collection.Id} created");

            return CreatedAtAction(nameof(Get), new { id = collection.Id }, collection);
        }

        /// <summary>Update collection (admin)</summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCollectionDTO dto)
        {
            var collection = await db.Collections.FindAsync(id);
            if (collection == null)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Name))
                collection.Name = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.Description))
                collection.Description = dto.Description;

            if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
                collection.ImageUrl = dto.ImageUrl;

            if (dto.Active.HasValue)
                collection.Active = dto.Active.Value;

            if (dto.StartDate.HasValue)
                collection.StartDate = dto.StartDate;

            if (dto.EndDate.HasValue)
                collection.EndDate = dto.EndDate;

            collection.UpdatedAt = DateTime.UtcNow;
            db.Collections.Update(collection);
            await db.SaveChangesAsync();
            logger.LogInformation($"Collection {id} updated");

            return Ok(collection);
        }

        /// <summary>Add product to collection (admin)</summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("{collectionId:int}/products")]
        public async Task<IActionResult> AddProduct(int collectionId, [FromBody] AddProductToCollectionDTO dto)
        {
            var collection = await db.Collections.FindAsync(collectionId);
            if (collection == null)
                return NotFound(new { error = "Collection not found" });

            var product = await db.Products.FindAsync(dto.ProductId);
            if (product == null)
                return BadRequest(new { error = "Product not found" });

            var existing = await db.ProductCollections
                .FirstOrDefaultAsync(pc => pc.CollectionId == collectionId && pc.ProductId == dto.ProductId);

            if (existing != null)
                return Conflict(new { error = "Product already in collection" });

            var collectionProduct = new ProductCollection
            {
                ProductId = dto.ProductId,
                CollectionId = collectionId,
                CreatedAt = DateTime.UtcNow
            };

            db.ProductCollections.Add(collectionProduct);
            await db.SaveChangesAsync();
            logger.LogInformation($"Product {dto.ProductId} added to collection {collectionId}");

            return Ok(new { message = "Product added to collection" });
        }

        /// <summary>Remove product from collection (admin)</summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{collectionId:int}/products/{productId:int}")]
        public async Task<IActionResult> RemoveProduct(int collectionId, int productId)
        {
            var collectionProduct = await db.ProductCollections
                .FirstOrDefaultAsync(pc => pc.CollectionId == collectionId && pc.ProductId == productId);

            if (collectionProduct == null)
                return NotFound();

            db.ProductCollections.Remove(collectionProduct);
            await db.SaveChangesAsync();
            logger.LogInformation($"Product {productId} removed from collection {collectionId}");

            return NoContent();
        }

        /// <summary>Delete collection (admin)</summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var collection = await db.Collections.FindAsync(id);
            if (collection == null)
                return NotFound();

            // EF Core cascade delete should handle this, but being explicit is safe
            var products = await db.ProductCollections
                .Where(pc => pc.CollectionId == id)
                .ToListAsync();

            db.ProductCollections.RemoveRange(products);
            db.Collections.Remove(collection);
            await db.SaveChangesAsync();
            logger.LogInformation($"Collection {id} deleted");

            return NoContent();
        }
    }
}
    

