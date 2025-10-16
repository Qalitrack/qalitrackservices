using System.Threading.Tasks;
using Masterdata.Core.DTOs.Drivers;
using Masterdata.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Masterdata.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverVehiclesController : ControllerBase
    {
        private readonly IDriverService _driverService;
        private readonly IVehicleService _vehicleService;

        public DriverVehiclesController(
            IDriverService driverService,
            IVehicleService vehicleService)
        {
            _driverService = driverService;
            _vehicleService = vehicleService;
        }

        [HttpPost]
        public async Task<IActionResult> AssignDriverToVehicle([FromBody] AssignDriverToVehicleRequest request)
        {
            // Verify driver exists
            var driver = await _driverService.GetByIdAsync(request.DriverId);
            if (driver == null)
            {
                return NotFound($"Driver with ID {request.DriverId} not found");
            }

            // Verify vehicle exists
            var vehicle = await _vehicleService.GetByIdAsync(request.VehicleId);
            if (vehicle == null)
            {
                return NotFound($"Vehicle with ID {request.VehicleId} not found");
            }

            await _driverService.AssignVehicleAsync(request.DriverId, request.VehicleId);
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveDriverFromVehicle([FromQuery] string driverId, [FromQuery] string vehicleId)
        {
            var result = await _driverService.RemoveVehicleAsync(driverId, vehicleId);
            if (!result) return NotFound("Driver-vehicle assignment not found");
            return NoContent();
        }
    }

    public class AssignDriverToVehicleRequest
    {
        public string DriverId { get; set; } = null!;
        public string VehicleId { get; set; } = null!;
    }
}
