namespace UserService.Core.Entities
{
    public class UserShift : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public virtual User User { get; set; } = null!;  // Navigation property for the user assigned to this shift

        public string ShiftId { get; set; } = string.Empty;
        public virtual Shift Shift { get; set; } = null!;  // Navigation property for the shift the user is assigned to

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;  // When the shift was assigned to the user
    }
}