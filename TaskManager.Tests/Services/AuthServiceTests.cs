using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.API.DTOs;
using TaskManager.API.Models;
using TaskManager.API.Services;
using TaskManager.Tests.Helpers;

namespace TaskManager.Tests.Services
{
    public class AuthServiceTests
    {
        private AuthService CreateService(string dbName)
        {
            var context = TestDbContextFactory.Create(dbName);

            var configData = new Dictionary<string, string?>
            {
                { "Jwt:Key", "ThisIsASuperSecretKeyForTestingPurposesOnly1234567890!" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var logger = new Mock<ILogger<AuthService>>();

            return new AuthService(context, configuration, logger.Object);
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ReturnsSuccessMessage()
        {
            // Arrange
            var service = CreateService(nameof(RegisterAsync_WithValidData_ReturnsSuccessMessage));
            var dto = new UserRegisterDto { Name = "Test User", Email = "test@example.com", Password = "password123" };

            // Act
            var result = await service.RegisterAsync(dto);

            // Assert
            Assert.Equal("User registered successfully.", result);
        }

        [Fact]
        public async Task RegisterAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
        {
            // Arrange
            var service = CreateService(nameof(RegisterAsync_WithDuplicateEmail_ThrowsInvalidOperationException));
            var dto = new UserRegisterDto { Name = "Test User", Email = "test@example.com", Password = "password123" };

            await service.RegisterAsync(dto);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterAsync(dto));
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsLoginResponse()
        {
            // Arrange
            var service = CreateService(nameof(LoginAsync_WithValidCredentials_ReturnsLoginResponse));
            var registerDto = new UserRegisterDto { Name = "Test User", Email = "test@example.com", Password = "password123" };
            await service.RegisterAsync(registerDto);

            var loginDto = new UserLoginDto { Email = "test@example.com", Password = "password123" };

            // Act
            var result = await service.LoginAsync(loginDto);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Token);
            Assert.Equal("Test User", result.Name);
            Assert.Equal("test@example.com", result.Email);
            Assert.Equal("User", result.Role);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var service = CreateService(nameof(LoginAsync_WithInvalidPassword_ThrowsUnauthorizedAccessException));
            var registerDto = new UserRegisterDto { Name = "Test User", Email = "test@example.com", Password = "password123" };
            await service.RegisterAsync(registerDto);

            var loginDto = new UserLoginDto { Email = "test@example.com", Password = "wrongpassword" };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(loginDto));
        }

        [Fact]
        public async Task LoginAsync_WithNonExistentEmail_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var service = CreateService(nameof(LoginAsync_WithNonExistentEmail_ThrowsUnauthorizedAccessException));
            var loginDto = new UserLoginDto { Email = "nonexistent@example.com", Password = "password123" };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(loginDto));
        }
    }
}
