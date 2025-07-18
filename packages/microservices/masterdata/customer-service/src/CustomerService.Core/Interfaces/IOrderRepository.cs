using CustomerService.Core.Entities;

namespace CustomerService.Core.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetOrdersPagedAsync(int page, int pageSize, string? status = null, string? customerId = null);
    Task<IEnumerable<Order>> GetCustomerOrdersAsync(string customerId, int page, int pageSize);
    Task<IEnumerable<Order>> GetSupplierOrdersAsync(string supplierId, int page, int pageSize);
    Task<int> GetOrderCountForDateAsync(DateTime date);
}