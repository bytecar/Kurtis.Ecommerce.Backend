using Kurtis.Common.DTOs;
using Kurtis.Common.Models;
using Kurtis.DAL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kurtis.Api.Users.Controllers
{
    [Route("api/user/preferences")]
    [ApiController]
    [Authorize]
    public class UserPreferencesController : ControllerBase
    {
        private readonly IUserPreferencesRepository _preferencesRepo;

        public UserPreferencesController(IUserPreferencesRepository preferencesRepo)
        {
            _preferencesRepo = preferencesRepo;
        }

        /// <summary>
        /// Get current user's preferences
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<UserPreferences>> GetPreferences()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0) return Unauthorized();

            var prefs = await _preferencesRepo.GetByUserIdAsync(userId);
            if (prefs == null) return NotFound();

            return Ok(prefs);
        }

        /// <summary>
        /// Create user preferences
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<UserPreferences>> CreatePreferences([FromBody] CreateUserPreferencesDTO dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0) return Unauthorized();

            var existing = await _preferencesRepo.GetByUserIdAsync(userId);
            if (existing != null) return Conflict("Preferences already exist for this user");

            var prefs = new UserPreferences
            {
                UserId = userId,
                FavoriteCategoriesJson = JsonSerializer.Serialize(dto.FavoriteCategories ?? Array.Empty<string>()),
                FavoriteColorsJson = JsonSerializer.Serialize(dto.FavoriteColors ?? Array.Empty<string>()),
                FavoriteOccasionsJson = JsonSerializer.Serialize(dto.FavoriteOccasions ?? Array.Empty<string>()),
                PriceRangeMin = dto.PriceRangeMin,
                PriceRangeMax = dto.PriceRangeMax,
                UpdatedAt = DateTime.UtcNow
            };

            await _preferencesRepo.AddAsync(prefs);
            return CreatedAtAction(nameof(GetPreferences), prefs);
        }

        /// <summary>
        /// Update user preferences
        /// </summary>
        [HttpPut]
        public async Task<ActionResult<UserPreferences>> UpdatePreferences([FromBody] UpdateUserPreferencesDTO dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0) return Unauthorized();

            var prefs = await _preferencesRepo.GetByUserIdAsync(userId);
            if (prefs == null)
            {
                // Auto-create if not exists
                prefs = new UserPreferences
                {
                    UserId = userId,
                    FavoriteCategoriesJson = JsonSerializer.Serialize(dto.FavoriteCategories ?? Array.Empty<string>()),
                    FavoriteColorsJson = JsonSerializer.Serialize(dto.FavoriteColors ?? Array.Empty<string>()),
                    FavoriteOccasionsJson = JsonSerializer.Serialize(dto.FavoriteOccasions ?? Array.Empty<string>()),
                    PriceRangeMin = dto.PriceRangeMin,
                    PriceRangeMax = dto.PriceRangeMax,
                    UpdatedAt = DateTime.UtcNow
                };
                await _preferencesRepo.AddAsync(prefs);
            }
            else
            {
                if (dto.FavoriteCategories != null)
                    prefs.FavoriteCategoriesJson = JsonSerializer.Serialize(dto.FavoriteCategories);
                
                if (dto.FavoriteColors != null)
                    prefs.FavoriteColorsJson = JsonSerializer.Serialize(dto.FavoriteColors);
                
                if (dto.FavoriteOccasions != null)
                    prefs.FavoriteOccasionsJson = JsonSerializer.Serialize(dto.FavoriteOccasions);
                
                if (dto.PriceRangeMin.HasValue)
                    prefs.PriceRangeMin = dto.PriceRangeMin;
                
                if (dto.PriceRangeMax.HasValue)
                    prefs.PriceRangeMax = dto.PriceRangeMax;

                prefs.UpdatedAt = DateTime.UtcNow;
                await _preferencesRepo.UpdateAsync(prefs);
            }

            return Ok(prefs);
        }
    }
}
