using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.Driver.Entities;
using QaliTrack.MasterData.Core.Modules.Driver.DTOs;
using QaliTrack.MasterData.Infrastructure.Data;

namespace QaliTrack.MasterData.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Tags("Driver Module")]
public class DriverController : ControllerBase
{
    private readonly MasterDataDbContext _context;

    public DriverController(MasterDataDbContext context)
    {
        _context = context;
    }

    #region Driver CRUD

    /// <summary>
    /// Get drivers with pagination and filtering - Returns summary view only
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<DriverSummaryDto>>>> GetDrivers(
        [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.Drivers
                .Select(d => new DriverSummaryDto
                {
                    Id = d.Id,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    FullName = d.FullName,
                    EmployeeId = d.EmployeeId,
                    Email = d.Email,
                    PhoneNumber = d.PhoneNumber,
                    Status = d.Status,
                    Department = d.Department ?? string.Empty,
                    HireDate = d.HireDate,
                    HasLicense = d.License != null,
                    BiometricEnabled = d.BiometricEnabled,
                    CreatedAt = d.CreatedAt
                })
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<DriverSummaryDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<DriverSummaryDto>>.ErrorResponse("Error retrieving drivers", ex.Message));
        }
    }

    /// <summary>
    /// Get driver details by ID - Returns detailed view without child collections
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DriverDetailDto>>> GetDriver(Guid id)
    {
        try
        {
            var driver = await _context.Drivers
                .Include(d => d.License)
                .Include(d => d.Profile)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (driver == null)
            {
                return NotFound(ApiResponse<DriverDetailDto>.ErrorResponse("Driver not found"));
            }

            var dto = new DriverDetailDto
            {
                Id = driver.Id,
                FirstName = driver.FirstName,
                LastName = driver.LastName,
                MiddleName = driver.MiddleName ?? string.Empty,
                FullName = driver.FullName,
                DateOfBirth = driver.DateOfBirth,
                Age = driver.Age,
                Gender = driver.Gender,
                Nationality = driver.Nationality,
                PhoneNumber = driver.PhoneNumber,
                Email = driver.Email,
                Address = driver.Address,
                City = driver.City,
                State = driver.State,
                Country = driver.Country,
                PostalCode = driver.PostalCode,
                EmergencyContactName = driver.EmergencyContactName,
                EmergencyContactPhone = driver.EmergencyContactPhone,
                EmergencyContactRelationship = driver.EmergencyContactRelationship,
                EmployeeId = driver.EmployeeId,
                HireDate = driver.HireDate,
                TerminationDate = driver.TerminationDate,
                EmploymentType = driver.EmploymentType,
                Status = driver.Status,
                Salary = driver.Salary,
                Department = driver.Department ?? string.Empty,
                Supervisor = driver.Supervisor ?? string.Empty,
                ProfilePhotoUrl = driver.ProfilePhotoUrl ?? string.Empty,
                BiometricRegistrationDate = driver.BiometricRegistrationDate,
                BiometricEnabled = driver.BiometricEnabled,
                Notes = driver.Notes ?? string.Empty,
                CreatedAt = driver.CreatedAt,
                UpdatedAt = driver.UpdatedAt,
                HasLicense = driver.License != null,
                HasProfile = driver.Profile != null
            };

            return Ok(ApiResponse<DriverDetailDto>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DriverDetailDto>.ErrorResponse("Error retrieving driver", ex.Message));
        }
    }

    /// <summary>
    /// Create new driver - Only essential fields required
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<DriverDetailDto>>> CreateDriver(CreateDriverDto dto)
    {
        try
        {
            var driver = new Driver
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                MiddleName = dto.MiddleName,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender ?? string.Empty,
                Nationality = dto.Nationality ?? string.Empty,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                Address = dto.Address,
                City = dto.City ?? string.Empty,
                Country = dto.Country ?? string.Empty,
                EmployeeId = dto.EmployeeId,
                HireDate = dto.HireDate,
                EmploymentType = dto.EmploymentType ?? "FullTime",
                Status = "Active",
                OrganizationId = dto.OrganizationId
            };

            _context.Drivers.Add(driver);
            await _context.SaveChangesAsync();

            var detailDto = new DriverDetailDto
            {
                Id = driver.Id,
                FirstName = driver.FirstName,
                LastName = driver.LastName,
                MiddleName = driver.MiddleName ?? string.Empty,
                FullName = driver.FullName,
                DateOfBirth = driver.DateOfBirth,
                Age = driver.Age,
                Gender = driver.Gender,
                Nationality = driver.Nationality,
                PhoneNumber = driver.PhoneNumber,
                Email = driver.Email,
                Address = driver.Address,
                City = driver.City,
                Country = driver.Country,
                EmployeeId = driver.EmployeeId,
                HireDate = driver.HireDate,
                EmploymentType = driver.EmploymentType,
                Status = driver.Status,
                CreatedAt = driver.CreatedAt,
                UpdatedAt = driver.UpdatedAt,
                HasLicense = false,
                HasProfile = false
            };

            return CreatedAtAction(nameof(GetDriver), 
                new { id = driver.Id }, 
                ApiResponse<DriverDetailDto>.SuccessResponse(detailDto, "Driver created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DriverDetailDto>.ErrorResponse("Error creating driver", ex.Message));
        }
    }

    /// <summary>
    /// Update driver basic information
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<DriverDetailDto>>> UpdateDriver(Guid id, UpdateDriverDto dto)
    {
        try
        {
            var driver = await _context.Drivers.FindAsync(id);
            if (driver == null)
            {
                return NotFound(ApiResponse<DriverDetailDto>.ErrorResponse("Driver not found"));
            }

            driver.FirstName = dto.FirstName;
            driver.LastName = dto.LastName;
            driver.MiddleName = dto.MiddleName;
            driver.PhoneNumber = dto.PhoneNumber;
            driver.Email = dto.Email;
            driver.Address = dto.Address;
            driver.City = dto.City ?? string.Empty;
            driver.State = dto.State ?? string.Empty;
            driver.Country = dto.Country ?? string.Empty;
            driver.PostalCode = dto.PostalCode ?? string.Empty;
            driver.EmergencyContactName = dto.EmergencyContactName ?? string.Empty;
            driver.EmergencyContactPhone = dto.EmergencyContactPhone ?? string.Empty;
            driver.EmergencyContactRelationship = dto.EmergencyContactRelationship ?? string.Empty;
            driver.EmploymentType = dto.EmploymentType ?? driver.EmploymentType;
            driver.Status = dto.Status;
            driver.Salary = dto.Salary;
            driver.Department = dto.Department;
            driver.Supervisor = dto.Supervisor;
            driver.TerminationDate = dto.TerminationDate;
            driver.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var detailDto = new DriverDetailDto
            {
                Id = driver.Id,
                FirstName = driver.FirstName,
                LastName = driver.LastName,
                MiddleName = driver.MiddleName ?? string.Empty,
                FullName = driver.FullName,
                DateOfBirth = driver.DateOfBirth,
                Age = driver.Age,
                Gender = driver.Gender,
                Nationality = driver.Nationality,
                PhoneNumber = driver.PhoneNumber,
                Email = driver.Email,
                Address = driver.Address,
                City = driver.City,
                State = driver.State,
                Country = driver.Country,
                PostalCode = driver.PostalCode,
                EmergencyContactName = driver.EmergencyContactName,
                EmergencyContactPhone = driver.EmergencyContactPhone,
                EmergencyContactRelationship = driver.EmergencyContactRelationship,
                EmployeeId = driver.EmployeeId,
                HireDate = driver.HireDate,
                TerminationDate = driver.TerminationDate,
                EmploymentType = driver.EmploymentType,
                Status = driver.Status,
                Salary = driver.Salary,
                Department = driver.Department ?? string.Empty,
                Supervisor = driver.Supervisor ?? string.Empty,
                Notes = driver.Notes ?? string.Empty,
                CreatedAt = driver.CreatedAt,
                UpdatedAt = driver.UpdatedAt
            };

            return Ok(ApiResponse<DriverDetailDto>.SuccessResponse(detailDto, "Driver updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DriverDetailDto>.ErrorResponse("Error updating driver", ex.Message));
        }
    }

    /// <summary>
    /// Delete driver (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteDriver(Guid id)
    {
        try
        {
            var driver = await _context.Drivers.FindAsync(id);
            if (driver == null)
            {
                return NotFound(ApiResponse.CreateError("Driver not found"));
            }

            driver.IsDeleted = true;
            driver.Status = "Terminated";
            driver.TerminationDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Driver deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting driver", ex.Message));
        }
    }

    #endregion

    #region License Management

    /// <summary>
    /// Get driver license
    /// </summary>
    [HttpGet("{id}/license")]
    public async Task<ActionResult<ApiResponse<DriverLicenseDto>>> GetDriverLicense(Guid id)
    {
        try
        {
            var license = await _context.DriverLicenses
                .FirstOrDefaultAsync(l => l.DriverId == id);

            if (license == null)
            {
                return NotFound(ApiResponse<DriverLicenseDto>.ErrorResponse("Driver license not found"));
            }

            var dto = new DriverLicenseDto
            {
                Id = license.Id,
                DriverId = license.DriverId,
                LicenseNumber = license.LicenseNumber,
                LicenseClass = license.LicenseClass,
                IssueDate = license.IssueDate,
                ExpiryDate = license.ExpiryDate,
                IssuingAuthority = license.IssuingAuthority,
                Restrictions = license.Restrictions,
                Status = license.Status,
                IsExpired = license.ExpiryDate < DateTime.Today,
                DaysUntilExpiry = (license.ExpiryDate - DateTime.Today).Days,
                CreatedAt = license.CreatedAt
            };

            return Ok(ApiResponse<DriverLicenseDto>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DriverLicenseDto>.ErrorResponse("Error retrieving driver license", ex.Message));
        }
    }

    /// <summary>
    /// Create/Update driver license
    /// </summary>
    [HttpPost("{id}/license")]
    public async Task<ActionResult<ApiResponse<DriverLicenseDto>>> CreateDriverLicense(Guid id, CreateDriverLicenseDto dto)
    {
        try
        {
            // Check if driver exists
            var driverExists = await _context.Drivers.AnyAsync(d => d.Id == id);
            if (!driverExists)
            {
                return NotFound(ApiResponse<DriverLicenseDto>.ErrorResponse("Driver not found"));
            }

            // Remove existing license if any
            var existingLicense = await _context.DriverLicenses.FirstOrDefaultAsync(l => l.DriverId == id);
            if (existingLicense != null)
            {
                _context.DriverLicenses.Remove(existingLicense);
            }

            var license = new DriverLicense
            {
                Id = Guid.NewGuid(),
                DriverId = id,
                LicenseNumber = dto.LicenseNumber,
                LicenseClass = dto.LicenseClass,
                IssueDate = dto.IssueDate,
                ExpiryDate = dto.ExpiryDate,
                IssuingAuthority = dto.IssuingAuthority,
                Restrictions = dto.Restrictions,
                Status = "Active"
            };

            _context.DriverLicenses.Add(license);
            await _context.SaveChangesAsync();

            var responseDto = new DriverLicenseDto
            {
                Id = license.Id,
                DriverId = license.DriverId,
                LicenseNumber = license.LicenseNumber,
                LicenseClass = license.LicenseClass,
                IssueDate = license.IssueDate,
                ExpiryDate = license.ExpiryDate,
                IssuingAuthority = license.IssuingAuthority,
                Restrictions = license.Restrictions,
                Status = license.Status,
                IsExpired = license.ExpiryDate < DateTime.Today,
                DaysUntilExpiry = (license.ExpiryDate - DateTime.Today).Days,
                CreatedAt = license.CreatedAt
            };

            return Ok(ApiResponse<DriverLicenseDto>.SuccessResponse(responseDto, "Driver license created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DriverLicenseDto>.ErrorResponse("Error creating driver license", ex.Message));
        }
    }

    #endregion

    #region Training Management

    /// <summary>
    /// Get driver trainings
    /// </summary>
    [HttpGet("{id}/trainings")]
    public async Task<ActionResult<ApiResponse<PagedResult<DriverTrainingDto>>>> GetDriverTrainings(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.DriverTrainings
                .Where(t => t.DriverId == id)
                .Select(t => new DriverTrainingDto
                {
                    Id = t.Id,
                    DriverId = t.DriverId,
                    TrainingType = t.TrainingType,
                    TrainingName = t.TrainingName,
                    TrainingProvider = t.TrainingProvider,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    Status = t.Status,
                    Score = t.Score,
                    IsCertified = t.IsCertified,
                    CertificateNumber = t.CertificateNumber,
                    CertificateExpiryDate = t.CertificateExpiryDate,
                    Cost = t.Cost,
                    Notes = t.Notes,
                    CreatedAt = t.CreatedAt
                })
                .OrderByDescending(t => t.StartDate)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<DriverTrainingDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<DriverTrainingDto>>.ErrorResponse("Error retrieving driver trainings", ex.Message));
        }
    }

    /// <summary>
    /// Add driver training
    /// </summary>
    [HttpPost("{id}/trainings")]
    public async Task<ActionResult<ApiResponse<DriverTrainingDto>>> CreateDriverTraining(Guid id, CreateDriverTrainingDto dto)
    {
        try
        {
            var training = new DriverTraining
            {
                Id = Guid.NewGuid(),
                DriverId = id,
                TrainingType = dto.TrainingType,
                TrainingName = dto.TrainingName,
                TrainingProvider = dto.TrainingProvider,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = dto.Status
            };

            _context.DriverTrainings.Add(training);
            await _context.SaveChangesAsync();

            var responseDto = new DriverTrainingDto
            {
                Id = training.Id,
                DriverId = training.DriverId,
                TrainingType = training.TrainingType,
                TrainingName = training.TrainingName,
                TrainingProvider = training.TrainingProvider,
                StartDate = training.StartDate,
                EndDate = training.EndDate,
                Status = training.Status,
                Score = training.Score,
                IsCertified = training.IsCertified,
                CertificateNumber = training.CertificateNumber,
                CertificateExpiryDate = training.CertificateExpiryDate,
                Cost = training.Cost,
                Notes = training.Notes,
                CreatedAt = training.CreatedAt
            };

            return Ok(ApiResponse<DriverTrainingDto>.SuccessResponse(responseDto, "Driver training created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DriverTrainingDto>.ErrorResponse("Error creating driver training", ex.Message));
        }
    }

    #endregion

    #region Medical Records Management

    /// <summary>
    /// Get driver medical records
    /// </summary>
    [HttpGet("{id}/medical-records")]
    public async Task<ActionResult<ApiResponse<PagedResult<DriverMedicalDto>>>> GetDriverMedicalRecords(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.DriverMedicals
                .Where(m => m.DriverId == id)
                .Select(m => new DriverMedicalDto
                {
                    Id = m.Id,
                    DriverId = m.DriverId,
                    ExaminationDate = m.ExaminationDate,
                    ExpiryDate = m.ExpiryDate,
                    CertificateNumber = m.CertificateNumber,
                    IssuingDoctor = m.IssuingDoctor,
                    MedicalFacility = m.MedicalFacility,
                    Status = m.Status,
                    Restrictions = m.Restrictions,
                    Conditions = m.Conditions,
                    RequiresGlasses = m.RequiresGlasses,
                    RequiresHearingAid = m.RequiresHearingAid,
                    BloodType = m.BloodType,
                    Allergies = m.Allergies,
                    Medications = m.Medications,
                    Notes = m.Notes,
                    IsExpired = m.ExpiryDate < DateTime.Today,
                    CreatedAt = m.CreatedAt
                })
                .OrderByDescending(m => m.ExaminationDate)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<DriverMedicalDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<DriverMedicalDto>>.ErrorResponse("Error retrieving medical records", ex.Message));
        }
    }

    /// <summary>
    /// Add driver medical record
    /// </summary>
    [HttpPost("{id}/medical-records")]
    public async Task<ActionResult<ApiResponse<DriverMedicalDto>>> CreateDriverMedicalRecord(Guid id, CreateDriverMedicalDto dto)
    {
        try
        {
            var medicalRecord = new DriverMedical
            {
                Id = Guid.NewGuid(),
                DriverId = id,
                ExaminationDate = dto.ExaminationDate,
                ExpiryDate = dto.ExpiryDate,
                CertificateNumber = dto.CertificateNumber,
                IssuingDoctor = dto.IssuingDoctor,
                MedicalFacility = dto.MedicalFacility,
                Status = dto.Status
            };

            _context.DriverMedicals.Add(medicalRecord);
            await _context.SaveChangesAsync();

            var responseDto = new DriverMedicalDto
            {
                Id = medicalRecord.Id,
                DriverId = medicalRecord.DriverId,
                ExaminationDate = medicalRecord.ExaminationDate,
                ExpiryDate = medicalRecord.ExpiryDate,
                CertificateNumber = medicalRecord.CertificateNumber,
                IssuingDoctor = medicalRecord.IssuingDoctor,
                MedicalFacility = medicalRecord.MedicalFacility,
                Status = medicalRecord.Status,
                Restrictions = medicalRecord.Restrictions,
                Conditions = medicalRecord.Conditions,
                RequiresGlasses = medicalRecord.RequiresGlasses,
                RequiresHearingAid = medicalRecord.RequiresHearingAid,
                BloodType = medicalRecord.BloodType,
                Allergies = medicalRecord.Allergies,
                Medications = medicalRecord.Medications,
                Notes = medicalRecord.Notes,
                IsExpired = medicalRecord.ExpiryDate < DateTime.Today,
                CreatedAt = medicalRecord.CreatedAt
            };

            return Ok(ApiResponse<DriverMedicalDto>.SuccessResponse(responseDto, "Medical record created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DriverMedicalDto>.ErrorResponse("Error creating medical record", ex.Message));
        }
    }

    #endregion

    #region Documents Management

    /// <summary>
    /// Get driver documents
    /// </summary>
    [HttpGet("{id}/documents")]
    public async Task<ActionResult<ApiResponse<PagedResult<DriverDocumentDto>>>> GetDriverDocuments(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.DriverDocuments
                .Where(d => d.DriverId == id)
                .Select(d => new DriverDocumentDto
                {
                    Id = d.Id,
                    DriverId = d.DriverId,
                    FileName = d.FileName,
                    OriginalFileName = d.OriginalFileName,
                    ContentType = d.ContentType,
                    FilePath = d.FilePath,
                    FileUrl = d.FileUrl,
                    FileSize = d.FileSize,
                    Category = d.Category,
                    Description = d.Description,
                    ExpiryDate = d.ExpiryDate,
                    IsActive = d.IsActive,
                    UploadedAt = d.UploadedAt,
                    UploadedBy = d.UploadedBy,
                    IsExpired = d.ExpiryDate.HasValue && d.ExpiryDate < DateTime.Today
                })
                .OrderByDescending(d => d.UploadedAt)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<DriverDocumentDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<DriverDocumentDto>>.ErrorResponse("Error retrieving driver documents", ex.Message));
        }
    }

    #endregion
}