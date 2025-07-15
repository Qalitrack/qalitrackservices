using System.Collections.Generic;
using UserService.Core.DTOs.Shift;

namespace UserService.Core.DTOs.Shift
{
    public class UsersAssignedToShiftDto
    {
        public string ShiftId { get; set; }
        public string ShiftName { get; set; }
        public List<UserDetailsDto> Users { get; set; } = new List<UserDetailsDto>();
        public int TotalUsers { get; set; }
    }
}
