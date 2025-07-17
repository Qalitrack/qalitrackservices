using System;
using System.Collections.Generic;

namespace UserService.Core.DTOs.Report
{
    public class UserShiftInfoDto
    {
        public string ShiftId { get; set; }
        public string ShiftName { get; set; }
        public string ShiftMode { get; set; }
        public DateTime AssignedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserReportDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }
        public List<UserShiftInfoDto>? AssignedShifts { get; set; } = new List<UserShiftInfoDto>();
        public int TotalShiftsAssigned => AssignedShifts?.Count ?? 0;
        public int ActiveShiftsAssigned => AssignedShifts?.Count(s => s.IsActive) ?? 0;
    }

    public class UserReportResponse
    {
        public List<UserReportDto> Users { get; set; } = new List<UserReportDto>();
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int UsersWithShifts { get; set; }
    }
}
