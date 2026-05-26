using System.Collections.Generic;
using UserService.Core.DTOs.Shift;

namespace UserService.Core.DTOs.Shift
{
    public class UsersAssignedToShiftDto
    {
        public required string ShiftId { get; set; }
        public required string ShiftName { get; set; }
        public List<UserDetailsDto> Users { get; set; } = new List<UserDetailsDto>();
        public int TotalUsers { get; set; }
    }
}
