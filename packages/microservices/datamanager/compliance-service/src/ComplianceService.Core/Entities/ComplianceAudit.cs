using System.ComponentModel.DataAnnotations;

namespace ComplianceService.Core.Entities
{
    public class ComplianceAudit
    {
        public int Id { get; set; }
        
        [StringLength(100)]
        public string EntityType { get; set; } = string.Empty; // VIOLATION, RULE, CHECK
        
        public int EntityId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE, RESOLVE, ACKNOWLEDGE
        
        [StringLength(100)]
        public string UserId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string OrganizationId { get; set; } = string.Empty;
        
        public string OldValues { get; set; } = string.Empty; // JSON of old values
        public string NewValues { get; set; } = string.Empty; // JSON of new values
        
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string IPAddress { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string UserAgent { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ComplianceViolation? ComplianceViolation { get; set; }
    }
}