using Microsoft.AspNetCore.Mvc;
using CustomerService.Core.DTOs;
using CustomerService.Core.Interfaces;

namespace CustomerService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : BaseController
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null, [FromQuery] string? customerId = null)
    {
        try
        {
            var orders = await _orderService.GetOrdersPagedAsync(page, pageSize, status, customerId);
            return HandleResult(Success(orders, "Orders retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<IEnumerable<OrderSummaryDto>>(ex.Message));
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto request)
    {
        try
        {
            var order = await _orderService.CreateOrderAsync(request);
            return HandleResult(Success(order, "Order created successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<OrderDto>(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(string id)
    {
        try
        {
            var order = await _orderService.GetOrderAsync(id);
            if (order == null)
            {
                return NotFound(Error<OrderDto>($"Order with ID {id} not found"));
            }

            return HandleResult(Success(order, "Order retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<OrderDto>(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(string id, [FromBody] UpdateOrderDto request)
    {
        try
        {
            var order = await _orderService.UpdateOrderAsync(id, request);
            return HandleResult(Success(order, "Order updated successfully"));
        }
        catch (ArgumentException ex)
        {
            return NotFound(Error<OrderDto>(ex.Message));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<OrderDto>(ex.Message));
        }
    }

    [HttpPost("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] OrderStatusUpdateDto request)
    {
        try
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, request);
            if (!result)
            {
                return NotFound(Error($"Order with ID {id} not found"));
            }

            return HandleResult(Success("Order status updated successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelOrder(string id, [FromBody] string? cancellationReason = null)
    {
        try
        {
            var result = await _orderService.CancelOrderAsync(id, cancellationReason);
            if (!result)
            {
                return NotFound(Error($"Order with ID {id} not found or cannot be cancelled"));
            }

            return HandleResult(Success("Order cancelled successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error(ex.Message));
        }
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetCustomerOrders(string customerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var orders = await _orderService.GetCustomerOrdersAsync(customerId, page, pageSize);
            return HandleResult(Success(orders, "Customer orders retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<IEnumerable<OrderSummaryDto>>(ex.Message));
        }
    }

    [HttpGet("supplier/{supplierId}")]
    public async Task<IActionResult> GetSupplierOrders(string supplierId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var orders = await _orderService.GetSupplierOrdersAsync(supplierId, page, pageSize);
            return HandleResult(Success(orders, "Supplier orders retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<IEnumerable<OrderSummaryDto>>(ex.Message));
        }
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var orders = await _orderService.GetPendingOrdersAsync(page, pageSize);
            return HandleResult(Success(orders, "Pending orders retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<IEnumerable<OrderSummaryDto>>(ex.Message));
        }
    }

    [HttpGet("in-transit")]
    public async Task<IActionResult> GetInTransitOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var orders = await _orderService.GetInTransitOrdersAsync(page, pageSize);
            return HandleResult(Success(orders, "In-transit orders retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<IEnumerable<OrderSummaryDto>>(ex.Message));
        }
    }

    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetOrderStatusHistory(string id)
    {
        try
        {
            var history = await _orderService.GetOrderStatusHistoryAsync(id);
            return HandleResult(Success(history, "Order status history retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<IEnumerable<OrderStatusHistoryDto>>(ex.Message));
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchOrders([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var orders = await _orderService.SearchOrdersAsync(query, page, pageSize);
            return HandleResult(Success(orders, "Orders search completed successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<IEnumerable<OrderSummaryDto>>(ex.Message));
        }
    }
}