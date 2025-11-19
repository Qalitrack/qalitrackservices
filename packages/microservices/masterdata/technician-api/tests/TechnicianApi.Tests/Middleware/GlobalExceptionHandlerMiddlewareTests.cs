using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using TechnicianApi.Api.Middleware;
using Xunit;

namespace TechnicianApi.Tests.Middleware;

public class GlobalExceptionHandlerMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _loggerMock;
    private readonly Mock<IWebHostEnvironment> _envMock;

    public GlobalExceptionHandlerMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        _envMock = new Mock<IWebHostEnvironment>();
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextDelegate_WhenNoException()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (HttpContext hc) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object, _envMock.Object);
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleDatabaseException_WithGenericMessage()
    {
        // Arrange
        RequestDelegate next = (HttpContext hc) =>
        {
            throw new DbUpdateException("Database error with sensitive info");
        };

        _envMock.Setup(e => e.EnvironmentName).Returns("Production");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object, _envMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
        context.Response.ContentType.Should().Be("application/json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseText, jsonOptions);

        errorResponse.Should().NotBeNull();
        errorResponse!.Message.Should().Be("A database operation failed. Please contact support.");
        errorResponse.ErrorId.Should().NotBeNullOrEmpty();
        errorResponse.Details.Should().BeNull(); // Should be null in production
        errorResponse.StackTrace.Should().BeNull();
    }

    [Fact]
    public async Task InvokeAsync_ShouldIncludeDetails_InDevelopment()
    {
        // Arrange
        var exceptionMessage = "Detailed error message";
        RequestDelegate next = (HttpContext hc) =>
        {
            throw new InvalidOperationException(exceptionMessage);
        };

        _envMock.Setup(e => e.EnvironmentName).Returns("Development");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object, _envMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseText, jsonOptions);

        errorResponse.Should().NotBeNull();
        errorResponse!.Details.Should().Be(exceptionMessage);
        errorResponse.StackTrace.Should().NotBeNull();
    }

    [Fact]
    public async Task InvokeAsync_ShouldMapArgumentException_ToBadRequest()
    {
        // Arrange
        RequestDelegate next = (HttpContext hc) =>
        {
            throw new ArgumentException("Invalid argument");
        };

        _envMock.Setup(e => e.EnvironmentName).Returns("Production");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object, _envMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseText, jsonOptions);

        errorResponse.Should().NotBeNull();
        errorResponse!.Message.Should().Be("Invalid request parameters.");
    }

    [Fact]
    public async Task InvokeAsync_ShouldMapUnauthorizedException_ToUnauthorized()
    {
        // Arrange
        RequestDelegate next = (HttpContext hc) =>
        {
            throw new UnauthorizedAccessException("Not authorized");
        };

        _envMock.Setup(e => e.EnvironmentName).Returns("Production");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object, _envMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseText, jsonOptions);

        errorResponse.Should().NotBeNull();
        errorResponse!.Message.Should().Be("You are not authorized to perform this action.");
    }

    [Fact]
    public async Task InvokeAsync_ShouldMapKeyNotFoundException_ToNotFound()
    {
        // Arrange
        RequestDelegate next = (HttpContext hc) =>
        {
            throw new KeyNotFoundException("Resource not found");
        };

        _envMock.Setup(e => e.EnvironmentName).Returns("Production");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object, _envMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogException()
    {
        // Arrange
        var exception = new Exception("Test exception");
        RequestDelegate next = (HttpContext hc) =>
        {
            throw exception;
        };

        _envMock.Setup(e => e.EnvironmentName).Returns("Production");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object, _envMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldGenerateUniqueErrorId()
    {
        // Arrange
        RequestDelegate next = (HttpContext hc) =>
        {
            throw new Exception("Test exception");
        };

        _envMock.Setup(e => e.EnvironmentName).Returns("Production");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object, _envMock.Object);

        var context1 = new DefaultHttpContext();
        context1.Response.Body = new MemoryStream();

        var context2 = new DefaultHttpContext();
        context2.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context1);
        await middleware.InvokeAsync(context2);

        // Assert
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        context1.Response.Body.Seek(0, SeekOrigin.Begin);
        var response1Text = await new StreamReader(context1.Response.Body).ReadToEndAsync();
        var error1 = JsonSerializer.Deserialize<ErrorResponse>(response1Text, jsonOptions);

        context2.Response.Body.Seek(0, SeekOrigin.Begin);
        var response2Text = await new StreamReader(context2.Response.Body).ReadToEndAsync();
        var error2 = JsonSerializer.Deserialize<ErrorResponse>(response2Text, jsonOptions);

        error1!.ErrorId.Should().NotBe(error2!.ErrorId);
    }
}
