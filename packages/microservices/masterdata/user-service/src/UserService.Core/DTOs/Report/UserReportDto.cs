using System;
using System.Collections.Generic;

namespace UserService.Core.DTOs.Report
{
    public class UserShiftInfoDto
    {
        public required string ShiftId { get; set; }
        public required string ShiftName { get; set; }
        public required string ShiftMode { get; set; }
        public DateTime AssignedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserReportDto
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public bool IsActive { get; set; }
        public List<UserShiftInfoDto>? AssignedShifts { get; set; } = new List<UserShiftInfoDto>();
        public int TotalShiftsAssigned => AssignedShifts?.Count ?? 0;
        public int ActiveShiftsAssigned => AssignedShifts?.Count(s => s.IsActive) ?? 0;
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime LastModified { get; set; }
    }

    public class UserReportResponse
    {
        public List<UserReportDto> Users { get; set; } = new List<UserReportDto>();
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int UsersWithShifts { get; set; }
    }
}
