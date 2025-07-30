using System.Threading.Tasks;
using UserService.Core.DTOs.Report;
using UserService.Core.DTOs.Common;

namespace UserService.Core.Interfaces
{
    public interface IReportService
    {
        Task<PagedResult<ShiftReportDto>> GenerateShiftReportAsync(PaginationParameters parameters);
        Task<PagedResult<UserReportDto>> GenerateUserReportAsync(PaginationParameters parameters);
        Task<ShiftReportDto> GetShiftDetailsReportAsync(string shiftId);
        Task<UserReportDto> GetUserDetailsReportAsync(string userId);
    }
}
