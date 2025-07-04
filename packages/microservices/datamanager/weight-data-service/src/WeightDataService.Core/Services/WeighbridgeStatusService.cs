using AutoMapper;
using WeightDataService.Core.DTOs;
using WeightDataService.Core.Entities;
using WeightDataService.Core.Interfaces;

namespace WeightDataService.Core.Services;

public class WeighbridgeStatusService : IWeighbridgeStatusService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public WeighbridgeStatusService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<WeighbridgeStatusDto>> GetWeighbridgesAsync(string organizationId)
    {
        var weighbridges = await _unitOfWork.WeighbridgeStatuses.GetByOrganizationAsync(organizationId);
        return _mapper.Map<List<WeighbridgeStatusDto>>(weighbridges);
    }

    public async Task<WeighbridgeStatusDto?> GetWeighbridgeByIdAsync(string weighbridgeId, string organizationId)
    {
        var weighbridge = await _unitOfWork.WeighbridgeStatuses.FirstOrDefaultAsync(
            w => w.WeighbridgeId == weighbridgeId && w.OrganizationId == organizationId);
        
        return weighbridge != null ? _mapper.Map<WeighbridgeStatusDto>(weighbridge) : null;
    }

    public async Task<WeighbridgeStatusDto> CreateWeighbridgeAsync(CreateWeighbridgeStatusDto createDto, string organizationId, string userId)
    {
        var existingWeighbridge = await _unitOfWork.WeighbridgeStatuses.GetByWeighbridgeIdAsync(createDto.WeighbridgeId);
        if (existingWeighbridge != null)
        {
            throw new InvalidOperationException($"Weighbridge with ID '{createDto.WeighbridgeId}' already exists");
        }

        var weighbridge = _mapper.Map<WeighbridgeStatus>(createDto);
        weighbridge.OrganizationId = organizationId;
        weighbridge.CreatedBy = userId;

        await _unitOfWork.WeighbridgeStatuses.AddAsync(weighbridge);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<WeighbridgeStatusDto>(weighbridge);
    }

    public async Task<WeighbridgeStatusDto?> UpdateWeighbridgeStatusAsync(string weighbridgeId, UpdateWeighbridgeStatusDto updateDto, string organizationId, string userId)
    {
        var weighbridge = await _unitOfWork.WeighbridgeStatuses.FirstOrDefaultAsync(
            w => w.WeighbridgeId == weighbridgeId && w.OrganizationId == organizationId);

        if (weighbridge == null)
            return null;

        _mapper.Map(updateDto, weighbridge);
        weighbridge.UpdatedBy = userId;

        await _unitOfWork.WeighbridgeStatuses.UpdateAsync(weighbridge);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<WeighbridgeStatusDto>(weighbridge);
    }

    public async Task<List<WeighbridgeStatusDto>> GetActiveWeighbridgesAsync(string organizationId)
    {
        var weighbridges = await _unitOfWork.WeighbridgeStatuses.GetActiveWeighbridgesAsync(organizationId);
        return _mapper.Map<List<WeighbridgeStatusDto>>(weighbridges);
    }

    public async Task<List<WeighbridgeStatusDto>> GetWeighbridgesRequiringCalibrationAsync(string organizationId)
    {
        var weighbridges = await _unitOfWork.WeighbridgeStatuses.GetRequiringCalibrationAsync(organizationId);
        return _mapper.Map<List<WeighbridgeStatusDto>>(weighbridges);
    }

    public async Task<bool> IsWeighbridgeAvailableAsync(string weighbridgeId, string organizationId)
    {
        var weighbridge = await _unitOfWork.WeighbridgeStatuses.FirstOrDefaultAsync(
            w => w.WeighbridgeId == weighbridgeId && w.OrganizationId == organizationId);

        return weighbridge != null && 
               weighbridge.Status == MaintenanceStatus.Active && 
               weighbridge.IsOnline;
    }

    public async Task<WeighbridgeStatusDto?> UpdateCurrentWeightAsync(string weighbridgeId, decimal currentWeight, string organizationId)
    {
        var weighbridge = await _unitOfWork.WeighbridgeStatuses.FirstOrDefaultAsync(
            w => w.WeighbridgeId == weighbridgeId && w.OrganizationId == organizationId);

        if (weighbridge == null)
            return null;

        weighbridge.CurrentWeight = currentWeight;
        weighbridge.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.WeighbridgeStatuses.UpdateAsync(weighbridge);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<WeighbridgeStatusDto>(weighbridge);
    }
}