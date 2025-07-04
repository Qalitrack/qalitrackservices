using AutoMapper;
using VehicleService.Core.DTOs;
using VehicleService.Core.Entities;
using VehicleService.Core.Interfaces;

namespace VehicleService.Core.Services;

public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IVehicleTypeRepository _vehicleTypeRepository;
    private readonly IVehicleRegistrationRepository _vehicleRegistrationRepository;
    private readonly IVehicleSpecificationRepository _vehicleSpecificationRepository;
    private readonly IVehicleDocumentRepository _vehicleDocumentRepository;
    private readonly IVehicleInspectionRepository _vehicleInspectionRepository;
    private readonly IVehicleInsuranceRepository _vehicleInsuranceRepository;
    private readonly IMapper _mapper;

    public VehicleService(
        IVehicleRepository vehicleRepository,
        IVehicleTypeRepository vehicleTypeRepository,
        IVehicleRegistrationRepository vehicleRegistrationRepository,
        IVehicleSpecificationRepository vehicleSpecificationRepository,
        IVehicleDocumentRepository vehicleDocumentRepository,
        IVehicleInspectionRepository vehicleInspectionRepository,
        IVehicleInsuranceRepository vehicleInsuranceRepository,
        IMapper mapper)
    {
        _vehicleRepository = vehicleRepository;
        _vehicleTypeRepository = vehicleTypeRepository;
        _vehicleRegistrationRepository = vehicleRegistrationRepository;
        _vehicleSpecificationRepository = vehicleSpecificationRepository;
        _vehicleDocumentRepository = vehicleDocumentRepository;
        _vehicleInspectionRepository = vehicleInspectionRepository;
        _vehicleInsuranceRepository = vehicleInsuranceRepository;
        _mapper = mapper;
    }

    #region Vehicle Management

    public async Task<VehicleDto> RegisterVehicleAsync(RegisterVehicleRequest request)
    {
        // Validate registration number uniqueness
        if (!await ValidateRegistrationNumberAsync(request.RegistrationNumber))
        {
            throw new InvalidOperationException($"Registration number '{request.RegistrationNumber}' already exists.");
        }

        // Validate VIN uniqueness if provided
        if (!string.IsNullOrEmpty(request.VIN) && !await ValidateVINAsync(request.VIN))
        {
            throw new InvalidOperationException($"VIN '{request.VIN}' already exists.");
        }

        // Validate vehicle type exists
        var vehicleType = await _vehicleTypeRepository.GetByIdAsync(request.VehicleTypeId);
        if (vehicleType == null)
        {
            throw new InvalidOperationException($"Vehicle type with ID '{request.VehicleTypeId}' not found.");
        }

        var vehicle = _mapper.Map<Vehicle>(request);
        vehicle.Id = Guid.NewGuid().ToString();
        vehicle.CreatedAt = DateTime.UtcNow;
        vehicle.Status = VehicleStatus.Active;

        var createdVehicle = await _vehicleRepository.AddAsync(vehicle);
        return _mapper.Map<VehicleDto>(createdVehicle);
    }

    public async Task<VehicleDto> UpdateVehicleAsync(string id, UpdateVehicleRequest request)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle == null)
        {
            throw new InvalidOperationException($"Vehicle with ID '{id}' not found.");
        }

        _mapper.Map(request, vehicle);
        vehicle.UpdatedAt = DateTime.UtcNow;

        var updatedVehicle = await _vehicleRepository.UpdateAsync(vehicle);
        return _mapper.Map<VehicleDto>(updatedVehicle);
    }

    public async Task<VehicleDto?> GetVehicleByIdAsync(string id)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        return vehicle != null ? _mapper.Map<VehicleDto>(vehicle) : null;
    }

    public async Task<VehicleDto?> GetVehicleByRegistrationNumberAsync(string registrationNumber)
    {
        var vehicle = await _vehicleRepository.GetByRegistrationNumberAsync(registrationNumber);
        return vehicle != null ? _mapper.Map<VehicleDto>(vehicle) : null;
    }

    public async Task<VehicleDto?> GetVehicleByVINAsync(string vin)
    {
        var vehicle = await _vehicleRepository.GetByVINAsync(vin);
        return vehicle != null ? _mapper.Map<VehicleDto>(vehicle) : null;
    }

    public async Task<List<VehicleDto>> GetAllVehiclesAsync()
    {
        var vehicles = await _vehicleRepository.GetAllAsync();
        return _mapper.Map<List<VehicleDto>>(vehicles);
    }

    public async Task<List<VehicleDto>> GetVehiclesByStatusAsync(VehicleStatus status)
    {
        var vehicles = await _vehicleRepository.GetByStatusAsync(status);
        return _mapper.Map<List<VehicleDto>>(vehicles);
    }

    public async Task<List<VehicleDto>> GetVehiclesByOwnerAsync(string ownerName)
    {
        var vehicles = await _vehicleRepository.GetByOwnerAsync(ownerName);
        return _mapper.Map<List<VehicleDto>>(vehicles);
    }

    public async Task<VehicleDto?> GetVehicleWithDetailsAsync(string id)
    {
        var vehicle = await _vehicleRepository.GetWithDetailsAsync(id);
        return vehicle != null ? _mapper.Map<VehicleDto>(vehicle) : null;
    }

    public async Task DeleteVehicleAsync(string id)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle == null)
        {
            throw new InvalidOperationException($"Vehicle with ID '{id}' not found.");
        }

        await _vehicleRepository.DeleteAsync(id);
    }

    public async Task<bool> ValidateRegistrationNumberAsync(string registrationNumber, string? excludeId = null)
    {
        return await _vehicleRepository.IsRegistrationNumberUniqueAsync(registrationNumber, excludeId);
    }

    public async Task<bool> ValidateVINAsync(string vin, string? excludeId = null)
    {
        return await _vehicleRepository.IsVINUniqueAsync(vin, excludeId);
    }

    #endregion

    #region Vehicle Type Management

    public async Task<VehicleTypeDto> CreateVehicleTypeAsync(CreateVehicleTypeRequest request)
    {
        // Validate name uniqueness
        var existingType = await _vehicleTypeRepository.GetByNameAsync(request.Name);
        if (existingType != null)
        {
            throw new InvalidOperationException($"Vehicle type with name '{request.Name}' already exists.");
        }

        var vehicleType = _mapper.Map<VehicleType>(request);
        vehicleType.Id = Guid.NewGuid().ToString();
        vehicleType.CreatedAt = DateTime.UtcNow;
        vehicleType.IsActive = true;

        var createdType = await _vehicleTypeRepository.AddAsync(vehicleType);
        return _mapper.Map<VehicleTypeDto>(createdType);
    }

    public async Task<VehicleTypeDto> UpdateVehicleTypeAsync(string id, UpdateVehicleTypeRequest request)
    {
        var vehicleType = await _vehicleTypeRepository.GetByIdAsync(id);
        if (vehicleType == null)
        {
            throw new InvalidOperationException($"Vehicle type with ID '{id}' not found.");
        }

        // Validate name uniqueness if changed
        if (vehicleType.Name != request.Name)
        {
            var existingType = await _vehicleTypeRepository.GetByNameAsync(request.Name);
            if (existingType != null && existingType.Id != id)
            {
                throw new InvalidOperationException($"Vehicle type with name '{request.Name}' already exists.");
            }
        }

        _mapper.Map(request, vehicleType);
        vehicleType.UpdatedAt = DateTime.UtcNow;

        var updatedType = await _vehicleTypeRepository.UpdateAsync(vehicleType);
        return _mapper.Map<VehicleTypeDto>(updatedType);
    }

    public async Task<VehicleTypeDto?> GetVehicleTypeByIdAsync(string id)
    {
        var vehicleType = await _vehicleTypeRepository.GetByIdAsync(id);
        return vehicleType != null ? _mapper.Map<VehicleTypeDto>(vehicleType) : null;
    }

    public async Task<List<VehicleTypeDto>> GetAllVehicleTypesAsync()
    {
        var vehicleTypes = await _vehicleTypeRepository.GetAllAsync();
        return _mapper.Map<List<VehicleTypeDto>>(vehicleTypes);
    }

    public async Task<List<VehicleTypeDto>> GetActiveVehicleTypesAsync()
    {
        var vehicleTypes = await _vehicleTypeRepository.GetActiveTypesAsync();
        return _mapper.Map<List<VehicleTypeDto>>(vehicleTypes);
    }

    public async Task DeleteVehicleTypeAsync(string id)
    {
        var vehicleType = await _vehicleTypeRepository.GetByIdAsync(id);
        if (vehicleType == null)
        {
            throw new InvalidOperationException($"Vehicle type with ID '{id}' not found.");
        }

        await _vehicleTypeRepository.DeleteAsync(id);
    }

    #endregion

    #region Vehicle Registration Management

    public async Task<VehicleRegistrationDto> CreateVehicleRegistrationAsync(CreateVehicleRegistrationRequest request)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
        if (vehicle == null)
        {
            throw new InvalidOperationException($"Vehicle with ID '{request.VehicleId}' not found.");
        }

        var registration = _mapper.Map<VehicleRegistration>(request);
        registration.Id = Guid.NewGuid().ToString();
        registration.CreatedAt = DateTime.UtcNow;
        registration.IsActive = true;

        var createdRegistration = await _vehicleRegistrationRepository.AddAsync(registration);
        return _mapper.Map<VehicleRegistrationDto>(createdRegistration);
    }

    public async Task<VehicleRegistrationDto> UpdateVehicleRegistrationAsync(string id, UpdateVehicleRegistrationRequest request)
    {
        var registration = await _vehicleRegistrationRepository.GetByIdAsync(id);
        if (registration == null)
        {
            throw new InvalidOperationException($"Vehicle registration with ID '{id}' not found.");
        }

        _mapper.Map(request, registration);
        registration.UpdatedAt = DateTime.UtcNow;

        var updatedRegistration = await _vehicleRegistrationRepository.UpdateAsync(registration);
        return _mapper.Map<VehicleRegistrationDto>(updatedRegistration);
    }

    public async Task<VehicleRegistrationDto?> GetVehicleRegistrationByIdAsync(string id)
    {
        var registration = await _vehicleRegistrationRepository.GetByIdAsync(id);
        return registration != null ? _mapper.Map<VehicleRegistrationDto>(registration) : null;
    }

    public async Task<VehicleRegistrationDto?> GetVehicleRegistrationByVehicleIdAsync(string vehicleId)
    {
        var registration = await _vehicleRegistrationRepository.GetByVehicleIdAsync(vehicleId);
        return registration != null ? _mapper.Map<VehicleRegistrationDto>(registration) : null;
    }

    public async Task<List<VehicleRegistrationDto>> GetExpiringRegistrationsAsync(DateTime beforeDate)
    {
        var registrations = await _vehicleRegistrationRepository.GetExpiringRegistrationsAsync(beforeDate);
        return _mapper.Map<List<VehicleRegistrationDto>>(registrations);
    }

    public async Task DeleteVehicleRegistrationAsync(string id)
    {
        await _vehicleRegistrationRepository.DeleteAsync(id);
    }

    #endregion

    #region Vehicle Specification Management

    public async Task<VehicleSpecificationDto> CreateVehicleSpecificationAsync(CreateVehicleSpecificationRequest request)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
        if (vehicle == null)
        {
            throw new InvalidOperationException($"Vehicle with ID '{request.VehicleId}' not found.");
        }

        var specification = _mapper.Map<VehicleSpecification>(request);
        specification.Id = Guid.NewGuid().ToString();
        specification.CreatedAt = DateTime.UtcNow;

        var createdSpecification = await _vehicleSpecificationRepository.AddAsync(specification);
        return _mapper.Map<VehicleSpecificationDto>(createdSpecification);
    }

    public async Task<VehicleSpecificationDto> UpdateVehicleSpecificationAsync(string id, UpdateVehicleSpecificationRequest request)
    {
        var specification = await _vehicleSpecificationRepository.GetByIdAsync(id);
        if (specification == null)
        {
            throw new InvalidOperationException($"Vehicle specification with ID '{id}' not found.");
        }

        _mapper.Map(request, specification);
        specification.UpdatedAt = DateTime.UtcNow;

        var updatedSpecification = await _vehicleSpecificationRepository.UpdateAsync(specification);
        return _mapper.Map<VehicleSpecificationDto>(updatedSpecification);
    }

    public async Task<VehicleSpecificationDto?> GetVehicleSpecificationByIdAsync(string id)
    {
        var specification = await _vehicleSpecificationRepository.GetByIdAsync(id);
        return specification != null ? _mapper.Map<VehicleSpecificationDto>(specification) : null;
    }

    public async Task<VehicleSpecificationDto?> GetVehicleSpecificationByVehicleIdAsync(string vehicleId)
    {
        var specification = await _vehicleSpecificationRepository.GetByVehicleIdAsync(vehicleId);
        return specification != null ? _mapper.Map<VehicleSpecificationDto>(specification) : null;
    }

    public async Task DeleteVehicleSpecificationAsync(string id)
    {
        await _vehicleSpecificationRepository.DeleteAsync(id);
    }

    #endregion

    #region Vehicle Document Management

    public async Task<VehicleDocumentDto> UploadVehicleDocumentAsync(UploadVehicleDocumentRequest request, byte[] fileData, string fileName, string contentType)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
        if (vehicle == null)
        {
            throw new InvalidOperationException($"Vehicle with ID '{request.VehicleId}' not found.");
        }

        // Generate file path (implementation would depend on storage strategy)
        var fileExtension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine("uploads", "vehicle-documents", uniqueFileName);

        // Save file (this is a simplified implementation)
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        await File.WriteAllBytesAsync(filePath, fileData);

        var document = _mapper.Map<VehicleDocument>(request);
        document.Id = Guid.NewGuid().ToString();
        document.FilePath = filePath;
        document.FileName = fileName;
        document.FileType = contentType;
        document.FileSize = fileData.Length;
        document.CreatedAt = DateTime.UtcNow;
        document.IsActive = true;

        var createdDocument = await _vehicleDocumentRepository.AddAsync(document);
        return _mapper.Map<VehicleDocumentDto>(createdDocument);
    }

    public async Task<VehicleDocumentDto> UpdateVehicleDocumentAsync(string id, UpdateVehicleDocumentRequest request)
    {
        var document = await _vehicleDocumentRepository.GetByIdAsync(id);
        if (document == null)
        {
            throw new InvalidOperationException($"Vehicle document with ID '{id}' not found.");
        }

        _mapper.Map(request, document);
        document.UpdatedAt = DateTime.UtcNow;

        var updatedDocument = await _vehicleDocumentRepository.UpdateAsync(document);
        return _mapper.Map<VehicleDocumentDto>(updatedDocument);
    }

    public async Task<VehicleDocumentDto?> GetVehicleDocumentByIdAsync(string id)
    {
        var document = await _vehicleDocumentRepository.GetByIdAsync(id);
        return document != null ? _mapper.Map<VehicleDocumentDto>(document) : null;
    }

    public async Task<List<VehicleDocumentDto>> GetVehicleDocumentsAsync(string vehicleId)
    {
        var documents = await _vehicleDocumentRepository.GetByVehicleIdAsync(vehicleId);
        return _mapper.Map<List<VehicleDocumentDto>>(documents);
    }

    public async Task<List<VehicleDocumentDto>> GetVehicleDocumentsByTypeAsync(string vehicleId, DocumentType documentType)
    {
        var documents = await _vehicleDocumentRepository.GetByDocumentTypeAsync(vehicleId, documentType);
        return _mapper.Map<List<VehicleDocumentDto>>(documents);
    }

    public async Task<List<VehicleDocumentDto>> GetExpiringDocumentsAsync(DateTime beforeDate)
    {
        var documents = await _vehicleDocumentRepository.GetExpiringDocumentsAsync(beforeDate);
        return _mapper.Map<List<VehicleDocumentDto>>(documents);
    }

    public async Task<byte[]?> GetDocumentFileAsync(string documentId)
    {
        var document = await _vehicleDocumentRepository.GetByIdAsync(documentId);
        if (document == null || !File.Exists(document.FilePath))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(document.FilePath);
    }

    public async Task DeleteVehicleDocumentAsync(string id)
    {
        var document = await _vehicleDocumentRepository.GetByIdAsync(id);
        if (document != null && File.Exists(document.FilePath))
        {
            File.Delete(document.FilePath);
        }
        
        await _vehicleDocumentRepository.DeleteAsync(id);
    }

    #endregion

    #region Vehicle Inspection Management

    public async Task<VehicleInspectionDto> RecordVehicleInspectionAsync(RecordVehicleInspectionRequest request)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
        if (vehicle == null)
        {
            throw new InvalidOperationException($"Vehicle with ID '{request.VehicleId}' not found.");
        }

        var inspection = _mapper.Map<VehicleInspection>(request);
        inspection.Id = Guid.NewGuid().ToString();
        inspection.CreatedAt = DateTime.UtcNow;
        inspection.IsValid = true;

        // Update vehicle's inspection dates
        vehicle.LastInspectionDate = request.InspectionDate;
        vehicle.NextInspectionDue = request.NextInspectionDue;
        vehicle.CurrentMileage = request.CurrentMileage;
        vehicle.UpdatedAt = DateTime.UtcNow;
        await _vehicleRepository.UpdateAsync(vehicle);

        var createdInspection = await _vehicleInspectionRepository.AddAsync(inspection);
        return _mapper.Map<VehicleInspectionDto>(createdInspection);
    }

    public async Task<VehicleInspectionDto> UpdateVehicleInspectionAsync(string id, UpdateVehicleInspectionRequest request)
    {
        var inspection = await _vehicleInspectionRepository.GetByIdAsync(id);
        if (inspection == null)
        {
            throw new InvalidOperationException($"Vehicle inspection with ID '{id}' not found.");
        }

        _mapper.Map(request, inspection);
        inspection.UpdatedAt = DateTime.UtcNow;

        var updatedInspection = await _vehicleInspectionRepository.UpdateAsync(inspection);
        return _mapper.Map<VehicleInspectionDto>(updatedInspection);
    }

    public async Task<VehicleInspectionDto?> GetVehicleInspectionByIdAsync(string id)
    {
        var inspection = await _vehicleInspectionRepository.GetByIdAsync(id);
        return inspection != null ? _mapper.Map<VehicleInspectionDto>(inspection) : null;
    }

    public async Task<List<VehicleInspectionDto>> GetVehicleInspectionsAsync(string vehicleId)
    {
        var inspections = await _vehicleInspectionRepository.GetByVehicleIdAsync(vehicleId);
        return _mapper.Map<List<VehicleInspectionDto>>(inspections);
    }

    public async Task<VehicleInspectionDto?> GetLatestVehicleInspectionAsync(string vehicleId)
    {
        var inspection = await _vehicleInspectionRepository.GetLatestInspectionAsync(vehicleId);
        return inspection != null ? _mapper.Map<VehicleInspectionDto>(inspection) : null;
    }

    public async Task<List<VehicleInspectionDto>> GetDueInspectionsAsync(DateTime beforeDate)
    {
        var inspections = await _vehicleInspectionRepository.GetDueInspectionsAsync(beforeDate);
        return _mapper.Map<List<VehicleInspectionDto>>(inspections);
    }

    public async Task DeleteVehicleInspectionAsync(string id)
    {
        await _vehicleInspectionRepository.DeleteAsync(id);
    }

    #endregion

    #region Vehicle Insurance Management

    public async Task<VehicleInsuranceDto> CreateVehicleInsuranceAsync(CreateVehicleInsuranceRequest request)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
        if (vehicle == null)
        {
            throw new InvalidOperationException($"Vehicle with ID '{request.VehicleId}' not found.");
        }

        var insurance = _mapper.Map<VehicleInsurance>(request);
        insurance.Id = Guid.NewGuid().ToString();
        insurance.CreatedAt = DateTime.UtcNow;
        insurance.IsActive = true;

        // Update vehicle's insurance expiry date
        vehicle.InsuranceExpiryDate = request.EndDate;
        vehicle.UpdatedAt = DateTime.UtcNow;
        await _vehicleRepository.UpdateAsync(vehicle);

        var createdInsurance = await _vehicleInsuranceRepository.AddAsync(insurance);
        return _mapper.Map<VehicleInsuranceDto>(createdInsurance);
    }

    public async Task<VehicleInsuranceDto> UpdateVehicleInsuranceAsync(string id, UpdateVehicleInsuranceRequest request)
    {
        var insurance = await _vehicleInsuranceRepository.GetByIdAsync(id);
        if (insurance == null)
        {
            throw new InvalidOperationException($"Vehicle insurance with ID '{id}' not found.");
        }

        _mapper.Map(request, insurance);
        insurance.UpdatedAt = DateTime.UtcNow;

        var updatedInsurance = await _vehicleInsuranceRepository.UpdateAsync(insurance);
        return _mapper.Map<VehicleInsuranceDto>(updatedInsurance);
    }

    public async Task<VehicleInsuranceDto?> GetVehicleInsuranceByIdAsync(string id)
    {
        var insurance = await _vehicleInsuranceRepository.GetByIdAsync(id);
        return insurance != null ? _mapper.Map<VehicleInsuranceDto>(insurance) : null;
    }

    public async Task<List<VehicleInsuranceDto>> GetVehicleInsuranceAsync(string vehicleId)
    {
        var insurance = await _vehicleInsuranceRepository.GetByVehicleIdAsync(vehicleId);
        return _mapper.Map<List<VehicleInsuranceDto>>(insurance);
    }

    public async Task<VehicleInsuranceDto?> GetActiveVehicleInsuranceAsync(string vehicleId)
    {
        var insurance = await _vehicleInsuranceRepository.GetActiveInsuranceAsync(vehicleId);
        return insurance != null ? _mapper.Map<VehicleInsuranceDto>(insurance) : null;
    }

    public async Task<List<VehicleInsuranceDto>> GetExpiringInsuranceAsync(DateTime beforeDate)
    {
        var insurance = await _vehicleInsuranceRepository.GetExpiringInsuranceAsync(beforeDate);
        return _mapper.Map<List<VehicleInsuranceDto>>(insurance);
    }

    public async Task DeleteVehicleInsuranceAsync(string id)
    {
        await _vehicleInsuranceRepository.DeleteAsync(id);
    }

    #endregion
}