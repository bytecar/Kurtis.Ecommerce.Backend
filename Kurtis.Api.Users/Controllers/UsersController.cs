using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Kurtis.Common.DTOs;
using Kurtis.Common.Models;

namespace Kurtis.Api.Users.Controllers
{
    /// <summary>All endpoints pertaining to Users</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController(UserManager<User> userManager, ILogger<UsersController> logger) : ControllerBase
    {

        /// <summary>Get user by ID (admin only)</summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound(new { error = "User not found" });

            var roles = await userManager.GetRolesAsync(user);
            return Ok(new
            {
                id = user.Id,
                email = user.Email,
                username = user.UserName,
                roles = roles,
                emailConfirmed = user.EmailConfirmed
            });
        }

        /// <summary>List all users (admin only)</summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult ListUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var users = userManager.Users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.UserName,
                    u.EmailConfirmed
                })
                .ToList();

            return Ok(new { data = users, page, pageSize });
        }

        /// <summary>Create a new user (admin only)</summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = new User
            {
                UserName = dto.Username,
                Email = dto.Email,
                EmailConfirmed = true, // Admins create confirmed users
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            // Assign role
            try 
            {
                var roleResult = await userManager.AddToRoleAsync(user, dto.Role);
                if (!roleResult.Succeeded)
                {
                    // If role assignment fails, delete the user to avoid partial state
                    await userManager.DeleteAsync(user);
                    return BadRequest(new { errors = roleResult.Errors.Select(e => e.Description) });
                }
            }
            catch (Exception ex)
            {
                await userManager.DeleteAsync(user);
                return BadRequest(new { error = $"Failed to assign role: {ex.Message}" });
            }

            logger.LogInformation($"User {user.Id} created by admin with role {dto.Role}");
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new { user.Id, user.Email, user.UserName, Role = dto.Role });
        }

        /// <summary>Update user profile (own user or admin)</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO dto)
        {
            var currentUserId = User.FindFirst("uid")?.Value;
            var isAdmin = User.IsInRole("Admin");

            if (currentUserId != id.ToString() && !isAdmin)
                return Forbid();

            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound();

            if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
            {
                var existingUser = await userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                    return BadRequest(new { error = "Email already in use" });

                user.Email = dto.Email;
                user.UserName = dto.Email; // Keep username in sync
            }

            if (!string.IsNullOrEmpty(dto.PhoneNumber))
                user.PhoneNumber = dto.PhoneNumber;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            logger.LogInformation($"User {id} profile updated");
            return Ok(new { message = "User updated successfully", user = new { user.Id, user.Email, user.UserName } });
        }

        /// <summary>Delete user (admin only)</summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound();

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            logger.LogInformation($"User {id} deleted");
            return NoContent();
        }

        /// <summary>Assign role to user (admin only)</summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("{id:int}/roles")]
        public async Task<IActionResult> AssignRole(int id, [FromBody] AssignRoleDTO dto)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound();

            if (!await userManager.IsInRoleAsync(user, dto.Role))
                await userManager.AddToRoleAsync(user, dto.Role);

            return Ok(new { message = $"Role {dto.Role} assigned to user {id}" });
        }

        /// <summary>Remove role from user (admin only)</summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}/roles/{role}")]
        public async Task<IActionResult> RemoveRole(int id, string role)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound();

            if (await userManager.IsInRoleAsync(user, role))
                await userManager.RemoveFromRoleAsync(user, role);

            return Ok(new { message = $"Role {role} removed from user {id}" });
        }
    }
}
