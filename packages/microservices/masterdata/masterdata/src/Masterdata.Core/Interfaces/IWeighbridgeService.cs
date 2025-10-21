using System;
using System.Threading.Tasks;
using Masterdata.Core.DTOs;
using Masterdata.Core.DTOs.Weighbridge;
using Masterdata.Core.Models;

namespace Masterdata.Core.Interfaces;

public interface IWeighbridgeService
{
    Task<PagedResult<WeighbridgeDto>> GetPagedWeighbridgesAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null);
    Task<WeighbridgeDto?> GetByIdAsync(Guid id);
    Task<WeighbridgeDto> CreateAsync(CreateWeighbridgeDto dto);
    Task<WeighbridgeDto?> UpdateAsync(Guid id, UpdateWeighbridgeDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> IsLocationAvailableAsync(string location, Guid? excludeId = null);
}
