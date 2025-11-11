
using Microsoft.AspNetCore.Mvc;
using Kurtis.Common.Models;
using Kurtis.DAL.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Kurtis.Api.Catalog.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _repo;
        public ProductsController(IProductRepository repo) { _repo = repo; }

        /// <summary>Search products with pagination</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var items = await _repo.SearchAsync(q, page, pageSize);
            return Ok(items);
        }

        /// <summary>Get product by id with related details</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var p = await _repo.GetWithDetailsAsync(id);
            if (p == null) return NotFound();
            return Ok(p);
        }

        /// <summary>Create a product (admin)</summary>
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product model)
        {
            model.CreatedAt = DateTime.UtcNow; model.UpdatedAt = DateTime.UtcNow;
            await _repo.AddAsync(model);            
            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        /// <summary>Update a product (admin)</summary>
        [Authorize(Roles = "admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Product model)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            existing.Name = model.Name; existing.Description = model.Description; existing.Price = model.Price;
            existing.DiscountedPrice = model.DiscountedPrice; existing.UpdatedAt = DateTime.UtcNow;
            _repo.UpdateAsync(existing);             
            return Ok(existing);
        }

        /// <summary>Delete a product (admin)</summary>
        [Authorize(Roles = "admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            await _repo.DeleteAsync(existing.Id);
            return NoContent();
        }
    }
}
