using Xunit;

namespace ProductService.Tests;

public class BasicTests
{
    [Fact]
    public void BasicTest_ShouldPass()
    {
        // Arrange
        var expected = true;
        
        // Act
        var actual = true;
        
        // Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void BasicMathTest_ShouldPass()
    {
        // Arrange
        var a = 2;
        var b = 3;
        var expected = 5;
        
        // Act
        var actual = a + b;
        
        // Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void StringTest_ShouldPass()
    {
        // Arrange
        var text = "Hello World";
        var expected = "Hello World";
        
        // Act
        var actual = text;
        
        // Assert
        Assert.Equal(expected, actual);
    }
}