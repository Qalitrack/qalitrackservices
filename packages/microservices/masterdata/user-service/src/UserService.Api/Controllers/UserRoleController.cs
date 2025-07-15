using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace UserService.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserRoleController(
        IUserRoleService userRoleService,
        ILogger<UserRoleController> logger)
        : ControllerBase
    {
        private readonly IUserRoleService _userRoleService = userRoleService ?? throw new ArgumentNullException(nameof(userRoleService));
        private readonly ILogger<UserRoleController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [HttpPost("{userId}/roles/{roleId}")]
        [Authorize(Policy = "users.manage")]
        public async Task AddRoleToUser(string userId, string roleId)
        {
            await _userRoleService.AddUserToRoleAsync(userId, roleId);
        }

        [HttpDelete("{userId}/roles/{roleId}")]
        [Authorize(Policy = "users.manage")]
        public async Task RemoveRoleFromUser(string userId, string roleId)
        {
            await _userRoleService.RemoveUserFromRoleAsync(userId, roleId);
        }
    }
}