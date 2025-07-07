using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserModule.Models;

[Table("users")]
public class User
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Username { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Department { get; set; }
    public bool IsFirstLogin { get; set; }
    public bool IsActive { get; set; }
    public bool LoginStatus { get; set; }
    public bool HasAssignedShift { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserShift> UserShifts { get; set; } = new List<UserShift>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public ICollection<UserActivity> UserActivities { get; set; } = new List<UserActivity>();
    public User? CreatedByUser { get; set; }
    public User? UpdatedByUser { get; set; }
    public IEnumerable<User>? UpdatedUsers { get; set; }
    public IEnumerable<User>? CreatedUsers { get; set; }
    public List<PersonalAccessToken> PersonalAccessTokens { get; set; } = new();
}