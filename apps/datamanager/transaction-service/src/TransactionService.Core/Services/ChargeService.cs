using AutoMapper;
using TransactionService.Core.DTOs;
using TransactionService.Core.Entities;
using TransactionService.Core.Interfaces;

namespace TransactionService.Core.Services;

public class ChargeService : IChargeService
{
    private readonly IChargeRepository _chargeRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;

    public ChargeService(
        IChargeRepository chargeRepository,
        ITransactionRepository transactionRepository,
        IMapper mapper)
    {
        _chargeRepository = chargeRepository;
        _transactionRepository = transactionRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TransactionChargeDto>> GetChargesAsync(string transactionId)
    {
        var charges = await _chargeRepository.GetByTransactionIdAsync(transactionId);
        return _mapper.Map<IEnumerable<TransactionChargeDto>>(charges);
    }

    public async Task<TransactionChargeDto> CreateChargeAsync(CreateChargeRequest request)
    {
        var charge = _mapper.Map<TransactionCharge>(request);
        
        // Calculate tax amount if tax rate is provided
        if (request.TaxRate.HasValue)
        {
            charge.TaxAmount = request.Amount * (request.TaxRate.Value / 100);
            charge.TotalAmount = request.Amount + charge.TaxAmount.Value;
        }
        else
        {
            charge.TotalAmount = request.Amount;
        }

        var createdCharge = await _chargeRepository.AddAsync(charge);
        return _mapper.Map<TransactionChargeDto>(createdCharge);
    }

    public async Task<TransactionChargeDto> UpdateChargeAsync(string chargeId, CreateChargeRequest request)
    {
        var existingCharge = await _chargeRepository.GetByIdAsync(chargeId);
        if (existingCharge == null)
            throw new InvalidOperationException($"Charge with ID {chargeId} not found");

        if (existingCharge.IsApproved)
            throw new InvalidOperationException("Cannot update an approved charge");

        // Update properties
        existingCharge.ChargeType = request.ChargeType;
        existingCharge.Description = request.Description;
        existingCharge.Amount = request.Amount;
        existingCharge.TaxRate = request.TaxRate;
        existingCharge.Currency = request.Currency;

        // Recalculate tax and total
        if (request.TaxRate.HasValue)
        {
            existingCharge.TaxAmount = request.Amount * (request.TaxRate.Value / 100);
            existingCharge.TotalAmount = request.Amount + existingCharge.TaxAmount.Value;
        }
        else
        {
            existingCharge.TaxAmount = null;
            existingCharge.TotalAmount = request.Amount;
        }

        var updatedCharge = await _chargeRepository.UpdateAsync(existingCharge);
        return _mapper.Map<TransactionChargeDto>(updatedCharge);
    }

    public async Task<bool> DeleteChargeAsync(string chargeId)
    {
        var charge = await _chargeRepository.GetByIdAsync(chargeId);
        if (charge == null)
            return false;

        if (charge.IsApproved)
            throw new InvalidOperationException("Cannot delete an approved charge");

        if (charge.IsPaid)
            throw new InvalidOperationException("Cannot delete a paid charge");

        return await _chargeRepository.DeleteByIdAsync(chargeId);
    }

    public async Task<bool> ApproveChargeAsync(string chargeId, string approvedBy)
    {
        var charge = await _chargeRepository.GetByIdAsync(chargeId);
        if (charge == null)
            return false;

        if (charge.IsApproved)
            throw new InvalidOperationException("Charge is already approved");

        charge.IsApproved = true;
        charge.ApprovedBy = approvedBy;
        charge.ApprovedAt = DateTime.UtcNow;

        await _chargeRepository.UpdateAsync(charge);
        return true;
    }

    public async Task<bool> ProcessPaymentAsync(string chargeId, string paymentReference)
    {
        var charge = await _chargeRepository.GetByIdAsync(chargeId);
        if (charge == null)
            return false;

        if (!charge.IsApproved)
            throw new InvalidOperationException("Cannot process payment for unapproved charge");

        if (charge.IsPaid)
            throw new InvalidOperationException("Charge is already paid");

        charge.IsPaid = true;
        charge.PaidAt = DateTime.UtcNow;
        charge.PaymentReference = paymentReference;

        await _chargeRepository.UpdateAsync(charge);
        return true;
    }

    public async Task<decimal> GetTotalAmountAsync(string transactionId)
    {
        return await _chargeRepository.GetTotalAmountAsync(transactionId);
    }

    public async Task<decimal> GetUnpaidAmountAsync(string transactionId)
    {
        return await _chargeRepository.GetUnpaidAmountAsync(transactionId);
    }

    public async Task<IEnumerable<TransactionChargeDto>> GetUnapprovedChargesAsync()
    {
        var charges = await _chargeRepository.GetUnapprovedChargesAsync();
        return _mapper.Map<IEnumerable<TransactionChargeDto>>(charges);
    }

    public async Task CalculateChargesAsync(string transactionId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null)
            throw new InvalidOperationException($"Transaction with ID {transactionId} not found");

        // Check if charges already exist to avoid duplicates
        var existingCharges = await _chargeRepository.GetByTransactionIdAsync(transactionId);
        if (existingCharges.Any())
            return;

        var charges = new List<TransactionCharge>();

        // Calculate base charge based on transaction type
        var baseCharge = CalculateBaseCharge(transaction);
        if (baseCharge > 0)
        {
            charges.Add(new TransactionCharge
            {
                TransactionId = transactionId,
                ChargeType = ChargeType.BaseCharge,
                Description = $"Base charge for {transaction.TransactionType}",
                Amount = baseCharge,
                TotalAmount = baseCharge,
                Currency = "USD"
            });
        }

        // Calculate weight penalty if applicable
        var weightPenalty = CalculateWeightPenalty(transaction);
        if (weightPenalty > 0)
        {
            charges.Add(new TransactionCharge
            {
                TransactionId = transactionId,
                ChargeType = ChargeType.WeightPenalty,
                Description = "Weight penalty charge",
                Amount = weightPenalty,
                TotalAmount = weightPenalty,
                Currency = "USD"
            });
        }

        // Calculate processing fee based on transaction complexity
        var processingFee = CalculateProcessingFee(transaction);
        if (processingFee > 0)
        {
            charges.Add(new TransactionCharge
            {
                TransactionId = transactionId,
                ChargeType = ChargeType.ProcessingFee,
                Description = "Transaction processing fee",
                Amount = processingFee,
                TotalAmount = processingFee,
                Currency = "USD"
            });
        }

        // Add standard documentation fee
        charges.Add(new TransactionCharge
        {
            TransactionId = transactionId,
            ChargeType = ChargeType.DocumentationFee,
            Description = "Documentation and record keeping fee",
            Amount = 5.00m,
            TotalAmount = 5.00m,
            Currency = "USD"
        });

        // Save all calculated charges
        foreach (var charge in charges)
        {
            await _chargeRepository.AddAsync(charge);
        }
    }

    private decimal CalculateBaseCharge(WeighingTransaction transaction)
    {
        return transaction.TransactionType switch
        {
            TransactionType.Incoming => 25.00m,
            TransactionType.Outgoing => 20.00m,
            TransactionType.Transfer => 15.00m,
            TransactionType.Inspection => 10.00m,
            _ => 0.00m
        };
    }

    private decimal CalculateWeightPenalty(WeighingTransaction transaction)
    {
        if (!transaction.NetWeight.HasValue)
            return 0.00m;

        // Apply penalty for overweight (assuming standard limit of 40 tons)
        const decimal weightLimit = 40000m; // 40 tons in kg
        if (transaction.NetWeight.Value > weightLimit)
        {
            var excessWeight = transaction.NetWeight.Value - weightLimit;
            return excessWeight * 0.05m; // $0.05 per kg excess
        }

        return 0.00m;
    }

    private decimal CalculateProcessingFee(WeighingTransaction transaction)
    {
        var baseFee = 10.00m;

        // Additional fee for weekend or after-hours processing
        var transactionHour = transaction.TransactionDate.Hour;
        var isWeekend = transaction.TransactionDate.DayOfWeek == DayOfWeek.Saturday || 
                       transaction.TransactionDate.DayOfWeek == DayOfWeek.Sunday;
        var isAfterHours = transactionHour < 6 || transactionHour > 18;

        if (isWeekend || isAfterHours)
        {
            baseFee += 15.00m; // Overtime fee
        }

        return baseFee;
    }
}