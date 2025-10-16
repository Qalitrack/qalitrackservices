using System.Collections.Generic;
using System.Threading.Tasks;
using Masterdata.Core.DTOs.Drivers;
using Masterdata.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Masterdata.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverSuppliersController : ControllerBase
    {
        private readonly IDriverSupplierService _driverSupplierService;

        public DriverSuppliersController(IDriverSupplierService driverSupplierService)
        {
            _driverSupplierService = driverSupplierService;
        }

        [HttpGet("driver/{driverId}")]
        public async Task<ActionResult<IEnumerable<DriverSupplierReadDto>>> GetByDriverId(string driverId)
        {
            var result = await _driverSupplierService.GetByDriverIdAsync(driverId);
            return Ok(result);
        }

        [HttpGet("supplier/{supplierId}")]
        public async Task<ActionResult<IEnumerable<DriverSupplierReadDto>>> GetBySupplierId(string supplierId)
        {
            var result = await _driverSupplierService.GetBySupplierIdAsync(supplierId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<DriverSupplierReadDto>> AssignDriverToSupplier([FromBody] AssignDriverToSupplierRequest request)
        {
            var result = await _driverSupplierService.AssignDriverToSupplierAsync(request.DriverId, request.SupplierId);
            return CreatedAtAction(nameof(GetByDriverId), new { driverId = result.DriverId }, result);
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveDriverFromSupplier([FromQuery] string driverId, [FromQuery] string supplierId)
        {
            var result = await _driverSupplierService.RemoveDriverFromSupplierAsync(driverId, supplierId);
            if (!result) return NotFound();
            return NoContent();
        }
    }

    public class AssignDriverToSupplierRequest
    {
        public string DriverId { get; set; } = null!;
        public string SupplierId { get; set; } = null!;
    }
}
