// using Microsoft.AspNetCore.Mvc;
// using UserService.Core.DTOs.User;
// using UserService.Core.Interfaces;
//
// namespace UserService.Api.Controllers;
//
// [Route("api/[controller]")]
// public class UsersController : BaseController
// {
//     private readonly IUserService _userService;
//     private readonly ILogger<UsersController> _logger;
//
//     public UsersController(IUserService userService, ILogger<UsersController> logger)
//     {
//         _userService = userService;
//         _logger = logger;
//     }
//
//     /// <summary>
//     /// Get all users
//     /// </summary>
//     [HttpGet]
//     public async Task<IActionResult> GetAll()
//     {
//         try
//         {
//             var users = await _userService.GetAllAsync();
//             return Ok(users);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error getting all users");
//             return InternalServerError("An error occurred while retrieving users");
//         }
//     }
//
//     /// <summary>
//     /// Get user by ID
//     /// </summary>
//     [HttpGet("{id}")]
//     public async Task<IActionResult> GetById(string id)
//     {
//         try
//         {
//             var user = await _userService.GetByIdAsync(id);
//             if (user == null)
//             {
//                 return NotFound("User not found");
//             }
//
//             return Ok(user);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error getting user with id {Id}", id);
//             return InternalServerError("An error occurred while retrieving user");
//         }
//     }
//
//     /// <summary>
//     /// Create a new user
//     /// </summary>
//     [HttpPost]
//     public async Task<IActionResult> Create([FromBody] CreateUserDto request)
//     {
//         try
//         {
//             var user = await _userService.CreateAsync(request);
//             return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error creating user");
//             return InternalServerError("An error occurred while creating user");
//         }
//     }
//
//     /// <summary>
//     /// Update an existing user
//     /// </summary>
//     [HttpPut("{id}")]
//     public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto request)
//     {
//         try
//         {
//             var user = await _userService.UpdateAsync(id, request);
//             if (user == null)
//             {
//                 return NotFound("User not found");
//             }
//
//             return Ok(user, "User updated successfully");
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error updating user with id {Id}", id);
//             return InternalServerError("An error occurred while updating user");
//         }
//     }
//
//     /// <summary>
//     /// Delete a user
//     /// </summary>
//     [HttpDelete("{id}")]
//     public async Task<IActionResult> Delete(string id)
//     {
//         try
//         {
//             var result = await _userService.DeleteAsync(id);
//             if (!result)
//             {
//                 return NotFound("User not found");
//             }
//
//             return Ok<object?>(null, "User deleted successfully");
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error deleting user with id {Id}", id);
//             return InternalServerError("An error occurred while deleting user");
//         }
//     }
//
//     /// <summary>
//     /// Check if user name is available
//     /// </summary>
//     
// }