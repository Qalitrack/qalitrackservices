using Microsoft.AspNetCore.Mvc;
using ServiceDiscovery.Services;

namespace ServiceDiscovery.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceDiscoveryController : ControllerBase
{
    private readonly IServiceAggregatorService _serviceAggregator;
    private readonly ILogger<ServiceDiscoveryController> _logger;

    public ServiceDiscoveryController(
        IServiceAggregatorService serviceAggregator,
        ILogger<ServiceDiscoveryController> logger)
    {
        _serviceAggregator = serviceAggregator;
        _logger = logger;
    }

    /// <summary>
    /// Get all services as JSON
    /// </summary>
    [HttpGet("services")]
    public async Task<IActionResult> GetServices()
    {
        try
        {
            var services = await _serviceAggregator.GetAllServicesAsync();
            return Ok(services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving services");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all services as HTML page
    /// </summary>
    [HttpGet("html")]
    [HttpGet("")]
    public async Task<IActionResult> GetServicesHtml()
    {
        try
        {
            var html = await _serviceAggregator.GetServicesHtmlAsync();
            return Content(html, "text/html");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating services HTML");
            return StatusCode(500, "Internal server error");
        }
    }
}

[ApiController]
[Route("")]
public class RootController : ControllerBase
{
    private readonly IServiceAggregatorService _serviceAggregator;

    public RootController(IServiceAggregatorService serviceAggregator)
    {
        _serviceAggregator = serviceAggregator;
    }

    /// <summary>
    /// Serve the services HTML at the root
    /// </summary>
    [HttpGet("")]
    [HttpGet("index.html")]
    public async Task<IActionResult> Index()
    {
        var html = await _serviceAggregator.GetServicesHtmlAsync();
        return Content(html, "text/html");
    }
}