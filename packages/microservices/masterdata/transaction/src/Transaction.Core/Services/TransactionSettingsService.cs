using AutoMapper;
using Transaction.Core.DTOs;
using Transaction.Core.Entities;
using Transaction.Core.Interfaces;

namespace Transaction.Core.Services;

public interface ITransactionSettingsService
{
    Task<TransactionSettingsResponseDto> GetSettingsAsync();
    Task<TransactionSettingsResponseDto> UpdateSettingsAsync(UpdateTransactionSettingsDto dto);
}

public class TransactionSettingsService : ITransactionSettingsService
{
    private readonly ITransactionSettingsRepository _repository;
    private readonly IMapper _mapper;

    public TransactionSettingsService(ITransactionSettingsRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<TransactionSettingsResponseDto> GetSettingsAsync()
    {
        var settings = await GetOrCreateAsync();
        return _mapper.Map<TransactionSettingsResponseDto>(settings);
    }

    public async Task<TransactionSettingsResponseDto> UpdateSettingsAsync(UpdateTransactionSettingsDto dto)
    {
        var settings = await GetOrCreateAsync();
        _mapper.Map(dto, settings);
        var updated = await _repository.SaveSettingsAsync(settings);
        return _mapper.Map<TransactionSettingsResponseDto>(updated);
    }

    private async Task<TransactionSettings> GetOrCreateAsync()
    {
        var settings = await _repository.GetCurrentSettingsAsync();
        return settings ?? await _repository.SaveSettingsAsync(new TransactionSettings());
    }
}
