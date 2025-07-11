using AutoMapper;
using CustomerService.Core.DTOs;
using CustomerService.Core.Entities;
using CustomerService.Core.Interfaces;

namespace CustomerService.Core.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;

    public OrderService(IOrderRepository orderRepository, ICustomerRepository customerRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _mapper = mapper;
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
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
        var orderNumber = await _orderRepository.GenerateOrderNumberAsync();

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

        await _orderRepository.AddAsync(order);

        // Create initial status history
        var statusHistory = new OrderStatusHistory
        {
            Id = Guid.NewGuid().ToString(),
            OrderId = order.Id,
            FromStatus = OrderStatus.Pending,
            ToStatus = OrderStatus.Pending,
            ChangedAt = DateTime.UtcNow,
            Reason = "Order created",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add status history (if you have a repository for it)
        // await _statusHistoryRepository.AddAsync(statusHistory);

        var orderWithDetails = await _orderRepository.GetOrderWithDetailsAsync(order.Id);
        return _mapper.Map<OrderDto>(orderWithDetails);
    }

    public async Task<OrderDto?> GetOrderAsync(string id)
    {
        var order = await _orderRepository.GetOrderWithDetailsAsync(id);
        return order != null ? _mapper.Map<OrderDto>(order) : null;
    }

    public async Task<IEnumerable<OrderSummaryDto>> GetOrdersPagedAsync(int page, int pageSize, string? status = null, string? customerId = null)
    {
        var orders = await _orderRepository.GetOrdersPagedAsync(page, pageSize, status, customerId);
        return _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);
    }

    public async Task<OrderDto> UpdateOrderAsync(string id, UpdateOrderDto updateOrderDto)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            throw new ArgumentException($"Order with ID {id} not found");
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

        await _orderRepository.UpdateAsync(order);

        var updatedOrder = await _orderRepository.GetOrderWithDetailsAsync(id);
        return _mapper.Map<OrderDto>(updatedOrder);
    }

    public async Task<bool> UpdateOrderStatusAsync(string id, OrderStatusUpdateDto statusUpdate)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return false;
        }

        if (!Enum.TryParse<OrderStatus>(statusUpdate.Status, out var newStatus))
        {
            throw new ArgumentException($"Invalid order status: {statusUpdate.Status}");
        }

        var oldStatus = order.Status;
        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);

        // Create status history entry
        var statusHistory = new OrderStatusHistory
        {
            Id = Guid.NewGuid().ToString(),
            OrderId = order.Id,
            FromStatus = oldStatus,
            ToStatus = newStatus,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = statusUpdate.ChangedBy,
            Reason = statusUpdate.Reason,
            Notes = statusUpdate.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add status history (you might need to create this repository method)
        // await _statusHistoryRepository.AddAsync(statusHistory);

        return true;
    }

    public async Task<bool> CancelOrderAsync(string id, string? cancellationReason = null)
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

        order.Status = OrderStatus.Cancelled;
        order.CancellationReason = cancellationReason;
        order.CancelledAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);

        return true;
    }

    public async Task<IEnumerable<OrderSummaryDto>> GetCustomerOrdersAsync(string customerId, int page, int pageSize)
    {
        var orders = await _orderRepository.GetCustomerOrdersAsync(customerId, page, pageSize);
        return _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);
    }

    public async Task<IEnumerable<OrderSummaryDto>> GetSupplierOrdersAsync(string supplierId, int page, int pageSize)
    {
        var orders = await _orderRepository.GetSupplierOrdersAsync(supplierId, page, pageSize);
        return _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);
    }

    public async Task<IEnumerable<OrderSummaryDto>> GetPendingOrdersAsync(int page, int pageSize)
    {
        var orders = await _orderRepository.GetPendingOrdersAsync(page, pageSize);
        return _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);
    }

    public async Task<IEnumerable<OrderSummaryDto>> GetInTransitOrdersAsync(int page, int pageSize)
    {
        var orders = await _orderRepository.GetInTransitOrdersAsync(page, pageSize);
        return _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);
    }

    public async Task<IEnumerable<OrderStatusHistoryDto>> GetOrderStatusHistoryAsync(string orderId)
    {
        var history = await _orderRepository.GetOrderStatusHistoryAsync(orderId);
        return _mapper.Map<IEnumerable<OrderStatusHistoryDto>>(history);
    }

    public async Task<IEnumerable<OrderSummaryDto>> SearchOrdersAsync(string query, int page, int pageSize)
    {
        var orders = await _orderRepository.SearchOrdersAsync(query, page, pageSize);
        return _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);
    }
}