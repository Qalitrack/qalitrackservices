using UserService.Core.Entities;
using UserService.Core.DTOs.Common;

namespace UserService.Core.Interfaces.Repositories
{
    public interface IShiftRepository : IRepository<Shift>  
    {
        new Task<IEnumerable<Shift>> GetAllAsync();

        
        Task<Shift?> GetByIdAsync(string id);

       
        new Task<Shift> CreateAsync(Shift shift);

        
        new Task<Shift?> UpdateAsync(Shift shift);

     
        new Task<bool> DeleteAsync(string id);

        
      
        Task<bool> IsShiftActiveAsync(string shiftId);

        Task<PagedResult<Shift>> GetDeletedPagedAsync(PaginationParameters parameters);
    
    }
}