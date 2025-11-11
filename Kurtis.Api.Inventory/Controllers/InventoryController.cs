
using Microsoft.AspNetCore.Mvc;
using Kurtis.DAL.Interfaces;
using Kurtis.Common.Models;
using Microsoft.AspNetCore.Authorization;

namespace Kurtis.ApiInventory.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryRepository _inv;
        public InventoryController(IInventoryRepository inv) { _inv = inv; }

        [HttpGet("{productId:int}")]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var items = await _inv.GetByProductIdAsync(productId);
            return Ok(items);
        }

        [HttpGet("item/{id:int}")]
        public async Task<IActionResult> GetItem(int id)
        {
            var item = await _inv.GetByIdAsync(id);
            if (item==null) return NotFound();
            return Ok(item);
        }

        [Authorize(Roles="admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Inventory model)
        {
            model.CreatedAt = DateTime.UtcNow; model.UpdatedAt = DateTime.UtcNow;
            await _inv.AddAsync(model);             
            return CreatedAtAction(nameof(GetItem), new { id = model.Id }, model);
        }

        [Authorize(Roles="admin")]
        [HttpPost("decrement")]
        public async Task<IActionResult> Decrement([FromBody] DecrementDto dto)
        {
            var ok = await _inv.DecrementStockAsync(dto.ProductId, dto.Size, dto.Quantity);
            if (!ok) return BadRequest(new { error = "insufficient or not found" });
            return Ok(new { success = true });
        }

        [Authorize(Roles="admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Inventory model)
        {
            var existing = await _inv.GetByIdAsync(id);
            if (existing==null) return NotFound();
            existing.Quantity = model.Quantity; 
            existing.Size = model.Size; 
            existing.UpdatedAt = DateTime.UtcNow;
            await _inv.UpdateAsync(existing);             
            return Ok(existing);
        }
    }

    public record DecrementDto(int ProductId, string Size, int Quantity);
}
