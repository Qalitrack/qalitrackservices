using FluentAssertions;
using UserService.Infrastructure.Services;
using Xunit;

namespace UserService.Tests.Services;

public class PasswordServiceTests
{
    private readonly PasswordService _passwordService;

    public PasswordServiceTests()
    {
        _passwordService = new PasswordService();
    }

    #region Password Hashing Tests

    [Fact]
    public void HashPassword_WithValidPassword_ShouldReturnHash()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash = _passwordService.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password);
        hash.Length.Should().BeGreaterThan(50); // BCrypt hashes are typically 60 characters
    }

    [Fact]
    public void HashPassword_WithSamePassword_ShouldReturnDifferentHashes()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = _passwordService.HashPassword(password);
        var hash2 = _passwordService.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2); // BCrypt uses different salts
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void HashPassword_WithNullOrEmpty_ShouldThrowException(string password)
    {
        // Act & Assert
        var action = () => _passwordService.HashPassword(password);
        action.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Password Verification Tests

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = _passwordService.HashPassword(password);

        // Act
        var isValid = _passwordService.VerifyPassword(password, hash);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword123!";
        var hash = _passwordService.HashPassword(password);

        // Act
        var isValid = _passwordService.VerifyPassword(wrongPassword, hash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithNullPassword_ShouldReturnFalse()
    {
        // Arrange
        var hash = _passwordService.HashPassword("TestPassword123!");

        // Act
        var isValid = _passwordService.VerifyPassword(null, hash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithNullHash_ShouldReturnFalse()
    {
        // Act
        var isValid = _passwordService.VerifyPassword("TestPassword123!", null);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithInvalidHash_ShouldReturnFalse()
    {
        // Act
        var isValid = _passwordService.VerifyPassword("TestPassword123!", "invalid_hash");

        // Assert
        isValid.Should().BeFalse();
    }

    #endregion

    #region Password Validation Tests

    [Theory]
    [InlineData("Password123!", true)]  // Valid: 12 chars, upper, lower, digit, special
    [InlineData("MyPass1!", true)]      // Valid: 8 chars, upper, lower, digit, special
    [InlineData("Complex@Pass123", true)] // Valid: 13 chars, upper, lower, digit, special
    [InlineData("password123!", false)] // Invalid: no uppercase
    [InlineData("PASSWORD123!", false)] // Invalid: no lowercase
    [InlineData("Password!", false)]    // Invalid: no digit
    [InlineData("Password123", false)]  // Invalid: no special character
    [InlineData("Pass1!", false)]       // Invalid: less than 8 characters
    [InlineData("", false)]             // Invalid: empty
    [InlineData("1234567!", false)]     // Invalid: no letters
    [InlineData("Passwords", false)]    // Invalid: no digit or special char
    public void IsValidPassword_WithVariousPasswords_ShouldReturnExpectedResult(string password, bool expected)
    {
        // Act
        var isValid = _passwordService.IsValidPassword(password);

        // Assert
        isValid.Should().Be(expected);
    }

    [Fact]
    public void IsValidPassword_WithNullPassword_ShouldReturnFalse()
    {
        // Act
        var isValid = _passwordService.IsValidPassword(null);

        // Assert
        isValid.Should().BeFalse();
    }

    #endregion

    #region Password Strength Tests

    [Theory]
    [InlineData("Weak1!", 1)]           // Weak: minimum requirements
    [InlineData("Medium12!", 2)]        // Medium: good length and complexity
    [InlineData("StrongP@ssw0rd123!", 3)] // Strong: long with high complexity
    [InlineData("VeryStr0ng!P@ssw0rd#2023", 4)] // Very Strong: very long with high complexity
    public void GetPasswordStrength_WithVariousPasswords_ShouldReturnCorrectStrength(string password, int expectedStrength)
    {
        // Act
        var strength = _passwordService.GetPasswordStrength(password);

        // Assert
        ((int)strength).Should().Be(expectedStrength);
    }

    [Fact]
    public void GetPasswordStrength_WithInvalidPassword_ShouldReturnWeak()
    {
        // Act
        var strength = _passwordService.GetPasswordStrength("weak");

        // Assert
        strength.Should().Be(PasswordStrength.Weak);
    }

    #endregion

    #region Generate Password Tests

    [Fact]
    public void GenerateSecurePassword_ShouldReturnValidPassword()
    {
        // Act
        var password = _passwordService.GenerateSecurePassword();

        // Assert
        password.Should().NotBeNullOrEmpty();
        password.Length.Should().BeGreaterOrEqualTo(12);
        _passwordService.IsValidPassword(password).Should().BeTrue();
    }

    [Fact]
    public void GenerateSecurePassword_WithCustomLength_ShouldReturnPasswordOfCorrectLength()
    {
        // Arrange
        var length = 16;

        // Act
        var password = _passwordService.GenerateSecurePassword(length);

        // Assert
        password.Should().NotBeNullOrEmpty();
        password.Length.Should().Be(length);
        _passwordService.IsValidPassword(password).Should().BeTrue();
    }

    [Theory]
    [InlineData(4)]   // Too short
    [InlineData(7)]   // Still too short
    public void GenerateSecurePassword_WithTooShortLength_ShouldThrowException(int length)
    {
        // Act & Assert
        var action = () => _passwordService.GenerateSecurePassword(length);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GenerateSecurePassword_MultipleCalls_ShouldReturnDifferentPasswords()
    {
        // Act
        var password1 = _passwordService.GenerateSecurePassword();
        var password2 = _passwordService.GenerateSecurePassword();

        // Assert
        password1.Should().NotBe(password2);
    }

    #endregion

    #region Password Reset Token Tests

    [Fact]
    public void GeneratePasswordResetToken_ShouldReturnUniqueTokens()
    {
        // Act
        var token1 = _passwordService.GeneratePasswordResetToken();
        var token2 = _passwordService.GeneratePasswordResetToken();

        // Assert
        token1.Should().NotBeNullOrEmpty();
        token2.Should().NotBeNullOrEmpty();
        token1.Should().NotBe(token2);
        token1.Length.Should().BeGreaterThan(20);
    }

    [Fact]
    public void ValidatePasswordResetToken_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        var token = _passwordService.GeneratePasswordResetToken();

        // Act
        var isValid = _passwordService.ValidatePasswordResetToken(token);

        // Assert
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("invalid")]
    [InlineData("too_short")]
    public void ValidatePasswordResetToken_WithInvalidToken_ShouldReturnFalse(string token)
    {
        // Act
        var isValid = _passwordService.ValidatePasswordResetToken(token);

        // Assert
        isValid.Should().BeFalse();
    }

    #endregion

    #region Password History Tests

    [Fact]
    public void IsPasswordInHistory_WithNewPassword_ShouldReturnFalse()
    {
        // Arrange
        var oldPasswords = new List<string>
        {
            _passwordService.HashPassword("OldPassword1!"),
            _passwordService.HashPassword("OldPassword2!"),
            _passwordService.HashPassword("OldPassword3!")
        };
        var newPassword = "NewPassword123!";

        // Act
        var isInHistory = _passwordService.IsPasswordInHistory(newPassword, oldPasswords);

        // Assert
        isInHistory.Should().BeFalse();
    }

    [Fact]
    public void IsPasswordInHistory_WithReusedPassword_ShouldReturnTrue()
    {
        // Arrange
        var reusedPassword = "ReusedPassword123!";
        var oldPasswords = new List<string>
        {
            _passwordService.HashPassword("OldPassword1!"),
            _passwordService.HashPassword(reusedPassword),
            _passwordService.HashPassword("OldPassword3!")
        };

        // Act
        var isInHistory = _passwordService.IsPasswordInHistory(reusedPassword, oldPasswords);

        // Assert
        isInHistory.Should().BeTrue();
    }

    [Fact]
    public void IsPasswordInHistory_WithEmptyHistory_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var emptyHistory = new List<string>();

        // Act
        var isInHistory = _passwordService.IsPasswordInHistory(password, emptyHistory);

        // Assert
        isInHistory.Should().BeFalse();
    }

    #endregion
}

public enum PasswordStrength
{
    Weak = 1,
    Medium = 2,
    Strong = 3,
    VeryStrong = 4
}