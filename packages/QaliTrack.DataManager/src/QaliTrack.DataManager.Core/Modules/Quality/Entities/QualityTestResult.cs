using QaliTrack.DataManager.Core.Common;

namespace QaliTrack.DataManager.Core.Modules.Quality.Entities;

/// <summary>
/// Quality test results for cement products
/// Links to specifications defined in MasterData ProductCatalog
/// </summary>
public class QualityTestResult : BaseEntity
{
    public Guid TransactionId { get; set; }
    public Guid ProductSpecificationId { get; set; } // Reference to ProductSpecification in MasterData
    public string BatchNumber { get; set; } = string.Empty;
    public decimal TestValue { get; set; }
    public string TestUnit { get; set; } = string.Empty;
    public DateTime TestDate { get; set; }
    public string TestMethod { get; set; } = string.Empty;
    public string TestResult { get; set; } = string.Empty; // Pass, Fail, OutOfRange
    public string? Comments { get; set; }
    public string TestedBy { get; set; } = string.Empty;
    public bool IsApproved { get; set; } = false;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
}