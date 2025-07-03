using DriverService.Core.Entities;

namespace DriverService.Core.DTOs;

public class DriverViolationDto
{
    public string Id { get; set; } = string.Empty;
    public ViolationType ViolationType { get; set; }
    public ViolationSeverity Severity { get; set; }
    public DateTime ViolationDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TicketNumber { get; set; } = string.Empty;
    public string IssuingOfficer { get; set; } = string.Empty;
    public string IssuingAgency { get; set; } = string.Empty;
    public decimal FineAmount { get; set; }
    public int Points { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime? CourtDate { get; set; }
    public string CourtLocation { get; set; } = string.Empty;
    public bool IsContested { get; set; }
    public string Resolution { get; set; } = string.Empty;
    public DateTime? ResolutionDate { get; set; }
    public bool AffectsEmployment { get; set; }
    public string ActionTaken { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string DriverId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateDriverViolationDto
{
    public ViolationType ViolationType { get; set; } = ViolationType.Other;
    public ViolationSeverity Severity { get; set; } = ViolationSeverity.Minor;
    public DateTime ViolationDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TicketNumber { get; set; } = string.Empty;
    public string IssuingOfficer { get; set; } = string.Empty;
    public string IssuingAgency { get; set; } = string.Empty;
    public decimal FineAmount { get; set; } = 0;
    public int Points { get; set; } = 0;
    public DateTime? CourtDate { get; set; }
    public string CourtLocation { get; set; } = string.Empty;
    public bool IsContested { get; set; } = false;
    public bool AffectsEmployment { get; set; } = true;
    public string ActionTaken { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string DriverId { get; set; } = string.Empty;
}

public class UpdateDriverViolationDto
{
    public ViolationType ViolationType { get; set; }
    public ViolationSeverity Severity { get; set; }
    public DateTime ViolationDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TicketNumber { get; set; } = string.Empty;
    public string IssuingOfficer { get; set; } = string.Empty;
    public string IssuingAgency { get; set; } = string.Empty;
    public decimal FineAmount { get; set; }
    public int Points { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime? CourtDate { get; set; }
    public string CourtLocation { get; set; } = string.Empty;
    public bool IsContested { get; set; }
    public string Resolution { get; set; } = string.Empty;
    public DateTime? ResolutionDate { get; set; }
    public bool AffectsEmployment { get; set; }
    public string ActionTaken { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}