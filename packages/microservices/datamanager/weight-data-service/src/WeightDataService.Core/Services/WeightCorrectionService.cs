using AutoMapper;
using WeightDataService.Core.DTOs;
using WeightDataService.Core.Entities;
using WeightDataService.Core.Interfaces;

namespace WeightDataService.Core.Services;

public class WeightCorrectionService : IWeightCorrectionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public WeightCorrectionService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<WeightCorrectionDto> CreateCorrectionAsync(CreateWeightCorrectionDto createDto, string organizationId, string userId)
    {
        var measurement = await _unitOfWork.WeightMeasurements.FirstOrDefaultAsync(
            m => m.Id == createDto.WeightMeasurementId && m.OrganizationId == organizationId && !m.IsDeleted);

        if (measurement == null)
        {
            throw new InvalidOperationException("Weight measurement not found");
        }

        if (!await CanUserCorrectMeasurementAsync(createDto.WeightMeasurementId, userId, organizationId))
        {
            throw new UnauthorizedAccessException("User not authorized to correct this measurement");
        }

        var correction = _mapper.Map<WeightCorrection>(createDto);
        correction.OriginalWeight = measurement.Weight;
        correction.AuthorizedBy = userId;
        correction.CreatedBy = userId;

        await _unitOfWork.WeightCorrections.AddAsync(correction);

        measurement.Status = MeasurementStatus.Corrected;
        measurement.Weight = createDto.CorrectedWeight;
        measurement.UpdatedBy = userId;
        
        if (measurement.TareWeight.HasValue)
        {
            measurement.NetWeight = createDto.CorrectedWeight - measurement.TareWeight.Value;
        }

        await _unitOfWork.WeightMeasurements.UpdateAsync(measurement);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<WeightCorrectionDto>(correction);
    }

    public async Task<WeightCorrectionDto?> ApproveCorrectionAsync(Guid correctionId, ApproveWeightCorrectionDto approveDto, string organizationId, string userId)
    {
        var correction = await _unitOfWork.WeightCorrections.FirstOrDefaultAsync(c => c.Id == correctionId);
        if (correction == null)
            return null;

        var measurement = await _unitOfWork.WeightMeasurements.FirstOrDefaultAsync(
            m => m.Id == correction.WeightMeasurementId && m.OrganizationId == organizationId);

        if (measurement == null)
            return null;

        if (!await CanUserApproveCorrection(correctionId, userId, organizationId))
        {
            throw new UnauthorizedAccessException("User not authorized to approve this correction");
        }

        _mapper.Map(approveDto, correction);
        correction.ApprovedBy = userId;
        correction.UpdatedBy = userId;

        if (approveDto.IsApproved)
        {
            measurement.Status = MeasurementStatus.Approved;
        }
        else
        {
            measurement.Status = MeasurementStatus.Rejected;
            measurement.Weight = correction.OriginalWeight;
            
            if (measurement.TareWeight.HasValue)
            {
                measurement.NetWeight = correction.OriginalWeight - measurement.TareWeight.Value;
            }
        }

        measurement.UpdatedBy = userId;
        
        await _unitOfWork.WeightCorrections.UpdateAsync(correction);
        await _unitOfWork.WeightMeasurements.UpdateAsync(measurement);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<WeightCorrectionDto>(correction);
    }

    public async Task<List<WeightCorrectionDto>> GetCorrectionsByMeasurementAsync(Guid measurementId, string organizationId)
    {
        var measurement = await _unitOfWork.WeightMeasurements.FirstOrDefaultAsync(
            m => m.Id == measurementId && m.OrganizationId == organizationId);

        if (measurement == null)
            return new List<WeightCorrectionDto>();

        var corrections = await _unitOfWork.WeightCorrections.GetByMeasurementIdAsync(measurementId);
        return _mapper.Map<List<WeightCorrectionDto>>(corrections);
    }

    public async Task<List<WeightCorrectionDto>> GetPendingCorrectionsAsync(string organizationId)
    {
        var corrections = await _unitOfWork.WeightCorrections.GetPendingApprovalsAsync(organizationId);
        return _mapper.Map<List<WeightCorrectionDto>>(corrections);
    }

    public async Task<List<WeightCorrectionDto>> GetRecentCorrectionsAsync(string organizationId, int days = 30)
    {
        var corrections = await _unitOfWork.WeightCorrections.GetRecentCorrectionsAsync(organizationId, days);
        return _mapper.Map<List<WeightCorrectionDto>>(corrections);
    }

    public async Task<bool> CanUserCorrectMeasurementAsync(Guid measurementId, string userId, string organizationId)
    {
        var measurement = await _unitOfWork.WeightMeasurements.FirstOrDefaultAsync(
            m => m.Id == measurementId && m.OrganizationId == organizationId && !m.IsDeleted);

        if (measurement == null)
            return false;

        return measurement.Status == MeasurementStatus.Pending || measurement.Status == MeasurementStatus.Validated;
    }

    public async Task<bool> CanUserApproveCorrection(Guid correctionId, string userId, string organizationId)
    {
        var correction = await _unitOfWork.WeightCorrections.FirstOrDefaultAsync(c => c.Id == correctionId);
        if (correction == null)
            return false;

        var measurement = await _unitOfWork.WeightMeasurements.FirstOrDefaultAsync(
            m => m.Id == correction.WeightMeasurementId && m.OrganizationId == organizationId);

        if (measurement == null)
            return false;

        return correction.AuthorizedBy != userId && !correction.IsApproved;
    }
}