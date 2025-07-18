using Microsoft.AspNetCore.Mvc;
using CustomerService.Core.DTOs;
using CustomerService.Core.Entities;
using CustomerService.Core.Interfaces;

namespace CustomerService.Api.Controllers;

[Route("api/[controller]")]
public class OrdersController : BaseController
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20, 
        [FromQuery] string? status = null, 
        [FromQuery] string? customerId = null)
    {
        try
        {
            var orders = await _orderService.GetOrdersAsync(page, pageSize, status, customerId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders");
            return InternalServerError("An error occurred while retrieving orders");
        }
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(string id)
    {
        try
        {
            var order = await _orderService.GetOrderAsync(id);
            if (order == null)
            {
                return NotFound("Order not found");
            }

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order with id {Id}", id);
            return InternalServerError("An error occurred while retrieving order");
        }
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto request)
    {
        try
        {
            var order = await _orderService.CreateOrderAsync(request);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return InternalServerError("An error occurred while creating order");
        }
    }

    /// <summary>
    /// Update an existing order
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(string id, [FromBody] UpdateOrderDto request)
    {
        try
        {
            var order = await _orderService.UpdateOrderAsync(id, request);
            if (order == null)
            {
                return NotFound("Order not found");
            }

            return Ok(order, "Order updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order with id {Id}", id);
            return InternalServerError("An error occurred while updating order");
        }
    }

    /// <summary>
    /// Update order status
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] UpdateOrderStatusDto request)
    {
        try
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, request.Status, request.Reason, request.ChangedBy);
            if (!result)
            {
                return NotFound("Order not found");
            }

            return Ok<object?>(null, "Order status updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status for id {Id}", id);
            return InternalServerError("An error occurred while updating order status");
        }
    }

    /// <summary>
    /// Cancel an order
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(string id, [FromBody] CancelOrderDto request)
    {
        try
        {
            var result = await _orderService.CancelOrderAsync(id, request.CancellationReason, request.CancelledBy);
            if (!result)
            {
                return NotFound("Order not found or cannot be cancelled");
            }

            return Ok<object?>(null, "Order cancelled successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order with id {Id}", id);
            return InternalServerError("An error occurred while cancelling order");
        }
    }

    /// <summary>
    /// Get orders for a specific customer
    /// </summary>
    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetCustomerOrders(string customerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var orders = await _orderService.GetCustomerOrdersAsync(customerId, page, pageSize);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for customer {CustomerId}", customerId);
            return InternalServerError("An error occurred while retrieving customer orders");
        }
    }

    /// <summary>
    /// Get orders for a specific supplier
    /// </summary>
    [HttpGet("supplier/{supplierId}")]
    public async Task<IActionResult> GetSupplierOrders(string supplierId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var orders = await _orderService.GetSupplierOrdersAsync(supplierId, page, pageSize);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for supplier {SupplierId}", supplierId);
            return InternalServerError("An error occurred while retrieving supplier orders");
        }
    }

    /// <summary>
    /// Get order status history
    /// </summary>
    [HttpGet("{id}/status-history")]
    public async Task<IActionResult> GetOrderStatusHistory(string id)
    {
        try
        {
            var history = await _orderService.GetOrderStatusHistoryAsync(id);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status history for order {Id}", id);
            return InternalServerError("An error occurred while retrieving order status history");
        }
    }
}