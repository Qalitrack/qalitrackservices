using System.Collections.Generic;
using UserService.Core.DTOs.Roles;

namespace UserService.Core.DTOs.Role
{
    public class UserBasicInfoDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class RoleWithUsersDto : RoleDto
    {
        public List<UserBasicInfoDto> Users { get; set; } = new List<UserBasicInfoDto>();
        public int TotalUsers { get; set; }
    }
}
