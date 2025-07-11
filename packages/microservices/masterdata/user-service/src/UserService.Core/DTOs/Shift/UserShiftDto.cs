namespace UserService.Core.DTOs
{
    public class UserShiftDto
    {
        public string UserId { get; set; } = string.Empty;
        public string ShiftId { get; set; } = string.Empty;
        public string ShiftName { get; set; } = string.Empty; // You could map the shift's name
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow; // When the shift was assigned
    }
}