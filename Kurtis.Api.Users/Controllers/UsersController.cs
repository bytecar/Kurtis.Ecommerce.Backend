
using Microsoft.AspNetCore.Mvc;
using Kurtis.Common.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Kurtis.Api.Users.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<IdentityUser<int>> _users;
        private readonly SignInManager<IdentityUser<int>> _signin;
        private readonly IConfiguration _cfg;

        public UsersController(UserManager<IdentityUser<int>> users, SignInManager<IdentityUser<int>> signin, IConfiguration cfg)
        {
            _users = users; _signin = signin; _cfg = cfg;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var user = new IdentityUser<int> { UserName = dto.Username, Email = dto.Email, NormalizedUserName = dto.DisplayName };
            var res = await _users.CreateAsync(user, dto.Password);
            if (!res.Succeeded) return BadRequest(res.Errors);
            await _users.AddToRoleAsync(user, "user");
            return Ok(new { user.Id, user.Email });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _users.FindByEmailAsync(dto.Email);
            if (user == null) return Unauthorized();
            var sig = await _signin.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!sig.Succeeded) return Unauthorized();
            var token = GenerateToken(user);
            return Ok(new { token });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var idClaim = User.FindFirst("uid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (idClaim==null) return Unauthorized();
            var user = await _users.FindByIdAsync(idClaim);
            if (user==null) return Unauthorized();
            return Ok(new { user.Id, user.Email, user.NormalizedUserName });
        }

        private string GenerateToken(IdentityUser<int> user)
        {
            var key = Encoding.ASCII.GetBytes(_cfg["Jwt:Key"] ?? "VerySecret_JWT_Key_ChangeThis");
            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("uid", user.Id.ToString())
            };
            var roles = _users.GetRolesAsync(user).GetAwaiter().GetResult();
            foreach(var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(4),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _cfg["Jwt:Issuer"],
                Audience = _cfg["Jwt:Audience"]
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public record RegisterDto(string Username, string Email, string Password, string? DisplayName);
    public record LoginDto(string Email, string Password);
}
