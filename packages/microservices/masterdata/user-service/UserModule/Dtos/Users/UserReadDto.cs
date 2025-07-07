namespace UserModule.Dtos.Users;

public class UserReadDto
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Department { get; set; }
    public bool IsFirstLogin { get; set; }
    public bool IsActive { get; set; }
    
    public bool HasAssignedShift { get; set; }
    
    public bool LoginStatus { get; set; } // Indicates if the user is logged in
    public string Role { get; set; } // User's primary role name

}