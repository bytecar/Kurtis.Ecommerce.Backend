using Microsoft.AspNetCore.Mvc;
using Kurtis.Common.Models;
using Kurtis.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Kurtis.Common.DTOs;

namespace Kurtis.Api.Catalog.Controllers
{
    /// <summary>All endpoints pertaining to Reviews</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController(KurtisDbContext db, ILogger<ReviewsController> logger) : ControllerBase
    {


        /// <summary>Get all reviews (admin only)</summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var total = await db.Set<Review>().CountAsync();
            var reviews = await db.Set<Review>()
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { data = reviews, total, page, pageSize });
        }

        /// <summary>Get reviews for a product</summary>
        [HttpGet("product/{productId:int}")]
        public async Task<IActionResult> GetByProduct(int productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var total = await db.Set<Review>().CountAsync(r => r.ProductId == productId);
            var reviews = await db.Set<Review>()
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { data = reviews, total, page, pageSize });
        }

        /// <summary>Get review by ID</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var review = await db.Set<Review>().FindAsync(id);
            if (review == null)
                return NotFound(new { error = "Review not found" });

            return Ok(review);
        }

        /// <summary>Create review (authenticated users)</summary>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReviewDTO dto)
        {
            if (dto.Rating < 1 || dto.Rating > 5)
                return BadRequest(new { error = "Rating must be between 1 and 5" });

            var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var product = await db.Products.FindAsync(dto.ProductId);
            if (product == null)
                return BadRequest(new { error = "Product not found" });

            var existingReview = await db.Set<Review>()
                .FirstOrDefaultAsync(r => r.ProductId == dto.ProductId && r.UserId == userId);

            if (existingReview != null)
                return Conflict(new { error = "You have already reviewed this product" });

            var review = new Review
            {
                ProductId = dto.ProductId,
                UserId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            db.Set<Review>().Add(review);

            var allReviews = await db.Set<Review>()
                .Where(r => r.ProductId == dto.ProductId)
                .ToListAsync();

            var avgRating = allReviews.Average(r => r.Rating);
            product.AverageRating = (double)avgRating;
            product.RatingCount = allReviews.Count + 1;

            await db.SaveChangesAsync();
            logger.LogInformation($"Review {review.Id} created for product {dto.ProductId}");

            return CreatedAtAction(nameof(Get), new { id = review.Id }, review);
        }

        /// <summary>Update review (own review or admin)</summary>
        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReviewDTO dto)
        {
            var review = await db.Set<Review>().FindAsync(id);
            if (review == null)
                return NotFound();

            var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            if (review.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            if (dto.Rating !=0)
            {
                if (dto.Rating < 1 || dto.Rating > 5)
                    return BadRequest(new { error = "Rating must be between 1 and 5" });

                review.Rating = dto.Rating;
            }

            if (!string.IsNullOrWhiteSpace(dto.Comment))
                review.Comment = dto.Comment;

            db.Set<Review>().Update(review);
            await db.SaveChangesAsync();
            logger.LogInformation($"Review {id} updated");

            return Ok(review);
        }

        /// <summary>Delete review (own review or admin)</summary>
        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await db.Set<Review>().FindAsync(id);
            if (review == null)
                return NotFound();

            var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            if (review.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var productId = review.ProductId;
            db.Set<Review>().Remove(review);

            var remainingReviews = await db.Set<Review>()
                .Where(r => r.ProductId == productId)
                .ToListAsync();

            var product = await db.Products.FindAsync(productId);
            if (product != null)
            {
                product.RatingCount = remainingReviews.Count;
                product.AverageRating = remainingReviews.Any() ? (double)remainingReviews.Average(r => r.Rating) : 0;
            }

            await db.SaveChangesAsync();
            logger.LogInformation($"Review {id} deleted");

            return NoContent();
        }
    }

}
