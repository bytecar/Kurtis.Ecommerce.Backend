using Kurtis.Common.DTOs;
using Kurtis.Common.Models;
using Kurtis.DAL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Kurtis.Api.Users.Controllers
{
    [Route("api/user/recently-viewed")]
    [ApiController]
    [Authorize]
    public class RecentlyViewedController : ControllerBase
    {
        private readonly IRecentlyViewedRepository _recentlyViewedRepo;

        public RecentlyViewedController(IRecentlyViewedRepository recentlyViewedRepo)
        {
            _recentlyViewedRepo = recentlyViewedRepo;
        }

        /// <summary>
        /// Get recently viewed products for the current user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecentlyViewed>>> GetRecentlyViewed()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0) return Unauthorized();

            var items = await _recentlyViewedRepo.GetByUserIdAsync(userId);
            return Ok(items);
        }

        /// <summary>
        /// Add a product to recently viewed history
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> AddRecentlyViewed([FromBody] CreateRecentlyViewedDTO dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0) return Unauthorized();

            var recentlyViewed = new RecentlyViewed
            {
                UserId = userId,
                ProductId = dto.ProductId,
                ViewedAt = DateTime.UtcNow
            };

            await _recentlyViewedRepo.AddAsync(recentlyViewed);
            
            // Cleanup old records
            await _recentlyViewedRepo.DeleteOldRecordsAsync(userId);

            return Ok();
        }
    }
}
