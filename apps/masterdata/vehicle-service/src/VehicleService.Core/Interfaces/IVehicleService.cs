using VehicleService.Core.DTOs;
using VehicleService.Core.Entities;

namespace VehicleService.Core.Interfaces;

public interface IVehicleService
{
    // Vehicle Management
    Task<VehicleDto> RegisterVehicleAsync(RegisterVehicleRequest request);
    Task<VehicleDto> UpdateVehicleAsync(string id, UpdateVehicleRequest request);
    Task<VehicleDto?> GetVehicleByIdAsync(string id);
    Task<VehicleDto?> GetVehicleByRegistrationNumberAsync(string registrationNumber);
    Task<VehicleDto?> GetVehicleByVINAsync(string vin);
    Task<List<VehicleDto>> GetAllVehiclesAsync();
    Task<List<VehicleDto>> GetVehiclesByStatusAsync(VehicleStatus status);
    Task<List<VehicleDto>> GetVehiclesByOwnerAsync(string ownerName);
    Task<VehicleDto?> GetVehicleWithDetailsAsync(string id);
    Task DeleteVehicleAsync(string id);
    Task<bool> ValidateRegistrationNumberAsync(string registrationNumber, string? excludeId = null);
    Task<bool> ValidateVINAsync(string vin, string? excludeId = null);

    // Vehicle Type Management
    Task<VehicleTypeDto> CreateVehicleTypeAsync(CreateVehicleTypeRequest request);
    Task<VehicleTypeDto> UpdateVehicleTypeAsync(string id, UpdateVehicleTypeRequest request);
    Task<VehicleTypeDto?> GetVehicleTypeByIdAsync(string id);
    Task<List<VehicleTypeDto>> GetAllVehicleTypesAsync();
    Task<List<VehicleTypeDto>> GetActiveVehicleTypesAsync();
    Task DeleteVehicleTypeAsync(string id);

    // Vehicle Registration Management
    Task<VehicleRegistrationDto> CreateVehicleRegistrationAsync(CreateVehicleRegistrationRequest request);
    Task<VehicleRegistrationDto> UpdateVehicleRegistrationAsync(string id, UpdateVehicleRegistrationRequest request);
    Task<VehicleRegistrationDto?> GetVehicleRegistrationByIdAsync(string id);
    Task<VehicleRegistrationDto?> GetVehicleRegistrationByVehicleIdAsync(string vehicleId);
    Task<List<VehicleRegistrationDto>> GetExpiringRegistrationsAsync(DateTime beforeDate);
    Task DeleteVehicleRegistrationAsync(string id);

    // Vehicle Specification Management
    Task<VehicleSpecificationDto> CreateVehicleSpecificationAsync(CreateVehicleSpecificationRequest request);
    Task<VehicleSpecificationDto> UpdateVehicleSpecificationAsync(string id, UpdateVehicleSpecificationRequest request);
    Task<VehicleSpecificationDto?> GetVehicleSpecificationByIdAsync(string id);
    Task<VehicleSpecificationDto?> GetVehicleSpecificationByVehicleIdAsync(string vehicleId);
    Task DeleteVehicleSpecificationAsync(string id);

    // Vehicle Document Management
    Task<VehicleDocumentDto> UploadVehicleDocumentAsync(UploadVehicleDocumentRequest request, byte[] fileData, string fileName, string contentType);
    Task<VehicleDocumentDto> UpdateVehicleDocumentAsync(string id, UpdateVehicleDocumentRequest request);
    Task<VehicleDocumentDto?> GetVehicleDocumentByIdAsync(string id);
    Task<List<VehicleDocumentDto>> GetVehicleDocumentsAsync(string vehicleId);
    Task<List<VehicleDocumentDto>> GetVehicleDocumentsByTypeAsync(string vehicleId, DocumentType documentType);
    Task<List<VehicleDocumentDto>> GetExpiringDocumentsAsync(DateTime beforeDate);
    Task<byte[]?> GetDocumentFileAsync(string documentId);
    Task DeleteVehicleDocumentAsync(string id);

    // Vehicle Inspection Management
    Task<VehicleInspectionDto> RecordVehicleInspectionAsync(RecordVehicleInspectionRequest request);
    Task<VehicleInspectionDto> UpdateVehicleInspectionAsync(string id, UpdateVehicleInspectionRequest request);
    Task<VehicleInspectionDto?> GetVehicleInspectionByIdAsync(string id);
    Task<List<VehicleInspectionDto>> GetVehicleInspectionsAsync(string vehicleId);
    Task<VehicleInspectionDto?> GetLatestVehicleInspectionAsync(string vehicleId);
    Task<List<VehicleInspectionDto>> GetDueInspectionsAsync(DateTime beforeDate);
    Task DeleteVehicleInspectionAsync(string id);

    // Vehicle Insurance Management
    Task<VehicleInsuranceDto> CreateVehicleInsuranceAsync(CreateVehicleInsuranceRequest request);
    Task<VehicleInsuranceDto> UpdateVehicleInsuranceAsync(string id, UpdateVehicleInsuranceRequest request);
    Task<VehicleInsuranceDto?> GetVehicleInsuranceByIdAsync(string id);
    Task<List<VehicleInsuranceDto>> GetVehicleInsuranceAsync(string vehicleId);
    Task<VehicleInsuranceDto?> GetActiveVehicleInsuranceAsync(string vehicleId);
    Task<List<VehicleInsuranceDto>> GetExpiringInsuranceAsync(DateTime beforeDate);
    Task DeleteVehicleInsuranceAsync(string id);
}