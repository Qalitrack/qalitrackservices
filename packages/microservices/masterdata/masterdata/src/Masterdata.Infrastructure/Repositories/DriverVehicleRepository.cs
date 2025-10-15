using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Masterdata.Core.Entities;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;
using Masterdata.Core.Utils;
using Masterdata.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Masterdata.Infrastructure.Repositories
{
    public class DriverVehicleRepository : Repository<Driver>, IDriverVehicleRepository
    {
        private readonly MasterdataDbContext _context;

        public DriverVehicleRepository(
            MasterdataDbContext context,
            IHttpContextAccessor httpContextAccessor,
            ITokenExtractionService tokenExtractionService,
            IAuditLogRepository auditLogRepository)
            : base(context, httpContextAccessor, tokenExtractionService, auditLogRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<bool> AssignVehicleToDriverAsync(string driverId, string vehicleId)
        {
            var driver = await base.GetByIdAsync(driverId);
            if (driver == null || driver.IsDeleted)
                return false;

            var vehicle = await _context.Vehicles
                .Include(v => v.AssignedDrivers)
                .FirstOrDefaultAsync(v => v.Id == vehicleId && !v.IsDeleted);

            if (vehicle == null)
                return false;

            // Check if already assigned
            if (driver.AssignedVehicles.Any(v => v.Id == vehicleId))
                return true;

            // Add to many-to-many relationship
            driver.AssignedVehicles.Add(vehicle);
            vehicle.AssignedDrivers.Add(driver);

            // Update both entities to ensure proper auditing
            await base.UpdateAsync(driver);
            return true;
        }

        public async Task<bool> RemoveVehicleFromDriverAsync(string driverId, string vehicleId)
        {
            var driver = await base.GetByIdAsync(driverId);
            if (driver == null || driver.IsDeleted)
                return false;

            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == vehicleId && !v.IsDeleted);

            if (vehicle == null)
                return false;

            // Remove from many-to-many relationship
            driver.AssignedVehicles.Remove(vehicle);
            vehicle.AssignedDrivers.Remove(driver);

            // If this driver was the primary driver, clear the primary driver
            if (vehicle.DriverId == driverId)
            {
                vehicle.DriverId = null;
            }

            // Update both entities to ensure proper auditing
            await base.UpdateAsync(driver);
            return true;
        }

        public async Task<bool> IsVehicleAssignedToDriverAsync(string driverId, string vehicleId)
        {
            var driver = await base.GetByIdAsync(driverId);
            if (driver == null || driver.IsDeleted)
                return false;

            return await _context.Vehicles
                .Where(v => v.Id == vehicleId && !v.IsDeleted)
                .SelectMany(v => v.AssignedDrivers)
                .AnyAsync(d => d.Id == driverId && !d.IsDeleted);
        }

        public async Task<IEnumerable<string>> GetDriverVehiclesAsync(string driverId)
        {
            var driver = await base.GetByIdAsync(driverId);
            if (driver == null || driver.IsDeleted)
                return Enumerable.Empty<string>();

            return driver.AssignedVehicles
                .Where(v => !v.IsDeleted)
                .Select(v => v.Id)
                .ToList();
        }

        public async Task<IEnumerable<string>> GetVehicleDriversAsync(string vehicleId)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.AssignedDrivers)
                .FirstOrDefaultAsync(v => v.Id == vehicleId && !v.IsDeleted);

            return vehicle?.AssignedDrivers
                .Where(d => !d.IsDeleted)
                .Select(d => d.Id)
                .ToList() ?? new List<string>();
        }

        public async Task SetPrimaryDriverAsync(string vehicleId, string? driverId)
        {
            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == vehicleId && !v.IsDeleted);

            if (vehicle == null)
                throw new KeyNotFoundException($"Vehicle with ID {vehicleId} not found");

            if (driverId != null)
            {
                // Verify the driver exists and is assigned to this vehicle
                var isAssigned = await IsVehicleAssignedToDriverAsync(driverId, vehicleId);
                if (!isAssigned)
                    throw new InvalidOperationException(
                        "Driver must be assigned to the vehicle before being set as primary");
            }

            vehicle.DriverId = driverId;
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAllVehicleAssignmentsAsync(string driverId)
        {
            var driver = await _context.Drivers
                .Include(d => d.AssignedVehicles)
                .FirstOrDefaultAsync(d => d.Id == driverId && !d.IsDeleted);

            if (driver == null) return;

            // Remove the driver from all assigned vehicles
            foreach (var vehicle in driver.AssignedVehicles.ToList())
            {
                driver.AssignedVehicles.Remove(vehicle);
                vehicle.AssignedDrivers.Remove(driver);
            }

            await _context.SaveChangesAsync();
        }

        public async Task ClearPrimaryDriverReferencesAsync(string driverId)
        {
            // Find all vehicles where this driver is the primary driver
            var vehicles = await _context.Vehicles
                .Where(v => v.DriverId == driverId && !v.IsDeleted)
                .ToListAsync();

            // Clear the primary driver reference
            foreach (var vehicle in vehicles)
            {
                vehicle.DriverId = null;
            }

            if (vehicles.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveAllDriverAssignmentsAsync(string vehicleId)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.AssignedDrivers)
                .FirstOrDefaultAsync(v => v.Id == vehicleId && !v.IsDeleted);

            if (vehicle == null) return;

            // Remove all driver assignments for this vehicle
            foreach (var driver in vehicle.AssignedDrivers.ToList())
            {
                vehicle.AssignedDrivers.Remove(driver);
                driver.AssignedVehicles.Remove(vehicle);
            }

            await _context.SaveChangesAsync();
        }
    }
}