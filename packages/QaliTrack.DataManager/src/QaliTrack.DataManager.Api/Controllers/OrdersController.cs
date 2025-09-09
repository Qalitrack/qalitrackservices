using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.DataManager.Core.Common;
using QaliTrack.DataManager.Core.Modules.Orders.DTOs;
using QaliTrack.DataManager.Core.Modules.Orders.Entities;
using QaliTrack.DataManager.Infrastructure.Data;
using AutoMapper;

namespace QaliTrack.DataManager.Api.Controllers;

[ApiController]
[Route("orders")]
[Tags("Orders Module")]
public class OrdersController : ControllerBase
{
    private readonly DataManagerDbContext _context;
    private readonly IMapper _mapper;

    public OrdersController(DataManagerDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    #region Customer Orders

    /// <summary>
    /// Get customer orders
    /// </summary>
    [HttpGet("customer-orders")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CustomerOrderDto>>>> GetCustomerOrders(
        [FromQuery] QueryParameters queryParams)
    {
        var query = _context.CustomerOrders
            .Include(co => co.OrderLines)
            .AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            query = query.Where(co => co.OrderNumber.Contains(queryParams.Search) || co.CreatedBy.Contains(queryParams.Search));
        }

        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        var customerOrders = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var customerOrderDtos = _mapper.Map<IEnumerable<CustomerOrderDto>>(customerOrders);

        return Ok(new ApiResponse<IEnumerable<CustomerOrderDto>>
        {
            Data = customerOrderDtos,
            Success = true,
            Message = "Customer orders retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    /// <summary>
    /// Get customer order by ID
    /// </summary>
    [HttpGet("customer-orders/{id}")]
    public async Task<ActionResult<ApiResponse<CustomerOrderDto>>> GetCustomerOrder(Guid id)
    {
        var customerOrder = await _context.CustomerOrders
            .Include(co => co.OrderLines)
            .FirstOrDefaultAsync(co => co.Id == id);

        if (customerOrder == null)
        {
            return NotFound(new ApiResponse<CustomerOrderDto> { Success = false, Message = "Customer order not found" });
        }

        var customerOrderDto = _mapper.Map<CustomerOrderDto>(customerOrder);
        return Ok(new ApiResponse<CustomerOrderDto> { Data = customerOrderDto, Success = true, Message = "Customer order retrieved successfully" });
    }

    /// <summary>
    /// Get customer order lines by order ID
    /// </summary>
    [HttpGet("customer-orders/{id}/lines")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CustomerOrderLineDto>>>> GetCustomerOrderLines(Guid id)
    {
        var orderLines = await _context.CustomerOrderLines
            .Where(col => col.CustomerOrderId == id)
            .ToListAsync();

        var orderLineDtos = _mapper.Map<IEnumerable<CustomerOrderLineDto>>(orderLines);
        return Ok(new ApiResponse<IEnumerable<CustomerOrderLineDto>> 
        { 
            Data = orderLineDtos, 
            Success = true, 
            Message = "Customer order lines retrieved successfully" 
        });
    }

    /// <summary>
    /// Get customer orders by customer ID
    /// </summary>
    [HttpGet("customer-orders/by-customer/{customerId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CustomerOrderDto>>>> GetCustomerOrdersByCustomer(Guid customerId)
    {
        var customerOrders = await _context.CustomerOrders
            .Include(co => co.OrderLines)
            .Where(co => co.CustomerId == customerId)
            .ToListAsync();

        var customerOrderDtos = _mapper.Map<IEnumerable<CustomerOrderDto>>(customerOrders);
        return Ok(new ApiResponse<IEnumerable<CustomerOrderDto>> 
        { 
            Data = customerOrderDtos, 
            Success = true, 
            Message = "Customer orders retrieved successfully" 
        });
    }

    /// <summary>
    /// Create new customer order
    /// </summary>
    [HttpPost("customer-orders")]
    public async Task<ActionResult<ApiResponse<CustomerOrderDto>>> CreateCustomerOrder(CreateCustomerOrderDto createCustomerOrderDto)
    {
        var customerOrder = _mapper.Map<CustomerOrder>(createCustomerOrderDto);
        _context.CustomerOrders.Add(customerOrder);
        await _context.SaveChangesAsync();

        var customerOrderDto = _mapper.Map<CustomerOrderDto>(customerOrder);
        return CreatedAtAction(nameof(GetCustomerOrder), new { id = customerOrder.Id },
            new ApiResponse<CustomerOrderDto> { Data = customerOrderDto, Success = true, Message = "Customer order created successfully" });
    }

    /// <summary>
    /// Update customer order
    /// </summary>
    [HttpPut("customer-orders/{id}")]
    public async Task<ActionResult<ApiResponse<CustomerOrderDto>>> UpdateCustomerOrder(Guid id, UpdateCustomerOrderDto updateCustomerOrderDto)
    {
        var customerOrder = await _context.CustomerOrders.FindAsync(id);
        if (customerOrder == null)
        {
            return NotFound(new ApiResponse<CustomerOrderDto> { Success = false, Message = "Customer order not found" });
        }

        _mapper.Map(updateCustomerOrderDto, customerOrder);
        await _context.SaveChangesAsync();

        var customerOrderDto = _mapper.Map<CustomerOrderDto>(customerOrder);
        return Ok(new ApiResponse<CustomerOrderDto> { Data = customerOrderDto, Success = true, Message = "Customer order updated successfully" });
    }

    /// <summary>
    /// Delete customer order
    /// </summary>
    [HttpDelete("customer-orders/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCustomerOrder(Guid id)
    {
        var customerOrder = await _context.CustomerOrders.FindAsync(id);
        if (customerOrder == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Customer order not found" });
        }

        _context.CustomerOrders.Remove(customerOrder);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object> { Success = true, Message = "Customer order deleted successfully" });
    }

    #endregion

    #region Purchase Orders

    /// <summary>
    /// Get purchase orders
    /// </summary>
    [HttpGet("purchase-orders")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PurchaseOrderDto>>>> GetPurchaseOrders(
        [FromQuery] QueryParameters queryParams)
    {
        var query = _context.PurchaseOrders
            .Include(po => po.OrderLines)
            .AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            query = query.Where(po => po.OrderNumber.Contains(queryParams.Search) || po.CreatedBy.Contains(queryParams.Search));
        }

        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        var purchaseOrders = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var purchaseOrderDtos = _mapper.Map<IEnumerable<PurchaseOrderDto>>(purchaseOrders);

        return Ok(new ApiResponse<IEnumerable<PurchaseOrderDto>>
        {
            Data = purchaseOrderDtos,
            Success = true,
            Message = "Purchase orders retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    /// <summary>
    /// Get purchase order by ID
    /// </summary>
    [HttpGet("purchase-orders/{id}")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> GetPurchaseOrder(Guid id)
    {
        var purchaseOrder = await _context.PurchaseOrders
            .Include(po => po.OrderLines)
            .FirstOrDefaultAsync(po => po.Id == id);

        if (purchaseOrder == null)
        {
            return NotFound(new ApiResponse<PurchaseOrderDto> { Success = false, Message = "Purchase order not found" });
        }

        var purchaseOrderDto = _mapper.Map<PurchaseOrderDto>(purchaseOrder);
        return Ok(new ApiResponse<PurchaseOrderDto> { Data = purchaseOrderDto, Success = true, Message = "Purchase order retrieved successfully" });
    }

    /// <summary>
    /// Get purchase order lines by order ID
    /// </summary>
    [HttpGet("purchase-orders/{id}/lines")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PurchaseOrderLineDto>>>> GetPurchaseOrderLines(Guid id)
    {
        var orderLines = await _context.PurchaseOrderLines
            .Where(pol => pol.PurchaseOrderId == id)
            .ToListAsync();

        var orderLineDtos = _mapper.Map<IEnumerable<PurchaseOrderLineDto>>(orderLines);
        return Ok(new ApiResponse<IEnumerable<PurchaseOrderLineDto>> 
        { 
            Data = orderLineDtos, 
            Success = true, 
            Message = "Purchase order lines retrieved successfully" 
        });
    }

    /// <summary>
    /// Create new purchase order
    /// </summary>
    [HttpPost("purchase-orders")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> CreatePurchaseOrder(CreatePurchaseOrderDto createPurchaseOrderDto)
    {
        var purchaseOrder = _mapper.Map<PurchaseOrder>(createPurchaseOrderDto);
        _context.PurchaseOrders.Add(purchaseOrder);
        await _context.SaveChangesAsync();

        var purchaseOrderDto = _mapper.Map<PurchaseOrderDto>(purchaseOrder);
        return CreatedAtAction(nameof(GetPurchaseOrder), new { id = purchaseOrder.Id },
            new ApiResponse<PurchaseOrderDto> { Data = purchaseOrderDto, Success = true, Message = "Purchase order created successfully" });
    }

    /// <summary>
    /// Update purchase order
    /// </summary>
    [HttpPut("purchase-orders/{id}")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> UpdatePurchaseOrder(Guid id, UpdatePurchaseOrderDto updatePurchaseOrderDto)
    {
        var purchaseOrder = await _context.PurchaseOrders.FindAsync(id);
        if (purchaseOrder == null)
        {
            return NotFound(new ApiResponse<PurchaseOrderDto> { Success = false, Message = "Purchase order not found" });
        }

        _mapper.Map(updatePurchaseOrderDto, purchaseOrder);
        await _context.SaveChangesAsync();

        var purchaseOrderDto = _mapper.Map<PurchaseOrderDto>(purchaseOrder);
        return Ok(new ApiResponse<PurchaseOrderDto> { Data = purchaseOrderDto, Success = true, Message = "Purchase order updated successfully" });
    }

    /// <summary>
    /// Delete purchase order
    /// </summary>
    [HttpDelete("purchase-orders/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeletePurchaseOrder(Guid id)
    {
        var purchaseOrder = await _context.PurchaseOrders.FindAsync(id);
        if (purchaseOrder == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Purchase order not found" });
        }

        _context.PurchaseOrders.Remove(purchaseOrder);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object> { Success = true, Message = "Purchase order deleted successfully" });
    }

    #endregion

    #region Inter-Plant Transfers

    /// <summary>
    /// Get inter-plant transfers
    /// </summary>
    [HttpGet("transfers")]
    public async Task<ActionResult<ApiResponse<IEnumerable<InterPlantTransferDto>>>> GetInterPlantTransfers(
        [FromQuery] QueryParameters queryParams)
    {
        var query = _context.InterPlantTransfers
            .Include(ipt => ipt.TransferLines)
            .AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            query = query.Where(ipt => ipt.TransferNumber.Contains(queryParams.Search) || ipt.CreatedBy.Contains(queryParams.Search));
        }

        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        var interPlantTransfers = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var interPlantTransferDtos = _mapper.Map<IEnumerable<InterPlantTransferDto>>(interPlantTransfers);

        return Ok(new ApiResponse<IEnumerable<InterPlantTransferDto>>
        {
            Data = interPlantTransferDtos,
            Success = true,
            Message = "Inter-plant transfers retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    /// <summary>
    /// Get inter-plant transfer by ID
    /// </summary>
    [HttpGet("transfers/{id}")]
    public async Task<ActionResult<ApiResponse<InterPlantTransferDto>>> GetInterPlantTransfer(Guid id)
    {
        var interPlantTransfer = await _context.InterPlantTransfers
            .Include(ipt => ipt.TransferLines)
            .FirstOrDefaultAsync(ipt => ipt.Id == id);

        if (interPlantTransfer == null)
        {
            return NotFound(new ApiResponse<InterPlantTransferDto> { Success = false, Message = "Inter-plant transfer not found" });
        }

        var interPlantTransferDto = _mapper.Map<InterPlantTransferDto>(interPlantTransfer);
        return Ok(new ApiResponse<InterPlantTransferDto> { Data = interPlantTransferDto, Success = true, Message = "Inter-plant transfer retrieved successfully" });
    }

    /// <summary>
    /// Get inter-plant transfers by route
    /// </summary>
    [HttpGet("transfers/by-route/{fromSiteId}/{toSiteId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<InterPlantTransferDto>>>> GetInterPlantTransfersByRoute(Guid fromSiteId, Guid toSiteId)
    {
        var interPlantTransfers = await _context.InterPlantTransfers
            .Include(ipt => ipt.TransferLines)
            .Where(ipt => ipt.FromSiteId == fromSiteId && ipt.ToSiteId == toSiteId)
            .ToListAsync();

        var interPlantTransferDtos = _mapper.Map<IEnumerable<InterPlantTransferDto>>(interPlantTransfers);
        return Ok(new ApiResponse<IEnumerable<InterPlantTransferDto>> 
        { 
            Data = interPlantTransferDtos, 
            Success = true, 
            Message = "Inter-plant transfers retrieved successfully" 
        });
    }

    /// <summary>
    /// Create new inter-plant transfer
    /// </summary>
    [HttpPost("transfers")]
    public async Task<ActionResult<ApiResponse<InterPlantTransferDto>>> CreateInterPlantTransfer(CreateInterPlantTransferDto createInterPlantTransferDto)
    {
        var interPlantTransfer = _mapper.Map<InterPlantTransfer>(createInterPlantTransferDto);
        _context.InterPlantTransfers.Add(interPlantTransfer);
        await _context.SaveChangesAsync();

        var interPlantTransferDto = _mapper.Map<InterPlantTransferDto>(interPlantTransfer);
        return CreatedAtAction(nameof(GetInterPlantTransfer), new { id = interPlantTransfer.Id },
            new ApiResponse<InterPlantTransferDto> { Data = interPlantTransferDto, Success = true, Message = "Inter-plant transfer created successfully" });
    }

    /// <summary>
    /// Update inter-plant transfer
    /// </summary>
    [HttpPut("transfers/{id}")]
    public async Task<ActionResult<ApiResponse<InterPlantTransferDto>>> UpdateInterPlantTransfer(Guid id, UpdateInterPlantTransferDto updateInterPlantTransferDto)
    {
        var interPlantTransfer = await _context.InterPlantTransfers.FindAsync(id);
        if (interPlantTransfer == null)
        {
            return NotFound(new ApiResponse<InterPlantTransferDto> { Success = false, Message = "Inter-plant transfer not found" });
        }

        _mapper.Map(updateInterPlantTransferDto, interPlantTransfer);
        await _context.SaveChangesAsync();

        var interPlantTransferDto = _mapper.Map<InterPlantTransferDto>(interPlantTransfer);
        return Ok(new ApiResponse<InterPlantTransferDto> { Data = interPlantTransferDto, Success = true, Message = "Inter-plant transfer updated successfully" });
    }

    /// <summary>
    /// Delete inter-plant transfer
    /// </summary>
    [HttpDelete("transfers/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteInterPlantTransfer(Guid id)
    {
        var interPlantTransfer = await _context.InterPlantTransfers.FindAsync(id);
        if (interPlantTransfer == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Inter-plant transfer not found" });
        }

        _context.InterPlantTransfers.Remove(interPlantTransfer);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object> { Success = true, Message = "Inter-plant transfer deleted successfully" });
    }

    #endregion
}