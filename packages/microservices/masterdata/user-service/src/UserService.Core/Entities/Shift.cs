using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Core.Entities
{
    public class Shift : BaseEntity
    {
        [Required]
        public string Name { get; set; } = string.Empty; // e.g., "Morning", "Evening"
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        public TimeSpan StartTime { get; set; } // e.g., 08:00:00
        [Required]
        public TimeSpan EndTime { get; set; } // e.g., 16:00:00
        [Required]
        public ShiftMode Mode { get; set; } // Can be "Strict" or "Open"
        
        // Auto-repeat functionality
        public bool AutoRepeatDaily { get; set; } = false; // Whether this shift repeats daily
        
        // Duration in minutes (optional - can be used instead of EndTime)
        public int? DurationMinutes { get; set; }

        // Real-time computed property, not persisted in the database
        [NotMapped]
        public bool IsActive
        {
            get
            {
                var now = DateTime.UtcNow.TimeOfDay;
                // Handle shifts that cross midnight (e.g., 16:00:00 to 00:00:00)
                if (StartTime < EndTime)
                {
                    return now >= StartTime && now <= EndTime;
                }
                else
                {
                    return now >= StartTime || now <= EndTime;
                }
            }
            // Add private setter for EF
            private set { }
        }

        // Navigation properties
        public virtual ICollection<UserShift> UserShifts { get; set; } = new List<UserShift>();
    }

    public enum ShiftMode
    {
        Strict = 1, // Only allowed users can log in
        Open = 0 // Anyone can log in
    }
}