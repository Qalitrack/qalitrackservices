namespace QaliTrack.MasterData.Core.Modules.Sacco.DTOs;

/// <summary>
/// Simplified DTO for creating a SACCO - only essential fields
/// </summary>
public class CreateSaccoDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime EstablishedDate { get; set; }
    public string SaccoType { get; set; } = string.Empty;
    public int MembershipCapacity { get; set; }
    public decimal ShareCapitalMinimum { get; set; }
    public decimal ShareValue { get; set; }
    
    // Optional basic fields
    public string? LicenseNumber { get; set; }
    public string? ContactPhone { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? RegulatoryAuthority { get; set; }
}

/// <summary>
/// DTO for updating basic SACCO information
/// </summary>
public class UpdateSaccoDto
{
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public int MembershipCapacity { get; set; }
    public decimal ShareCapitalMinimum { get; set; }
    public decimal ShareValue { get; set; }
    
    // Optional fields
    public string? ContactPhone { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Simplified DTO for SACCO list view
/// </summary>
public class SaccoSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string SaccoType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int MembershipCapacity { get; set; }
    public int CurrentMemberCount { get; set; }
    public DateTime EstablishedDate { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Detailed DTO for single SACCO view - without child collections
/// </summary>
public class SaccoDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public DateTime EstablishedDate { get; set; }
    public string SaccoType { get; set; } = string.Empty;
    public int MembershipCapacity { get; set; }
    public int CurrentMemberCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Website { get; set; }
    public decimal ShareCapitalMinimum { get; set; }
    public decimal ShareValue { get; set; }
    public string? LogoUrl { get; set; }
    public string? RegulatoryAuthority { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Status flags
    public bool HasFinancialData { get; set; }
    public bool HasActiveMembers { get; set; }
    public bool HasCommittees { get; set; }
    public bool HasServices { get; set; }
}

// SACCO Member DTOs
public class CreateSaccoMemberDto
{
    public Guid SaccoId { get; set; }
    public string MemberNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime JoinDate { get; set; }
    public string MembershipType { get; set; } = string.Empty;
    
    // Optional fields
    public string? MiddleName { get; set; }
    public string? Occupation { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
}

public class SaccoMemberDto
{
    public Guid Id { get; set; }
    public Guid SaccoId { get; set; }
    public string MemberNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime JoinDate { get; set; }
    public DateTime? ExitDate { get; set; }
    public string MembershipType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal SharesOwned { get; set; }
    public decimal ShareValue { get; set; }
    public decimal TotalContributions { get; set; }
    public string? Occupation { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

// SACCO Committee DTOs
public class CreateSaccoCommitteeDto
{
    public Guid SaccoId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CommitteeType { get; set; } = string.Empty;
    public DateTime EstablishedDate { get; set; }
    public int MaxMembers { get; set; } = 5;
    public string? MeetingSchedule { get; set; }
}

public class SaccoCommitteeDto
{
    public Guid Id { get; set; }
    public Guid SaccoId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CommitteeType { get; set; } = string.Empty;
    public DateTime EstablishedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int MaxMembers { get; set; }
    public int CurrentMemberCount { get; set; }
    public string? Responsibilities { get; set; }
    public string? MeetingSchedule { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

// SACCO Financial DTOs
public class CreateSaccoFinancialDto
{
    public Guid SaccoId { get; set; }
    public decimal TotalShares { get; set; }
    public decimal ShareCapital { get; set; }
    public decimal ReservesFund { get; set; }
    public decimal LoansFund { get; set; }
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal OutstandingLoans { get; set; }
    public decimal AnnualIncome { get; set; }
    public decimal OperatingExpenses { get; set; }
    public DateTime LastAuditDate { get; set; }
    public string? AuditorName { get; set; }
}

public class SaccoFinancialDto
{
    public Guid Id { get; set; }
    public Guid SaccoId { get; set; }
    public decimal TotalShares { get; set; }
    public decimal ShareCapital { get; set; }
    public decimal ReservesFund { get; set; }
    public decimal LoansFund { get; set; }
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal NetWorth { get; set; }
    public decimal OutstandingLoans { get; set; }
    public decimal BadDebts { get; set; }
    public decimal AnnualIncome { get; set; }
    public decimal OperatingExpenses { get; set; }
    public decimal NetProfit { get; set; }
    public DateTime LastAuditDate { get; set; }
    public string? AuditorName { get; set; }
    public string FinancialStatus { get; set; } = string.Empty;
    public DateTime UpdatedDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

// SACCO Loan DTOs
public class CreateSaccoLoanDto
{
    public Guid SaccoId { get; set; }
    public Guid SaccoMemberId { get; set; }
    public string LoanNumber { get; set; } = string.Empty;
    public string LoanType { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int RepaymentPeriodMonths { get; set; }
    public DateTime ApplicationDate { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string? Collateral { get; set; }
    public string? Guarantors { get; set; }
}

public class SaccoLoanDto
{
    public Guid Id { get; set; }
    public Guid SaccoId { get; set; }
    public Guid SaccoMemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string LoanNumber { get; set; } = string.Empty;
    public string LoanType { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int RepaymentPeriodMonths { get; set; }
    public decimal MonthlyPayment { get; set; }
    public decimal OutstandingBalance { get; set; }
    public DateTime ApplicationDate { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public DateTime? DisbursementDate { get; set; }
    public DateTime? MaturityDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string? Collateral { get; set; }
    public string? Guarantors { get; set; }
    public string ApprovedBy { get; set; } = string.Empty;
    public string? RepaymentHistory { get; set; }
    public bool IsOverdue { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

// SACCO Share DTOs
public class CreateSaccoShareDto
{
    public Guid SaccoId { get; set; }
    public Guid SaccoMemberId { get; set; }
    public string ShareCertificateNumber { get; set; } = string.Empty;
    public int NumberOfShares { get; set; }
    public decimal ShareValue { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}

public class SaccoShareDto
{
    public Guid Id { get; set; }
    public Guid SaccoId { get; set; }
    public Guid SaccoMemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string ShareCertificateNumber { get; set; } = string.Empty;
    public int NumberOfShares { get; set; }
    public decimal ShareValue { get; set; }
    public decimal TotalValue { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? TransferDate { get; set; }
    public string? TransferTo { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

// SACCO Service DTOs
public class CreateSaccoServiceDto
{
    public Guid SaccoId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public DateTime LaunchDate { get; set; }
    public decimal? ServiceFee { get; set; }
    public string? ProcessingTime { get; set; }
    public string? ResponsibleCommittee { get; set; }
}

public class SaccoServiceDto
{
    public Guid Id { get; set; }
    public Guid SaccoId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public decimal? ServiceFee { get; set; }
    public string? Eligibility { get; set; }
    public string? Requirements { get; set; }
    public string? ProcessingTime { get; set; }
    public bool IsActive { get; set; }
    public DateTime LaunchDate { get; set; }
    public string? ResponsibleCommittee { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

// SACCO Meeting DTOs
public class CreateSaccoMeetingDto
{
    public Guid SaccoId { get; set; }
    public Guid? SaccoCommitteeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime MeetingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public string Venue { get; set; } = string.Empty;
    public string MeetingType { get; set; } = string.Empty;
    public string OrganizedBy { get; set; } = string.Empty;
    public string? Agenda { get; set; }
}

public class SaccoMeetingDto
{
    public Guid Id { get; set; }
    public Guid SaccoId { get; set; }
    public Guid? SaccoCommitteeId { get; set; }
    public string? CommitteeName { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime MeetingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string Venue { get; set; } = string.Empty;
    public string MeetingType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Agenda { get; set; }
    public string? Minutes { get; set; }
    public string? Resolutions { get; set; }
    public int AttendeesCount { get; set; }
    public string? Documents { get; set; }
    public string OrganizedBy { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}