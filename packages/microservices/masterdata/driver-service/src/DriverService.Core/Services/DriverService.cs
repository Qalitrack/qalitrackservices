using AutoMapper;
using DriverService.Core.DTOs;
using DriverService.Core.Entities;
using DriverService.Core.Interfaces;

namespace DriverService.Core.Services;

public class DriverService : IDriverService
{
    private readonly IDriverRepository _driverRepository;
    private readonly IDriverLicenseRepository _licenseRepository;
    private readonly IDriverProfileRepository _profileRepository;
    private readonly IDriverDocumentRepository _documentRepository;
    private readonly IDriverViolationRepository _violationRepository;
    private readonly IMapper _mapper;

    public DriverService(
        IDriverRepository driverRepository,
        IDriverLicenseRepository licenseRepository,
        IDriverProfileRepository profileRepository,
        IDriverDocumentRepository documentRepository,
        IDriverViolationRepository violationRepository,
        IMapper mapper)
    {
        _driverRepository = driverRepository;
        _licenseRepository = licenseRepository;
        _profileRepository = profileRepository;
        _documentRepository = documentRepository;
        _violationRepository = violationRepository;
        _mapper = mapper;
    }

    // Driver CRUD operations
    public async Task<IEnumerable<DriverDto>> GetAllDriversAsync()
    {
        var drivers = await _driverRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<DriverDto>>(drivers);
    }

    public async Task<DriverDto?> GetDriverByIdAsync(string id)
    {
        var driver = await _driverRepository.GetDriverWithAllDetailsAsync(id);
        return driver != null ? _mapper.Map<DriverDto>(driver) : null;
    }

    public async Task<DriverDto> CreateDriverAsync(CreateDriverDto createDriverDto)
    {
        var driver = _mapper.Map<Driver>(createDriverDto);
        var createdDriver = await _driverRepository.AddAsync(driver);
        return _mapper.Map<DriverDto>(createdDriver);
    }

    public async Task<DriverDto> UpdateDriverAsync(string id, UpdateDriverDto updateDriverDto)
    {
        var existingDriver = await _driverRepository.GetByIdAsync(id);
        if (existingDriver == null)
        {
            throw new ArgumentException($"Driver with ID {id} not found");
        }

        _mapper.Map(updateDriverDto, existingDriver);
        existingDriver.UpdatedAt = DateTime.UtcNow;
        
        var updatedDriver = await _driverRepository.UpdateAsync(existingDriver);
        return _mapper.Map<DriverDto>(updatedDriver);
    }

    public async Task DeleteDriverAsync(string id)
    {
        var driver = await _driverRepository.GetByIdAsync(id);
        if (driver == null)
        {
            throw new ArgumentException($"Driver with ID {id} not found");
        }

        driver.IsDeleted = true;
        driver.UpdatedAt = DateTime.UtcNow;
        await _driverRepository.UpdateAsync(driver);
    }

    // Driver search and filtering
    public async Task<IEnumerable<DriverDto>> GetActiveDriversAsync()
    {
        var drivers = await _driverRepository.GetActiveDriversAsync();
        return _mapper.Map<IEnumerable<DriverDto>>(drivers);
    }

    public async Task<IEnumerable<DriverDto>> GetDriversByStatusAsync(DriverStatus status)
    {
        var drivers = await _driverRepository.GetDriversByStatusAsync(status);
        return _mapper.Map<IEnumerable<DriverDto>>(drivers);
    }

    public async Task<IEnumerable<DriverDto>> SearchDriversAsync(string searchTerm)
    {
        var drivers = await _driverRepository.SearchDriversAsync(searchTerm);
        return _mapper.Map<IEnumerable<DriverDto>>(drivers);
    }

    public async Task<DriverDto?> GetDriverByEmployeeIdAsync(string employeeId)
    {
        var driver = await _driverRepository.GetDriverByEmployeeIdAsync(employeeId);
        return driver != null ? _mapper.Map<DriverDto>(driver) : null;
    }

    public async Task<DriverDto?> GetDriverByEmailAsync(string email)
    {
        var driver = await _driverRepository.GetDriverByEmailAsync(email);
        return driver != null ? _mapper.Map<DriverDto>(driver) : null;
    }

    // License management
    public async Task<DriverLicenseDto?> GetDriverLicenseAsync(string driverId)
    {
        var license = await _licenseRepository.GetByDriverIdAsync(driverId);
        return license != null ? _mapper.Map<DriverLicenseDto>(license) : null;
    }

    public async Task<DriverLicenseDto> CreateDriverLicenseAsync(CreateDriverLicenseDto createLicenseDto)
    {
        var license = _mapper.Map<DriverLicense>(createLicenseDto);
        var createdLicense = await _licenseRepository.AddAsync(license);
        return _mapper.Map<DriverLicenseDto>(createdLicense);
    }

    public async Task<DriverLicenseDto> UpdateDriverLicenseAsync(string driverId, UpdateDriverLicenseDto updateLicenseDto)
    {
        var existingLicense = await _licenseRepository.GetByDriverIdAsync(driverId);
        if (existingLicense == null)
        {
            throw new ArgumentException($"License for driver {driverId} not found");
        }

        _mapper.Map(updateLicenseDto, existingLicense);
        existingLicense.UpdatedAt = DateTime.UtcNow;
        
        var updatedLicense = await _licenseRepository.UpdateAsync(existingLicense);
        return _mapper.Map<DriverLicenseDto>(updatedLicense);
    }

    public async Task<IEnumerable<DriverDto>> GetDriversWithExpiringLicensesAsync(int daysAhead = 30)
    {
        var drivers = await _driverRepository.GetDriversWithExpiringLicensesAsync(daysAhead);
        return _mapper.Map<IEnumerable<DriverDto>>(drivers);
    }

    public async Task<LicenseValidationResult> ValidateLicenseAsync(string driverId)
    {
        var license = await _licenseRepository.GetByDriverIdAsync(driverId);
        if (license == null)
        {
            return new LicenseValidationResult
            {
                IsValid = false,
                ValidationMessage = "License not found"
            };
        }

        var isExpired = license.ExpiryDate < DateTime.UtcNow;
        var daysToExpiry = (license.ExpiryDate - DateTime.UtcNow).Days;

        return new LicenseValidationResult
        {
            IsValid = license.Status == LicenseStatus.Active && !isExpired,
            ExpiryDate = license.ExpiryDate,
            DaysToExpiry = daysToExpiry,
            Categories = license.Categories,
            Status = license.Status,
            ValidationMessage = isExpired ? "License has expired" : 
                               daysToExpiry <= 30 ? "License expires soon" : 
                               license.Status != LicenseStatus.Active ? "License is not active" : 
                               "License is valid"
        };
    }

    // Driver profile management
    public async Task<DriverProfileDto?> GetDriverProfileAsync(string driverId)
    {
        var profile = await _profileRepository.GetByDriverIdAsync(driverId);
        return profile != null ? _mapper.Map<DriverProfileDto>(profile) : null;
    }

    public async Task<DriverProfileDto> CreateDriverProfileAsync(CreateDriverProfileDto createProfileDto)
    {
        var profile = _mapper.Map<DriverProfile>(createProfileDto);
        var createdProfile = await _profileRepository.AddAsync(profile);
        return _mapper.Map<DriverProfileDto>(createdProfile);
    }

    public async Task<DriverProfileDto> UpdateDriverProfileAsync(string driverId, UpdateDriverProfileDto updateProfileDto)
    {
        var existingProfile = await _profileRepository.GetByDriverIdAsync(driverId);
        if (existingProfile == null)
        {
            throw new ArgumentException($"Profile for driver {driverId} not found");
        }

        _mapper.Map(updateProfileDto, existingProfile);
        existingProfile.UpdatedAt = DateTime.UtcNow;
        
        var updatedProfile = await _profileRepository.UpdateAsync(existingProfile);
        return _mapper.Map<DriverProfileDto>(updatedProfile);
    }

    // Document management
    public async Task<IEnumerable<DriverDocumentDto>> GetDriverDocumentsAsync(string driverId)
    {
        var documents = await _documentRepository.GetByDriverIdAsync(driverId);
        return _mapper.Map<IEnumerable<DriverDocumentDto>>(documents);
    }

    public async Task<DriverDocumentDto> AddDriverDocumentAsync(CreateDriverDocumentDto createDocumentDto)
    {
        var document = _mapper.Map<DriverDocument>(createDocumentDto);
        var createdDocument = await _documentRepository.AddAsync(document);
        return _mapper.Map<DriverDocumentDto>(createdDocument);
    }

    public async Task<DriverDocumentDto> UpdateDriverDocumentAsync(string documentId, UpdateDriverDocumentDto updateDocumentDto)
    {
        var existingDocument = await _documentRepository.GetByIdAsync(documentId);
        if (existingDocument == null)
        {
            throw new ArgumentException($"Document with ID {documentId} not found");
        }

        _mapper.Map(updateDocumentDto, existingDocument);
        existingDocument.UpdatedAt = DateTime.UtcNow;
        
        var updatedDocument = await _documentRepository.UpdateAsync(existingDocument);
        return _mapper.Map<DriverDocumentDto>(updatedDocument);
    }

    public async Task DeleteDriverDocumentAsync(string documentId)
    {
        var document = await _documentRepository.GetByIdAsync(documentId);
        if (document == null)
        {
            throw new ArgumentException($"Document with ID {documentId} not found");
        }

        document.IsDeleted = true;
        document.UpdatedAt = DateTime.UtcNow;
        await _documentRepository.UpdateAsync(document);
    }

    // Violation management
    public async Task<IEnumerable<DriverViolationDto>> GetDriverViolationsAsync(string driverId)
    {
        var violations = await _violationRepository.GetByDriverIdAsync(driverId);
        return _mapper.Map<IEnumerable<DriverViolationDto>>(violations);
    }

    public async Task<DriverViolationDto> AddDriverViolationAsync(CreateDriverViolationDto createViolationDto)
    {
        var violation = _mapper.Map<DriverViolation>(createViolationDto);
        var createdViolation = await _violationRepository.AddAsync(violation);
        return _mapper.Map<DriverViolationDto>(createdViolation);
    }

    public async Task<DriverViolationDto> UpdateDriverViolationAsync(string violationId, UpdateDriverViolationDto updateViolationDto)
    {
        var existingViolation = await _violationRepository.GetByIdAsync(violationId);
        if (existingViolation == null)
        {
            throw new ArgumentException($"Violation with ID {violationId} not found");
        }

        _mapper.Map(updateViolationDto, existingViolation);
        existingViolation.UpdatedAt = DateTime.UtcNow;
        
        var updatedViolation = await _violationRepository.UpdateAsync(existingViolation);
        return _mapper.Map<DriverViolationDto>(updatedViolation);
    }

    public async Task DeleteDriverViolationAsync(string violationId)
    {
        var violation = await _violationRepository.GetByIdAsync(violationId);
        if (violation == null)
        {
            throw new ArgumentException($"Violation with ID {violationId} not found");
        }

        violation.IsDeleted = true;
        violation.UpdatedAt = DateTime.UtcNow;
        await _violationRepository.UpdateAsync(violation);
    }

    // Biometric management
    public async Task RegisterBiometricAsync(string driverId, BiometricRegistrationDto biometricDto)
    {
        var driver = await _driverRepository.GetByIdAsync(driverId);
        if (driver == null)
        {
            throw new ArgumentException($"Driver with ID {driverId} not found");
        }

        // Register biometric data based on type
        switch (biometricDto.BiometricType.ToLower())
        {
            case "fingerprint":
                driver.FingerprintData = biometricDto.BiometricData;
                break;
            case "face":
                driver.FaceRecognitionData = biometricDto.BiometricData;
                break;
            default:
                throw new ArgumentException($"Unsupported biometric type: {biometricDto.BiometricType}");
        }

        driver.BiometricRegistrationDate = DateTime.UtcNow;
        driver.BiometricEnabled = true;
        driver.UpdatedAt = DateTime.UtcNow;

        await _driverRepository.UpdateAsync(driver);
    }

    public async Task<BiometricVerificationResultDto> VerifyBiometricAsync(string driverId, BiometricVerificationDto verificationDto)
    {
        var driver = await _driverRepository.GetByIdAsync(driverId);
        if (driver == null)
        {
            throw new ArgumentException($"Driver with ID {driverId} not found");
        }

        if (!driver.BiometricEnabled)
        {
            return new BiometricVerificationResultDto
            {
                IsMatch = false,
                ConfidenceScore = 0.0,
                BiometricType = verificationDto.BiometricType,
                VerificationTimestamp = DateTime.UtcNow,
                Notes = "Biometric authentication not enabled for this driver"
            };
        }

        // Simple verification logic (in real implementation, use proper biometric libraries)
        string storedBiometricData = verificationDto.BiometricType.ToLower() switch
        {
            "fingerprint" => driver.FingerprintData ?? "",
            "face" => driver.FaceRecognitionData ?? "",
            _ => ""
        };

        if (string.IsNullOrEmpty(storedBiometricData))
        {
            return new BiometricVerificationResultDto
            {
                IsMatch = false,
                ConfidenceScore = 0.0,
                BiometricType = verificationDto.BiometricType,
                VerificationTimestamp = DateTime.UtcNow,
                Notes = $"No {verificationDto.BiometricType} data registered for this driver"
            };
        }

        // Simple comparison (in real implementation, use proper biometric matching algorithms)
        bool isMatch = storedBiometricData == verificationDto.BiometricData;
        double confidenceScore = isMatch ? 0.95 : 0.0;

        return new BiometricVerificationResultDto
        {
            IsMatch = isMatch,
            ConfidenceScore = confidenceScore,
            BiometricType = verificationDto.BiometricType,
            VerificationTimestamp = DateTime.UtcNow,
            Notes = isMatch ? "Biometric match successful" : "Biometric match failed"
        };
    }

    public async Task UpdateBiometricSettingsAsync(string driverId, BiometricSettingsDto settingsDto)
    {
        var driver = await _driverRepository.GetByIdAsync(driverId);
        if (driver == null)
        {
            throw new ArgumentException($"Driver with ID {driverId} not found");
        }

        driver.BiometricEnabled = settingsDto.BiometricEnabled;
        
        // If biometric is disabled, optionally clear biometric data
        if (!settingsDto.BiometricEnabled)
        {
            if (!settingsDto.FingerprintEnabled)
            {
                driver.FingerprintData = null;
            }
            if (!settingsDto.FaceRecognitionEnabled)
            {
                driver.FaceRecognitionData = null;
            }
        }

        driver.UpdatedAt = DateTime.UtcNow;
        await _driverRepository.UpdateAsync(driver);
    }
}