namespace TransactionService.Core.Entities;

public enum StepStatus
{
    NotStarted = 1,
    InProgress = 2,
    Completed = 3,
    Skipped = 4,
    Failed = 5,
    RequiresAttention = 6
}