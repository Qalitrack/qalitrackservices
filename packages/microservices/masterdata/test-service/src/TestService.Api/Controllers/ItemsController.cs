using Microsoft.AspNetCore.Mvc;
using TestService.Core.DTOs;
using TestService.Core.Interfaces;

namespace TestService.Api.Controllers;

[Route("api/[controller]")]
public class ItemsController : BaseController
{
    private readonly IItemService _itemService;
    private readonly ILogger<ItemsController> _logger;

    public ItemsController(IItemService itemService, ILogger<ItemsController> logger)
    {
        _itemService = itemService;
        _logger = logger;
    }

    /// <summary>
    /// Get all items
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var items = await _itemService.GetAllAsync();
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all items");
            return InternalServerError("An error occurred while retrieving items");
        }
    }

    /// <summary>
    /// Get item by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var item = await _itemService.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound("Item not found");
            }

            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting item with id {Id}", id);
            return InternalServerError("An error occurred while retrieving item");
        }
    }

    /// <summary>
    /// Create a new item
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateItemDto request)
    {
        try
        {
            var item = await _itemService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating item");
            return InternalServerError("An error occurred while creating item");
        }
    }

    /// <summary>
    /// Update an existing item
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateItemDto request)
    {
        try
        {
            var item = await _itemService.UpdateAsync(id, request);
            if (item == null)
            {
                return NotFound("Item not found");
            }

            return Ok(item, "Item updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating item with id {Id}", id);
            return InternalServerError("An error occurred while updating item");
        }
    }

    /// <summary>
    /// Delete a item
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var result = await _itemService.DeleteAsync(id);
            if (!result)
            {
                return NotFound("Item not found");
            }

            return Ok<object?>(null, "Item deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting item with id {Id}", id);
            return InternalServerError("An error occurred while deleting item");
        }
    }

    /// <summary>
    /// Check if item name is available
    /// </summary>
    [HttpGet("check-name/{name}")]
    public async Task<IActionResult> CheckName(string name)
    {
        try
        {
            var available = await _itemService.IsNameAvailableAsync(name);
            return Ok(new { available }, available ? "Name is available" : "Name is not available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking item name availability");
            return InternalServerError("An error occurred while checking name");
        }
    }
}