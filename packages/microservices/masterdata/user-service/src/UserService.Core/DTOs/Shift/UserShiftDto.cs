namespace UserService.Core.DTOs.Shift
{
    public class UserShiftDto
    {
        public string UserId { get; set; } = string.Empty;
        public string ShiftId { get; set; } = string.Empty;
        public string ShiftName { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}