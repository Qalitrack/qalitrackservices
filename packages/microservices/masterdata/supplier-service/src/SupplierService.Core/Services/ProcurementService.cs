using AutoMapper;
using SupplierService.Core.DTOs;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;

namespace SupplierService.Core.Services;

public class ProcurementService : IProcurementService
{
    private readonly IProcurementRepository _procurementRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMapper _mapper;

    public ProcurementService(
        IProcurementRepository procurementRepository,
        ISupplierRepository supplierRepository,
        IMapper mapper)
    {
        _procurementRepository = procurementRepository;
        _supplierRepository = supplierRepository;
        _mapper = mapper;
    }

    public async Task<ProcurementDto> CreateProcurementAsync(CreateProcurementRequest request)
    {
        // Validate supplier exists
        var supplier = await _supplierRepository.GetByIdAsync(request.SupplierId);
        if (supplier == null)
            throw new KeyNotFoundException($"Supplier with ID '{request.SupplierId}' not found");

        var procurement = _mapper.Map<Procurement>(request);
        procurement.ProcurementNumber = await GenerateProcurementNumberAsync();
        procurement.Status = ProcurementStatus.Draft;
        procurement.RequestDate = DateTime.UtcNow;

        // Calculate total estimated value from items
        if (request.Items.Any())
        {
            procurement.EstimatedValue = request.Items.Sum(item => 
                (item.UnitPrice ?? 0) * item.Quantity);
        }

        var createdProcurement = await _procurementRepository.AddAsync(procurement);
        return _mapper.Map<ProcurementDto>(createdProcurement);
    }

    public async Task<ProcurementDto> UpdateProcurementAsync(string id, UpdateProcurementRequest request)
    {
        var procurement = await _procurementRepository.GetByIdAsync(id);
        if (procurement == null)
            throw new KeyNotFoundException($"Procurement with ID '{id}' not found");

        _mapper.Map(request, procurement);
        procurement.UpdatedAt = DateTime.UtcNow;

        var updatedProcurement = await _procurementRepository.UpdateAsync(procurement);
        return _mapper.Map<ProcurementDto>(updatedProcurement);
    }

    public async Task<ProcurementDto?> GetProcurementByIdAsync(string id)
    {
        var procurement = await _procurementRepository.GetByIdAsync(id);
        return procurement == null ? null : _mapper.Map<ProcurementDto>(procurement);
    }

    public async Task<IEnumerable<ProcurementDto>> GetAllProcurementsAsync()
    {
        var procurements = await _procurementRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ProcurementDto>>(procurements);
    }

    public async Task<IEnumerable<ProcurementDto>> GetProcurementsBySupplierAsync(string supplierId)
    {
        var procurements = await _procurementRepository.GetBySupplierIdAsync(supplierId);
        return _mapper.Map<IEnumerable<ProcurementDto>>(procurements);
    }

    public async Task<IEnumerable<ProcurementDto>> GetProcurementsByStatusAsync(ProcurementStatus status)
    {
        var procurements = await _procurementRepository.GetByStatusAsync(status);
        return _mapper.Map<IEnumerable<ProcurementDto>>(procurements);
    }

    public async Task<IEnumerable<ProcurementDto>> GetProcurementsByRequestedByAsync(string requestedBy)
    {
        var procurements = await _procurementRepository.GetByRequestedByAsync(requestedBy);
        return _mapper.Map<IEnumerable<ProcurementDto>>(procurements);
    }

    public async Task<IEnumerable<ProcurementDto>> GetProcurementsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var procurements = await _procurementRepository.GetByDateRangeAsync(startDate, endDate);
        return _mapper.Map<IEnumerable<ProcurementDto>>(procurements);
    }

    public async Task<IEnumerable<ProcurementDto>> GetPendingApprovalsAsync()
    {
        var procurements = await _procurementRepository.GetPendingApprovalsAsync();
        return _mapper.Map<IEnumerable<ProcurementDto>>(procurements);
    }

    public async Task<IEnumerable<ProcurementDto>> GetOverdueDeliveriesAsync()
    {
        var procurements = await _procurementRepository.GetOverdueDeliveriesAsync();
        return _mapper.Map<IEnumerable<ProcurementDto>>(procurements);
    }

    public async Task<ProcurementDto?> GetProcurementByNumberAsync(string procurementNumber)
    {
        var procurement = await _procurementRepository.GetByProcurementNumberAsync(procurementNumber);
        return procurement == null ? null : _mapper.Map<ProcurementDto>(procurement);
    }

    public async Task<decimal> GetTotalValueBySupplierAsync(string supplierId, DateTime? startDate = null, DateTime? endDate = null)
    {
        return await _procurementRepository.GetTotalValueBySupplierAsync(supplierId, startDate, endDate);
    }

    public async Task DeleteProcurementAsync(string id)
    {
        var procurement = await _procurementRepository.GetByIdAsync(id);
        if (procurement == null)
            throw new KeyNotFoundException($"Procurement with ID '{id}' not found");

        // Only allow deletion of draft procurements
        if (procurement.Status != ProcurementStatus.Draft)
            throw new InvalidOperationException("Only draft procurements can be deleted");

        await _procurementRepository.DeleteAsync(id);
    }

    public async Task<ProcurementDto> ApproveProcurementAsync(string id, string approvedBy)
    {
        var procurement = await _procurementRepository.GetByIdAsync(id);
        if (procurement == null)
            throw new KeyNotFoundException($"Procurement with ID '{id}' not found");

        if (procurement.Status != ProcurementStatus.Submitted && procurement.Status != ProcurementStatus.UnderReview)
            throw new InvalidOperationException("Only submitted or under review procurements can be approved");

        procurement.Status = ProcurementStatus.Approved;
        procurement.ApprovedBy = approvedBy;
        procurement.ApprovalDate = DateTime.UtcNow;
        procurement.UpdatedAt = DateTime.UtcNow;

        var updatedProcurement = await _procurementRepository.UpdateAsync(procurement);
        return _mapper.Map<ProcurementDto>(updatedProcurement);
    }

    public async Task<ProcurementDto> RejectProcurementAsync(string id, string rejectedBy, string reason)
    {
        var procurement = await _procurementRepository.GetByIdAsync(id);
        if (procurement == null)
            throw new KeyNotFoundException($"Procurement with ID '{id}' not found");

        if (procurement.Status != ProcurementStatus.Submitted && procurement.Status != ProcurementStatus.UnderReview)
            throw new InvalidOperationException("Only submitted or under review procurements can be rejected");

        procurement.Status = ProcurementStatus.Rejected;
        procurement.ApprovedBy = rejectedBy;
        procurement.Notes = $"{procurement.Notes}\n\nRejected: {reason}";
        procurement.UpdatedAt = DateTime.UtcNow;

        var updatedProcurement = await _procurementRepository.UpdateAsync(procurement);
        return _mapper.Map<ProcurementDto>(updatedProcurement);
    }

    public async Task<ProcurementDto> MarkAsOrderedAsync(string id, string purchaseOrderNumber)
    {
        var procurement = await _procurementRepository.GetByIdAsync(id);
        if (procurement == null)
            throw new KeyNotFoundException($"Procurement with ID '{id}' not found");

        if (procurement.Status != ProcurementStatus.Approved)
            throw new InvalidOperationException("Only approved procurements can be marked as ordered");

        procurement.Status = ProcurementStatus.Ordered;
        procurement.PurchaseOrderNumber = purchaseOrderNumber;
        procurement.OrderDate = DateTime.UtcNow;
        procurement.UpdatedAt = DateTime.UtcNow;

        var updatedProcurement = await _procurementRepository.UpdateAsync(procurement);
        return _mapper.Map<ProcurementDto>(updatedProcurement);
    }

    public async Task<ProcurementDto> MarkAsDeliveredAsync(string id, DateTime deliveryDate, decimal? actualValue = null)
    {
        var procurement = await _procurementRepository.GetByIdAsync(id);
        if (procurement == null)
            throw new KeyNotFoundException($"Procurement with ID '{id}' not found");

        if (procurement.Status != ProcurementStatus.Ordered && procurement.Status != ProcurementStatus.PartiallyDelivered)
            throw new InvalidOperationException("Only ordered or partially delivered procurements can be marked as delivered");

        procurement.Status = ProcurementStatus.Delivered;
        procurement.ActualDeliveryDate = deliveryDate;
        if (actualValue.HasValue)
            procurement.ActualValue = actualValue.Value;
        procurement.UpdatedAt = DateTime.UtcNow;

        var updatedProcurement = await _procurementRepository.UpdateAsync(procurement);
        return _mapper.Map<ProcurementDto>(updatedProcurement);
    }

    private async Task<string> GenerateProcurementNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month;
        var sequence = await GetNextSequenceNumberAsync(year, month);
        return $"PR{year:D4}{month:D2}{sequence:D4}";
    }

    private async Task<int> GetNextSequenceNumberAsync(int year, int month)
    {
        // This is a simplified implementation
        // In a real system, you might want to use a separate sequence table
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        
        var procurements = await _procurementRepository.GetByDateRangeAsync(startDate, endDate);
        return procurements.Count() + 1;
    }
}