using Masterdata.Core.DTOs.Axle;
using Masterdata.Core.Models;

namespace Masterdata.Core.Interfaces;

public interface IAxleConfigurationService
{
    Task<AxleConfigurationDto> GetAxleConfigurationByIdAsync(string id);
    Task<PagedResult<AxleConfigurationDto>> GetAxleConfigurationsAsync(int pageNumber = 1, int pageSize = 10);
    Task<AxleConfigurationDto> CreateAxleConfigurationAsync(CreateAxleConfigurationDto dto);
    Task<AxleConfigurationDto> UpdateAxleConfigurationAsync(UpdateAxleConfigurationDto dto);
    Task<bool> DeleteAxleConfigurationAsync(string id);
    Task<bool> ToggleAxleConfigurationStatusAsync(string id, bool isActive);
}
