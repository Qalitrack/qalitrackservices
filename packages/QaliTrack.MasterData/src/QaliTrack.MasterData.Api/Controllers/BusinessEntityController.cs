using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.BusinessEntities.Entities;
using QaliTrack.MasterData.Core.Modules.BusinessEntities.DTOs;
using QaliTrack.MasterData.Infrastructure.Data;

namespace QaliTrack.MasterData.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Tags("Business Entity Module")]
public class BusinessEntityController : ControllerBase
{
    private readonly MasterDataDbContext _context;

    public BusinessEntityController(MasterDataDbContext context)
    {
        _context = context;
    }

    #region Business Entity CRUD

    /// <summary>
    /// Get business entities with pagination and filtering - Returns summary view only
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<BusinessEntitySummaryDto>>>> GetBusinessEntities(
        [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.BusinessEntities
                .Select(be => new BusinessEntitySummaryDto
                {
                    Id = be.Id,
                    Name = be.Name,
                    Code = be.Code,
                    ContactEmail = be.ContactEmail,
                    ContactPhone = be.ContactPhone,
                    EntityType = be.EntityType,
                    Status = be.Status,
                    City = be.City,
                    Country = be.Country,
                    CreatedAt = be.CreatedAt,
                    IsCustomer = be.CustomerProfile != null,
                    IsSupplier = be.SupplierProfile != null,
                    IsTransporter = be.TransporterProfile != null
                })
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<BusinessEntitySummaryDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<BusinessEntitySummaryDto>>.ErrorResponse("Error retrieving business entities", ex.Message));
        }
    }

    /// <summary>
    /// Get business entity details by ID - Returns detailed view without child collections
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<BusinessEntityDetailDto>>> GetBusinessEntity(Guid id)
    {
        try
        {
            var entity = await _context.BusinessEntities
                .Include(be => be.CustomerProfile)
                .Include(be => be.SupplierProfile)
                .Include(be => be.TransporterProfile)
                .FirstOrDefaultAsync(be => be.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<BusinessEntityDetailDto>.ErrorResponse("Business entity not found"));
            }

            var dto = new BusinessEntityDetailDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Code = entity.Code,
                RegistrationNumber = entity.RegistrationNumber,
                TaxNumber = entity.TaxNumber,
                ContactEmail = entity.ContactEmail,
                ContactPhone = entity.ContactPhone,
                Address = entity.Address,
                City = entity.City,
                State = entity.State,
                Country = entity.Country,
                PostalCode = entity.PostalCode,
                EntityType = entity.EntityType,
                Status = entity.Status,
                Website = entity.Website,
                Description = entity.Description,
                Notes = entity.Notes,
                EstablishedDate = entity.EstablishedDate,
                EmployeeCount = entity.EmployeeCount,
                Industry = entity.Industry,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                IsCustomer = entity.CustomerProfile != null,
                IsSupplier = entity.SupplierProfile != null,
                IsTransporter = entity.TransporterProfile != null
            };

            return Ok(ApiResponse<BusinessEntityDetailDto>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<BusinessEntityDetailDto>.ErrorResponse("Error retrieving business entity", ex.Message));
        }
    }

    /// <summary>
    /// Create new business entity - Only essential fields required
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<BusinessEntityDetailDto>>> CreateBusinessEntity(CreateBusinessEntityDto dto)
    {
        try
        {
            var entity = new BusinessEntity
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Code = dto.Code,
                RegistrationNumber = dto.RegistrationNumber,
                TaxNumber = dto.TaxNumber ?? string.Empty,
                ContactEmail = dto.ContactEmail,
                ContactPhone = dto.ContactPhone,
                Address = dto.Address,
                City = dto.City ?? string.Empty,
                Country = dto.Country ?? string.Empty,
                EntityType = dto.EntityType,
                Status = "Active",
                Website = dto.Website ?? string.Empty,
                OrganizationId = dto.OrganizationId
            };

            _context.BusinessEntities.Add(entity);
            await _context.SaveChangesAsync();

            var detailDto = new BusinessEntityDetailDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Code = entity.Code,
                RegistrationNumber = entity.RegistrationNumber,
                TaxNumber = entity.TaxNumber,
                ContactEmail = entity.ContactEmail,
                ContactPhone = entity.ContactPhone,
                Address = entity.Address,
                City = entity.City,
                Country = entity.Country,
                EntityType = entity.EntityType,
                Status = entity.Status,
                Website = entity.Website,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                IsCustomer = false,
                IsSupplier = false,
                IsTransporter = false
            };

            return CreatedAtAction(nameof(GetBusinessEntity), 
                new { id = entity.Id }, 
                ApiResponse<BusinessEntityDetailDto>.SuccessResponse(detailDto, "Business entity created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<BusinessEntityDetailDto>.ErrorResponse("Error creating business entity", ex.Message));
        }
    }

    /// <summary>
    /// Update business entity basic information
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<BusinessEntityDetailDto>>> UpdateBusinessEntity(Guid id, UpdateBusinessEntityDto dto)
    {
        try
        {
            var entity = await _context.BusinessEntities.FindAsync(id);
            if (entity == null)
            {
                return NotFound(ApiResponse<BusinessEntityDetailDto>.ErrorResponse("Business entity not found"));
            }

            entity.Name = dto.Name;
            entity.ContactEmail = dto.ContactEmail;
            entity.ContactPhone = dto.ContactPhone;
            entity.Address = dto.Address;
            entity.Status = dto.Status;
            entity.TaxNumber = dto.TaxNumber ?? string.Empty;
            entity.City = dto.City ?? string.Empty;
            entity.State = dto.State ?? string.Empty;
            entity.Country = dto.Country ?? string.Empty;
            entity.PostalCode = dto.PostalCode ?? string.Empty;
            entity.Website = dto.Website ?? string.Empty;
            entity.Description = dto.Description ?? string.Empty;
            entity.Notes = dto.Notes ?? string.Empty;
            entity.EstablishedDate = dto.EstablishedDate;
            entity.EmployeeCount = dto.EmployeeCount;
            entity.Industry = dto.Industry ?? string.Empty;

            await _context.SaveChangesAsync();

            var detailDto = new BusinessEntityDetailDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Code = entity.Code,
                RegistrationNumber = entity.RegistrationNumber,
                TaxNumber = entity.TaxNumber,
                ContactEmail = entity.ContactEmail,
                ContactPhone = entity.ContactPhone,
                Address = entity.Address,
                City = entity.City,
                State = entity.State,
                Country = entity.Country,
                PostalCode = entity.PostalCode,
                EntityType = entity.EntityType,
                Status = entity.Status,
                Website = entity.Website,
                Description = entity.Description,
                Notes = entity.Notes,
                EstablishedDate = entity.EstablishedDate,
                EmployeeCount = entity.EmployeeCount,
                Industry = entity.Industry,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };

            return Ok(ApiResponse<BusinessEntityDetailDto>.SuccessResponse(detailDto, "Business entity updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<BusinessEntityDetailDto>.ErrorResponse("Error updating business entity", ex.Message));
        }
    }

    /// <summary>
    /// Delete business entity (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteBusinessEntity(Guid id)
    {
        try
        {
            var entity = await _context.BusinessEntities.FindAsync(id);
            if (entity == null)
            {
                return NotFound(ApiResponse.CreateError("Business entity not found"));
            }

            entity.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Business entity deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting business entity", ex.Message));
        }
    }

    #endregion

    #region Customer Profile Management

    /// <summary>
    /// Create customer profile for business entity
    /// </summary>
    [HttpPost("{id}/customer-profile")]
    public async Task<ActionResult<ApiResponse<CustomerProfileDto>>> CreateCustomerProfile(Guid id, CreateCustomerProfileDto dto)
    {
        try
        {
            dto.BusinessEntityId = id;
            
            var profile = new CustomerProfile
            {
                Id = Guid.NewGuid(),
                BusinessEntityId = dto.BusinessEntityId,
                CreditLimit = dto.CreditLimit,
                AvailableCredit = dto.CreditLimit,
                PaymentTermsDays = dto.PaymentTermsDays,
                Currency = dto.Currency,
                PreferredContactMethod = dto.PreferredContactMethod,
                BillingAddress = dto.BillingAddress ?? string.Empty,
                Status = "Active"
            };

            _context.CustomerProfiles.Add(profile);
            await _context.SaveChangesAsync();

            var responseDto = new CustomerProfileDto
            {
                Id = profile.Id,
                BusinessEntityId = profile.BusinessEntityId,
                CreditLimit = profile.CreditLimit,
                AvailableCredit = profile.AvailableCredit,
                UsedCredit = profile.UsedCredit,
                PaymentTermsDays = profile.PaymentTermsDays,
                Currency = profile.Currency,
                PreferredContactMethod = profile.PreferredContactMethod,
                BillingAddress = profile.BillingAddress,
                Status = profile.Status,
                CreatedAt = profile.CreatedAt
            };

            return Ok(ApiResponse<CustomerProfileDto>.SuccessResponse(responseDto, "Customer profile created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CustomerProfileDto>.ErrorResponse("Error creating customer profile", ex.Message));
        }
    }

    /// <summary>
    /// Update customer profile for business entity
    /// </summary>
    [HttpPut("{id}/customer-profile")]
    public async Task<ActionResult<ApiResponse<CustomerProfileDto>>> UpdateCustomerProfile(Guid id, UpdateCustomerProfileDto dto)
    {
        try
        {
            var profile = await _context.CustomerProfiles
                .FirstOrDefaultAsync(cp => cp.BusinessEntityId == id);

            if (profile == null)
            {
                return NotFound(ApiResponse<CustomerProfileDto>.ErrorResponse("Customer profile not found"));
            }

            profile.CreditLimit = dto.CreditLimit;
            profile.AvailableCredit = dto.CreditLimit - profile.UsedCredit;
            profile.PaymentTermsDays = dto.PaymentTermsDays;
            profile.Currency = dto.Currency;
            profile.PreferredContactMethod = dto.PreferredContactMethod;
            profile.BillingAddress = dto.BillingAddress ?? string.Empty;
            profile.Status = dto.Status;
            profile.Notes = dto.Notes ?? string.Empty;

            await _context.SaveChangesAsync();

            var responseDto = new CustomerProfileDto
            {
                Id = profile.Id,
                BusinessEntityId = profile.BusinessEntityId,
                CreditLimit = profile.CreditLimit,
                AvailableCredit = profile.AvailableCredit,
                UsedCredit = profile.UsedCredit,
                PaymentTermsDays = profile.PaymentTermsDays,
                Currency = profile.Currency,
                CreditRating = profile.CreditRating,
                LastCreditReview = profile.LastCreditReview,
                NextCreditReview = profile.NextCreditReview,
                SecurityDeposit = profile.SecurityDeposit,
                PreferredContactMethod = profile.PreferredContactMethod,
                PreferredLanguage = profile.PreferredLanguage,
                BillingAddress = profile.BillingAddress,
                Status = profile.Status,
                Notes = profile.Notes,
                CreatedAt = profile.CreatedAt
            };

            return Ok(ApiResponse<CustomerProfileDto>.SuccessResponse(responseDto, "Customer profile updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CustomerProfileDto>.ErrorResponse("Error updating customer profile", ex.Message));
        }
    }

    /// <summary>
    /// Partially update customer profile for business entity
    /// </summary>
    [HttpPatch("{id}/customer-profile")]
    public async Task<ActionResult<ApiResponse<CustomerProfileDto>>> PatchCustomerProfile(Guid id, PatchCustomerProfileDto dto)
    {
        try
        {
            var profile = await _context.CustomerProfiles
                .FirstOrDefaultAsync(cp => cp.BusinessEntityId == id);

            if (profile == null)
            {
                return NotFound(ApiResponse<CustomerProfileDto>.ErrorResponse("Customer profile not found"));
            }

            if (dto.CreditLimit.HasValue)
            {
                profile.CreditLimit = dto.CreditLimit.Value;
                profile.AvailableCredit = dto.CreditLimit.Value - profile.UsedCredit;
            }
            if (dto.PaymentTermsDays.HasValue)
                profile.PaymentTermsDays = dto.PaymentTermsDays.Value;
            if (dto.Currency != null)
                profile.Currency = dto.Currency;
            if (dto.PreferredContactMethod != null)
                profile.PreferredContactMethod = dto.PreferredContactMethod;
            if (dto.BillingAddress != null)
                profile.BillingAddress = dto.BillingAddress;
            if (dto.Status != null)
                profile.Status = dto.Status;
            if (dto.Notes != null)
                profile.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var responseDto = new CustomerProfileDto
            {
                Id = profile.Id,
                BusinessEntityId = profile.BusinessEntityId,
                CreditLimit = profile.CreditLimit,
                AvailableCredit = profile.AvailableCredit,
                UsedCredit = profile.UsedCredit,
                PaymentTermsDays = profile.PaymentTermsDays,
                Currency = profile.Currency,
                CreditRating = profile.CreditRating,
                LastCreditReview = profile.LastCreditReview,
                NextCreditReview = profile.NextCreditReview,
                SecurityDeposit = profile.SecurityDeposit,
                PreferredContactMethod = profile.PreferredContactMethod,
                PreferredLanguage = profile.PreferredLanguage,
                BillingAddress = profile.BillingAddress,
                Status = profile.Status,
                Notes = profile.Notes,
                CreatedAt = profile.CreatedAt
            };

            return Ok(ApiResponse<CustomerProfileDto>.SuccessResponse(responseDto, "Customer profile updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CustomerProfileDto>.ErrorResponse("Error updating customer profile", ex.Message));
        }
    }

    /// <summary>
    /// Delete customer profile for business entity
    /// </summary>
    [HttpDelete("{id}/customer-profile")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCustomerProfile(Guid id)
    {
        try
        {
            var profile = await _context.CustomerProfiles
                .FirstOrDefaultAsync(cp => cp.BusinessEntityId == id);

            if (profile == null)
            {
                return NotFound(ApiResponse.CreateError("Customer profile not found"));
            }

            _context.CustomerProfiles.Remove(profile);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Customer profile deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting customer profile", ex.Message));
        }
    }

    /// <summary>
    /// Get customer profile for business entity
    /// </summary>
    [HttpGet("{id}/customer-profile")]
    public async Task<ActionResult<ApiResponse<CustomerProfileDto>>> GetCustomerProfile(Guid id)
    {
        try
        {
            var profile = await _context.CustomerProfiles
                .FirstOrDefaultAsync(cp => cp.BusinessEntityId == id);

            if (profile == null)
            {
                return NotFound(ApiResponse<CustomerProfileDto>.ErrorResponse("Customer profile not found"));
            }

            var dto = new CustomerProfileDto
            {
                Id = profile.Id,
                BusinessEntityId = profile.BusinessEntityId,
                CreditLimit = profile.CreditLimit,
                AvailableCredit = profile.AvailableCredit,
                UsedCredit = profile.UsedCredit,
                PaymentTermsDays = profile.PaymentTermsDays,
                Currency = profile.Currency,
                CreditRating = profile.CreditRating,
                LastCreditReview = profile.LastCreditReview,
                NextCreditReview = profile.NextCreditReview,
                SecurityDeposit = profile.SecurityDeposit,
                PreferredContactMethod = profile.PreferredContactMethod,
                PreferredLanguage = profile.PreferredLanguage,
                BillingAddress = profile.BillingAddress,
                Status = profile.Status,
                Notes = profile.Notes,
                CreatedAt = profile.CreatedAt
            };

            return Ok(ApiResponse<CustomerProfileDto>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CustomerProfileDto>.ErrorResponse("Error retrieving customer profile", ex.Message));
        }
    }

    #endregion

    #region Supplier Profile Management

    /// <summary>
    /// Create supplier profile for business entity
    /// </summary>
    [HttpPost("{id}/supplier-profile")]
    public async Task<ActionResult<ApiResponse<SupplierProfileDto>>> CreateSupplierProfile(Guid id, CreateSupplierProfileDto dto)
    {
        try
        {
            dto.BusinessEntityId = id;
            
            var profile = new SupplierProfile
            {
                Id = Guid.NewGuid(),
                BusinessEntityId = dto.BusinessEntityId,
                SupplierType = dto.SupplierType,
                QualityRating = dto.QualityRating,
                LeadTimeDays = dto.LeadTimeDays,
                MinOrderValue = dto.MinOrderValue,
                PaymentTerms = dto.PaymentTerms,
                Status = "Active"
            };

            _context.SupplierProfiles.Add(profile);
            await _context.SaveChangesAsync();

            var responseDto = new SupplierProfileDto
            {
                Id = profile.Id,
                BusinessEntityId = profile.BusinessEntityId,
                SupplierType = profile.SupplierType,
                QualityRating = profile.QualityRating,
                LeadTimeDays = profile.LeadTimeDays,
                MinOrderValue = profile.MinOrderValue,
                PaymentTerms = profile.PaymentTerms,
                Status = profile.Status,
                CreatedAt = profile.CreatedAt
            };

            return Ok(ApiResponse<SupplierProfileDto>.SuccessResponse(responseDto, "Supplier profile created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<SupplierProfileDto>.ErrorResponse("Error creating supplier profile", ex.Message));
        }
    }

    /// <summary>
    /// Update supplier profile for business entity
    /// </summary>
    [HttpPut("{id}/supplier-profile")]
    public async Task<ActionResult<ApiResponse<SupplierProfileDto>>> UpdateSupplierProfile(Guid id, UpdateSupplierProfileDto dto)
    {
        try
        {
            var profile = await _context.SupplierProfiles
                .FirstOrDefaultAsync(sp => sp.BusinessEntityId == id);

            if (profile == null)
            {
                return NotFound(ApiResponse<SupplierProfileDto>.ErrorResponse("Supplier profile not found"));
            }

            profile.SupplierType = dto.SupplierType;
            profile.QualityRating = dto.QualityRating;
            profile.LeadTimeDays = dto.LeadTimeDays;
            profile.MinOrderValue = dto.MinOrderValue;
            profile.PaymentTerms = dto.PaymentTerms;
            profile.Status = dto.Status;
            profile.Notes = dto.Notes ?? string.Empty;

            await _context.SaveChangesAsync();

            var responseDto = new SupplierProfileDto
            {
                Id = profile.Id,
                BusinessEntityId = profile.BusinessEntityId,
                SupplierType = profile.SupplierType,
                QualityRating = profile.QualityRating,
                DeliveryRating = profile.DeliveryRating,
                IsVerified = profile.IsVerified,
                VerificationDate = profile.VerificationDate,
                CertificationLevel = profile.CertificationLevel,
                LeadTimeDays = profile.LeadTimeDays,
                MinOrderValue = profile.MinOrderValue,
                PaymentTerms = profile.PaymentTerms,
                Status = profile.Status,
                Notes = profile.Notes,
                CreatedAt = profile.CreatedAt
            };

            return Ok(ApiResponse<SupplierProfileDto>.SuccessResponse(responseDto, "Supplier profile updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<SupplierProfileDto>.ErrorResponse("Error updating supplier profile", ex.Message));
        }
    }

    /// <summary>
    /// Partially update supplier profile for business entity
    /// </summary>
    [HttpPatch("{id}/supplier-profile")]
    public async Task<ActionResult<ApiResponse<SupplierProfileDto>>> PatchSupplierProfile(Guid id, PatchSupplierProfileDto dto)
    {
        try
        {
            var profile = await _context.SupplierProfiles
                .FirstOrDefaultAsync(sp => sp.BusinessEntityId == id);

            if (profile == null)
            {
                return NotFound(ApiResponse<SupplierProfileDto>.ErrorResponse("Supplier profile not found"));
            }

            if (dto.SupplierType != null)
                profile.SupplierType = dto.SupplierType;
            if (dto.QualityRating != null)
                profile.QualityRating = dto.QualityRating;
            if (dto.LeadTimeDays.HasValue)
                profile.LeadTimeDays = dto.LeadTimeDays.Value;
            if (dto.MinOrderValue.HasValue)
                profile.MinOrderValue = dto.MinOrderValue.Value;
            if (dto.PaymentTerms != null)
                profile.PaymentTerms = dto.PaymentTerms;
            if (dto.Status != null)
                profile.Status = dto.Status;
            if (dto.Notes != null)
                profile.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var responseDto = new SupplierProfileDto
            {
                Id = profile.Id,
                BusinessEntityId = profile.BusinessEntityId,
                SupplierType = profile.SupplierType,
                QualityRating = profile.QualityRating,
                DeliveryRating = profile.DeliveryRating,
                IsVerified = profile.IsVerified,
                VerificationDate = profile.VerificationDate,
                CertificationLevel = profile.CertificationLevel,
                LeadTimeDays = profile.LeadTimeDays,
                MinOrderValue = profile.MinOrderValue,
                PaymentTerms = profile.PaymentTerms,
                Status = profile.Status,
                Notes = profile.Notes,
                CreatedAt = profile.CreatedAt
            };

            return Ok(ApiResponse<SupplierProfileDto>.SuccessResponse(responseDto, "Supplier profile updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<SupplierProfileDto>.ErrorResponse("Error updating supplier profile", ex.Message));
        }
    }

    /// <summary>
    /// Delete supplier profile for business entity
    /// </summary>
    [HttpDelete("{id}/supplier-profile")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteSupplierProfile(Guid id)
    {
        try
        {
            var profile = await _context.SupplierProfiles
                .FirstOrDefaultAsync(sp => sp.BusinessEntityId == id);

            if (profile == null)
            {
                return NotFound(ApiResponse.CreateError("Supplier profile not found"));
            }

            _context.SupplierProfiles.Remove(profile);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Supplier profile deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting supplier profile", ex.Message));
        }
    }

    /// <summary>
    /// Get supplier profile for business entity
    /// </summary>
    [HttpGet("{id}/supplier-profile")]
    public async Task<ActionResult<ApiResponse<SupplierProfileDto>>> GetSupplierProfile(Guid id)
    {
        try
        {
            var profile = await _context.SupplierProfiles
                .FirstOrDefaultAsync(sp => sp.BusinessEntityId == id);

            if (profile == null)
            {
                return NotFound(ApiResponse<SupplierProfileDto>.ErrorResponse("Supplier profile not found"));
            }

            var dto = new SupplierProfileDto
            {
                Id = profile.Id,
                BusinessEntityId = profile.BusinessEntityId,
                SupplierType = profile.SupplierType,
                QualityRating = profile.QualityRating,
                DeliveryRating = profile.DeliveryRating,
                IsVerified = profile.IsVerified,
                VerificationDate = profile.VerificationDate,
                CertificationLevel = profile.CertificationLevel,
                LeadTimeDays = profile.LeadTimeDays,
                MinOrderValue = profile.MinOrderValue,
                PaymentTerms = profile.PaymentTerms,
                Status = profile.Status,
                Notes = profile.Notes,
                CreatedAt = profile.CreatedAt
            };

            return Ok(ApiResponse<SupplierProfileDto>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<SupplierProfileDto>.ErrorResponse("Error retrieving supplier profile", ex.Message));
        }
    }

    #endregion

    #region Transporter Profile Management

    /// <summary>
    /// Create transporter profile for business entity
    /// </summary>
    [HttpPost("{id}/transporter-profile")]
    public async Task<ActionResult<ApiResponse<TransporterProfileDto>>> CreateTransporterProfile(Guid id, CreateTransporterProfileDto dto)
    {
        try
        {
            dto.BusinessEntityId = id;
            
            var profile = new TransporterProfile
            {
                Id = Guid.NewGuid(),
                BusinessEntityId = dto.BusinessEntityId,
                TransporterType = dto.TransporterType,
                FleetSize = dto.FleetSize,
                OperatingLicense = dto.OperatingLicense,
                LicenseExpiryDate = dto.LicenseExpiryDate,
                ServiceAreas = dto.ServiceAreas,
                BaseRate = dto.BaseRate,
                RateStructure = dto.RateStructure,
                Status = "Active"
            };

            _context.TransporterProfiles.Add(profile);
            await _context.SaveChangesAsync();

            var responseDto = new TransporterProfileDto
            {
                Id = profile.Id,
                BusinessEntityId = profile.BusinessEntityId,
                TransporterType = profile.TransporterType,
                FleetSize = profile.FleetSize,
                OperatingLicense = profile.OperatingLicense,
                LicenseExpiryDate = profile.LicenseExpiryDate,
                ServiceAreas = profile.ServiceAreas,
                BaseRate = profile.BaseRate ?? 0,
                RateStructure = profile.RateStructure,
                Status = profile.Status,
                CreatedAt = profile.CreatedAt
            };

            return Ok(ApiResponse<TransporterProfileDto>.SuccessResponse(responseDto, "Transporter profile created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<TransporterProfileDto>.ErrorResponse("Error creating transporter profile", ex.Message));
        }
    }

    /// <summary>
    /// Update transporter profile for business entity
    /// </summary>
    [HttpPut("{id}/transporter-profile")]
    public async Task<ActionResult<ApiResponse<TransporterProfileDto>>> UpdateTransporterProfile(Guid id, UpdateTransporterProfileDto dto)
    {
        try
        {
            var profile = await _context.TransporterProfiles
                .FirstOrDefaultAsync(tp => tp.BusinessEntityId == id);

            if (profile == null)
            {
                return NotFound(ApiResponse<TransporterProfileDto>.ErrorResponse("Transporter profile not found"));
            }

            profile.TransporterType = dto.TransporterType;
            profile.FleetSize = dto.FleetSize;
            profile.OperatingLicense = dto.OperatingLicense;
            profile.LicenseExpiryDate = dto.LicenseExpiryDate;
            profile.ServiceAreas = dto.ServiceAreas;
            profile.BaseRate = dto.BaseRate;
            profile.RateStructure = dto.RateStructure;
            profile.Status = dto.Status;
            profile.Notes = dto.Notes ?? string.Empty;

            await _context.SaveChangesAsync();

            var responseDto = new TransporterProfileDto
            {
                Id = profile.Id,
                BusinessEntityId = profile.BusinessEntityId,
                TransporterType = profile.TransporterType,
                FleetSize = profile.FleetSize,
                OperatingLicense = profile.OperatingLicense,
                LicenseExpiryDate = profile.LicenseExpiryDate,
                Rating = (int)(profile.Rating ?? 0),
                ServiceAreas = profile.ServiceAreas,
                SpecializedServices = profile.SpecializedServices,
                BaseRate = profile.BaseRate ?? 0,
                RateStructure = profile.RateStructure,
                Status = profile.Status,
                Notes = profile.Notes,
                CreatedAt = profile.CreatedAt
            };

            return Ok(ApiResponse<TransporterProfileDto>.SuccessResponse(responseDto, "Transporter profile updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<TransporterProfileDto>.ErrorResponse("Error updating transporter profile", ex.Message));
        }
    }

    /// <summary>
    /// Partially update transporter profile for business entity
    /// </summary>
    [HttpPatch("{id}/transporter-profile")]
    public async Task<ActionResult<ApiResponse<TransporterProfileDto>>> PatchTransporterProfile(Guid id, PatchTransporterProfileDto dto)
    {
        try
        {
            var profile = await _context.TransporterProfiles
                .FirstOrDefaultAsync(tp => tp.BusinessEntityId == id);

            if (profile == null)
            {
                return NotFound(ApiResponse<TransporterProfileDto>.ErrorResponse("Transporter profile not found"));
            }

            if (dto.TransporterType != null)
                profile.TransporterType = dto.TransporterType;
            if (dto.FleetSize.HasValue)
                profile.FleetSize = dto.FleetSize.Value;
            if (dto.OperatingLicense != null)
                profile.OperatingLicense = dto.OperatingLicense;
            if (dto.LicenseExpiryDate.HasValue)
                profile.LicenseExpiryDate = dto.LicenseExpiryDate.Value;
            if (dto.ServiceAreas != null)
                profile.ServiceAreas = dto.ServiceAreas;
            if (dto.BaseRate.HasValue)
                profile.BaseRate = dto.BaseRate.Value;
            if (dto.RateStructure != null)
                profile.RateStructure = dto.RateStructure;
            if (dto.Status != null)
                profile.Status = dto.Status;
            if (dto.Notes != null)
                profile.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var responseDto = new TransporterProfileDto
            {
                Id = profile.Id,
                BusinessEntityId = profile.BusinessEntityId,
                TransporterType = profile.TransporterType,
                FleetSize = profile.FleetSize,
                OperatingLicense = profile.OperatingLicense,
                LicenseExpiryDate = profile.LicenseExpiryDate,
                Rating = (int)(profile.Rating ?? 0),
                ServiceAreas = profile.ServiceAreas,
                SpecializedServices = profile.SpecializedServices,
                BaseRate = profile.BaseRate ?? 0,
                RateStructure = profile.RateStructure,
                Status = profile.Status,
                Notes = profile.Notes,
                CreatedAt = profile.CreatedAt
            };

            return Ok(ApiResponse<TransporterProfileDto>.SuccessResponse(responseDto, "Transporter profile updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<TransporterProfileDto>.ErrorResponse("Error updating transporter profile", ex.Message));
        }
    }

    /// <summary>
    /// Delete transporter profile for business entity
    /// </summary>
    [HttpDelete("{id}/transporter-profile")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteTransporterProfile(Guid id)
    {
        try
        {
            var profile = await _context.TransporterProfiles
                .FirstOrDefaultAsync(tp => tp.BusinessEntityId == id);

            if (profile == null)
            {
                return NotFound(ApiResponse.CreateError("Transporter profile not found"));
            }

            _context.TransporterProfiles.Remove(profile);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Transporter profile deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting transporter profile", ex.Message));
        }
    }

    /// <summary>
    /// Get transporter profile for business entity
    /// </summary>
    [HttpGet("{id}/transporter-profile")]
    public async Task<ActionResult<ApiResponse<TransporterProfileDto>>> GetTransporterProfile(Guid id)
    {
        try
        {
            var profile = await _context.TransporterProfiles
                .FirstOrDefaultAsync(tp => tp.BusinessEntityId == id);

            if (profile == null)
            {
                return NotFound(ApiResponse<TransporterProfileDto>.ErrorResponse("Transporter profile not found"));
            }

            var dto = new TransporterProfileDto
            {
                Id = profile.Id,
                BusinessEntityId = profile.BusinessEntityId,
                TransporterType = profile.TransporterType,
                FleetSize = profile.FleetSize,
                OperatingLicense = profile.OperatingLicense,
                LicenseExpiryDate = profile.LicenseExpiryDate,
                Rating = (int)(profile.Rating ?? 0),
                ServiceAreas = profile.ServiceAreas,
                SpecializedServices = profile.SpecializedServices,
                BaseRate = profile.BaseRate ?? 0,
                RateStructure = profile.RateStructure,
                Status = profile.Status,
                Notes = profile.Notes,
                CreatedAt = profile.CreatedAt
            };

            return Ok(ApiResponse<TransporterProfileDto>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<TransporterProfileDto>.ErrorResponse("Error retrieving transporter profile", ex.Message));
        }
    }

    #endregion

    #region Contacts Management

    /// <summary>
    /// Get contacts for business entity
    /// </summary>
    [HttpGet("{id}/contacts")]
    public async Task<ActionResult<ApiResponse<PagedResult<ContactDto>>>> GetBusinessEntityContacts(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.BusinessEntityContacts
                .Where(c => c.BusinessEntityId == id)
                .Select(c => new ContactDto
                {
                    Id = c.Id,
                    BusinessEntityId = c.BusinessEntityId,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Email = c.Email,
                    Phone = c.Phone,
                    Mobile = c.Mobile,
                    Position = c.Position,
                    Department = c.Department,
                    ContactType = c.ContactType,
                    IsPrimary = c.IsPrimary,
                    IsActive = c.IsActive,
                    Notes = c.Notes,
                    CreatedAt = c.CreatedAt
                })
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<ContactDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<ContactDto>>.ErrorResponse("Error retrieving contacts", ex.Message));
        }
    }

    /// <summary>
    /// Partially update contact for business entity
    /// </summary>
    [HttpPatch("{id}/contacts/{contactId}")]
    public async Task<ActionResult<ApiResponse<ContactDto>>> PatchBusinessEntityContact(Guid id, Guid contactId, PatchContactDto dto)
    {
        try
        {
            var contact = await _context.BusinessEntityContacts
                .FirstOrDefaultAsync(c => c.Id == contactId && c.BusinessEntityId == id);

            if (contact == null)
            {
                return NotFound(ApiResponse<ContactDto>.ErrorResponse("Contact not found"));
            }

            if (dto.FirstName != null)
                contact.FirstName = dto.FirstName;
            if (dto.LastName != null)
                contact.LastName = dto.LastName;
            if (dto.Email != null)
                contact.Email = dto.Email;
            if (dto.Phone != null)
                contact.Phone = dto.Phone;
            if (dto.Position != null)
                contact.Position = dto.Position;
            if (dto.ContactType != null)
                contact.ContactType = dto.ContactType;
            if (dto.IsPrimary.HasValue)
                contact.IsPrimary = dto.IsPrimary.Value;
            if (dto.IsActive.HasValue)
                contact.IsActive = dto.IsActive.Value;
            if (dto.Notes != null)
                contact.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var responseDto = new ContactDto
            {
                Id = contact.Id,
                BusinessEntityId = contact.BusinessEntityId,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                Email = contact.Email,
                Phone = contact.Phone,
                Mobile = contact.Mobile,
                Position = contact.Position,
                Department = contact.Department,
                ContactType = contact.ContactType,
                IsPrimary = contact.IsPrimary,
                IsActive = contact.IsActive,
                Notes = contact.Notes,
                CreatedAt = contact.CreatedAt
            };

            return Ok(ApiResponse<ContactDto>.SuccessResponse(responseDto, "Contact updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ContactDto>.ErrorResponse("Error updating contact", ex.Message));
        }
    }

    /// <summary>
    /// Delete contact for business entity
    /// </summary>
    [HttpDelete("{id}/contacts/{contactId}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteBusinessEntityContact(Guid id, Guid contactId)
    {
        try
        {
            var contact = await _context.BusinessEntityContacts
                .FirstOrDefaultAsync(c => c.Id == contactId && c.BusinessEntityId == id);

            if (contact == null)
            {
                return NotFound(ApiResponse.CreateError("Contact not found"));
            }

            _context.BusinessEntityContacts.Remove(contact);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Contact deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting contact", ex.Message));
        }
    }

    /// <summary>
    /// Update contact for business entity
    /// </summary>
    [HttpPut("{id}/contacts/{contactId}")]
    public async Task<ActionResult<ApiResponse<ContactDto>>> UpdateBusinessEntityContact(Guid id, Guid contactId, UpdateContactDto dto)
    {
        try
        {
            var contact = await _context.BusinessEntityContacts
                .FirstOrDefaultAsync(c => c.Id == contactId && c.BusinessEntityId == id);

            if (contact == null)
            {
                return NotFound(ApiResponse<ContactDto>.ErrorResponse("Contact not found"));
            }

            contact.FirstName = dto.FirstName;
            contact.LastName = dto.LastName;
            contact.Email = dto.Email;
            contact.Phone = dto.Phone;
            contact.Position = dto.Position;
            contact.ContactType = dto.ContactType;
            contact.IsPrimary = dto.IsPrimary;
            contact.IsActive = dto.IsActive;
            contact.Notes = dto.Notes ?? string.Empty;

            await _context.SaveChangesAsync();

            var responseDto = new ContactDto
            {
                Id = contact.Id,
                BusinessEntityId = contact.BusinessEntityId,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                Email = contact.Email,
                Phone = contact.Phone,
                Mobile = contact.Mobile,
                Position = contact.Position,
                Department = contact.Department,
                ContactType = contact.ContactType,
                IsPrimary = contact.IsPrimary,
                IsActive = contact.IsActive,
                Notes = contact.Notes,
                CreatedAt = contact.CreatedAt
            };

            return Ok(ApiResponse<ContactDto>.SuccessResponse(responseDto, "Contact updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ContactDto>.ErrorResponse("Error updating contact", ex.Message));
        }
    }

    /// <summary>
    /// Add contact to business entity
    /// </summary>
    [HttpPost("{id}/contacts")]
    public async Task<ActionResult<ApiResponse<ContactDto>>> CreateBusinessEntityContact(Guid id, CreateContactDto dto)
    {
        try
        {
            var contact = new BusinessEntityContact
            {
                Id = Guid.NewGuid(),
                BusinessEntityId = id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                Position = dto.Position,
                ContactType = dto.ContactType,
                IsPrimary = dto.IsPrimary,
                IsActive = true
            };

            _context.BusinessEntityContacts.Add(contact);
            await _context.SaveChangesAsync();

            var responseDto = new ContactDto
            {
                Id = contact.Id,
                BusinessEntityId = contact.BusinessEntityId,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                Email = contact.Email,
                Phone = contact.Phone,
                Position = contact.Position,
                ContactType = contact.ContactType,
                IsPrimary = contact.IsPrimary,
                IsActive = contact.IsActive,
                CreatedAt = contact.CreatedAt
            };

            return Ok(ApiResponse<ContactDto>.SuccessResponse(responseDto, "Contact created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ContactDto>.ErrorResponse("Error creating contact", ex.Message));
        }
    }

    #endregion
}