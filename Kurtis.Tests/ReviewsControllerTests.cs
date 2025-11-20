using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Kurtis.Api.Catalog.Controllers;
using Kurtis.Common.Models;
using Kurtis.DAL;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Kurtis.Tests.Controllers
{
    public class ReviewsControllerTests : IDisposable
    {
        private readonly KurtisDbContext _context;
        private readonly ReviewsController _controller;

        public ReviewsControllerTests()
        {
            var options = new DbContextOptionsBuilder<KurtisDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new KurtisDbContext(options);
            var logger = new LoggerFactory().CreateLogger<ReviewsController>();
            _controller = new ReviewsController(_context, logger);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithReviews()
        {
            // Arrange
            _context.Reviews.AddRange(
                new Review { Id = 1, ProductId = 1, UserId = 1, Rating = 5, Comment = "Great!", CreatedAt = DateTime.UtcNow },
                new Review { Id = 2, ProductId = 2, UserId = 2, Rating = 4, Comment = "Good", CreatedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAll(1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            
            var value = okResult.Value;
            var dataProperty = value.GetType().GetProperty("data");
            Assert.NotNull(dataProperty);
            
            var reviews = dataProperty.GetValue(value) as IEnumerable<Review>;
            Assert.NotNull(reviews);
            Assert.Equal(2, reviews.Count());
        }

        [Fact]
        public async Task GetAll_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var reviews = Enumerable.Range(1, 50).Select(i => new Review
            {
                ProductId = 1,
                UserId = 1,
                Rating = 5,
                Comment = $"Review {i}",
                CreatedAt = DateTime.UtcNow
            }).ToList();
            
            _context.Reviews.AddRange(reviews);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAll(2, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            
            var value = okResult.Value;
            var dataProperty = value.GetType().GetProperty("data");
            var reviewsData = dataProperty?.GetValue(value) as IEnumerable<Review>;
            
            Assert.NotNull(reviewsData);
            Assert.Equal(20, reviewsData.Count());
        }

        [Fact]
        public async Task GetAll_EmptyDatabase_ReturnsEmptyList()
        {
            // Act
            var result = await _controller.GetAll(1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var dataProperty = value.GetType().GetProperty("data");
            var reviews = dataProperty?.GetValue(value) as IEnumerable<Review>;
            
            Assert.NotNull(reviews);
            Assert.Empty(reviews);
        }
    }
}
