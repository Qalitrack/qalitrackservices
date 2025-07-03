using DriverService.Core.DTOs;
using DriverService.Core.Entities;

namespace DriverService.Core.Interfaces;

public interface IDriverService
{
    // Driver CRUD operations
    Task<IEnumerable<DriverDto>> GetAllDriversAsync();
    Task<DriverDto?> GetDriverByIdAsync(string id);
    Task<DriverDto> CreateDriverAsync(CreateDriverDto createDriverDto);
    Task<DriverDto> UpdateDriverAsync(string id, UpdateDriverDto updateDriverDto);
    Task DeleteDriverAsync(string id);

    // Driver search and filtering
    Task<IEnumerable<DriverDto>> GetActiveDriversAsync();
    Task<IEnumerable<DriverDto>> GetDriversByStatusAsync(DriverStatus status);
    Task<IEnumerable<DriverDto>> SearchDriversAsync(string searchTerm);
    Task<DriverDto?> GetDriverByEmployeeIdAsync(string employeeId);
    Task<DriverDto?> GetDriverByEmailAsync(string email);

    // License management
    Task<DriverLicenseDto?> GetDriverLicenseAsync(string driverId);
    Task<DriverLicenseDto> CreateDriverLicenseAsync(CreateDriverLicenseDto createLicenseDto);
    Task<DriverLicenseDto> UpdateDriverLicenseAsync(string driverId, UpdateDriverLicenseDto updateLicenseDto);
    Task<IEnumerable<DriverDto>> GetDriversWithExpiringLicensesAsync(int daysAhead = 30);
    Task<LicenseValidationResult> ValidateLicenseAsync(string driverId);

    // Driver profile management
    Task<DriverProfileDto?> GetDriverProfileAsync(string driverId);
    Task<DriverProfileDto> CreateDriverProfileAsync(CreateDriverProfileDto createProfileDto);
    Task<DriverProfileDto> UpdateDriverProfileAsync(string driverId, UpdateDriverProfileDto updateProfileDto);

    // Document management
    Task<IEnumerable<DriverDocumentDto>> GetDriverDocumentsAsync(string driverId);
    Task<DriverDocumentDto> AddDriverDocumentAsync(CreateDriverDocumentDto createDocumentDto);
    Task<DriverDocumentDto> UpdateDriverDocumentAsync(string documentId, UpdateDriverDocumentDto updateDocumentDto);
    Task DeleteDriverDocumentAsync(string documentId);

    // Violation management
    Task<IEnumerable<DriverViolationDto>> GetDriverViolationsAsync(string driverId);
    Task<DriverViolationDto> AddDriverViolationAsync(CreateDriverViolationDto createViolationDto);
    Task<DriverViolationDto> UpdateDriverViolationAsync(string violationId, UpdateDriverViolationDto updateViolationDto);
    Task DeleteDriverViolationAsync(string violationId);
}