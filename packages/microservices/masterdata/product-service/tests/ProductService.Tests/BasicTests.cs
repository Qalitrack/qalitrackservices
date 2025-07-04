using Xunit;
using FluentAssertions;

namespace ProductService.Tests;

[Trait("Category", "Unit")]
public class BasicTests
{
    [Fact]
    public void BasicTest_ShouldPass()
    {
        // Arrange
        var value = "test";

        // Act
        var result = value.ToUpper();

        // Assert
        result.Should().Be("TEST");
    }

    [Fact]
    public void ProductService_ShouldHaveCorrectAssemblyReference()
    {
        // Arrange & Act
        var coreAssembly = typeof(ProductService.Core.Entities.Product).Assembly;
        var apiAssembly = typeof(ProductService.Api.Controllers.ProductsController).Assembly;

        // Assert
        coreAssembly.Should().NotBeNull();
        apiAssembly.Should().NotBeNull();
    }
}