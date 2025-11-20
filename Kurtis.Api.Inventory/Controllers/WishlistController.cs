using Microsoft.AspNetCore.Mvc;
using Kurtis.Common.Models;
using Kurtis.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Kurtis.Api.Catalog.Controllers
{
    /// <summary>All endpoints pertaining to WishLists</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WishlistController(KurtisDbContext db, ILogger<WishlistController> logger) : ControllerBase
    {

        /// <summary>Get user's wishlist</summary>
        [HttpGet]
        public async Task<IActionResult> GetWishlist([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");

            var total = await db.Set<Wishlist>().CountAsync(w => w.UserId == userId);
            var wishlist = await db.Set<Wishlist>()
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.AddedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var productIds = wishlist.Select(w => w.ProductId).ToList();
            var products = await db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            return Ok(new { data = products, total, page, pageSize });
        }

        /// <summary>Check if product is in wishlist</summary>
        [HttpGet("contains/{productId:int}")]
        public async Task<IActionResult> Contains(int productId)
        {
            var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var exists = await db.Set<Wishlist>()
                .AnyAsync(w => w.UserId == userId && w.ProductId == productId);

            return Ok(new { inWishlist = exists });
        }

        /// <summary>Add product to wishlist</summary>
        [HttpPost("{productId:int}")]
        public async Task<IActionResult> Add(int productId)
        {
            var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var product = await db.Products.FindAsync(productId);

            if (product == null)
                return BadRequest(new { error = "Product not found" });

            var existing = await db.Set<Wishlist>()
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (existing != null)
                return Conflict(new { error = "Product already in wishlist" });

            var wishlist = new Wishlist
            {
                UserId = userId,
                ProductId = productId,
                AddedAt = DateTime.UtcNow
            };

            db.Set<Wishlist>().Add(wishlist);
            await db.SaveChangesAsync();
            logger.LogInformation($"Product {productId} added to wishlist for user {userId}");

            return Ok(new { message = "Product added to wishlist" });
        }

        /// <summary>Remove product from wishlist</summary>
        [HttpDelete("{productId:int}")]
        public async Task<IActionResult> Remove(int productId)
        {
            var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var wishlist = await db.Set<Wishlist>()
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (wishlist == null)
                return NotFound();

            db.Set<Wishlist>().Remove(wishlist);
            await db.SaveChangesAsync();
            logger.LogInformation($"Product {productId} removed from wishlist for user {userId}");

            return NoContent();
        }

        /// <summary>Clear entire wishlist</summary>
        [HttpDelete]
        public async Task<IActionResult> Clear()
        {
            var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var wishlists = await db.Set<Wishlist>()
                .Where(w => w.UserId == userId)
                .ToListAsync();

            db.Set<Wishlist>().RemoveRange(wishlists);
            await db.SaveChangesAsync();
            logger.LogInformation($"Wishlist cleared for user {userId}");

            return Ok(new { message = "Wishlist cleared" });
        }
    }
}
