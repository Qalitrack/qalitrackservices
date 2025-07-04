using WeightDataService.Core.DTOs;

namespace WeightDataService.Core.Interfaces;

public interface IWeightCorrectionService
{
    Task<WeightCorrectionDto> CreateCorrectionAsync(CreateWeightCorrectionDto createDto, string organizationId, string userId);
    Task<WeightCorrectionDto?> ApproveCorrectionAsync(Guid correctionId, ApproveWeightCorrectionDto approveDto, string organizationId, string userId);
    Task<List<WeightCorrectionDto>> GetCorrectionsByMeasurementAsync(Guid measurementId, string organizationId);
    Task<List<WeightCorrectionDto>> GetPendingCorrectionsAsync(string organizationId);
    Task<List<WeightCorrectionDto>> GetRecentCorrectionsAsync(string organizationId, int days = 30);
    Task<bool> CanUserCorrectMeasurementAsync(Guid measurementId, string userId, string organizationId);
    Task<bool> CanUserApproveCorrection(Guid correctionId, string userId, string organizationId);
}