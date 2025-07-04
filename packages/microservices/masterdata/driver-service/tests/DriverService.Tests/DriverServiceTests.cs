using DriverService.Core.Entities;

namespace DriverService.Tests;

public class DriverServiceTests
{
    [Fact]
    public void Driver_ShouldHaveValidDefaults()
    {
        // Arrange & Act
        var driver = new Driver();

        // Assert
        Assert.NotNull(driver.Id);
        Assert.NotEqual(Guid.Empty.ToString(), driver.Id);
        Assert.Equal(DriverStatus.Active, driver.Status);
        Assert.Equal(EmploymentType.FullTime, driver.EmploymentType);
        Assert.False(driver.IsDeleted);
        Assert.True(driver.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void DriverLicense_ShouldHaveValidDefaults()
    {
        // Arrange & Act
        var license = new DriverLicense();

        // Assert
        Assert.NotNull(license.Id);
        Assert.NotEqual(Guid.Empty.ToString(), license.Id);
        Assert.Equal(LicenseStatus.Active, license.Status);
        Assert.Equal(0, license.Points);
        Assert.False(license.IsDeleted);
        Assert.True(license.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void DriverProfile_ShouldHaveValidDefaults()
    {
        // Arrange & Act
        var profile = new DriverProfile();

        // Assert
        Assert.NotNull(profile.Id);
        Assert.NotEqual(Guid.Empty.ToString(), profile.Id);
        Assert.True(profile.HasCleanRecord);
        Assert.Equal(5.0m, profile.SafetyRating);
        Assert.Equal(5.0m, profile.PerformanceRating);
        Assert.True(profile.IsAvailableForOvertire);
        Assert.True(profile.IsAvailableForWeekends);
        Assert.True(profile.IsAvailableForNightShifts);
        Assert.False(profile.IsDeleted);
        Assert.True(profile.CreatedAt <= DateTime.UtcNow);
    }
}