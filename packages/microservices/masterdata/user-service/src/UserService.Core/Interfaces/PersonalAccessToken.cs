using System.ComponentModel.DataAnnotations;

namespace UserService.Core.Entities;

public class PersonalAccessToken : BaseEntity
{

        [Required]
        public string UserId { get; set; }
        [Required]
        public User User { get; set; }

        [Required]
        [StringLength(255)]
        public string Token { get; set; }
        
    
        public DateTime? ExpiresAt { get; set; }

        public DateTime? LastUsedAt { get; set; }

        [Required]
        public bool IsRevoked { get; set; } = false;
        
        public ShiftMode ShiftMode { get; set; }  
    }

    // Enum to represent Shift Modes (Strict vs Open)
    internal enum ShiftMode
    {
        Strict = 0,
        Open = 1
    }
