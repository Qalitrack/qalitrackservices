namespace UserService.Core.Entities
{
    public class UserShift : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public virtual User User { get; set; } = null!;

        public string ShiftId { get; set; } = string.Empty;
        public virtual Shift Shift { get; set; } = null!;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }

}