using Microsoft.EntityFrameworkCore;
using CustomerService.Core.Entities;
using CustomerService.Core.Interfaces;
using CustomerService.Infrastructure.Data;

namespace CustomerService.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(CustomerServiceDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Order>> GetOrdersPagedAsync(int page, int pageSize, string? status = null, string? customerId = null)
    {
        var query = _dbSet.AsQueryable();

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
            .Include(o => o.Customer)
            .Include(o => o.Supplier)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetCustomerOrdersAsync(string customerId, int page, int pageSize)
    {
        return await _dbSet
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(o => o.Customer)
            .Include(o => o.Supplier)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetSupplierOrdersAsync(string supplierId, int page, int pageSize)
    {
        return await _dbSet
            .Where(o => o.SupplierId == supplierId)
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(o => o.Customer)
            .Include(o => o.Supplier)
            .ToListAsync();
    }

    public async Task<int> GetOrderCountForDateAsync(DateTime date)
    {
        return await _dbSet
            .Where(o => o.OrderDate.Date == date.Date)
            .CountAsync();
    }
}