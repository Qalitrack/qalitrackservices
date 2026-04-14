using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserService.Core.Enums;

namespace UserService.Core.Entities;

public class ShiftNotification
{
    public required string Id { get; set; }
    public required string ShiftInstanceId { get; set; }
    public required string EmployeeEmail { get; set; }
    public required string EmployeeName { get; set; }
    public required string ShiftName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public required string NotificationType { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
    public DateTime SentTime { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}