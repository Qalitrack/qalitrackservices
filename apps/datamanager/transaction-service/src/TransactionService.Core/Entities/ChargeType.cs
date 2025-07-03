namespace TransactionService.Core.Entities;

public enum ChargeType
{
    BaseCharge = 1,
    WeightPenalty = 2,
    StorageFee = 3,
    ProcessingFee = 4,
    OvertimeFee = 5,
    DocumentationFee = 6,
    Tax = 7,
    Discount = 8,
    Fine = 9
}