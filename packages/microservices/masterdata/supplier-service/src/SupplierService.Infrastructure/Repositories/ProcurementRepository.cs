using Microsoft.EntityFrameworkCore;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;
using SupplierService.Infrastructure.Data;

namespace SupplierService.Infrastructure.Repositories;

public class ProcurementRepository : Repository<Procurement>, IProcurementRepository
{
    public ProcurementRepository(SupplierDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Procurement>> GetBySupplierIdAsync(string supplierId)
    {
        return await _context.Procurements
            .Where(p => p.SupplierId == supplierId)
            .Include(p => p.Supplier)
            .Include(p => p.Items)
            .OrderByDescending(p => p.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Procurement>> GetByStatusAsync(ProcurementStatus status)
    {
        return await _context.Procurements
            .Where(p => p.Status == status)
            .Include(p => p.Supplier)
            .Include(p => p.Items)
            .OrderByDescending(p => p.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Procurement>> GetByRequestedByAsync(string requestedBy)
    {
        return await _context.Procurements
            .Where(p => p.RequestedBy == requestedBy)
            .Include(p => p.Supplier)
            .Include(p => p.Items)
            .OrderByDescending(p => p.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Procurement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Procurements
            .Where(p => p.RequestDate >= startDate && p.RequestDate <= endDate)
            .Include(p => p.Supplier)
            .Include(p => p.Items)
            .OrderByDescending(p => p.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Procurement>> GetPendingApprovalsAsync()
    {
        return await _context.Procurements
            .Where(p => p.Status == ProcurementStatus.Submitted || p.Status == ProcurementStatus.UnderReview)
            .Include(p => p.Supplier)
            .Include(p => p.Items)
            .OrderBy(p => p.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Procurement>> GetOverdueDeliveriesAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Procurements
            .Where(p => p.Status == ProcurementStatus.Ordered && 
                       p.ExpectedDeliveryDate.HasValue && 
                       p.ExpectedDeliveryDate.Value.Date < today)
            .Include(p => p.Supplier)
            .Include(p => p.Items)
            .OrderBy(p => p.ExpectedDeliveryDate)
            .ToListAsync();
    }

    public async Task<Procurement?> GetByProcurementNumberAsync(string procurementNumber)
    {
        return await _context.Procurements
            .Where(p => p.ProcurementNumber == procurementNumber)
            .Include(p => p.Supplier)
            .Include(p => p.Items)
            .FirstOrDefaultAsync();
    }

    public async Task<decimal> GetTotalValueBySupplierAsync(string supplierId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Procurements
            .Where(p => p.SupplierId == supplierId && p.ActualValue.HasValue);

        if (startDate.HasValue)
            query = query.Where(p => p.OrderDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(p => p.OrderDate <= endDate.Value);

        return await query.SumAsync(p => p.ActualValue ?? 0);
    }

    public override async Task<IEnumerable<Procurement>> GetAllAsync()
    {
        return await _context.Procurements
            .Include(p => p.Supplier)
            .Include(p => p.Items)
            .OrderByDescending(p => p.RequestDate)
            .ToListAsync();
    }

    public override async Task<Procurement?> GetByIdAsync(string id)
    {
        return await _context.Procurements
            .Where(p => p.Id == id)
            .Include(p => p.Supplier)
            .Include(p => p.Items)
            .FirstOrDefaultAsync();
    }
}