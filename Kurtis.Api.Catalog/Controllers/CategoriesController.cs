using Microsoft.AspNetCore.Mvc;
using Kurtis.Common.Models;
using Kurtis.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Kurtis.Api.Catalog.Controllers
{
    /// <summary>All endpoints pertaining to Categories</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController(KurtisDbContext db, ILogger<CategoriesController> logger) : ControllerBase
    {

        /// <summary>Get all categories with pagination</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var total = await db.Categories.CountAsync();
            var categories = await db.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { data = categories, total, page, pageSize });
        }

        /// <summary>Get categories by gender</summary>
        [HttpGet("by-gender/{gender}")]
        public async Task<IActionResult> GetByGender(string gender)
        {
            var categories = await db.Categories
                .Where(c => c.Gender == gender)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Ok(categories);
        }

        /// <summary>Get category by ID with product count</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var category = await db.Categories.FindAsync(id);
            if (category == null)
                return NotFound(new { error = "Category not found" });

            var productCount = await db.Products.CountAsync(p => p.CategoryId == id);
            return Ok(new { category, productCount });
        }

        /// <summary>Create category (admin only)</summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { error = "Category name required" });

            var existing = await db.Categories.FirstOrDefaultAsync(c => c.Name == dto.Name);
            if (existing != null)
                return Conflict(new { error = "Category already exists" });

            var category = new Category
            {
                Name = dto.Name,
                Label = dto.Label,
                Description = dto.Description,
                Gender = dto.Gender,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.Categories.Add(category);
            await db.SaveChangesAsync();
            logger.LogInformation($"Category {category.Id} created");

            return CreatedAtAction(nameof(Get), new { id = category.Id }, category);
        }

        /// <summary>Update category (admin only)</summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDTO dto)
        {
            var category = await db.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != category.Name)
            {
                var existing = await db.Categories.FirstOrDefaultAsync(c => c.Name == dto.Name);
                if (existing != null)
                    return Conflict(new { error = "Category name already exists" });

                category.Name = dto.Name;
            }

            if (!string.IsNullOrWhiteSpace(dto.Label))
                category.Label = dto.Label;

            if (!string.IsNullOrWhiteSpace(dto.Description))
                category.Description = dto.Description;

            if (!string.IsNullOrWhiteSpace(dto.Gender))
                category.Gender = dto.Gender;

            category.UpdatedAt = DateTime.UtcNow;
            db.Categories.Update(category);
            await db.SaveChangesAsync();
            logger.LogInformation($"Category {id} updated");

            return Ok(category);
        }

        /// <summary>Delete category (admin only)</summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await db.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            var hasProducts = await db.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
                return BadRequest(new { error = "Cannot delete category with associated products" });

            db.Categories.Remove(category);
            await db.SaveChangesAsync();
            logger.LogInformation($"Category {id} deleted");

            return NoContent();
        }
    }
}
