using System.ComponentModel.DataAnnotations;
using UserService.Core.DTOs;
using UserService.Core.Enums;

namespace UserService.Core.Entities;

public class PersonalAccessToken : BaseEntity
{

    [Required]
    public required string UserId { get; set; }
    [Required]
    public User User { get; set; } = null!;

    [Required]
    [StringLength(1024)]
    public required string Token { get; set; }

    public DateTime? LastUsedAt { get; set; }

    [Required]
    public bool IsRevoked { get; set; } = false;

    public required string Jti { set; get; }
        
    public ShiftMode ShiftMode { get; set; }  
}
