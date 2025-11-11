
using Xunit;
using FluentAssertions;
using Moq;
using Kurtis.DAL.Interfaces;
using Kurtis.Common.Models;
using Kurtis.Api.Catalog.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Kurtis.Tests.Controllers
{
    public class ProductsControllerTests
    {
        [Fact]
        public async Task GetAll_Returns_Ok()
        {
            var mockRepo = new Mock<IProductRepository>();
            mockRepo.Setup(r => r.SearchAsync(null,1,20)).ReturnsAsync(new List<Product>());
            var controller = new ProductsController(mockRepo.Object);
            var res = await controller.GetAll(null, 2, 5);
            res.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Get_Returns_NotFound_When_Missing()
        {
            var mockRepo = new Mock<IProductRepository>();
            mockRepo.Setup(r => r.GetWithDetailsAsync(1)).ReturnsAsync((Product?)null);
            var controller = new ProductsController(mockRepo.Object);
            var res = await controller.Get(1);
            res.Should().BeOfType<NotFoundResult>();
        }
    }
}                                                                              