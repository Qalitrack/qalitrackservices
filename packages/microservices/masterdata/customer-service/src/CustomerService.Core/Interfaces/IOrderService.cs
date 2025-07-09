using CustomerService.Core.DTOs;

namespace CustomerService.Core.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);
    Task<OrderDto?> GetOrderAsync(string id);
    Task<IEnumerable<OrderSummaryDto>> GetOrdersPagedAsync(int page, int pageSize, string? status = null, string? customerId = null);
    Task<OrderDto> UpdateOrderAsync(string id, UpdateOrderDto updateOrderDto);
    Task<bool> UpdateOrderStatusAsync(string id, OrderStatusUpdateDto statusUpdate);
    Task<bool> CancelOrderAsync(string id, string? cancellationReason = null);
    Task<IEnumerable<OrderSummaryDto>> GetCustomerOrdersAsync(string customerId, int page, int pageSize);
    Task<IEnumerable<OrderSummaryDto>> GetSupplierOrdersAsync(string supplierId, int page, int pageSize);
    Task<IEnumerable<OrderSummaryDto>> GetPendingOrdersAsync(int page, int pageSize);
    Task<IEnumerable<OrderSummaryDto>> GetInTransitOrdersAsync(int page, int pageSize);
    Task<IEnumerable<OrderStatusHistoryDto>> GetOrderStatusHistoryAsync(string orderId);
    Task<IEnumerable<OrderSummaryDto>> SearchOrdersAsync(string query, int page, int pageSize);
}