using System;
using System.Text.Json;

namespace Masterdata.Core.DTOs.Customer;

public class CustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public JsonDocument? ContactInfo { get; set; }
    public string? Status { get; set; }
    public string? Logo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
