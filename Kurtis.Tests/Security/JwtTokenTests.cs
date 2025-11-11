
using Xunit;
using FluentAssertions;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

namespace Kurtis.Tests.Security
{
    public class JwtTokenTests
    {
        [Fact]
        public void Can_Create_And_Validate_Jwt()
        {
            var key = Encoding.ASCII.GetBytes("VerySecret_JWT_Key_ChangeThis");
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateJwtSecurityToken(subject: new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "u") }),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256));
            var jwt = tokenHandler.WriteToken(token);
            jwt.Should().NotBeNullOrEmpty();
        }
    }
}
