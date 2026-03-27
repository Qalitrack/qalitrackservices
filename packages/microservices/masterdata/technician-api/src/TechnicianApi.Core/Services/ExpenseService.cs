using AutoMapper;
using TechnicianApi.Core.DTOs.Trip;
using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class ExpenseService : IExpenseService
{
    private readonly IRepository<Expense> _repository;
    private readonly IMapper _mapper;

    public ExpenseService(IRepository<Expense> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ExpenseResponseDto?> GetByIdAsync(string id)
    {
        var expense = await _repository.GetByIdAsync(id);
        return expense == null ? null : _mapper.Map<ExpenseResponseDto>(expense);
    }

    public async Task<PagedResponseDto<ExpenseResponseDto>> GetPagedAsync(
        int pageNumber, int pageSize, string? tripId = null)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter: e => tripId == null || e.TripId == tripId
        );

        var dtos = _mapper.Map<IEnumerable<ExpenseResponseDto>>(items);

        return new PagedResponseDto<ExpenseResponseDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<ExpenseResponseDto> CreateAsync(CreateExpenseDto dto)
    {
        var expense = _mapper.Map<Expense>(dto);
        var created = await _repository.CreateAsync(expense);
        return _mapper.Map<ExpenseResponseDto>(created);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }
}
