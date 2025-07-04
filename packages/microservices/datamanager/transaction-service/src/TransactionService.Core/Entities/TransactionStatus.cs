namespace TransactionService.Core.Entities;

public enum TransactionStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
    OnHold = 5,
    Disputed = 6
}