using Kurtis.common.DTOs;
using Kurtis.Common.DTOs;
using Kurtis.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Kurtis.Api.Auth.Controllers
{
    /// <summary>All endpoints pertaining to Authentication</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IConfiguration config,
        ILogger<AuthController> logger) : ControllerBase
    {

        /// <summary>Register a new user with email/password</summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = new User()
            {
                UserName = dto.Username,
                Email = dto.Email,
                EmailConfirmed = false,
            };

            var result = await userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            await userManager.AddToRoleAsync(user, "customer");
            logger.LogInformation($"User {user.Email} registered successfully");

            return Ok(new { id = user.Id, email = user.Email, username = user.UserName });
        }

        /// <summary>Login with email and password</summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                logger.LogWarning($"Login attempt for non-existent user: {dto.Email}");
                return Unauthorized(new { error = "Invalid credentials" });
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
            {
                logger.LogWarning($"Failed login attempt for user: {dto.Email}");
                return Unauthorized(new { error = "Invalid credentials" });
            }

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            logger.LogInformation($"User {user.Email} logged in successfully");

            return Ok(new
            {
                accessToken = token,
                refreshToken = refreshToken,
                user = new { id = user.Id, email = user.Email, username = user.UserName }
            });
        }

        /// <summary>Refresh JWT token using refresh token</summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO dto)
        {
            if (string.IsNullOrEmpty(dto.RefreshToken))
                return BadRequest(new { error = "Refresh token required" });

            try
            {
                var principal = GetPrincipalFromExpiredToken(dto.AccessToken);
                var userId = principal?.FindFirst("uid")?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "Invalid token" });

                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                    return Unauthorized(new { error = "User not found" });

                var newAccessToken = GenerateJwtToken(user);
                var newRefreshToken = GenerateRefreshToken();

                return Ok(new { accessToken = newAccessToken, refreshToken = newRefreshToken });
            }
            catch (Exception ex)
            {
                logger.LogError($"Token refresh failed: {ex.Message}");
                return Unauthorized(new { error = "Invalid refresh token" });
            }
        }

        /// <summary>Get current authenticated user profile</summary>
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "User not found in token" });

            var user = await userManager.FindByIdAsync(userId);
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

        /// <summary>Logout user (invalidate token client-side)</summary>
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            logger.LogInformation($"User {User.Identity?.Name} logged out");
            return Ok(new { message = "Logged out successfully" });
        }

        /// <summary>Change user password (requires authentication)</summary>
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            logger.LogInformation($"User {user.Email} changed password");
            return Ok(new { message = "Password changed successfully" });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new InvalidOperationException("JWT Key not configured");

            var key = Encoding.ASCII.GetBytes(jwtKey);
            var roles = userManager.GetRolesAsync(user).GetAwaiter().GetResult();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim("uid", user.Id.ToString())
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(int.Parse(config["Jwt:ExpiryHours"] ?? "4")),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = config["Jwt:Issuer"],
                Audience = config["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var key = Encoding.ASCII.GetBytes(config["Jwt:Key"] ?? "");
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = config["Jwt:Audience"],
                    ValidateLifetime = false // Allow expired tokens for refresh
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
