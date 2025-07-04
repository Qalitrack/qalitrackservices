using TransporterService.Core.DTOs;
using TransporterService.Core.Entities;

namespace TransporterService.Core.Interfaces;

public interface ITransporterService
{
    // Transporter Management
    Task<TransporterDto> RegisterTransporterAsync(RegisterTransporterRequest request);
    Task<TransporterDto> GetTransporterByIdAsync(string id);
    Task<IEnumerable<TransporterDto>> GetAllTransportersAsync();
    Task<IEnumerable<TransporterDto>> GetActiveTransportersAsync();
    Task<IEnumerable<TransporterDto>> GetTransportersByTypeAsync(TransporterType type);
    Task<IEnumerable<TransporterDto>> GetTransportersByStatusAsync(TransporterStatus status);
    Task<TransporterDto> UpdateTransporterAsync(string id, UpdateTransporterRequest request);
    Task<bool> DeleteTransporterAsync(string id);
    Task<IEnumerable<TransporterDto>> SearchTransportersAsync(string searchTerm);
    Task<IEnumerable<TransporterDto>> GetAvailableTransportersAsync(DateTime date, string? routeId = null);
    
    // Contact Management
    Task<TransporterContactDto> AddContactAsync(CreateTransporterContactRequest request);
    Task<IEnumerable<TransporterContactDto>> GetContactsByTransporterIdAsync(string transporterId);
    Task<TransporterContactDto?> GetPrimaryContactAsync(string transporterId);
    Task<bool> DeleteContactAsync(string contactId);
    
    // Fleet Management
    Task<TransporterFleetDto> AddFleetVehicleAsync(AddFleetVehicleRequest request);
    Task<IEnumerable<TransporterFleetDto>> GetFleetByTransporterIdAsync(string transporterId);
    Task<IEnumerable<TransporterFleetDto>> GetAvailableVehiclesAsync(string transporterId);
    Task<bool> DeleteFleetVehicleAsync(string vehicleId);
    
    // Driver Management
    Task<TransporterDriverDto> AssignDriverAsync(AssignDriverRequest request);
    Task<IEnumerable<TransporterDriverDto>> GetDriversByTransporterIdAsync(string transporterId);
    Task<IEnumerable<TransporterDriverDto>> GetActiveDriversAsync(string transporterId);
    Task<bool> UnassignDriverAsync(string driverId);
    
    // License Management
    Task<IEnumerable<TransporterLicenseDto>> GetLicensesByTransporterIdAsync(string transporterId);
    Task<IEnumerable<TransporterLicenseDto>> GetExpiringLicensesAsync(string transporterId, int daysAhead = 30);
    
    // Insurance Management
    Task<IEnumerable<TransporterInsuranceDto>> GetInsuranceByTransporterIdAsync(string transporterId);
    Task<IEnumerable<TransporterInsuranceDto>> GetExpiringInsuranceAsync(string transporterId, int daysAhead = 30);
    
    // Contract Management
    Task<IEnumerable<TransporterContractDto>> GetContractsByTransporterIdAsync(string transporterId);
    Task<IEnumerable<TransporterContractDto>> GetActiveContractsAsync(string transporterId);
    
    // Performance Management
    Task<IEnumerable<TransporterPerformanceDto>> GetPerformanceByTransporterIdAsync(string transporterId);
    Task<TransporterPerformanceSummaryDto> GetPerformanceSummaryAsync(string transporterId);
}