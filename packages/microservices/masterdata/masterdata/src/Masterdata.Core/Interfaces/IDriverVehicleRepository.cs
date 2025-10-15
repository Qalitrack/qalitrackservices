using System.Threading.Tasks;
using System.Collections.Generic;

namespace Masterdata.Core.Interfaces
{
    public interface IDriverVehicleRepository
    {
        Task<bool> AssignVehicleToDriverAsync(string driverId, string vehicleId);
        Task<bool> RemoveVehicleFromDriverAsync(string driverId, string vehicleId);
        Task<bool> IsVehicleAssignedToDriverAsync(string driverId, string vehicleId);
        Task<IEnumerable<string>> GetDriverVehiclesAsync(string driverId);
        Task<IEnumerable<string>> GetVehicleDriversAsync(string vehicleId);
        Task SetPrimaryDriverAsync(string vehicleId, string? driverId);
        
        /// <summary>
        /// Removes all vehicle assignments for a driver
        /// </summary>
        Task RemoveAllVehicleAssignmentsAsync(string driverId);
        
        /// <summary>
        /// Clears the primary driver reference from all vehicles where the specified driver is set as primary
        /// </summary>
        Task ClearPrimaryDriverReferencesAsync(string driverId);
        
        /// <summary>
        /// Removes all driver assignments for a vehicle
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle</param>
        Task RemoveAllDriverAssignmentsAsync(string vehicleId);
    }
}
