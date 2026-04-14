using UserService.Core.DTOs.Common;
using UserService.Core.DTOs.Report;

namespace UserService.Core.Interfaces.Services
{
    public interface IReportService
    {
        Task<PagedResult<ShiftReportDto>> GenerateShiftReportAsync(PaginationParameters parameters);
        Task<PagedResult<UserReportDto>> GenerateUserReportAsync(PaginationParameters parameters);
        Task<ShiftReportDto?> GetShiftDetailsReportAsync(string shiftId);
        Task<UserReportDto?> GetUserDetailsReportAsync(string userId);
    }
}
