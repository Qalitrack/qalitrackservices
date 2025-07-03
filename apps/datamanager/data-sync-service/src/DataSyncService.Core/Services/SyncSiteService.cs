using AutoMapper;
using Microsoft.Extensions.Logging;
using DataSyncService.Core.DTOs;
using DataSyncService.Core.Entities;
using DataSyncService.Core.Enums;
using DataSyncService.Core.Interfaces;

namespace DataSyncService.Core.Services;

public class SyncSiteService : ISyncSiteService
{
    private readonly ISyncSiteRepository _syncSiteRepository;
    private readonly ISiteHealthMonitor _siteHealthMonitor;
    private readonly IMapper _mapper;
    private readonly ILogger<SyncSiteService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public SyncSiteService(
        ISyncSiteRepository syncSiteRepository,
        ISiteHealthMonitor siteHealthMonitor,
        IMapper mapper,
        ILogger<SyncSiteService> logger,
        IUnitOfWork unitOfWork)
    {
        _syncSiteRepository = syncSiteRepository;
        _siteHealthMonitor = siteHealthMonitor;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<SyncSiteDto>> CreateSiteAsync(CreateSyncSiteRequest request)
    {
        try
        {
            _logger.LogInformation("Creating sync site {SiteId}", request.SiteId);

            // Check if site already exists
            var existingSite = await _syncSiteRepository.GetBySiteIdAsync(request.SiteId);
            if (existingSite != null)
            {
                return new ApiResponse<SyncSiteDto>
                {
                    Success = false,
                    Message = $"Site with ID '{request.SiteId}' already exists",
                    ErrorCode = "SITE_ALREADY_EXISTS"
                };
            }

            var site = _mapper.Map<SyncSite>(request);
            await _syncSiteRepository.AddAsync(site);
            await _unitOfWork.SaveChangesAsync();

            var siteDto = _mapper.Map<SyncSiteDto>(site);

            _logger.LogInformation("Sync site {SiteId} created successfully", request.SiteId);

            return new ApiResponse<SyncSiteDto>
            {
                Success = true,
                Message = "Site created successfully",
                Data = siteDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sync site {SiteId}", request.SiteId);
            return new ApiResponse<SyncSiteDto>
            {
                Success = false,
                Message = "An error occurred while creating the site",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<SyncSiteDto>> UpdateSiteAsync(int id, UpdateSyncSiteRequest request)
    {
        try
        {
            _logger.LogInformation("Updating sync site {SiteId}", id);

            var site = await _syncSiteRepository.GetByIdAsync(id);
            if (site == null)
            {
                return new ApiResponse<SyncSiteDto>
                {
                    Success = false,
                    Message = "Site not found",
                    ErrorCode = "SITE_NOT_FOUND"
                };
            }

            _mapper.Map(request, site);
            await _syncSiteRepository.UpdateAsync(site);
            await _unitOfWork.SaveChangesAsync();

            var siteDto = _mapper.Map<SyncSiteDto>(site);

            _logger.LogInformation("Sync site {SiteId} updated successfully", id);

            return new ApiResponse<SyncSiteDto>
            {
                Success = true,
                Message = "Site updated successfully",
                Data = siteDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sync site {SiteId}", id);
            return new ApiResponse<SyncSiteDto>
            {
                Success = false,
                Message = "An error occurred while updating the site",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteSiteAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting sync site {SiteId}", id);

            var site = await _syncSiteRepository.GetByIdAsync(id);
            if (site == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Site not found",
                    ErrorCode = "SITE_NOT_FOUND"
                };
            }

            await _syncSiteRepository.DeleteAsync(site);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Sync site {SiteId} deleted successfully", id);

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Site deleted successfully",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sync site {SiteId}", id);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the site",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<SyncSiteDto>> GetSiteAsync(int id)
    {
        try
        {
            var site = await _syncSiteRepository.GetByIdAsync(id);
            if (site == null)
            {
                return new ApiResponse<SyncSiteDto>
                {
                    Success = false,
                    Message = "Site not found",
                    ErrorCode = "SITE_NOT_FOUND"
                };
            }

            var siteDto = _mapper.Map<SyncSiteDto>(site);

            return new ApiResponse<SyncSiteDto>
            {
                Success = true,
                Message = "Site retrieved successfully",
                Data = siteDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sync site {SiteId}", id);
            return new ApiResponse<SyncSiteDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the site",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<SyncSiteDto>> GetSiteBySiteIdAsync(string siteId)
    {
        try
        {
            var site = await _syncSiteRepository.GetBySiteIdAsync(siteId);
            if (site == null)
            {
                return new ApiResponse<SyncSiteDto>
                {
                    Success = false,
                    Message = "Site not found",
                    ErrorCode = "SITE_NOT_FOUND"
                };
            }

            var siteDto = _mapper.Map<SyncSiteDto>(site);

            return new ApiResponse<SyncSiteDto>
            {
                Success = true,
                Message = "Site retrieved successfully",
                Data = siteDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sync site {SiteId}", siteId);
            return new ApiResponse<SyncSiteDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the site",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<IEnumerable<SyncSiteDto>>> GetAllSitesAsync()
    {
        try
        {
            var sites = await _syncSiteRepository.GetAllAsync();
            var siteDtos = _mapper.Map<IEnumerable<SyncSiteDto>>(sites);

            return new ApiResponse<IEnumerable<SyncSiteDto>>
            {
                Success = true,
                Message = "Sites retrieved successfully",
                Data = siteDtos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all sync sites");
            return new ApiResponse<IEnumerable<SyncSiteDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving sites",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<IEnumerable<SyncSiteDto>>> GetActiveSitesAsync()
    {
        try
        {
            var sites = await _syncSiteRepository.GetActiveSites();
            var siteDtos = _mapper.Map<IEnumerable<SyncSiteDto>>(sites);

            return new ApiResponse<IEnumerable<SyncSiteDto>>
            {
                Success = true,
                Message = "Active sites retrieved successfully",
                Data = siteDtos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active sync sites");
            return new ApiResponse<IEnumerable<SyncSiteDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving active sites",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<IEnumerable<SyncSiteDto>>> GetSitesByStatusAsync(SiteStatus status)
    {
        try
        {
            var sites = await _syncSiteRepository.GetSitesByStatusAsync(status);
            var siteDtos = _mapper.Map<IEnumerable<SyncSiteDto>>(sites);

            return new ApiResponse<IEnumerable<SyncSiteDto>>
            {
                Success = true,
                Message = $"Sites with status {status} retrieved successfully",
                Data = siteDtos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sync sites by status {Status}", status);
            return new ApiResponse<IEnumerable<SyncSiteDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving sites by status",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<HealthCheckDto>> CheckSiteHealthAsync(string siteId)
    {
        try
        {
            var healthCheck = await _siteHealthMonitor.CheckSiteHealthAsync(siteId);

            return new ApiResponse<HealthCheckDto>
            {
                Success = true,
                Message = "Health check completed successfully",
                Data = healthCheck
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking health for site {SiteId}", siteId);
            return new ApiResponse<HealthCheckDto>
            {
                Success = false,
                Message = "An error occurred while checking site health",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<IEnumerable<HealthCheckDto>>> CheckAllSitesHealthAsync()
    {
        try
        {
            var healthChecks = await _siteHealthMonitor.CheckAllSitesHealthAsync();

            return new ApiResponse<IEnumerable<HealthCheckDto>>
            {
                Success = true,
                Message = "Health checks completed successfully",
                Data = healthChecks
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking health for all sites");
            return new ApiResponse<IEnumerable<HealthCheckDto>>
            {
                Success = false,
                Message = "An error occurred while checking sites health",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<bool>> UpdateSiteStatusAsync(string siteId, SiteStatus status)
    {
        try
        {
            await _syncSiteRepository.UpdateSiteStatusAsync(siteId, status);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Site status updated successfully",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for site {SiteId}", siteId);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while updating site status",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<PagedResult<SyncSiteDto>>> GetSitesAsync(PagingRequest pagingRequest, SiteStatus? status = null)
    {
        try
        {
            var (sites, totalCount) = await _syncSiteRepository.GetPagedAsync(
                pagingRequest.PageNumber,
                pagingRequest.PageSize,
                status.HasValue ? s => s.Status == status.Value : null,
                s => s.Name,
                pagingRequest.SortDescending);

            var siteDtos = _mapper.Map<IEnumerable<SyncSiteDto>>(sites);

            var pagedResult = new PagedResult<SyncSiteDto>
            {
                Items = siteDtos,
                TotalCount = totalCount,
                PageNumber = pagingRequest.PageNumber,
                PageSize = pagingRequest.PageSize
            };

            return new ApiResponse<PagedResult<SyncSiteDto>>
            {
                Success = true,
                Message = "Sites retrieved successfully",
                Data = pagedResult
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged sync sites");
            return new ApiResponse<PagedResult<SyncSiteDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving sites",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<Dictionary<SiteStatus, int>>> GetSiteStatisticsAsync()
    {
        try
        {
            var statistics = await _syncSiteRepository.GetSiteStatisticsAsync();

            return new ApiResponse<Dictionary<SiteStatus, int>>
            {
                Success = true,
                Message = "Site statistics retrieved successfully",
                Data = statistics
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving site statistics");
            return new ApiResponse<Dictionary<SiteStatus, int>>
            {
                Success = false,
                Message = "An error occurred while retrieving site statistics",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }
}