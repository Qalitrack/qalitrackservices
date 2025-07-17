using System.Threading.Tasks;
using UserService.Core.DTOs.Report;

namespace UserService.Core.Interfaces
{
    public interface IReportService
    {
        Task<ShiftReportResponse> GenerateShiftReportAsync();
        Task<UserReportResponse> GenerateUserReportAsync();
        Task<ShiftReportDto> GetShiftDetailsReportAsync(string shiftId);
        Task<UserReportDto> GetUserDetailsReportAsync(string userId);
    }
}
