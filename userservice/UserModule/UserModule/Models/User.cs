using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserModule.Models;

[Table("users")]
public class User
{
    //Gets or sets the unique identifier for the user.
    public Guid Id { get; set; }

 
    public string Username { get; set; }

    public string Email { get; set; }

    public string PasswordHash { get; set; }

    public string FirstName { get; set; }

   
    public string LastName { get; set; }

  
    public string? Department { get; set; }

    public bool IsActive { get; set; }

    // Gets or sets a value indicating the login status.
    public bool LoginStatus { get; set; }

    // Gets or sets a value indicating whether the user has an assigned shift.
    public bool HasAssignedShift { get; set; }

    // Gets or sets the creation timestamp.
    public DateTime CreatedAt { get; set; }

    //Gets or sets the last update timestamp.
    public DateTime UpdatedAt { get; set; }
    
    // Gets or sets the last login timestamp.
    
    public DateTime? LastLoginAt { get; set; }

    // Gets or sets the ID of the user who created this user.
    public Guid? CreatedBy { get; set; }

    // Gets or sets the ID of the user who last updated this user.
    public Guid? UpdatedBy { get; set; }

    // Navigation properties

    // Gets or sets the user roles.
    public ICollection<UserRole> UserRoles { get; set; }

    // Gets or sets the user shifts.
    public ICollection<UserShift> UserShifts { get; set; }

    // Gets or sets the audit logs.
    public ICollection<AuditLog> AuditLogs { get; set; }

    // Gets or sets the user activities.
    public ICollection<UserActivity> UserActivities { get; set; }
    
    // Gets or sets the user who created this user.
    public User? CreatedByUser { get; set; }

    // Gets or sets the user who last updated this user.
    public User? UpdatedByUser { get; set; }

    
    // Gets or sets the users updated by this user.
    public IEnumerable<User>? UpdatedUsers { get; set; }

    
    // Gets or sets the users created by this user.
    public IEnumerable<User>? CreatedUsers { get; set; }
}