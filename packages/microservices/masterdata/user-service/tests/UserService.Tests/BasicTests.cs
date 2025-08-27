using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.Core.DTOs.Auth;
using UserService.Core.DTOs.User;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Interfaces.Services;
using UserService.Core.Mappings;
using UserService.Core.Services;
using Xunit;

namespace UserService.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IRoleService> _mockRoleService;
    private readonly IMapper _mapper;
    private readonly UserService.Core.Services.UserService _userService;
    private readonly ILogger<UserService.Core.Services.UserService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    

    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockRoleService = new Mock<IRoleService>();
        _logger = new Logger<UserService.Core.Services.UserService>(new LoggerFactory());
        _httpContextAccessor = new HttpContextAccessor();
        
        // Initialize missing mocks
        var mockRoleRepository = new Mock<IRoleRepository>();
        
        // Create a simple mock for PasswordPolicyService
        var mockPasswordPolicyService = new Mock<UserService.Core.Services.PasswordPolicyService>(
            new Mock<IPasswordPolicyRepository>().Object,
            new Mock<IConfiguration>().Object,
            new Mock<ICacheService>().Object,
            new Logger<PasswordPolicyService>(new LoggerFactory()),
            _mockUserRepository.Object
        );
        
        var config = new MapperConfiguration(cfg => cfg.AddProfile<UserProfile>());
        _mapper = config.CreateMapper();
        
        _userService = new UserService.Core.Services.UserService(
            _mockUserRepository.Object,
            _mapper,
            mockRoleRepository.Object,
            _httpContextAccessor,
            mockPasswordPolicyService.Object
            );
    }

    [Fact]
    public void TestFramework_IsWorking()
    {
        // Simple test to verify xunit is working
        Assert.True(true);
    }

    [Fact]
    public void UserService_IsInstantiated()
    {
        // Test that the user service can be created
        Assert.NotNull(_userService);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsUserReadDto()
    {
        // Arrange
        var userId = "test-user-id";
        var user = new User
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            MobileNumber = "1234567890",
            IsActive = true,
            IsDeleted = false
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, true))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.GetByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("john.doe@example.com", result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingUser_ReturnsNull()
    {
        // Arrange
        var userId = "non-existing-id";
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, true))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetByIdAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidUser_ReturnsUserReadDto()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            MobileNumber = "9876543210",
            Password = "SecurePass123!",
            ConfirmPassword = "SecurePass123!"
        };

        var createdUser = new User
        {
            Id = "new-user-id",
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            Email = createUserDto.Email,
            MobileNumber = createUserDto.MobileNumber,
            IsActive = false,
            IsFirstLogin = true,
            IsDeleted = false
        };

        _mockUserRepository.Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) =>
            {
                user.Id = createdUser.Id;
                user.CreatedAt = DateTime.UtcNow;
                return user;
            });

        // Act
        var result = await _userService.CreateAsync(createUserDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createUserDto.FirstName, result.FirstName);
        Assert.Equal(createUserDto.LastName, result.LastName);
        Assert.Equal(createUserDto.Email, result.Email);
        Assert.False(result.IsActive);
        Assert.True(result.IsFirstLogin);
        
        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task ValidateUserCredentials_ValidCredentials_ReturnsUser()
    {
        // Arrange
        var email = "john.doe@example.com";
        var password = "ValidPassword123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        
        var user = new User
        {
            Id = "test-user-id",
            Email = email,
            Password = hashedPassword,
            IsActive = true,
            IsDeleted = false
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(email.ToLower()))
            .ReturnsAsync(user);
        
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.ValidateUserCredentials(email, password);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(email, result.Email);
        
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task ValidateUserCredentials_InvalidPassword_ReturnsNull()
    {
        // Arrange
        var email = "john.doe@example.com";
        var password = "InvalidPassword";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!");
        
        var user = new User
        {
            Id = "test-user-id",
            Email = email,
            Password = hashedPassword,
            IsActive = true,
            IsDeleted = false
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(email.ToLower()))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.ValidateUserCredentials(email, password);

        // Assert
        Assert.Null(result);
        
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ExistingUser_ReturnsTrue()
    {
        // Arrange
        var userId = "test-user-id";
        var existingUser = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            IsActive = true,
            IsDeleted = false
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, true))
            .ReturnsAsync(existingUser);
        _mockUserRepository.Setup(x => x.DeleteAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.DeleteAsync(userId);

        // Assert
        Assert.True(result);
        _mockUserRepository.Verify(x => x.GetByIdAsync(userId, true), Times.Once);
        _mockUserRepository.Verify(x => x.DeleteAsync(userId), Times.Once);
    }
}