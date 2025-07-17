using System;
using System.Collections.Generic;

namespace UserService.Core.DTOs.Report
{
    public class ShiftReportDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Mode { get; set; }
        public bool IsActive { get; set; }
        public int AssignedUsersCount { get; set; }
        public DateTime? LastModified { get; set; }
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
