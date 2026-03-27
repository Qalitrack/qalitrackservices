using TechnicianApi.Core.DTOs.Base;

namespace TechnicianApi.Core.DTOs.Trip;

public class VehicleMileageResponseDto : BaseResponseDto
{
    public string TruckId { get; set; } = string.Empty;
    public string DriverId { get; set; } = string.Empty;
    public decimal StartMileage { get; set; }
    public decimal EndMileage { get; set; }
    public decimal Mileage { get; set; }
    public string? ProofImageUrl { get; set; }
    public string? ProofEndImageUrl { get; set; }
    public DateTime Date { get; set; }
    public string? UserId { get; set; }
    public bool Synced { get; set; }
}
