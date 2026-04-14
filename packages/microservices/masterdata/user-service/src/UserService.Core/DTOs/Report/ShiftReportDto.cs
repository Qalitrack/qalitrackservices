using System;
using System.Collections.Generic;

namespace UserService.Core.DTOs.Report
{
    public class AssignedUserDto
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public DateTime AssignedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class ShiftReportDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public required string Mode { get; set; }
        public bool IsActive { get; set; }
        public int AssignedUsersCount { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<AssignedUserDto>? AssignedUsers { get; set; } = new List<AssignedUserDto>();
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        
        public DateTime? CreatedAt { get; set; }
      
        
        
    }

    public class ShiftReportResponse
    {
        public List<ShiftReportDto> Shifts { get; set; } = new List<ShiftReportDto>();
        public int TotalShifts { get; set; }
        public int ActiveShifts { get; set; }
        public int StrictModeShifts { get; set; }
        public int OpenModeShifts { get; set; }
    }
}
