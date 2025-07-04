namespace TransactionService.Core.Entities;

public enum WorkflowStep
{
    VehicleArrival = 1,
    DocumentCheck = 2,
    EntryWeighing = 3,
    LoadingUnloading = 4,
    ExitWeighing = 5,
    PaymentProcessing = 6,
    Completion = 7,
    DisputeResolution = 8
}