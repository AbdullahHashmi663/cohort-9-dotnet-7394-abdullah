using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text.Json;
using TaskManager.API.Middleware;

namespace TaskManager.Tests.Middleware
{
    public class ExceptionHandlingMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_WhenNoException_CallsNext()
        {
            // Arrange
            var nextCalled = false;
            RequestDelegate next = (context) => { nextCalled = true; return Task.CompletedTask; };
            var logger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
            var middleware = new ExceptionHandlingMiddleware(next, logger.Object);
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task InvokeAsync_WhenKeyNotFoundException_Returns404()
        {
            // Arrange
            RequestDelegate next = (context) => throw new KeyNotFoundException("Resource not found");
            var logger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
            var middleware = new ExceptionHandlingMiddleware(next, logger.Object);
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.NotFound, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_WhenUnauthorizedAccessException_Returns401()
        {
            // Arrange
            RequestDelegate next = (context) => throw new UnauthorizedAccessException("Unauthorized");
            var logger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
            var middleware = new ExceptionHandlingMiddleware(next, logger.Object);
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_WhenInvalidOperationException_Returns400()
        {
            // Arrange
            RequestDelegate next = (context) => throw new InvalidOperationException("Bad request");
            var logger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
            var middleware = new ExceptionHandlingMiddleware(next, logger.Object);
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.BadRequest, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_WhenUnhandledException_Returns500WithGenericMessage()
        {
            // Arrange
            RequestDelegate next = (context) => throw new Exception("Internal error details");
            var logger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
            var middleware = new ExceptionHandlingMiddleware(next, logger.Object);
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
            Assert.DoesNotContain("Internal error details", body); // Should not leak internal details
            Assert.Contains("internal server error", body, StringComparison.OrdinalIgnoreCase);
        }
    }
}
