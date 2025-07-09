using CustomerService.Core.Entities;

namespace CustomerService.Core.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetOrdersPagedAsync(int page, int pageSize, string? status = null, string? customerId = null);
    Task<IEnumerable<Order>> GetCustomerOrdersAsync(string customerId, int page, int pageSize);
    Task<IEnumerable<Order>> GetSupplierOrdersAsync(string supplierId, int page, int pageSize);
    Task<IEnumerable<Order>> GetPendingOrdersAsync(int page, int pageSize);
    Task<IEnumerable<Order>> GetInTransitOrdersAsync(int page, int pageSize);
    Task<IEnumerable<OrderStatusHistory>> GetOrderStatusHistoryAsync(string orderId);
    Task<IEnumerable<Order>> SearchOrdersAsync(string query, int page, int pageSize);
    Task<Order?> GetOrderWithDetailsAsync(string id);
    Task<string> GenerateOrderNumberAsync();
}