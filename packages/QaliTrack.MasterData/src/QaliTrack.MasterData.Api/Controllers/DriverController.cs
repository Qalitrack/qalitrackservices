using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.Driver.Entities;
using QaliTrack.MasterData.Core.Modules.Driver.DTOs;
using QaliTrack.MasterData.Core.Modules.Relationships.DTOs;
using QaliTrack.MasterData.Core.Modules.Relationships.Entities;
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
    /// Update driver license
    /// </summary>
    [HttpPut("{id}/license")]
    public async Task<ActionResult<ApiResponse<DriverLicenseDto>>> UpdateDriverLicense(Guid id, UpdateDriverLicenseDto dto)
    {
        try
        {
            var license = await _context.DriverLicenses
                .FirstOrDefaultAsync(l => l.DriverId == id);

            if (license == null)
            {
                return NotFound(ApiResponse<DriverLicenseDto>.ErrorResponse("Driver license not found"));
            }

            license.LicenseNumber = dto.LicenseNumber;
            license.LicenseClass = dto.LicenseClass;
            license.IssueDate = dto.IssueDate;
            license.ExpiryDate = dto.ExpiryDate;
            license.IssuingAuthority = dto.IssuingAuthority;
            license.Restrictions = dto.Restrictions;
            license.Status = dto.Status;

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

            return Ok(ApiResponse<DriverLicenseDto>.SuccessResponse(responseDto, "Driver license updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DriverLicenseDto>.ErrorResponse("Error updating driver license", ex.Message));
        }
    }

    /// <summary>
    /// Partially update driver license
    /// </summary>
    [HttpPatch("{id}/license")]
    public async Task<ActionResult<ApiResponse<DriverLicenseDto>>> PatchDriverLicense(Guid id, PatchDriverLicenseDto dto)
    {
        try
        {
            var license = await _context.DriverLicenses
                .FirstOrDefaultAsync(l => l.DriverId == id);

            if (license == null)
            {
                return NotFound(ApiResponse<DriverLicenseDto>.ErrorResponse("Driver license not found"));
            }

            if (dto.LicenseNumber != null)
                license.LicenseNumber = dto.LicenseNumber;
            if (dto.LicenseClass != null)
                license.LicenseClass = dto.LicenseClass;
            if (dto.IssueDate.HasValue)
                license.IssueDate = dto.IssueDate.Value;
            if (dto.ExpiryDate.HasValue)
                license.ExpiryDate = dto.ExpiryDate.Value;
            if (dto.IssuingAuthority != null)
                license.IssuingAuthority = dto.IssuingAuthority;
            if (dto.Restrictions != null)
                license.Restrictions = dto.Restrictions;
            if (dto.Status != null)
                license.Status = dto.Status;

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

            return Ok(ApiResponse<DriverLicenseDto>.SuccessResponse(responseDto, "Driver license updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DriverLicenseDto>.ErrorResponse("Error updating driver license", ex.Message));
        }
    }

    /// <summary>
    /// Delete driver license
    /// </summary>
    [HttpDelete("{id}/license")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteDriverLicense(Guid id)
    {
        try
        {
            var license = await _context.DriverLicenses
                .FirstOrDefaultAsync(l => l.DriverId == id);

            if (license == null)
            {
                return NotFound(ApiResponse.CreateError("Driver license not found"));
            }

            _context.DriverLicenses.Remove(license);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Driver license deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting driver license", ex.Message));
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
    /// Update driver training
    /// </summary>
    [HttpPut("{id}/trainings/{trainingId}")]
    public async Task<ActionResult<ApiResponse<DriverTrainingDto>>> UpdateDriverTraining(Guid id, Guid trainingId, UpdateDriverTrainingDto dto)
    {
        try
        {
            var training = await _context.DriverTrainings
                .FirstOrDefaultAsync(t => t.Id == trainingId && t.DriverId == id);

            if (training == null)
            {
                return NotFound(ApiResponse<DriverTrainingDto>.ErrorResponse("Driver training not found"));
            }

            training.TrainingName = dto.TrainingName;
            training.TrainingType = dto.TrainingType;
            training.TrainingProvider = dto.TrainingProvider;
            training.StartDate = dto.StartDate;
            training.EndDate = dto.EndDate;
            training.Status = dto.Status;
            training.Score = dto.Score;
            training.IsCertified = dto.IsCertified;
            training.CertificateNumber = dto.CertificateNumber;
            training.CertificateExpiryDate = dto.CertificateExpiryDate;
            training.Cost = dto.Cost;
            training.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var responseDto = new DriverTrainingDto
            {
                Id = training.Id,
                DriverId = training.DriverId,
                TrainingName = training.TrainingName,
                TrainingType = training.TrainingType,
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

            return Ok(ApiResponse<DriverTrainingDto>.SuccessResponse(responseDto, "Driver training updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DriverTrainingDto>.ErrorResponse("Error updating driver training", ex.Message));
        }
    }

    /// <summary>
    /// Partially update driver training
    /// </summary>
    [HttpPatch("{id}/trainings/{trainingId}")]
    public async Task<ActionResult<ApiResponse<DriverTrainingDto>>> PatchDriverTraining(Guid id, Guid trainingId, PatchDriverTrainingDto dto)
    {
        try
        {
            var training = await _context.DriverTrainings
                .FirstOrDefaultAsync(t => t.Id == trainingId && t.DriverId == id);

            if (training == null)
            {
                return NotFound(ApiResponse<DriverTrainingDto>.ErrorResponse("Driver training not found"));
            }

            if (dto.TrainingName != null)
                training.TrainingName = dto.TrainingName;
            if (dto.TrainingType != null)
                training.TrainingType = dto.TrainingType;
            if (dto.TrainingProvider != null)
                training.TrainingProvider = dto.TrainingProvider;
            if (dto.StartDate.HasValue)
                training.StartDate = dto.StartDate.Value;
            if (dto.EndDate.HasValue)
                training.EndDate = dto.EndDate.Value;
            if (dto.Status != null)
                training.Status = dto.Status;
            if (dto.Score.HasValue)
                training.Score = dto.Score.Value;
            if (dto.IsCertified.HasValue)
                training.IsCertified = dto.IsCertified.Value;
            if (dto.CertificateNumber != null)
                training.CertificateNumber = dto.CertificateNumber;
            if (dto.CertificateExpiryDate.HasValue)
                training.CertificateExpiryDate = dto.CertificateExpiryDate.Value;
            if (dto.Cost.HasValue)
                training.Cost = dto.Cost.Value;
            if (dto.Notes != null)
                training.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var responseDto = new DriverTrainingDto
            {
                Id = training.Id,
                DriverId = training.DriverId,
                TrainingName = training.TrainingName,
                TrainingType = training.TrainingType,
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

            return Ok(ApiResponse<DriverTrainingDto>.SuccessResponse(responseDto, "Driver training updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DriverTrainingDto>.ErrorResponse("Error updating driver training", ex.Message));
        }
    }

    /// <summary>
    /// Delete driver training
    /// </summary>
    [HttpDelete("{id}/trainings/{trainingId}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteDriverTraining(Guid id, Guid trainingId)
    {
        try
        {
            var training = await _context.DriverTrainings
                .FirstOrDefaultAsync(t => t.Id == trainingId && t.DriverId == id);

            if (training == null)
            {
                return NotFound(ApiResponse.CreateError("Driver training not found"));
            }

            _context.DriverTrainings.Remove(training);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Driver training deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting driver training", ex.Message));
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
    /// Update driver medical record
    /// </summary>
    [HttpPut("{id}/medical-records/{recordId}")]
    public async Task<ActionResult<ApiResponse<DriverMedicalDto>>> UpdateDriverMedicalRecord(Guid id, Guid recordId, UpdateDriverMedicalDto dto)
    {
        try
        {
            var medicalRecord = await _context.DriverMedicals
                .FirstOrDefaultAsync(m => m.Id == recordId && m.DriverId == id);

            if (medicalRecord == null)
            {
                return NotFound(ApiResponse<DriverMedicalDto>.ErrorResponse("Medical record not found"));
            }

            medicalRecord.ExaminationDate = dto.ExaminationDate;
            medicalRecord.ExpiryDate = dto.ExpiryDate;
            medicalRecord.CertificateNumber = dto.CertificateNumber;
            medicalRecord.IssuingDoctor = dto.IssuingDoctor;
            medicalRecord.MedicalFacility = dto.MedicalFacility;
            medicalRecord.Status = dto.Status;
            medicalRecord.Restrictions = dto.Restrictions;
            medicalRecord.Conditions = dto.Conditions;
            medicalRecord.RequiresGlasses = dto.RequiresGlasses;
            medicalRecord.RequiresHearingAid = dto.RequiresHearingAid;
            medicalRecord.BloodType = dto.BloodType;
            medicalRecord.Allergies = dto.Allergies;
            medicalRecord.Medications = dto.Medications;
            medicalRecord.Notes = dto.Notes;

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

            return Ok(ApiResponse<DriverMedicalDto>.SuccessResponse(responseDto, "Medical record updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DriverMedicalDto>.ErrorResponse("Error updating medical record", ex.Message));
        }
    }

    /// <summary>
    /// Partially update driver medical record
    /// </summary>
    [HttpPatch("{id}/medical-records/{recordId}")]
    public async Task<ActionResult<ApiResponse<DriverMedicalDto>>> PatchDriverMedicalRecord(Guid id, Guid recordId, PatchDriverMedicalDto dto)
    {
        try
        {
            var medicalRecord = await _context.DriverMedicals
                .FirstOrDefaultAsync(m => m.Id == recordId && m.DriverId == id);

            if (medicalRecord == null)
            {
                return NotFound(ApiResponse<DriverMedicalDto>.ErrorResponse("Medical record not found"));
            }

            if (dto.ExaminationDate.HasValue)
                medicalRecord.ExaminationDate = dto.ExaminationDate.Value;
            if (dto.ExpiryDate.HasValue)
                medicalRecord.ExpiryDate = dto.ExpiryDate.Value;
            if (dto.CertificateNumber != null)
                medicalRecord.CertificateNumber = dto.CertificateNumber;
            if (dto.IssuingDoctor != null)
                medicalRecord.IssuingDoctor = dto.IssuingDoctor;
            if (dto.MedicalFacility != null)
                medicalRecord.MedicalFacility = dto.MedicalFacility;
            if (dto.Status != null)
                medicalRecord.Status = dto.Status;
            if (dto.Restrictions != null)
                medicalRecord.Restrictions = dto.Restrictions;
            if (dto.Conditions != null)
                medicalRecord.Conditions = dto.Conditions;
            if (dto.RequiresGlasses.HasValue)
                medicalRecord.RequiresGlasses = dto.RequiresGlasses.Value;
            if (dto.RequiresHearingAid.HasValue)
                medicalRecord.RequiresHearingAid = dto.RequiresHearingAid.Value;
            if (dto.BloodType != null)
                medicalRecord.BloodType = dto.BloodType;
            if (dto.Allergies != null)
                medicalRecord.Allergies = dto.Allergies;
            if (dto.Medications != null)
                medicalRecord.Medications = dto.Medications;
            if (dto.Notes != null)
                medicalRecord.Notes = dto.Notes;

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

            return Ok(ApiResponse<DriverMedicalDto>.SuccessResponse(responseDto, "Medical record updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DriverMedicalDto>.ErrorResponse("Error updating medical record", ex.Message));
        }
    }

    /// <summary>
    /// Delete driver medical record
    /// </summary>
    [HttpDelete("{id}/medical-records/{recordId}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteDriverMedicalRecord(Guid id, Guid recordId)
    {
        try
        {
            var medicalRecord = await _context.DriverMedicals
                .FirstOrDefaultAsync(m => m.Id == recordId && m.DriverId == id);

            if (medicalRecord == null)
            {
                return NotFound(ApiResponse.CreateError("Medical record not found"));
            }

            _context.DriverMedicals.Remove(medicalRecord);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Medical record deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting medical record", ex.Message));
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

    #region Driver Relationships

    /// <summary>
    /// Get driver's SACCO memberships
    /// </summary>
    [HttpGet("{id}/saccos")]
    public async Task<ActionResult<ApiResponse<PagedResult<DriverSaccoMembershipDto>>>> GetDriverSaccos(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.DriverSaccoMemberships
                .Where(m => m.DriverId == id)
                .Select(m => new DriverSaccoMembershipDto
                {
                    Id = m.Id,
                    DriverId = m.DriverId,
                    SaccoId = m.SaccoId,
                    MembershipDate = m.MembershipDate,
                    ExpiryDate = m.ExpiryDate,
                    MembershipNumber = m.MembershipNumber,
                    Status = m.Status,
                    ShareContribution = m.ShareContribution,
                    MonthlyContribution = m.MonthlyContribution,
                    MembershipType = m.MembershipType,
                    Benefits = m.Benefits,
                    IsActive = m.IsActive,
                    Notes = m.Notes,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt,
                    IsExpired = m.ExpiryDate.HasValue && m.ExpiryDate < DateTime.Today,
                    DriverName = "Driver Name", // TODO: Join with Driver entity
                    SaccoName = "SACCO Name" // TODO: Join with SACCO entity
                })
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<DriverSaccoMembershipDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<DriverSaccoMembershipDto>>.ErrorResponse("Error retrieving driver SACCO memberships", ex.Message));
        }
    }

    /// <summary>
    /// Get driver's vehicle assignments
    /// </summary>
    [HttpGet("{id}/vehicles")]
    public async Task<ActionResult<ApiResponse<PagedResult<DriverVehicleAssignmentDto>>>> GetDriverVehicles(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.DriverVehicleAssignments
                .Where(a => a.DriverId == id)
                .Select(a => new DriverVehicleAssignmentDto
                {
                    Id = a.Id,
                    DriverId = a.DriverId,
                    VehicleId = a.VehicleId,
                    AssignedDate = a.AssignedDate,
                    UnassignedDate = a.UnassignedDate,
                    IsPrimary = a.IsPrimary,
                    IsActive = a.IsActive,
                    AssignmentType = a.AssignmentType,
                    Reason = a.Reason,
                    AssignedBy = a.AssignedBy,
                    UnassignedBy = a.UnassignedBy,
                    Notes = a.Notes,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    DriverName = "Driver Name", // TODO: Join with Driver entity
                    VehicleRegistrationNumber = "Vehicle Reg", // TODO: Join with Vehicle entity
                    IsCurrentlyAssigned = a.IsActive && a.UnassignedDate == null
                })
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<DriverVehicleAssignmentDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<DriverVehicleAssignmentDto>>.ErrorResponse("Error retrieving driver vehicle assignments", ex.Message));
        }
    }

    /// <summary>
    /// Get driver's employment history with transporters
    /// </summary>
    [HttpGet("{id}/employment")]
    public async Task<ActionResult<ApiResponse<PagedResult<DriverTransporterEmploymentDto>>>> GetDriverEmployment(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.DriverTransporterEmployments
                .Where(e => e.DriverId == id)
                .Select(e => new DriverTransporterEmploymentDto
                {
                    Id = e.Id,
                    DriverId = e.DriverId,
                    TransporterProfileId = e.TransporterProfileId,
                    HireDate = e.HireDate,
                    TerminationDate = e.TerminationDate,
                    EmploymentType = e.EmploymentType,
                    Status = e.Status,
                    Salary = e.Salary,
                    Position = e.Position,
                    Department = e.Department,
                    ReportsTo = e.ReportsTo,
                    TerminationReason = e.TerminationReason,
                    IsActive = e.IsActive,
                    Notes = e.Notes,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                    DriverName = "Driver Name", // TODO: Join with Driver entity
                    TransporterName = "Transporter Name", // TODO: Join with Transporter entity
                    IsCurrentEmployment = e.IsActive && e.TerminationDate == null
                })
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<DriverTransporterEmploymentDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<DriverTransporterEmploymentDto>>.ErrorResponse("Error retrieving driver employment history", ex.Message));
        }
    }

    #endregion
}