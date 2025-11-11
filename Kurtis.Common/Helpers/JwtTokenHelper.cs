using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Kurtis.Common.Helpers
{
    /// <summary>
    /// Centralized helper for creating and validating JWT tokens.
    /// Supports additional testing utilities for expiry and tampering.
    /// </summary>
    public static class JwtTokenHelper
    {
        private static readonly string DefaultSecret = "VerySecret_JWT_Key_ChangeThis";

        /// <summary>
        /// Generate a valid JWT for the specified user and role.
        /// </summary>
        public static string GenerateToken(string username, string role, int expireMinutes = 60, string? secret = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret ?? DefaultSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Generate a token with a custom absolute expiry date (for expiry testing).
        /// </summary>
        public static string GenerateTokenWithExpiry(string username, string role, DateTime expiryUtc, string? secret = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret ?? DefaultSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = expiryUtc,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Generate a tampered JWT — valid structure but invalid signature.
        /// </summary>
        public static string GenerateTamperedToken(string username = "user", string role = "User")
        {
            // Create a normal token first
            var validToken = GenerateToken(username, role, 30);
            // Tamper the signature part (JWT is base64url header.payload.signature)
            var parts = validToken.Split('.');
            if (parts.Length == 3)
                parts[2] = Convert.ToBase64String(Encoding.UTF8.GetBytes("invalidsig"));
            return string.Join('.', parts);
        }

        /// <summary>
        /// Validate token signature and expiry; return claims principal or null.
        /// </summary>
        public static ClaimsPrincipal? ValidateToken(string token, string? secret = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret ?? DefaultSecret);
            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
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
