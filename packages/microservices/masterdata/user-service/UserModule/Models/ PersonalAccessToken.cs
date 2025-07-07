using System.ComponentModel.DataAnnotations;

namespace UserModule.Models
{
    public class PersonalAccessToken
    {
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        [Required]
        public User User { get; set; }

        [Required]
        [StringLength(255)]
        public string Token { get; set; }

        [Required]
        public string Name { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public DateTime? LastUsedAt { get; set; }
        [Required]
        
        public bool IsRevoked { get; set; } = false;
        [Required]

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}