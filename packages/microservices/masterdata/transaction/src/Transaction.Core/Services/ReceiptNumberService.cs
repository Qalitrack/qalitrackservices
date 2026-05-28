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
    private const string Prefix = "NCCU";
    private const int SequenceLength = 6;

    public ReceiptNumberService(
        ITransactionRepository transactionRepository, 
        ITimeService timeService)
    {
        _transactionRepository = transactionRepository ?? 
            throw new ArgumentNullException(nameof(transactionRepository));
        _timeService = timeService ?? 
            throw new ArgumentNullException(nameof(timeService));
    }

    /// <summary>
    /// Generates a receipt number in format: QSL-YYYYMMDD-XXXXXX
    /// Example: QSL-20240202-000001
    /// </summary>
    public async Task<string> GenerateReceiptNumberAsync()
    {
        var now = _timeService.Now;
        
        // Format: YYYYMMDD (e.g., 20240202)
        var datePart = now.ToString("yyyyMMdd");
        
        // Get the latest receipt number for today
        var searchPattern = $"{Prefix}-{datePart}";
        var latestReceipt = await _transactionRepository.GetLatestReceiptNumberAsync(searchPattern);
        
        int sequenceNumber = 1;
        
        if (!string.IsNullOrEmpty(latestReceipt) && 
            latestReceipt.StartsWith(searchPattern))
        {
            // Extract the sequence part (last 6 digits)
            // Format is QSL-YYYYMMDD-XXXXXX, so we need the part after the last hyphen
            var lastHyphenIndex = latestReceipt.LastIndexOf('-');
            if (lastHyphenIndex >= 0 && lastHyphenIndex < latestReceipt.Length - 1)
            {
                var sequencePart = latestReceipt.Substring(lastHyphenIndex + 1);
                if (int.TryParse(sequencePart, out int lastSequence))
                {
                    sequenceNumber = lastSequence + 1;
                }
            }
        }
        
        // Format: QSL-YYYYMMDD-XXXXXX
        var sequenceFormatted = sequenceNumber.ToString().PadLeft(SequenceLength, '0');
        return $"{Prefix}-{datePart}-{sequenceFormatted}";
    }
}