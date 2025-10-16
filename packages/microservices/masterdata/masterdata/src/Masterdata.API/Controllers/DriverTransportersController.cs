using System.Collections.Generic;
using System.Threading.Tasks;
using Masterdata.Core.DTOs.Drivers;
using Masterdata.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Masterdata.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverTransportersController : ControllerBase
    {
        private readonly IDriverTransporterService _driverTransporterService;

        public DriverTransportersController(IDriverTransporterService driverTransporterService)
        {
            _driverTransporterService = driverTransporterService;
        }

        [HttpGet("driver/{driverId}")]
        public async Task<ActionResult<IEnumerable<DriverTransporterReadDto>>> GetByDriverId(string driverId)
        {
            var result = await _driverTransporterService.GetByDriverIdAsync(driverId);
            return Ok(result);
        }

        [HttpGet("transporter/{transporterId}")]
        public async Task<ActionResult<IEnumerable<DriverTransporterReadDto>>> GetByTransporterId(string transporterId)
        {
            var result = await _driverTransporterService.GetByTransporterIdAsync(transporterId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<DriverTransporterReadDto>> AssignDriverToTransporter([FromBody] AssignDriverToTransporterRequest request)
        {
            var result = await _driverTransporterService.AssignDriverToTransporterAsync(request.DriverId, request.TransporterId);
            return CreatedAtAction(nameof(GetByDriverId), new { driverId = result.DriverId }, result);
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveDriverFromTransporter([FromQuery] string driverId, [FromQuery] string transporterId)
        {
            var result = await _driverTransporterService.RemoveDriverFromTransporterAsync(driverId, transporterId);
            if (!result) return NotFound();
            return NoContent();
        }
    }

    public class AssignDriverToTransporterRequest
    {
        public string DriverId { get; set; } = null!;
        public string TransporterId { get; set; } = null!;
    }
}
