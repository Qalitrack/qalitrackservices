using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using UserService.Core.DTOs;
using UserService.Core.Enums;

namespace UserService.Core.Entities
{
      public class Shift : BaseEntity
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public TimeSpan StartTime { get; set; }
        
        [Required]
        public TimeSpan EndTime { get; set; }
        
        [Required]
        public ShiftMode Mode { get; set; }
        
        
        // New enhanced scheduling properties
        private DateTime _startDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        public DateTime StartDate
        {
            get => _startDate;
            set => _startDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        
        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get => _endDate;
            set => _endDate = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
        }
        public ShiftStatus Status { get; set; } = ShiftStatus.Draft;
        public ShiftType Type { get; set; } = ShiftType.Recurring;
        public int RequiredStaffCount { get; set; } = 1;
        
        // Enhanced recurrence properties
        public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;
        public int RecurrenceInterval { get; set; } = 1; // Every X days/weeks/months
        
        // JSON serialized arrays for flexible scheduling
        public string? CustomDaysJson { get; set; }
        public string? ExceptionDatesJson { get; set; }
        
        // Non-mapped computed properties
        [NotMapped]
        public DayOfWeek[] CustomDays
        {
            get => string.IsNullOrEmpty(CustomDaysJson) 
                ? Array.Empty<DayOfWeek>() 
                : JsonSerializer.Deserialize<DayOfWeek[]>(CustomDaysJson) ?? Array.Empty<DayOfWeek>();
            set => CustomDaysJson = JsonSerializer.Serialize(value);
        }
        
        [NotMapped]
        public DateTime[] ExceptionDates
        {
            get
            {
                if (string.IsNullOrEmpty(ExceptionDatesJson))
                    return Array.Empty<DateTime>();
                
                var dates = JsonSerializer.Deserialize<DateTime[]>(ExceptionDatesJson) ?? Array.Empty<DateTime>();
                return Array.ConvertAll(dates, d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
            }
            set
            {
                if (value != null)
                {
                    var utcDates = Array.ConvertAll(value, d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
                    ExceptionDatesJson = JsonSerializer.Serialize(utcDates);
                }
                else
                {
                    ExceptionDatesJson = null;
                }
            }
        }
        
        [NotMapped]
        public bool IsActive
        {
            get
            {
                var now = DateTime.UtcNow.TimeOfDay;
                if (StartTime < EndTime)
                {
                    return now >= StartTime && now <= EndTime;
                }
                else
                {
                    return now >= StartTime || now <= EndTime;
                }
            }
            private set { }
        }
        
        // Navigation properties
        public virtual ICollection<UserShift> UserShifts { get; set; } = new List<UserShift>();
        public virtual ICollection<ShiftInstance> ShiftInstances { get; set; } = new List<ShiftInstance>();
    }
}