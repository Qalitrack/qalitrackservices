using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserService.Core.Enums;

namespace UserService.Core.Entities;

public class ShiftNotification
{
    public string Id { get; set; }
    public string ShiftInstanceId { get; set; }
    public string EmployeeEmail { get; set; }
    public string EmployeeName { get; set; }
    public string ShiftName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string NotificationType { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public DateTime SentTime { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}