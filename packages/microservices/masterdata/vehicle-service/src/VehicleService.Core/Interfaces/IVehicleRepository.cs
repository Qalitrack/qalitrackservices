using VehicleService.Core.Entities;

namespace VehicleService.Core.Interfaces;

public interface IVehicleRepository : IRepository<Vehicle>
{
    Task<Vehicle?> GetByRegistrationNumberAsync(string registrationNumber);
    Task<Vehicle?> GetByVINAsync(string vin);
    Task<List<Vehicle>> GetByOwnerAsync(string ownerName);
    Task<List<Vehicle>> GetByStatusAsync(VehicleStatus status);
    Task<List<Vehicle>> GetByVehicleTypeAsync(string vehicleTypeId);
    Task<bool> IsRegistrationNumberUniqueAsync(string registrationNumber, string? excludeId = null);
    Task<bool> IsVINUniqueAsync(string vin, string? excludeId = null);
    Task<List<Vehicle>> GetExpiringInsuranceAsync(DateTime beforeDate);
    Task<List<Vehicle>> GetExpiringRegistrationAsync(DateTime beforeDate);
    Task<List<Vehicle>> GetDueForInspectionAsync(DateTime beforeDate);
    Task<Vehicle?> GetWithDetailsAsync(string id);
}

public interface IVehicleTypeRepository : IRepository<VehicleType>
{
    Task<VehicleType?> GetByNameAsync(string name);
    Task<List<VehicleType>> GetByCategoryAsync(string category);
    Task<List<VehicleType>> GetActiveTypesAsync();
    Task<bool> IsNameUniqueAsync(string name, string? excludeId = null);
}

public interface IVehicleRegistrationRepository : IRepository<VehicleRegistration>
{
    Task<VehicleRegistration?> GetByVehicleIdAsync(string vehicleId);
    Task<VehicleRegistration?> GetByRegistrationNumberAsync(string registrationNumber);
    Task<List<VehicleRegistration>> GetExpiringRegistrationsAsync(DateTime beforeDate);
    Task<List<VehicleRegistration>> GetByIssuingAuthorityAsync(string authority);
}

public interface IVehicleSpecificationRepository : IRepository<VehicleSpecification>
{
    Task<VehicleSpecification?> GetByVehicleIdAsync(string vehicleId);
}

public interface IVehicleDocumentRepository : IRepository<VehicleDocument>
{
    Task<List<VehicleDocument>> GetByVehicleIdAsync(string vehicleId);
    Task<List<VehicleDocument>> GetByDocumentTypeAsync(string vehicleId, DocumentType documentType);
    Task<List<VehicleDocument>> GetExpiringDocumentsAsync(DateTime beforeDate);
    Task<List<VehicleDocument>> GetRequiredDocumentsAsync(string vehicleId);
}

public interface IVehicleInspectionRepository : IRepository<VehicleInspection>
{
    Task<List<VehicleInspection>> GetByVehicleIdAsync(string vehicleId);
    Task<VehicleInspection?> GetLatestInspectionAsync(string vehicleId);
    Task<List<VehicleInspection>> GetByInspectionTypeAsync(string vehicleId, InspectionType inspectionType);
    Task<List<VehicleInspection>> GetByResultAsync(InspectionResult result);
    Task<List<VehicleInspection>> GetDueInspectionsAsync(DateTime beforeDate);
}

public interface IVehicleInsuranceRepository : IRepository<VehicleInsurance>
{
    Task<List<VehicleInsurance>> GetByVehicleIdAsync(string vehicleId);
    Task<VehicleInsurance?> GetActiveInsuranceAsync(string vehicleId);
    Task<List<VehicleInsurance>> GetByInsuranceCompanyAsync(string company);
    Task<List<VehicleInsurance>> GetExpiringInsuranceAsync(DateTime beforeDate);
    Task<List<VehicleInsurance>> GetByPolicyNumberAsync(string policyNumber);
}