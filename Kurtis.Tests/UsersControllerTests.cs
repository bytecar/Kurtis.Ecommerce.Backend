using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Kurtis.Api.Users.Controllers;
using Kurtis.Common.Models;
using Kurtis.Common.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Kurtis.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<ILogger<UsersController>> _mockLogger;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            var store = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null);
            _mockLogger = new Mock<ILogger<UsersController>>();
            _controller = new UsersController(_mockUserManager.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateUser_ValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var dto = new CreateUserDTO
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Test@123",
                Role = "customer"
            };

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            
            _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.CreateUser(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(UsersController.GetUser), createdResult.ActionName);
            _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<User>(), dto.Password), Times.Once);
            _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), dto.Role), Times.Once);
        }

        [Fact]
        public async Task CreateUser_UserCreationFails_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CreateUserDTO
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Test@123",
                Role = "customer"
            };

            var errors = new[] { new IdentityError { Description = "User creation failed" } };
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(errors));

            // Act
            var result = await _controller.CreateUser(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task CreateUser_RoleAssignmentFails_DeletesUserAndReturnsBadRequest()
        {
            // Arrange
            var dto = new CreateUserDTO
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Test@123",
                Role = "admin"
            };

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            
            var roleErrors = new[] { new IdentityError { Description = "Role assignment failed" } };
            _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(roleErrors));
            
            _mockUserManager.Setup(x => x.DeleteAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.CreateUser(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            _mockUserManager.Verify(x => x.DeleteAsync(It.IsAny<User>()), Times.Once);
        }
    }
}
