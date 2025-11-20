using Kurtis.Common.DTOs;
using Kurtis.Common.Models;
using Kurtis.DAL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Kurtis.Api.Inventory.Controllers
{
    [Route("api/returns")]
    [ApiController]
    [Authorize]
    public class ReturnsController : ControllerBase
    {
        private readonly IReturnRepository _returnRepo;

        public ReturnsController(IReturnRepository returnRepo)
        {
            _returnRepo = returnRepo;
        }

        /// <summary>
        /// Get all returns (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Return>>> GetAllReturns()
        {
            var returns = await _returnRepo.GetAllAsync();
            return Ok(returns);
        }

        /// <summary>
        /// Get returns for current user
        /// </summary>
        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<Return>>> GetUserReturns()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0) return Unauthorized();

            var returns = await _returnRepo.GetByUserIdAsync(userId);
            return Ok(returns);
        }

        /// <summary>
        /// Create a return request
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Return>> CreateReturn([FromBody] CreateReturnDTO dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0) return Unauthorized();

            // TODO: Verify order belongs to user (would need OrderRepository here)

            var returnRequest = new Return
            {
                OrderId = dto.OrderId,
                OrderItemId = dto.OrderItemId,
                Reason = dto.Reason,
                Status = "pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _returnRepo.AddAsync(returnRequest);
            return CreatedAtAction(nameof(GetUserReturns), new { id = returnRequest.Id }, returnRequest);
        }

        /// <summary>
        /// Update return status (Admin only)
        /// </summary>
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Return>> UpdateStatus(int id, [FromBody] UpdateReturnDTO dto)
        {
            var returnRequest = await _returnRepo.GetByIdAsync(id);
            if (returnRequest == null) return NotFound();

            returnRequest.Status = dto.Status;
            returnRequest.UpdatedAt = DateTime.UtcNow;

            await _returnRepo.UpdateAsync(returnRequest);
            return Ok(returnRequest);
        }
    }
}
