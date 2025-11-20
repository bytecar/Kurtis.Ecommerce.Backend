using Microsoft.AspNetCore.Mvc;
using Kurtis.Common.Models;
using Kurtis.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Kurtis.Common.DTOs;
using Kurtis.common.DTOs;

namespace Kurtis.Api.Orders.Controllers
{
    /// <summary>All endpoints pertaining to Order</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController(KurtisDbContext db, ILogger<OrdersController> logger) : ControllerBase
    {

        /// <summary>Get all orders (admin) or user's orders</summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            IQueryable<Order> query = db.Orders;

            if (!isAdmin)
                query = query.Where(o => o.UserId == userId);

            var total = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { data = orders, total, page, pageSize });
        }

        /// <summary>Get order by ID with items</summary>
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var order = await db.Orders.FindAsync(id);
            if (order == null)
                return NotFound(new { error = "Order not found" });

            var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            if (order.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var items = await db.OrderItems
                .Where(oi => oi.OrderId == id)
                .ToListAsync();

            return Ok(new { order, items });
        }

        /// <summary>Get order statistics (admin)</summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats([FromQuery] int days = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            var orders = await db.Orders
                .Where(o => o.CreatedAt >= cutoffDate)
                .ToListAsync();

            var totalRevenue = orders.Sum(o => o.Total);
            var averageOrderValue = orders.Any() ? orders.Average(o => o.Total) : 0;

            return Ok(new
            {
                totalOrders = orders.Count,
                totalRevenue = totalRevenue,
                averageOrderValue = averageOrderValue,
                byStatus = orders.GroupBy(o => o.Status)
                    .Select(g => new { status = g.Key, count = g.Count() })
            });
        }

        /// <summary>Create order</summary>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderDTO dto)
        {
            var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");

            if (dto.Items == null || !dto.Items.Any())
                return BadRequest(new { error = "Order must contain at least one item" });

            var order = new Order
            {
                UserId = userId,
                Email = dto.Email,
                FullName = dto.FullName,
                Address = dto.Address,
                City = dto.City,
                State = dto.State,
                PostalCode = dto.PostalCode,
                Phone = dto.Phone,
                Status = "pending",
                Total = dto.Total,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            foreach (var item in dto.Items)
            {
                var product = await db.Products.FindAsync(item.ProductId);
                if (product == null)
                    continue;

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Size = item.Size,
                    Quantity = item.Quantity,
                    Price = item.Price
                };

                db.OrderItems.Add(orderItem);

                // Decrement inventory
                var inventory = await db.Set<Kurtis.Common.Models.Inventory>()
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.Size == item.Size);
                if (inventory != null)
                {
                    inventory.Quantity -= item.Quantity;
                }
            }

            await db.SaveChangesAsync();
            logger.LogInformation($"Order {order.Id} created for user {userId}");

            return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
        }

        /// <summary>Update order status (admin)</summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDTO dto)
        {
            var order = await db.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            var validStatuses = new[] { "pending", "processing", "shipped", "delivered", "cancelled" };
            if (!validStatuses.Contains(dto.Status))
                return BadRequest(new { error = "Invalid status" });

            order.Status = dto.Status;
            order.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            logger.LogInformation($"Order {id} status updated to {dto.Status}");

            return Ok(order);
        }

        /// <summary>Cancel order</summary>
        [Authorize]
        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await db.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            if (order.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            if (order.Status != "pending" && order.Status != "processing")
                return BadRequest(new { error = "Cannot cancel order in this status" });

            order.Status = "cancelled";
            order.UpdatedAt = DateTime.UtcNow;

            var items = await db.OrderItems
                .Where(oi => oi.OrderId == id)
                .ToListAsync();

            foreach (var item in items)
            {
                var inventory = await db.Set<Kurtis.Common.Models.Inventory>()
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.Size == item.Size);
                if (inventory != null)
                {
                    inventory.Quantity += item.Quantity;
                }
            }

            await db.SaveChangesAsync();
            logger.LogInformation($"Order {id} cancelled");

            return Ok(new { message = "Order cancelled successfully", order });
        }
    }
}
