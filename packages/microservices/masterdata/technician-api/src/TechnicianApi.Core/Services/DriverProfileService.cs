using AutoMapper;
using TechnicianApi.Core.DTOs.Driver;
using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class DriverProfileService : IDriverProfileService
{
    private readonly IRepository<DriverProfile> _repository;
    private readonly IRepository<LicenseClass> _licenseClassRepository;
    private readonly IMapper _mapper;

    public DriverProfileService(
        IRepository<DriverProfile> repository,
        IRepository<LicenseClass> licenseClassRepository,
        IMapper mapper)
    {
        _repository = repository;
        _licenseClassRepository = licenseClassRepository;
        _mapper = mapper;
    }

    public async Task<DriverProfileResponseDto?> GetByIdAsync(string id)
    {
        var profile = await _repository.GetByIdAsync(id);
        return profile == null ? null : _mapper.Map<DriverProfileResponseDto>(profile);
    }

    public async Task<DriverProfileResponseDto?> GetCurrentProfileByDriverIdAsync(string driverId)
    {
        var profile = await _repository.FirstOrDefaultAsync(p => p.DriverId == driverId && p.IsCurrent);
        return profile == null ? null : _mapper.Map<DriverProfileResponseDto>(profile);
    }

    public async Task<PagedResponseDto<DriverProfileResponseDto>> GetPagedAsync(
        int pageNumber, int pageSize, string? driverId = null, string? status = null)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter: p =>
                (driverId == null || p.DriverId == driverId) &&
                (status == null || p.Status.ToString() == status)
        );

        var dtos = _mapper.Map<IEnumerable<DriverProfileResponseDto>>(items);

        return new PagedResponseDto<DriverProfileResponseDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<DriverProfileResponseDto> CreateAsync(CreateDriverProfileDto dto)
    {
        var profile = _mapper.Map<DriverProfile>(dto);

        // Set version number
        var existingProfiles = await _repository.FindAsync(p => p.DriverId == dto.DriverId);
        profile.VersionNumber = existingProfiles.Count() + 1;

        var created = await _repository.CreateAsync(profile);
        return _mapper.Map<DriverProfileResponseDto>(created);
    }

    public async Task<DriverProfileResponseDto?> UpdateAsync(string id, UpdateDriverProfileDto dto)
    {
        var profile = await _repository.GetByIdAsync(id);
        if (profile == null) return null;

        _mapper.Map(dto, profile);
        var updated = await _repository.UpdateAsync(profile);
        return _mapper.Map<DriverProfileResponseDto>(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<DriverProfileResponseDto?> SubmitForApprovalAsync(string id)
    {
        var profile = await _repository.GetByIdAsync(id);
        if (profile == null) return null;

        profile.Status = DriverProfileStatus.Pending;
        profile.SubmittedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(profile);
        return _mapper.Map<DriverProfileResponseDto>(updated);
    }

    public async Task<DriverProfileResponseDto?> ApproveProfileAsync(
        string id, ApproveDriverProfileDto dto, string reviewedBy)
    {
        var profile = await _repository.GetByIdAsync(id);
        if (profile == null) return null;

        profile.Status = dto.Action.ToLower() switch
        {
            "approve" => DriverProfileStatus.Approved,
            "reject" => DriverProfileStatus.Rejected,
            "changes_requested" => DriverProfileStatus.ChangesRequested,
            _ => profile.Status
        };

        profile.ReviewedAt = DateTime.UtcNow;
        profile.ReviewedBy = reviewedBy;

        if (dto.Action.ToLower() == "approve")
        {
            profile.ApprovalNotes = dto.Notes;
            profile.IsCurrent = true;

            // Unset other current profiles
            var otherProfiles = await _repository.FindAsync(p =>
                p.DriverId == profile.DriverId &&
                p.Id != id &&
                p.IsCurrent);

            foreach (var other in otherProfiles)
            {
                other.IsCurrent = false;
                await _repository.UpdateAsync(other);
            }
        }
        else if (dto.Action.ToLower() == "reject")
        {
            profile.RejectionReason = dto.Notes;
        }

        var updated = await _repository.UpdateAsync(profile);
        return _mapper.Map<DriverProfileResponseDto>(updated);
    }

    public async Task<IEnumerable<DriverProfileResponseDto>> GetProfileHistoryAsync(string driverId)
    {
        var profiles = await _repository.FindAsync(p => p.DriverId == driverId);
        return _mapper.Map<IEnumerable<DriverProfileResponseDto>>(profiles.OrderByDescending(p => p.VersionNumber));
    }

    public async Task<IEnumerable<DriverProfileResponseDto>> GetExpiringLicensesAsync(int daysThreshold)
    {
        var targetDate = DateTime.UtcNow.AddDays(daysThreshold);
        var profiles = await _repository.FindAsync(p =>
            p.IsCurrent &&
            p.LicenseExpiryDate.HasValue &&
            p.LicenseExpiryDate.Value <= targetDate);

        return _mapper.Map<IEnumerable<DriverProfileResponseDto>>(profiles);
    }

    public async Task<DriverProfileResponseDto?> UpdateProfilePhotoAsync(string id, string photoUrl)
    {
        var profile = await _repository.GetByIdAsync(id);
        if (profile == null) return null;

        profile.ProfilePhotoUrl = photoUrl;
        var updated = await _repository.UpdateAsync(profile);
        return _mapper.Map<DriverProfileResponseDto>(updated);
    }

    public async Task<DriverProfileResponseDto?> UpdateLicenseFrontAsync(string id, string imageUrl)
    {
        var profile = await _repository.GetByIdAsync(id);
        if (profile == null) return null;

        profile.LicenseFrontImageUrl = imageUrl;
        var updated = await _repository.UpdateAsync(profile);
        return _mapper.Map<DriverProfileResponseDto>(updated);
    }

    public async Task<DriverProfileResponseDto?> UpdateLicenseBackAsync(string id, string imageUrl)
    {
        var profile = await _repository.GetByIdAsync(id);
        if (profile == null) return null;

        profile.LicenseBackImageUrl = imageUrl;
        var updated = await _repository.UpdateAsync(profile);
        return _mapper.Map<DriverProfileResponseDto>(updated);
    }

    public async Task<DriverProfileResponseDto?> UpdateIdFrontAsync(string id, string imageUrl)
    {
        var profile = await _repository.GetByIdAsync(id);
        if (profile == null) return null;

        profile.IdFrontImageUrl = imageUrl;
        var updated = await _repository.UpdateAsync(profile);
        return _mapper.Map<DriverProfileResponseDto>(updated);
    }

    public async Task<DriverProfileResponseDto?> UpdateIdBackAsync(string id, string imageUrl)
    {
        var profile = await _repository.GetByIdAsync(id);
        if (profile == null) return null;

        profile.IdBackImageUrl = imageUrl;
        var updated = await _repository.UpdateAsync(profile);
        return _mapper.Map<DriverProfileResponseDto>(updated);
    }
}
