namespace UserService.Core.DTOs.Shift;

 public class ShiftCoverageReport
{
    public DateTime ReportDate { get; set; }
    public string ShiftInstanceId { get; set; } = string.Empty;
    public string ShiftName { get; set; } = string.Empty;
    public DateTime ScheduledStartTime { get; set; }
    public DateTime ScheduledEndTime { get; set; }
    public int RequiredStaff { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public bool IsFullyCovered { get; set; }
    public decimal CoveragePercentage { get; set; }
}