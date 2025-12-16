using System;
using System.Threading.Tasks;
using Transaction.Core.Interfaces;

namespace Transaction.Core.Services;

public interface IReceiptNumberService
{
    Task<string> GenerateReceiptNumberAsync();
}

public class ReceiptNumberService : IReceiptNumberService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITimeService _timeService;
    private const string Prefix = "WB";
    private const int SequenceLength = 6;

    public ReceiptNumberService(ITransactionRepository transactionRepository, ITimeService timeService)
    {
        _transactionRepository = transactionRepository;
        _timeService = timeService;
    }

    public async Task<string> GenerateReceiptNumberAsync()
    {
        var now = _timeService.Now;
        var datePart = now.ToString("yyMMdd");
        var latestReceipt = await _transactionRepository.GetLatestReceiptNumberAsync(datePart);
        
        int sequenceNumber = 1;
        if (!string.IsNullOrEmpty(latestReceipt) && 
            latestReceipt.StartsWith($"{Prefix}{datePart}"))
        {
            var sequencePart = latestReceipt.Substring(Prefix.Length + datePart.Length);
            if (int.TryParse(sequencePart, out int lastSequence))
            {
                sequenceNumber = lastSequence + 1;
            }
        }

        return $"{Prefix}{datePart}{sequenceNumber.ToString().PadLeft(SequenceLength, '0')}";
    }
}
