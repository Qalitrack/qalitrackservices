using System.ComponentModel.DataAnnotations;

namespace ArchiveService.Core.Entities
{
    public class ArchivedWeightMeasurement
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string OriginalMeasurementId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string TransactionId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string VehicleId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string WeighbridgeId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string OrganizationId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string MeasurementType { get; set; } = string.Empty; // FIRST_WEIGHT, SECOND_WEIGHT, TARE_WEIGHT, GROSS_WEIGHT
        
        public decimal Weight { get; set; }
        
        [StringLength(10)]
        public string WeightUnit { get; set; } = "KG";
        
        public DateTime OriginalMeasurementDate { get; set; }
        public DateTime ArchivedDate { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string OperatorId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string OperatorName { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;
        
        public bool IsVerified { get; set; }
        public bool IsCalibrated { get; set; }
        
        [StringLength(500)]
        public string CalibrationDetails { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string MeasurementDetails { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string ArchivedBy { get; set; } = string.Empty;
        
        // Navigation properties
        public int ArchiveMetadataId { get; set; }
        public virtual ArchiveMetadata ArchiveMetadata { get; set; } = null!;
    }
}