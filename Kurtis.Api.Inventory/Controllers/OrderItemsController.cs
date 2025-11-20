using Kurtis.Common.Models;
using Kurtis.DAL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kurtis.Api.Inventory.Controllers
{
    [Route("api/orders/{orderId}/items")]
    [ApiController]
    [Authorize]
    public class OrderItemsController : ControllerBase
    {
        private readonly IOrderItemRepository _orderItemRepo;

        public OrderItemsController(IOrderItemRepository orderItemRepo)
        {
            _orderItemRepo = orderItemRepo;
        }

        /// <summary>
        /// Get all items for a specific order
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderItem>>> GetByOrderId(int orderId)
        {
            var items = await _orderItemRepo.GetByOrderIdAsync(orderId);
            return Ok(items);
        }

        /// <summary>
        /// Add an item to an order
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrderItem>> AddItem(int orderId, [FromBody] OrderItem orderItem)
        {
            if (orderId != orderItem.OrderId)
            {
                return BadRequest("Order ID mismatch");
            }

            await _orderItemRepo.AddAsync(orderItem);
            return CreatedAtAction(nameof(GetByOrderId), new { orderId = orderId }, orderItem);
        }

        /// <summary>
        /// Remove an item from an order
        /// </summary>
        [HttpDelete("{itemId}")]
        public async Task<IActionResult> RemoveItem(int orderId, int itemId)
        {
            var item = await _orderItemRepo.GetByIdAsync(itemId);
            if (item == null || item.OrderId != orderId)
            {
                return NotFound();
            }

            await _orderItemRepo.DeleteAsync(itemId);
            return NoContent();
        }
    }
}
