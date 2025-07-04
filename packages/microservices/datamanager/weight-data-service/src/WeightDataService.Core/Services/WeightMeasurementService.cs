using AutoMapper;
using WeightDataService.Core.DTOs;
using WeightDataService.Core.Entities;
using WeightDataService.Core.Interfaces;

namespace WeightDataService.Core.Services;

public class WeightMeasurementService : IWeightMeasurementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IWeighbridgeStatusService _weighbridgeStatusService;

    public WeightMeasurementService(IUnitOfWork unitOfWork, IMapper mapper, IWeighbridgeStatusService weighbridgeStatusService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _weighbridgeStatusService = weighbridgeStatusService;
    }

    public async Task<PagedResultDto<WeightMeasurementDto>> GetMeasurementsAsync(string organizationId, SearchFilterDto filter)
    {
        var skip = (filter.Page - 1) * filter.PageSize;
        var measurements = await _unitOfWork.WeightMeasurements.SearchAsync(
            organizationId, 
            filter.SearchTerm, 
            null, 
            filter.FromDate, 
            filter.ToDate, 
            skip, 
            filter.PageSize);

        var totalCount = await _unitOfWork.WeightMeasurements.GetCountByOrganizationAsync(organizationId);
        var measurementDtos = _mapper.Map<List<WeightMeasurementDto>>(measurements);

        return new PagedResultDto<WeightMeasurementDto>
        {
            Items = measurementDtos,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<WeightMeasurementDto?> GetMeasurementByIdAsync(Guid id, string organizationId)
    {
        var measurement = await _unitOfWork.WeightMeasurements.FirstOrDefaultAsync(
            m => m.Id == id && m.OrganizationId == organizationId && !m.IsDeleted);
        
        return measurement != null ? _mapper.Map<WeightMeasurementDto>(measurement) : null;
    }

    public async Task<WeightMeasurementDto?> GetMeasurementByTicketAsync(string ticketReference, string organizationId)
    {
        var measurement = await _unitOfWork.WeightMeasurements.GetByTicketReferenceAsync(ticketReference, organizationId);
        return measurement != null ? _mapper.Map<WeightMeasurementDto>(measurement) : null;
    }

    public async Task<WeightMeasurementDto> CreateMeasurementAsync(CreateWeightMeasurementDto createDto, string organizationId, string userId)
    {
        if (!await ValidateWeightAsync(createDto.Weight, createDto.WeighbridgeId))
        {
            throw new InvalidOperationException("Weight validation failed");
        }

        if (!await _weighbridgeStatusService.IsWeighbridgeAvailableAsync(createDto.WeighbridgeId, organizationId))
        {
            throw new InvalidOperationException("Weighbridge is not available");
        }

        var measurement = _mapper.Map<WeightMeasurement>(createDto);
        measurement.OrganizationId = organizationId;
        measurement.CreatedBy = userId;

        if (createDto.TareWeight.HasValue)
        {
            measurement.NetWeight = createDto.Weight - createDto.TareWeight.Value;
        }

        await _unitOfWork.WeightMeasurements.AddAsync(measurement);
        await _unitOfWork.SaveChangesAsync();

        await _weighbridgeStatusService.UpdateCurrentWeightAsync(createDto.WeighbridgeId, createDto.Weight, organizationId);

        return _mapper.Map<WeightMeasurementDto>(measurement);
    }

    public async Task<WeightMeasurementDto?> UpdateMeasurementAsync(Guid id, UpdateWeightMeasurementDto updateDto, string organizationId, string userId)
    {
        var measurement = await _unitOfWork.WeightMeasurements.FirstOrDefaultAsync(
            m => m.Id == id && m.OrganizationId == organizationId && !m.IsDeleted);

        if (measurement == null)
            return null;

        _mapper.Map(updateDto, measurement);
        measurement.UpdatedBy = userId;

        if (updateDto.Weight.HasValue && measurement.TareWeight.HasValue)
        {
            measurement.NetWeight = updateDto.Weight.Value - measurement.TareWeight.Value;
        }

        await _unitOfWork.WeightMeasurements.UpdateAsync(measurement);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<WeightMeasurementDto>(measurement);
    }

    public async Task<bool> DeleteMeasurementAsync(Guid id, string organizationId, string userId)
    {
        var measurement = await _unitOfWork.WeightMeasurements.FirstOrDefaultAsync(
            m => m.Id == id && m.OrganizationId == organizationId && !m.IsDeleted);

        if (measurement == null)
            return false;

        measurement.IsDeleted = true;
        measurement.UpdatedBy = userId;

        await _unitOfWork.WeightMeasurements.UpdateAsync(measurement);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<List<WeightMeasurementDto>> GetPendingMeasurementsAsync(string organizationId)
    {
        var measurements = await _unitOfWork.WeightMeasurements.GetPendingMeasurementsAsync(organizationId);
        return _mapper.Map<List<WeightMeasurementDto>>(measurements);
    }

    public async Task<List<WeightMeasurementDto>> GetMeasurementsByVehicleAsync(string vehicleRegistration, string organizationId)
    {
        var measurements = await _unitOfWork.WeightMeasurements.GetByVehicleRegistrationAsync(vehicleRegistration, organizationId);
        return _mapper.Map<List<WeightMeasurementDto>>(measurements);
    }

    public async Task<List<WeightMeasurementDto>> GetMeasurementsByWeighbridgeAsync(string weighbridgeId, DateTime? fromDate, DateTime? toDate)
    {
        var measurements = await _unitOfWork.WeightMeasurements.GetByWeighbridgeAsync(weighbridgeId, fromDate, toDate);
        return _mapper.Map<List<WeightMeasurementDto>>(measurements);
    }

    public async Task<bool> ValidateWeightAsync(decimal weight, string weighbridgeId)
    {
        if (weight <= 0)
            return false;

        var weighbridge = await _unitOfWork.WeighbridgeStatuses.GetByWeighbridgeIdAsync(weighbridgeId);
        if (weighbridge == null)
            return false;

        return weight >= weighbridge.MinCapacity && weight <= weighbridge.MaxCapacity;
    }
}