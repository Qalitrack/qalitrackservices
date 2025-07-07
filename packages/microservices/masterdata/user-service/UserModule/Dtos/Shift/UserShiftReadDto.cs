using UserModule.Dtos.Users;

namespace UserModule.Dtos.Shift;

public class UserShiftReadDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ShiftId { get; set; }
    public DateTime AssignedDate { get; set; }
    public Guid? AssignedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
        
    // Navigation properties
    public string UserEmail { get; set; }
    public string UserName { get; set; }
    public string ShiftName { get; set; }
    public DateTime ShiftStartTime { get; set; }
    public DateTime ShiftEndTime { get; set; }
    public bool ShiftIsActive { get; set; }
        
    // Full objects (optional)
    public ShiftReadDto Shift { get; set; }
    public UserReadDto User { get; set; }
}