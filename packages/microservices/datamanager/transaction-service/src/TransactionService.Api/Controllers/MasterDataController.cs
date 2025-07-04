using Microsoft.AspNetCore.Mvc;
using TransactionService.Core.DTOs;
using TransactionService.Core.Interfaces;

namespace TransactionService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MasterDataController : BaseController
{
    private readonly IMasterDataIntegrationService _masterDataService;

    public MasterDataController(IMasterDataIntegrationService masterDataService)
    {
        _masterDataService = masterDataService;
    }

    /// <summary>
    /// Validate a vehicle ID
    /// </summary>
    [HttpGet("validate/vehicle/{vehicleId}")]
    public async Task<IActionResult> ValidateVehicle(string vehicleId)
    {
        try
        {
            var isValid = await _masterDataService.ValidateVehicleAsync(vehicleId);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = isValid ? "Vehicle is valid" : "Vehicle not found",
                Data = isValid
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Validate a driver ID
    /// </summary>
    [HttpGet("validate/driver/{driverId}")]
    public async Task<IActionResult> ValidateDriver(string driverId)
    {
        try
        {
            var isValid = await _masterDataService.ValidateDriverAsync(driverId);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = isValid ? "Driver is valid" : "Driver not found",
                Data = isValid
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Validate a supplier ID
    /// </summary>
    [HttpGet("validate/supplier/{supplierId}")]
    public async Task<IActionResult> ValidateSupplier(string supplierId)
    {
        try
        {
            var isValid = await _masterDataService.ValidateSupplierAsync(supplierId);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = isValid ? "Supplier is valid" : "Supplier not found",
                Data = isValid
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Validate a customer ID
    /// </summary>
    [HttpGet("validate/customer/{customerId}")]
    public async Task<IActionResult> ValidateCustomer(string customerId)
    {
        try
        {
            var isValid = await _masterDataService.ValidateCustomerAsync(customerId);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = isValid ? "Customer is valid" : "Customer not found",
                Data = isValid
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Validate a product ID
    /// </summary>
    [HttpGet("validate/product/{productId}")]
    public async Task<IActionResult> ValidateProduct(string productId)
    {
        try
        {
            var isValid = await _masterDataService.ValidateProductAsync(productId);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = isValid ? "Product is valid" : "Product not found",
                Data = isValid
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Validate a route ID
    /// </summary>
    [HttpGet("validate/route/{routeId}")]
    public async Task<IActionResult> ValidateRoute(string routeId)
    {
        try
        {
            var isValid = await _masterDataService.ValidateRouteAsync(routeId);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = isValid ? "Route is valid" : "Route not found",
                Data = isValid
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Validate a weighbridge ID
    /// </summary>
    [HttpGet("validate/weighbridge/{weighbridgeId}")]
    public async Task<IActionResult> ValidateWeighbridge(string weighbridgeId)
    {
        try
        {
            var isValid = await _masterDataService.ValidateWeighbridgeAsync(weighbridgeId);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = isValid ? "Weighbridge is valid" : "Weighbridge not found",
                Data = isValid
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Validate an organization ID
    /// </summary>
    [HttpGet("validate/organization/{organizationId}")]
    public async Task<IActionResult> ValidateOrganization(string organizationId)
    {
        try
        {
            var isValid = await _masterDataService.ValidateOrganizationAsync(organizationId);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = isValid ? "Organization is valid" : "Organization not found",
                Data = isValid
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get vehicle details
    /// </summary>
    [HttpGet("details/vehicle/{vehicleId}")]
    public async Task<IActionResult> GetVehicleDetails(string vehicleId)
    {
        try
        {
            var details = await _masterDataService.GetVehicleDetailsAsync(vehicleId);
            return HandleResult(details, "Vehicle not found");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get driver details
    /// </summary>
    [HttpGet("details/driver/{driverId}")]
    public async Task<IActionResult> GetDriverDetails(string driverId)
    {
        try
        {
            var details = await _masterDataService.GetDriverDetailsAsync(driverId);
            return HandleResult(details, "Driver not found");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get supplier details
    /// </summary>
    [HttpGet("details/supplier/{supplierId}")]
    public async Task<IActionResult> GetSupplierDetails(string supplierId)
    {
        try
        {
            var details = await _masterDataService.GetSupplierDetailsAsync(supplierId);
            return HandleResult(details, "Supplier not found");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get customer details
    /// </summary>
    [HttpGet("details/customer/{customerId}")]
    public async Task<IActionResult> GetCustomerDetails(string customerId)
    {
        try
        {
            var details = await _masterDataService.GetCustomerDetailsAsync(customerId);
            return HandleResult(details, "Customer not found");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get product details
    /// </summary>
    [HttpGet("details/product/{productId}")]
    public async Task<IActionResult> GetProductDetails(string productId)
    {
        try
        {
            var details = await _masterDataService.GetProductDetailsAsync(productId);
            return HandleResult(details, "Product not found");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Validate multiple entities at once
    /// </summary>
    [HttpPost("validate/batch")]
    public async Task<IActionResult> ValidateBatch([FromBody] BatchValidationRequest request)
    {
        try
        {
            var results = new Dictionary<string, bool>();

            if (!string.IsNullOrEmpty(request.VehicleId))
                results["Vehicle"] = await _masterDataService.ValidateVehicleAsync(request.VehicleId);
            
            if (!string.IsNullOrEmpty(request.DriverId))
                results["Driver"] = await _masterDataService.ValidateDriverAsync(request.DriverId);
            
            if (!string.IsNullOrEmpty(request.SupplierId))
                results["Supplier"] = await _masterDataService.ValidateSupplierAsync(request.SupplierId);
            
            if (!string.IsNullOrEmpty(request.CustomerId))
                results["Customer"] = await _masterDataService.ValidateCustomerAsync(request.CustomerId);
            
            if (!string.IsNullOrEmpty(request.ProductId))
                results["Product"] = await _masterDataService.ValidateProductAsync(request.ProductId);
            
            if (!string.IsNullOrEmpty(request.RouteId))
                results["Route"] = await _masterDataService.ValidateRouteAsync(request.RouteId);
            
            if (!string.IsNullOrEmpty(request.WeighbridgeId))
                results["Weighbridge"] = await _masterDataService.ValidateWeighbridgeAsync(request.WeighbridgeId);
            
            if (!string.IsNullOrEmpty(request.OrganizationId))
                results["Organization"] = await _masterDataService.ValidateOrganizationAsync(request.OrganizationId);

            var allValid = results.Values.All(v => v);

            return Ok(new ApiResponse<object>
            {
                Success = allValid,
                Message = allValid ? "All entities are valid" : "Some entities are invalid",
                Data = new { ValidationResults = results, AllValid = allValid }
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}

public class BatchValidationRequest
{
    public string? VehicleId { get; set; }
    public string? DriverId { get; set; }
    public string? SupplierId { get; set; }
    public string? CustomerId { get; set; }
    public string? ProductId { get; set; }
    public string? RouteId { get; set; }
    public string? WeighbridgeId { get; set; }
    public string? OrganizationId { get; set; }
}