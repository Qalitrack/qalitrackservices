using AutoMapper;
using CustomerService.Core.DTOs;
using CustomerService.Core.Entities;
using CustomerService.Core.Interfaces;
using System.Linq;

namespace CustomerService.Core.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IRepository<OrderStatusHistory> _statusHistoryRepository;
    private readonly IMapper _mapper;

    public OrderService(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IRepository<OrderStatusHistory> statusHistoryRepository,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _statusHistoryRepository = statusHistoryRepository;
        _mapper = mapper;
    }

    public async Task<OrderReadDto> CreateOrderAsync(CreateOrderDto createOrderDto)
    {
        // Validate customer exists
        var customer = await _customerRepository.GetByIdAsync(createOrderDto.CustomerId);
        if (customer == null)
        {
            throw new ArgumentException($"Customer with ID {createOrderDto.CustomerId} not found");
        }

        // Validate supplier if provided
        if (!string.IsNullOrEmpty(createOrderDto.SupplierId))
        {
            var supplier = await _customerRepository.GetByIdAsync(createOrderDto.SupplierId);
            if (supplier == null)
            {
                throw new ArgumentException($"Supplier with ID {createOrderDto.SupplierId} not found");
            }
        }

        // Generate order number
        var orderNumber = await GenerateOrderNumberAsync();

        // Create order entity
        var order = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = orderNumber,
            CustomerId = createOrderDto.CustomerId,
            SupplierId = createOrderDto.SupplierId,
            ProductId = createOrderDto.ProductId,
            ProductName = createOrderDto.ProductName,
            Quantity = createOrderDto.Quantity,
            UnitOfMeasure = createOrderDto.UnitOfMeasure,
            UnitPrice = createOrderDto.UnitPrice,
            TotalAmount = createOrderDto.Quantity * createOrderDto.UnitPrice,
            OrderDate = DateTime.UtcNow,
            ExpectedDeliveryDate = createOrderDto.ExpectedDeliveryDate,
            Status = OrderStatus.Pending,
            OrderType = Enum.TryParse<OrderType>(createOrderDto.OrderType, out var orderType) ? orderType : OrderType.Purchase,
            TransporterId = createOrderDto.TransporterId,
            RouteId = createOrderDto.RouteId,
            OriginLocation = createOrderDto.OriginLocation,
            DestinationLocation = createOrderDto.DestinationLocation,
            QualitySpecifications = createOrderDto.QualitySpecifications,
            SpecialInstructions = createOrderDto.SpecialInstructions,
            TolerancePercentage = createOrderDto.TolerancePercentage,
            CustomerOrderReference = createOrderDto.CustomerOrderReference,
            SupplierOrderReference = createOrderDto.SupplierOrderReference,
            Notes = createOrderDto.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Auto-assign transporter if customer has one
        if (string.IsNullOrEmpty(order.TransporterId) && !string.IsNullOrEmpty(customer.TransporterId))
        {
            order.TransporterId = customer.TransporterId;
        }
        else if (string.IsNullOrEmpty(order.TransporterId) && !string.IsNullOrEmpty(customer.PreferredTransporterId))
        {
            order.TransporterId = customer.PreferredTransporterId;
        }

        var createdOrder = await _orderRepository.CreateAsync(order);

        // Create initial status history
        await CreateStatusHistoryAsync(createdOrder.Id, OrderStatus.Pending, OrderStatus.Pending, "Order created", null);

        return _mapper.Map<OrderReadDto>(createdOrder);
    }

    public async Task<OrderReadDto?> GetOrderAsync(string id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        return order != null ? _mapper.Map<OrderReadDto>(order) : null;
    }

    public async Task<IEnumerable<OrderReadDto>> GetOrdersAsync(int page = 1, int pageSize = 20, string? status = null, string? customerId = null)
    {
        var orders = await _orderRepository.GetOrdersPagedAsync(page, pageSize, status, customerId);
        return _mapper.Map<IEnumerable<OrderReadDto>>(orders);
    }

    public async Task<OrderReadDto?> UpdateOrderAsync(string id, UpdateOrderDto updateOrderDto)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return null;
        }

        // Update order properties
        if (updateOrderDto.Quantity.HasValue)
        {
            order.Quantity = updateOrderDto.Quantity.Value;
            order.TotalAmount = order.Quantity * order.UnitPrice;
        }

        if (updateOrderDto.UnitPrice.HasValue)
        {
            order.UnitPrice = updateOrderDto.UnitPrice.Value;
            order.TotalAmount = order.Quantity * order.UnitPrice;
        }

        if (updateOrderDto.ExpectedDeliveryDate.HasValue)
        {
            order.ExpectedDeliveryDate = updateOrderDto.ExpectedDeliveryDate.Value;
        }

        if (updateOrderDto.ActualDeliveryDate.HasValue)
        {
            order.ActualDeliveryDate = updateOrderDto.ActualDeliveryDate.Value;
        }

        if (!string.IsNullOrEmpty(updateOrderDto.TransporterId))
        {
            order.TransporterId = updateOrderDto.TransporterId;
        }

        if (!string.IsNullOrEmpty(updateOrderDto.RouteId))
        {
            order.RouteId = updateOrderDto.RouteId;
        }

        if (!string.IsNullOrEmpty(updateOrderDto.QualitySpecifications))
        {
            order.QualitySpecifications = updateOrderDto.QualitySpecifications;
        }

        if (!string.IsNullOrEmpty(updateOrderDto.SpecialInstructions))
        {
            order.SpecialInstructions = updateOrderDto.SpecialInstructions;
        }

        if (updateOrderDto.TolerancePercentage.HasValue)
        {
            order.TolerancePercentage = updateOrderDto.TolerancePercentage.Value;
        }

        if (!string.IsNullOrEmpty(updateOrderDto.WeighingTransactionId))
        {
            order.WeighingTransactionId = updateOrderDto.WeighingTransactionId;
        }

        if (!string.IsNullOrEmpty(updateOrderDto.Notes))
        {
            order.Notes = updateOrderDto.Notes;
        }

        order.UpdatedAt = DateTime.UtcNow;

        var updatedOrder = await _orderRepository.UpdateAsync(order);
        return updatedOrder != null ? _mapper.Map<OrderReadDto>(updatedOrder) : null;
    }

    public async Task<bool> UpdateOrderStatusAsync(string id, OrderStatus newStatus, string? reason = null, string? changedBy = null)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return false;
        }

        var oldStatus = order.Status;
        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);

        // Create status history entry
        await CreateStatusHistoryAsync(order.Id, oldStatus, newStatus, reason, changedBy);

        return true;
    }

    public async Task<bool> CancelOrderAsync(string id, string? cancellationReason = null, string? cancelledBy = null)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return false;
        }

        // Only allow cancellation of certain statuses
        if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
        {
            return false;
        }

        var oldStatus = order.Status;
        order.Status = OrderStatus.Cancelled;
        order.CancellationReason = cancellationReason;
        order.CancelledAt = DateTime.UtcNow;
        order.CancelledBy = cancelledBy;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);

        // Create status history entry
        await CreateStatusHistoryAsync(order.Id, oldStatus, OrderStatus.Cancelled, cancellationReason, cancelledBy);

        return true;
    }

    public async Task<IEnumerable<OrderReadDto>> GetCustomerOrdersAsync(string customerId, int page = 1, int pageSize = 20)
    {
        var orders = await _orderRepository.GetCustomerOrdersAsync(customerId, page, pageSize);
        return _mapper.Map<IEnumerable<OrderReadDto>>(orders);
    }

    public async Task<IEnumerable<OrderReadDto>> GetSupplierOrdersAsync(string supplierId, int page = 1, int pageSize = 20)
    {
        var orders = await _orderRepository.GetSupplierOrdersAsync(supplierId, page, pageSize);
        return _mapper.Map<IEnumerable<OrderReadDto>>(orders);
    }

    public async Task<IEnumerable<OrderStatusHistoryReadDto>> GetOrderStatusHistoryAsync(string orderId)
    {
        var history = await _statusHistoryRepository.FindAsync(h => h.OrderId == orderId);
        return _mapper.Map<IEnumerable<OrderStatusHistoryReadDto>>(history.OrderBy(h => h.ChangedAt));
    }

    private async Task<string> GenerateOrderNumberAsync()
    {
        var today = DateTime.UtcNow;
        var prefix = $"ORD-{today:yyyyMMdd}";
        var count = await _orderRepository.GetOrderCountForDateAsync(today.Date);
        return $"{prefix}-{(count + 1):D4}";
    }

    private async Task CreateStatusHistoryAsync(string orderId, OrderStatus fromStatus, OrderStatus toStatus, string? reason, string? changedBy)
    {
        var statusHistory = new OrderStatusHistory
        {
            Id = Guid.NewGuid().ToString(),
            OrderId = orderId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = changedBy,
            Reason = reason,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _statusHistoryRepository.CreateAsync(statusHistory);
    }
}