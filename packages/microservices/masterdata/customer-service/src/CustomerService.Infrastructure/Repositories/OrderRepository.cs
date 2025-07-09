using Microsoft.EntityFrameworkCore;
using CustomerService.Core.Entities;
using CustomerService.Core.Interfaces;
using CustomerService.Infrastructure.Data;

namespace CustomerService.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    private new readonly CustomerDbContext _context;

    public OrderRepository(CustomerDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetOrdersPagedAsync(int page, int pageSize, string? status = null, string? customerId = null)
    {
        var query = _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Supplier)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var orderStatus))
        {
            query = query.Where(o => o.Status == orderStatus);
        }

        if (!string.IsNullOrEmpty(customerId))
        {
            query = query.Where(o => o.CustomerId == customerId);
        }

        return await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetCustomerOrdersAsync(string customerId, int page, int pageSize)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Supplier)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetSupplierOrdersAsync(string supplierId, int page, int pageSize)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Supplier)
            .Where(o => o.SupplierId == supplierId)
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetPendingOrdersAsync(int page, int pageSize)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Supplier)
            .Where(o => o.Status == OrderStatus.Pending)
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetInTransitOrdersAsync(int page, int pageSize)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Supplier)
            .Where(o => o.Status == OrderStatus.InTransit)
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrderStatusHistory>> GetOrderStatusHistoryAsync(string orderId)
    {
        return await _context.OrderStatusHistory
            .Where(h => h.OrderId == orderId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> SearchOrdersAsync(string query, int page, int pageSize)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Supplier)
            .Where(o => o.OrderNumber.Contains(query) ||
                       o.Customer.Name.Contains(query) ||
                       o.ProductName.Contains(query) ||
                       (o.CustomerOrderReference != null && o.CustomerOrderReference.Contains(query)))
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderWithDetailsAsync(string id)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Supplier)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<string> GenerateOrderNumberAsync()
    {
        var today = DateTime.UtcNow.Date;
        var prefix = $"ORD{today:yyyyMMdd}";
        
        var lastOrder = await _context.Orders
            .Where(o => o.OrderNumber.StartsWith(prefix))
            .OrderByDescending(o => o.OrderNumber)
            .FirstOrDefaultAsync();

        var sequence = 1;
        if (lastOrder != null)
        {
            var lastSequence = lastOrder.OrderNumber.Substring(prefix.Length);
            if (int.TryParse(lastSequence, out var parsed))
            {
                sequence = parsed + 1;
            }
        }

        return $"{prefix}{sequence:D4}";
    }
}