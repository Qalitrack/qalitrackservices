using SupplierService.Core.DTOs;
using SupplierService.Core.Entities;

namespace SupplierService.Core.Interfaces;

public interface IProcurementService
{
    Task<ProcurementDto> CreateProcurementAsync(CreateProcurementRequest request);
    Task<ProcurementDto> UpdateProcurementAsync(string id, UpdateProcurementRequest request);
    Task<ProcurementDto?> GetProcurementByIdAsync(string id);
    Task<IEnumerable<ProcurementDto>> GetAllProcurementsAsync();
    Task<IEnumerable<ProcurementDto>> GetProcurementsBySupplierAsync(string supplierId);
    Task<IEnumerable<ProcurementDto>> GetProcurementsByStatusAsync(ProcurementStatus status);
    Task<IEnumerable<ProcurementDto>> GetProcurementsByRequestedByAsync(string requestedBy);
    Task<IEnumerable<ProcurementDto>> GetProcurementsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<ProcurementDto>> GetPendingApprovalsAsync();
    Task<IEnumerable<ProcurementDto>> GetOverdueDeliveriesAsync();
    Task<ProcurementDto?> GetProcurementByNumberAsync(string procurementNumber);
    Task<decimal> GetTotalValueBySupplierAsync(string supplierId, DateTime? startDate = null, DateTime? endDate = null);
    Task DeleteProcurementAsync(string id);
    Task<ProcurementDto> ApproveProcurementAsync(string id, string approvedBy);
    Task<ProcurementDto> RejectProcurementAsync(string id, string rejectedBy, string reason);
    Task<ProcurementDto> MarkAsOrderedAsync(string id, string purchaseOrderNumber);
    Task<ProcurementDto> MarkAsDeliveredAsync(string id, DateTime deliveryDate, decimal? actualValue = null);
}