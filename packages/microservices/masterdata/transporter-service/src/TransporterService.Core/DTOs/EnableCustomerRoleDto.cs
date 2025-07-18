using System.ComponentModel.DataAnnotations;

namespace TransporterService.Core.DTOs;

public class EnableCustomerRoleDto
{
    [Required]
    public string CustomerServiceReference { get; set; } = string.Empty;
    
    public string? CustomerNotes { get; set; }
}