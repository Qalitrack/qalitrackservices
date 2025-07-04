using WeighbridgeService.Core.Entities;

namespace WeighbridgeService.Core.Interfaces;

public interface IWeighbridgeOperatorRepository : IRepository<WeighbridgeOperator>
{
    Task<IEnumerable<WeighbridgeOperator>> GetByWeighbridgeIdAsync(string weighbridgeId);
    Task<WeighbridgeOperator?> GetByOperatorIdAsync(string operatorId);
    Task<IEnumerable<WeighbridgeOperator>> GetByStatusAsync(OperatorStatus status);
    Task<IEnumerable<WeighbridgeOperator>> GetActiveOperatorsAsync();
    Task<IEnumerable<WeighbridgeOperator>> GetOperatorsNeedingTrainingAsync();
    Task<IEnumerable<WeighbridgeOperator>> GetOperatorsWithExpiredCertificationAsync();
}

public interface IWeighbridgeScheduleRepository : IRepository<WeighbridgeSchedule>
{
    Task<IEnumerable<WeighbridgeSchedule>> GetByWeighbridgeIdAsync(string weighbridgeId);
    Task<IEnumerable<WeighbridgeSchedule>> GetByOperatorIdAsync(string operatorId);
    Task<IEnumerable<WeighbridgeSchedule>> GetByDateAsync(DateTime date);
    Task<IEnumerable<WeighbridgeSchedule>> GetActiveSchedulesAsync();
}