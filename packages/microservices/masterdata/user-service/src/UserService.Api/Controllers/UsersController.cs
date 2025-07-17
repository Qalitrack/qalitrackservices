using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Core.DTOs.User;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace UserService.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger<UsersController> _logger;
        private readonly IUserRoleService _userRoleService;

        public UsersController(
            IUserService userService, 
            IUserRoleService userRoleService,
            IMapper mapper, 
            ILogger<UsersController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _userRoleService = userRoleService ?? throw new ArgumentNullException(nameof(userRoleService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [Authorize(Policy = "users.view")]
        public async Task<IEnumerable<UserReadDto>> GetAll()
        {
            try
            {
                return (IEnumerable<UserReadDto>)await _userService.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                throw;
            }
        }

        [HttpGet("deleted")]
        [Authorize(Policy = "users.manage")]
        public async Task<IEnumerable<UserReadDto>> GetDeleted()
        {
            try
            {
                return (IEnumerable<UserReadDto>)await _userService.GetDeletedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deleted users");
                throw;
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "users.view")]
        public async Task<UserReadDto> GetById(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found");
            }
            return user;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<UserReadDto> Create([FromBody] CreateUserDto createUserDto)
        {
            return await _userService.CreateAsync(createUserDto);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "users.manage")]
        public async Task<UserReadDto> Update(string id, [FromBody] UpdateUserDto updateUserDto)
        {
            UserReadDto result = await _userService.UpdateAsync(id, updateUserDto);
            if (result == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found");
            }
            return result;
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "users.manage")]
        public async Task Delete(string id)
        {
            var result = await _userService.DeleteAsync(id);
            if (!result)
            {
                throw new KeyNotFoundException($"User with ID {id} not found");
            }
        }

        [HttpPatch("{id}/restore")]
        [Authorize(Policy = "users.manage")]
        public async Task Restore(string id)
        {
            var result = await _userService.RestoreAsync(id);
            if (!result)
            {
                throw new KeyNotFoundException($"Deleted user with ID {id} not found");
            }
        }

        [HttpGet("{userId}/permissions")]
        [Authorize(Policy = "users.view")]
        public async Task<IEnumerable<string>> GetUserPermissions(string userId)
        {
            // Ensure the user is provided
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Enumerable.Empty<string>();  // Return empty if no userId is provided
            }

            // Get the permissions for the user from the service layer
            var permissions = await _userService.GetUserPermissionsAsync(userId);

            // If permissions are null or empty, handle gracefully
            if (permissions == null || !permissions.Any())
            {
                return Enumerable.Empty<string>();  // Return empty if no permissions are found
            }

            // Return the list of permission names (strings)
            return permissions.Select(p => p.Name);  // Assuming Permission has a 'Name' property
        }



    }
}