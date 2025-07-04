namespace ComplianceService.Core.DTOs
{
    public class WeightMeasurementDto
    {
        public string Id { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string VehicleId { get; set; } = string.Empty;
        public string DriverId { get; set; } = string.Empty;
        public string WeighbridgeId { get; set; } = string.Empty;
        public string OrganizationId { get; set; } = string.Empty;
        public decimal GrossWeight { get; set; }
        public decimal TareWeight { get; set; }
        public decimal NetWeight { get; set; }
        public List<AxleWeightDto> AxleWeights { get; set; } = new();
        public string VehicleType { get; set; } = string.Empty;
        public string VehicleClass { get; set; } = string.Empty;
        public int AxleCount { get; set; }
        public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;
        public string MeasuredBy { get; set; } = string.Empty;
        public string Unit { get; set; } = "KG";
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    public class AxleWeightDto
    {
        public int AxleNumber { get; set; }
        public decimal Weight { get; set; }
        public string Position { get; set; } = string.Empty; // FRONT, REAR, etc.
        public string Type { get; set; } = string.Empty; // SINGLE, DUAL, etc.
    }
}