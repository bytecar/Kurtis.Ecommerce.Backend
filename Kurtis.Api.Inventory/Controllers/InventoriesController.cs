using Microsoft.AspNetCore.Mvc;
using Kurtis.DAL.Interfaces;
using Kurtis.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Kurtis.DAL;
using Microsoft.EntityFrameworkCore;

namespace Kurtis.ApiInventory.Controllers
{
    /// <summary>All endpoints pertaining to Inventory</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class InventoriesController(IInventoryRepository inv, KurtisDbContext db, ILogger<InventoriesController> logger) : ControllerBase
    {

        /// <summary>Get inventory for a product</summary>
        [HttpGet("product/{productId:int}")]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var items = await inv.GetByProductIdAsync(productId);
            if (!items.Any())
                return NotFound(new { error = "No inventory found for product" });

            return Ok(items);
        }

        /// <summary>Get inventory item by ID</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetItem(int id)
        {
            var item = await inv.GetByIdAsync(id);
            if (item == null)
                return NotFound(new { error = "Inventory item not found" });

            return Ok(item);
        }

        /// <summary>Get low stock items (admin)</summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 10)
        {
            var items = await db.Set<Inventory>()
                .Where(i => i.Quantity <= threshold)
                .OrderBy(i => i.Quantity)
                .ToListAsync();

            return Ok(new { threshold, items });
        }

        /// <summary>Get inventory analytics (admin)</summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics()
        {
            var inventory = await db.Set<Inventory>().ToListAsync();
            var totalUnits = inventory.Sum(i => i.Quantity);
            var averageUnits = inventory.Count > 0 ? totalUnits / inventory.Count : 0;

            return Ok(new
            {
                totalItems = inventory.Count,
                totalUnits = totalUnits,
                averageUnits = averageUnits,
                highestStock = inventory.OrderByDescending(i => i.Quantity).FirstOrDefault(),
                lowestStock = inventory.OrderBy(i => i.Quantity).FirstOrDefault()
            });
        }

        /// <summary>Create inventory item (admin)</summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInventoryDto dto)
        {
            if (dto.Quantity < 0)
                return BadRequest(new { error = "Quantity cannot be negative" });

            var product = await db.Products.FindAsync(dto.ProductId);
            if (product == null)
                return BadRequest(new { error = "Product not found" });

            var existing = await db.Set<Inventory>()
                .FirstOrDefaultAsync(i => i.ProductId == dto.ProductId && i.Size == dto.Size);

            if (existing != null)
                return Conflict(new { error = "Inventory for this product and size already exists" });

            var inventory = new Inventory
            {
                ProductId = dto.ProductId,
                Size = dto.Size,
                Quantity = dto.Quantity,
                UpdatedAt = DateTime.UtcNow
            };

            await inv.AddAsync(inventory);
            logger.LogInformation($"Inventory created for product {dto.ProductId}");

            return CreatedAtAction(nameof(GetItem), new { id = inventory.Id }, inventory);
        }

        /// <summary>Update inventory quantity (admin)</summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateInventoryDto dto)
        {
            var existing = await inv.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { error = "Inventory item not found" });

            if (dto.Quantity.HasValue)
            {
                if (dto.Quantity < 0)
                    return BadRequest(new { error = "Quantity cannot be negative" });

                existing.Quantity = dto.Quantity.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.Size))
                existing.Size = dto.Size;

            existing.UpdatedAt = DateTime.UtcNow;
            await inv.UpdateAsync(existing);
            logger.LogInformation($"Inventory {id} updated");

            return Ok(existing);
        }

        /// <summary>Decrement stock (typically for orders)</summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("decrement")]
        public async Task<IActionResult> Decrement([FromBody] DecrementStockDto dto)
        {
            if (dto.Quantity <= 0)
                return BadRequest(new { error = "Quantity must be positive" });

            var ok = await inv.DecrementStockAsync(dto.ProductId, dto.Size, dto.Quantity);
            if (!ok)
                return BadRequest(new { error = "Insufficient stock or inventory not found" });

            logger.LogInformation($"Stock decremented: Product {dto.ProductId}, Size {dto.Size}, Qty {dto.Quantity}");
            return Ok(new { success = true, message = "Stock decremented successfully" });
        }

        /// <summary>Increment stock (typically for returns)</summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("increment")]
        public async Task<IActionResult> Increment([FromBody] IncrementStockDto dto)
        {
            if (dto.Quantity <= 0)
                return BadRequest(new { error = "Quantity must be positive" });

            var inventory = await db.Set<Inventory>()
                .FirstOrDefaultAsync(i => i.ProductId == dto.ProductId && i.Size == dto.Size);

            if (inventory == null)
                return BadRequest(new { error = "Inventory not found" });

            inventory.Quantity += dto.Quantity;
            inventory.UpdatedAt = DateTime.UtcNow;
            await inv.UpdateAsync(inventory);

            logger.LogInformation($"Stock incremented: Product {dto.ProductId}, Size {dto.Size}, Qty {dto.Quantity}");
            return Ok(new { success = true, message = "Stock incremented successfully" });
        }

        /// <summary>Delete inventory item (admin)</summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await inv.GetByIdAsync(id);
            if (item == null)
                return NotFound();

            await inv.DeleteAsync(id);
            logger.LogInformation($"Inventory {id} deleted");

            return NoContent();
        }
    }

    public record CreateInventoryDto(int ProductId, string Size, int Quantity);
    public record UpdateInventoryDto(int? Quantity, string? Size);
    public record DecrementStockDto(int ProductId, string Size, int Quantity);
    public record IncrementStockDto(int ProductId, string Size, int Quantity);
}
