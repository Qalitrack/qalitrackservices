namespace WeightDataService.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IWeightMeasurementRepository WeightMeasurements { get; }
    IWeighbridgeStatusRepository WeighbridgeStatuses { get; }
    IWeightCorrectionRepository WeightCorrections { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}