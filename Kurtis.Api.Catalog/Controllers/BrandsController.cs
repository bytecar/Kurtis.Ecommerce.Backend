using Microsoft.AspNetCore.Mvc;
using Kurtis.Common.Models;
using Kurtis.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Kurtis.Common.DTOs;

namespace Kurtis.Api.Catalog.Controllers
{
    /// <summary>All endpoints pertaining to Brands</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BrandsController(KurtisDbContext db, ILogger<BrandsController> logger) : ControllerBase
    {

        /// <summary>Get all brands with pagination</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var total = await db.Brands.CountAsync();
            var brands = await db.Brands
                .AsNoTracking()
                .OrderBy(b => b.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { data = brands, total, page, pageSize });
        }

        /// <summary>Search brands by name</summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var brands = await db.Brands
                .Where(b => b.Name.Contains(q) || b.Label.Contains(q))
                .OrderBy(b => b.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { data = brands, page, pageSize });
        }

        /// <summary>Get brand by ID with product count</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var brand = await db.Brands.FindAsync(id);
            if (brand == null)
                return NotFound(new { error = "Brand not found" });

            var productCount = await db.Products.CountAsync(p => p.BrandId == id);
            return Ok(new { brand, productCount });
        }

        /// <summary>Create new brand (admin only)</summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBrandDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { error = "Brand name required" });

            var existing = await db.Brands.FirstOrDefaultAsync(b => b.Name == dto.Name);
            if (existing != null)
                return Conflict(new { error = "Brand with this name already exists" });

            var brand = new Brand
            {
                Name = dto.Name,
                Label = dto.Label,
                Description = dto.Description,
                Logo = dto.Logo,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.Brands.Add(brand);
            await db.SaveChangesAsync();
            logger.LogInformation($"Brand {brand.Id} created: {brand.Name}");

            return CreatedAtAction(nameof(Get), new { id = brand.Id }, brand);
        }

        /// <summary>Update brand (admin only)</summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBrandDTO dto)
        {
            var brand = await db.Brands.FindAsync(id);
            if (brand == null)
                return NotFound(new { error = "Brand not found" });

            if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != brand.Name)
            {
                var existing = await db.Brands.FirstOrDefaultAsync(b => b.Name == dto.Name);
                if (existing != null)
                    return Conflict(new { error = "Brand with this name already exists" });

                brand.Name = dto.Name;
            }

            if (!string.IsNullOrWhiteSpace(dto.Label))
                brand.Label = dto.Label;

            if (!string.IsNullOrWhiteSpace(dto.Description))
                brand.Description = dto.Description;

            if (!string.IsNullOrWhiteSpace(dto.Logo))
                brand.Logo = dto.Logo;

            brand.UpdatedAt = DateTime.UtcNow;
            db.Brands.Update(brand);
            await db.SaveChangesAsync();
            logger.LogInformation($"Brand {id} updated");

            return Ok(brand);
        }

        /// <summary>Delete brand (admin only)</summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var brand = await db.Brands.FindAsync(id);
            if (brand == null)
                return NotFound();

            var hasProducts = await db.Products.AnyAsync(p => p.BrandId == id);
            if (hasProducts)
                return BadRequest(new { error = "Cannot delete brand with associated products" });

            db.Brands.Remove(brand);
            await db.SaveChangesAsync();
            logger.LogInformation($"Brand {id} deleted");

            return NoContent();
        }
    }
}
