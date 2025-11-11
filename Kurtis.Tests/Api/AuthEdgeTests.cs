
using FluentAssertions;
using Kurtis.Common.Helpers;
using Kurtis.Tests.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace Kurtis.Tests.Api
{
    public class AuthEdgeTests
    {
        [Fact]
        public async Task ExpiredToken_IsUnauthorized()
        {
            var factory = new WebApplicationFactory<Kurtis.Api.Catalog.Program>();
            var client = TestHelpers.CreateClient(factory);            
            var token = JwtTokenHelper.GenerateTokenWithExpiry("u", "User", DateTime.UtcNow.AddMinutes(-10));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var res = await client.GetAsync("/api/products");
            res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task TamperedToken_IsUnauthorized()
        {
            var factory = new WebApplicationFactory<Kurtis.Api.Catalog.Program>();
            var client = TestHelpers.CreateClient(factory);
            var token = JwtTokenHelper.GenerateTamperedToken();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var res = await client.GetAsync("/api/products");
            res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        [Fact]
        public void Expired_Token_Should_Fail_Validation()
        {
            var expired = JwtTokenHelper.GenerateTokenWithExpiry("u", "User", DateTime.UtcNow.AddMinutes(-10));
            JwtTokenHelper.ValidateToken(expired).Should().BeNull();
        }

        [Fact]
        public void Tampered_Token_Should_Fail_Validation()
        {
            var tampered = JwtTokenHelper.GenerateTamperedToken();
            JwtTokenHelper.ValidateToken(tampered).Should().BeNull();
        }

    }
}
