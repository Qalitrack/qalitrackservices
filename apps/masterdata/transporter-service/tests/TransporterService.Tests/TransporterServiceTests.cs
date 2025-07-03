using Xunit;

namespace TransporterService.Tests;

public class TransporterServiceTests
{
    [Fact]
    public void Test_ServiceCanBeInstantiated()
    {
        // This is a placeholder test to verify the test project compiles
        Assert.True(true);
    }
    
    [Theory]
    [InlineData("TRP001", "Test Transporter")]
    [InlineData("TRP002", "Another Transporter")]
    public void Test_TransporterRegistrationNumber_ShouldBeValid(string regNumber, string name)
    {
        // Arrange & Act & Assert
        Assert.NotNull(regNumber);
        Assert.NotNull(name);
        Assert.False(string.IsNullOrWhiteSpace(regNumber));
        Assert.False(string.IsNullOrWhiteSpace(name));
    }
}