using CustomerService.Core.DTOs;
using CustomerService.Core.Entities;

namespace CustomerService.Core.Interfaces;

public interface IOrderService
{
    Task<OrderReadDto> CreateOrderAsync(CreateOrderDto createOrderDto);
    Task<OrderReadDto?> GetOrderAsync(string id);
    Task<IEnumerable<OrderReadDto>> GetOrdersAsync(int page = 1, int pageSize = 20, string? status = null, string? customerId = null);
    Task<OrderReadDto?> UpdateOrderAsync(string id, UpdateOrderDto updateOrderDto);
    Task<bool> UpdateOrderStatusAsync(string id, OrderStatus newStatus, string? reason = null, string? changedBy = null);
    Task<bool> CancelOrderAsync(string id, string? cancellationReason = null, string? cancelledBy = null);
    Task<IEnumerable<OrderReadDto>> GetCustomerOrdersAsync(string customerId, int page = 1, int pageSize = 20);
    Task<IEnumerable<OrderReadDto>> GetSupplierOrdersAsync(string supplierId, int page = 1, int pageSize = 20);
    Task<IEnumerable<OrderStatusHistoryReadDto>> GetOrderStatusHistoryAsync(string orderId);
}