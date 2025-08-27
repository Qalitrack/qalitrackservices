using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.Weighbridge.DTOs;
using QaliTrack.MasterData.Core.Modules.Weighbridge.Entities;
using QaliTrack.MasterData.Infrastructure.Data;

namespace QaliTrack.MasterData.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Tags("Weighbridge Module")]
public class WeighbridgeController : ControllerBase
{
    private readonly MasterDataDbContext _context;

    public WeighbridgeController(MasterDataDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get weighbridges with Django-style filtering, searching, and pagination
    /// </summary>
    /// <param name="queryParams">Query parameters for filtering, search, and pagination</param>
    /// <returns>Paginated list of weighbridges</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<WeighbridgeSummaryDto>>>> GetWeighbridges(
        [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.Weighbridges
                .Select(w => new WeighbridgeSummaryDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Code = w.Code,
                    Location = w.Location,
                    Status = w.Status,
                    Type = w.Type,
                    MaxCapacity = w.MaxCapacity,
                    MinCapacity = w.MinCapacity,
                    IsActive = w.IsActive,
                    LastCalibrationDate = w.LastCalibrationDate,
                    NextCalibrationDate = w.NextCalibrationDate,
                    IsCalibrationDue = w.NextCalibrationDate.HasValue && w.NextCalibrationDate < DateTime.Today,
                    TransactionCount = w.Transactions.Count,
                    CreatedAt = w.CreatedAt
                })
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<WeighbridgeSummaryDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<WeighbridgeSummaryDto>>.ErrorResponse("Error retrieving weighbridges", ex.Message));
        }
    }

    /// <summary>
    /// Get weighbridge by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<WeighbridgeDetailDto>>> GetWeighbridge(Guid id)
    {
        try
        {
            var weighbridge = await _context.Weighbridges
                .Where(w => w.Id == id)
                .Select(w => new WeighbridgeDetailDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Code = w.Code,
                    Location = w.Location,
                    Latitude = w.Latitude,
                    Longitude = w.Longitude,
                    Status = w.Status,
                    Type = w.Type,
                    MaxCapacity = w.MaxCapacity,
                    MinCapacity = w.MinCapacity,
                    Accuracy = w.Accuracy,
                    Manufacturer = w.Manufacturer,
                    Model = w.Model,
                    SerialNumber = w.SerialNumber,
                    InstallationDate = w.InstallationDate,
                    LastCalibrationDate = w.LastCalibrationDate,
                    NextCalibrationDate = w.NextCalibrationDate,
                    LastMaintenanceDate = w.LastMaintenanceDate,
                    NextMaintenanceDate = w.NextMaintenanceDate,
                    CertificateNumber = w.CertificateNumber,
                    CertificateExpiryDate = w.CertificateExpiryDate,
                    CalibrationAuthority = w.CalibrationAuthority,
                    OperatingHours = w.OperatingHours,
                    ContactPerson = w.ContactPerson,
                    ContactPhone = w.ContactPhone,
                    ServiceFee = w.ServiceFee,
                    Currency = w.Currency,
                    IsActive = w.IsActive,
                    OrganizationId = w.OrganizationId,
                    Notes = w.Notes,
                    CreatedAt = w.CreatedAt,
                    UpdatedAt = w.UpdatedAt,
                    HasCalibrations = w.Calibrations.Any(),
                    HasMaintenanceRecords = w.MaintenanceRecords.Any(),
                    HasTransactions = w.Transactions.Any(),
                    HasDocuments = w.Documents.Any(d => d.IsActive),
                    IsCalibrationDue = w.NextCalibrationDate.HasValue && w.NextCalibrationDate < DateTime.Today,
                    IsMaintenanceDue = w.NextMaintenanceDate.HasValue && w.NextMaintenanceDate < DateTime.Today
                })
                .FirstOrDefaultAsync();

            if (weighbridge == null)
            {
                return NotFound(ApiResponse<WeighbridgeDetailDto>.ErrorResponse("Weighbridge not found"));
            }

            return Ok(ApiResponse<WeighbridgeDetailDto>.SuccessResponse(weighbridge));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WeighbridgeDetailDto>.ErrorResponse("Error retrieving weighbridge", ex.Message));
        }
    }

    /// <summary>
    /// Create new weighbridge
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<WeighbridgeDetailDto>>> CreateWeighbridge(CreateWeighbridgeDto dto)
    {
        try
        {
            var weighbridge = new Weighbridge
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Code = dto.Code,
                Location = dto.Location,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Status = dto.Status,
                Type = dto.Type,
                MaxCapacity = dto.MaxCapacity,
                MinCapacity = dto.MinCapacity,
                Accuracy = dto.Accuracy,
                Manufacturer = dto.Manufacturer,
                Model = dto.Model,
                SerialNumber = dto.SerialNumber,
                InstallationDate = dto.InstallationDate,
                CertificateNumber = dto.CertificateNumber,
                CertificateExpiryDate = dto.CertificateExpiryDate,
                CalibrationAuthority = dto.CalibrationAuthority,
                OperatingHours = dto.OperatingHours,
                ContactPerson = dto.ContactPerson,
                ContactPhone = dto.ContactPhone,
                ServiceFee = dto.ServiceFee,
                Currency = dto.Currency,
                OrganizationId = dto.OrganizationId,
                Notes = dto.Notes
            };

            _context.Weighbridges.Add(weighbridge);
            await _context.SaveChangesAsync();

            var responseDto = new WeighbridgeDetailDto
            {
                Id = weighbridge.Id,
                Name = weighbridge.Name,
                Code = weighbridge.Code,
                Location = weighbridge.Location,
                Latitude = weighbridge.Latitude,
                Longitude = weighbridge.Longitude,
                Status = weighbridge.Status,
                Type = weighbridge.Type,
                MaxCapacity = weighbridge.MaxCapacity,
                MinCapacity = weighbridge.MinCapacity,
                Accuracy = weighbridge.Accuracy,
                Manufacturer = weighbridge.Manufacturer,
                Model = weighbridge.Model,
                SerialNumber = weighbridge.SerialNumber,
                InstallationDate = weighbridge.InstallationDate,
                LastCalibrationDate = weighbridge.LastCalibrationDate,
                NextCalibrationDate = weighbridge.NextCalibrationDate,
                LastMaintenanceDate = weighbridge.LastMaintenanceDate,
                NextMaintenanceDate = weighbridge.NextMaintenanceDate,
                CertificateNumber = weighbridge.CertificateNumber,
                CertificateExpiryDate = weighbridge.CertificateExpiryDate,
                CalibrationAuthority = weighbridge.CalibrationAuthority,
                OperatingHours = weighbridge.OperatingHours,
                ContactPerson = weighbridge.ContactPerson,
                ContactPhone = weighbridge.ContactPhone,
                ServiceFee = weighbridge.ServiceFee,
                Currency = weighbridge.Currency,
                IsActive = weighbridge.IsActive,
                OrganizationId = weighbridge.OrganizationId,
                Notes = weighbridge.Notes,
                CreatedAt = weighbridge.CreatedAt,
                UpdatedAt = weighbridge.UpdatedAt,
                HasCalibrations = false,
                HasMaintenanceRecords = false,
                HasTransactions = false,
                HasDocuments = false,
                IsCalibrationDue = false,
                IsMaintenanceDue = false
            };

            return CreatedAtAction(nameof(GetWeighbridge), 
                new { id = weighbridge.Id }, 
                ApiResponse<WeighbridgeDetailDto>.SuccessResponse(responseDto, "Weighbridge created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WeighbridgeDetailDto>.ErrorResponse("Error creating weighbridge", ex.Message));
        }
    }

    /// <summary>
    /// Update weighbridge
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<WeighbridgeDetailDto>>> UpdateWeighbridge(Guid id, UpdateWeighbridgeDto dto)
    {
        try
        {
            var weighbridge = await _context.Weighbridges.FindAsync(id);
            if (weighbridge == null)
            {
                return NotFound(ApiResponse<WeighbridgeDetailDto>.ErrorResponse("Weighbridge not found"));
            }

            weighbridge.Name = dto.Name;
            weighbridge.Location = dto.Location;
            weighbridge.Latitude = dto.Latitude;
            weighbridge.Longitude = dto.Longitude;
            weighbridge.Status = dto.Status;
            weighbridge.Type = dto.Type;
            weighbridge.MaxCapacity = dto.MaxCapacity;
            weighbridge.MinCapacity = dto.MinCapacity;
            weighbridge.Accuracy = dto.Accuracy;
            weighbridge.Manufacturer = dto.Manufacturer;
            weighbridge.Model = dto.Model;
            weighbridge.CertificateNumber = dto.CertificateNumber;
            weighbridge.CertificateExpiryDate = dto.CertificateExpiryDate;
            weighbridge.CalibrationAuthority = dto.CalibrationAuthority;
            weighbridge.OperatingHours = dto.OperatingHours;
            weighbridge.ContactPerson = dto.ContactPerson;
            weighbridge.ContactPhone = dto.ContactPhone;
            weighbridge.ServiceFee = dto.ServiceFee;
            weighbridge.Currency = dto.Currency;
            weighbridge.IsActive = dto.IsActive;
            weighbridge.Notes = dto.Notes;
            weighbridge.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var responseDto = await _context.Weighbridges
                .Where(w => w.Id == id)
                .Select(w => new WeighbridgeDetailDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Code = w.Code,
                    Location = w.Location,
                    Latitude = w.Latitude,
                    Longitude = w.Longitude,
                    Status = w.Status,
                    Type = w.Type,
                    MaxCapacity = w.MaxCapacity,
                    MinCapacity = w.MinCapacity,
                    Accuracy = w.Accuracy,
                    Manufacturer = w.Manufacturer,
                    Model = w.Model,
                    SerialNumber = w.SerialNumber,
                    InstallationDate = w.InstallationDate,
                    LastCalibrationDate = w.LastCalibrationDate,
                    NextCalibrationDate = w.NextCalibrationDate,
                    LastMaintenanceDate = w.LastMaintenanceDate,
                    NextMaintenanceDate = w.NextMaintenanceDate,
                    CertificateNumber = w.CertificateNumber,
                    CertificateExpiryDate = w.CertificateExpiryDate,
                    CalibrationAuthority = w.CalibrationAuthority,
                    OperatingHours = w.OperatingHours,
                    ContactPerson = w.ContactPerson,
                    ContactPhone = w.ContactPhone,
                    ServiceFee = w.ServiceFee,
                    Currency = w.Currency,
                    IsActive = w.IsActive,
                    OrganizationId = w.OrganizationId,
                    Notes = w.Notes,
                    CreatedAt = w.CreatedAt,
                    UpdatedAt = w.UpdatedAt,
                    HasCalibrations = w.Calibrations.Any(),
                    HasMaintenanceRecords = w.MaintenanceRecords.Any(),
                    HasTransactions = w.Transactions.Any(),
                    HasDocuments = w.Documents.Any(d => d.IsActive),
                    IsCalibrationDue = w.NextCalibrationDate.HasValue && w.NextCalibrationDate < DateTime.Today,
                    IsMaintenanceDue = w.NextMaintenanceDate.HasValue && w.NextMaintenanceDate < DateTime.Today
                })
                .FirstOrDefaultAsync();

            return Ok(ApiResponse<WeighbridgeDetailDto>.SuccessResponse(responseDto!, "Weighbridge updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WeighbridgeDetailDto>.ErrorResponse("Error updating weighbridge", ex.Message));
        }
    }

    /// <summary>
    /// Delete weighbridge (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteWeighbridge(Guid id)
    {
        try
        {
            var weighbridge = await _context.Weighbridges.FindAsync(id);
            if (weighbridge == null)
            {
                return NotFound(ApiResponse.CreateError("Weighbridge not found"));
            }

            weighbridge.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Weighbridge deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting weighbridge", ex.Message));
        }
    }

    /// <summary>
    /// Get weighbridge transactions
    /// </summary>
    [HttpGet("{id}/transactions")]
    public async Task<ActionResult<ApiResponse<PagedResult<WeighbridgeTransactionDto>>>> GetWeighbridgeTransactions(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.WeighbridgeTransactions
                .Where(t => t.WeighbridgeId == id)
                .Select(t => new WeighbridgeTransactionDto
                {
                    Id = t.Id,
                    WeighbridgeId = t.WeighbridgeId,
                    TicketNumber = t.TicketNumber,
                    TransactionDate = t.TransactionDate,
                    TransactionType = t.TransactionType,
                    VehicleNumber = t.VehicleNumber,
                    DriverName = t.DriverName,
                    CustomerName = t.CustomerName,
                    ProductType = t.ProductType,
                    GrossWeight = t.GrossWeight,
                    TareWeight = t.TareWeight,
                    NetWeight = t.NetWeight,
                    WeightUnit = t.WeightUnit,
                    ServiceFee = t.ServiceFee,
                    PaymentMethod = t.PaymentMethod,
                    Status = t.Status,
                    OperatorName = t.OperatorName,
                    Comments = t.Comments,
                    ReceiptUrl = t.ReceiptUrl,
                    IsPrinted = t.IsPrinted,
                    CreatedAt = t.CreatedAt
                })
                .OrderByDescending(t => t.TransactionDate)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<WeighbridgeTransactionDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<WeighbridgeTransactionDto>>.ErrorResponse("Error retrieving weighbridge transactions", ex.Message));
        }
    }

    /// <summary>
    /// Create weighbridge transaction
    /// </summary>
    [HttpPost("{id}/transactions")]
    public async Task<ActionResult<ApiResponse<WeighbridgeTransactionDto>>> CreateTransaction(Guid id, CreateWeighbridgeTransactionDto dto)
    {
        try
        {
            if (!await WeighbridgeExists(id))
            {
                return NotFound(ApiResponse<WeighbridgeTransactionDto>.ErrorResponse("Weighbridge not found"));
            }

            var transaction = new WeighbridgeTransaction
            {
                Id = Guid.NewGuid(),
                WeighbridgeId = id,
                TicketNumber = dto.TicketNumber,
                TransactionDate = dto.TransactionDate,
                TransactionType = dto.TransactionType,
                VehicleNumber = dto.VehicleNumber,
                DriverName = dto.DriverName,
                CustomerName = dto.CustomerName,
                ProductType = dto.ProductType,
                GrossWeight = dto.GrossWeight,
                TareWeight = dto.TareWeight,
                NetWeight = dto.NetWeight,
                WeightUnit = dto.WeightUnit,
                ServiceFee = dto.ServiceFee,
                PaymentMethod = dto.PaymentMethod,
                OperatorName = dto.OperatorName,
                Comments = dto.Comments
            };

            _context.WeighbridgeTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            var responseDto = new WeighbridgeTransactionDto
            {
                Id = transaction.Id,
                WeighbridgeId = transaction.WeighbridgeId,
                TicketNumber = transaction.TicketNumber,
                TransactionDate = transaction.TransactionDate,
                TransactionType = transaction.TransactionType,
                VehicleNumber = transaction.VehicleNumber,
                DriverName = transaction.DriverName,
                CustomerName = transaction.CustomerName,
                ProductType = transaction.ProductType,
                GrossWeight = transaction.GrossWeight,
                TareWeight = transaction.TareWeight,
                NetWeight = transaction.NetWeight,
                WeightUnit = transaction.WeightUnit,
                ServiceFee = transaction.ServiceFee,
                PaymentMethod = transaction.PaymentMethod,
                Status = transaction.Status,
                OperatorName = transaction.OperatorName,
                Comments = transaction.Comments,
                ReceiptUrl = transaction.ReceiptUrl,
                IsPrinted = transaction.IsPrinted,
                CreatedAt = transaction.CreatedAt
            };

            return Ok(ApiResponse<WeighbridgeTransactionDto>.SuccessResponse(responseDto, "Transaction created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WeighbridgeTransactionDto>.ErrorResponse("Error creating transaction", ex.Message));
        }
    }

    /// <summary>
    /// Get weighbridge calibrations
    /// </summary>
    [HttpGet("{id}/calibrations")]
    public async Task<ActionResult<ApiResponse<PagedResult<WeighbridgeCalibrationDto>>>> GetWeighbridgeCalibrations(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.WeighbridgeCalibrations
                .Where(c => c.WeighbridgeId == id)
                .Select(c => new WeighbridgeCalibrationDto
                {
                    Id = c.Id,
                    WeighbridgeId = c.WeighbridgeId,
                    CalibrationDate = c.CalibrationDate,
                    CalibrationBy = c.CalibrationBy,
                    CertificateNumber = c.CertificateNumber,
                    CertificateExpiryDate = c.CertificateExpiryDate,
                    CalibrationAuthority = c.CalibrationAuthority,
                    Status = c.Status,
                    AccuracyAchieved = c.AccuracyAchieved,
                    TestWeights = c.TestWeights,
                    TestResults = c.TestResults,
                    Adjustments = c.Adjustments,
                    CalibrationCost = c.CalibrationCost,
                    NextCalibrationDate = c.NextCalibrationDate,
                    Notes = c.Notes,
                    CreatedAt = c.CreatedAt,
                    IsExpired = c.CertificateExpiryDate < DateTime.Today,
                    DaysToExpiry = c.CertificateExpiryDate > DateTime.Today ? (int)(c.CertificateExpiryDate - DateTime.Today).TotalDays : 0
                })
                .OrderByDescending(c => c.CalibrationDate)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<WeighbridgeCalibrationDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<WeighbridgeCalibrationDto>>.ErrorResponse("Error retrieving weighbridge calibrations", ex.Message));
        }
    }

    /// <summary>
    /// Add calibration record to weighbridge
    /// </summary>
    [HttpPost("{id}/calibrations")]
    public async Task<ActionResult<ApiResponse<WeighbridgeCalibrationDto>>> CreateWeighbridgeCalibration(Guid id, CreateWeighbridgeCalibrationDto dto)
    {
        try
        {
            if (!await WeighbridgeExists(id))
            {
                return NotFound(ApiResponse<WeighbridgeCalibrationDto>.ErrorResponse("Weighbridge not found"));
            }

            var calibration = new WeighbridgeCalibration
            {
                Id = Guid.NewGuid(),
                WeighbridgeId = id,
                CalibrationDate = dto.CalibrationDate,
                CalibrationBy = dto.CalibrationBy,
                CertificateNumber = dto.CertificateNumber,
                CertificateExpiryDate = dto.CertificateExpiryDate,
                CalibrationAuthority = dto.CalibrationAuthority,
                Status = dto.Status,
                AccuracyAchieved = dto.AccuracyAchieved,
                TestWeights = dto.TestWeights,
                TestResults = dto.TestResults,
                Adjustments = dto.Adjustments,
                CalibrationCost = dto.CalibrationCost,
                NextCalibrationDate = dto.NextCalibrationDate,
                Notes = dto.Notes
            };

            _context.WeighbridgeCalibrations.Add(calibration);
            
            // Update weighbridge last calibration date
            var weighbridge = await _context.Weighbridges.FindAsync(id);
            if (weighbridge != null)
            {
                weighbridge.LastCalibrationDate = dto.CalibrationDate;
                weighbridge.NextCalibrationDate = dto.NextCalibrationDate;
            }

            await _context.SaveChangesAsync();

            var responseDto = new WeighbridgeCalibrationDto
            {
                Id = calibration.Id,
                WeighbridgeId = calibration.WeighbridgeId,
                CalibrationDate = calibration.CalibrationDate,
                CalibrationBy = calibration.CalibrationBy,
                CertificateNumber = calibration.CertificateNumber,
                CertificateExpiryDate = calibration.CertificateExpiryDate,
                CalibrationAuthority = calibration.CalibrationAuthority,
                Status = calibration.Status,
                AccuracyAchieved = calibration.AccuracyAchieved,
                TestWeights = calibration.TestWeights,
                TestResults = calibration.TestResults,
                Adjustments = calibration.Adjustments,
                CalibrationCost = calibration.CalibrationCost,
                NextCalibrationDate = calibration.NextCalibrationDate,
                Notes = calibration.Notes,
                CreatedAt = calibration.CreatedAt,
                IsExpired = calibration.CertificateExpiryDate < DateTime.Today,
                DaysToExpiry = calibration.CertificateExpiryDate > DateTime.Today ? (int)(calibration.CertificateExpiryDate - DateTime.Today).TotalDays : 0
            };

            return Ok(ApiResponse<WeighbridgeCalibrationDto>.SuccessResponse(responseDto, "Weighbridge calibration record created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WeighbridgeCalibrationDto>.ErrorResponse("Error creating weighbridge calibration record", ex.Message));
        }
    }

    /// <summary>
    /// Get weighbridge maintenance records
    /// </summary>
    [HttpGet("{id}/maintenance")]
    public async Task<ActionResult<ApiResponse<PagedResult<WeighbridgeMaintenanceDto>>>> GetWeighbridgeMaintenance(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.WeighbridgeMaintenances
                .Where(m => m.WeighbridgeId == id)
                .Select(m => new WeighbridgeMaintenanceDto
                {
                    Id = m.Id,
                    WeighbridgeId = m.WeighbridgeId,
                    MaintenanceDate = m.MaintenanceDate,
                    MaintenanceType = m.MaintenanceType,
                    ServiceProvider = m.ServiceProvider,
                    WorkOrderNumber = m.WorkOrderNumber,
                    WorkPerformed = m.WorkPerformed,
                    PartsReplaced = m.PartsReplaced,
                    ServiceCost = m.ServiceCost,
                    PartsCost = m.PartsCost,
                    TotalCost = m.TotalCost,
                    Status = m.Status,
                    NextMaintenanceDate = m.NextMaintenanceDate,
                    Warranty = m.Warranty,
                    TechnicianName = m.TechnicianName,
                    Notes = m.Notes,
                    CreatedAt = m.CreatedAt,
                    IsWarrantyActive = !string.IsNullOrEmpty(m.Warranty)
                })
                .OrderByDescending(m => m.MaintenanceDate)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<WeighbridgeMaintenanceDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<WeighbridgeMaintenanceDto>>.ErrorResponse("Error retrieving weighbridge maintenance records", ex.Message));
        }
    }

    /// <summary>
    /// Add maintenance record to weighbridge
    /// </summary>
    [HttpPost("{id}/maintenance")]
    public async Task<ActionResult<ApiResponse<WeighbridgeMaintenanceDto>>> CreateWeighbridgeMaintenance(Guid id, CreateWeighbridgeMaintenanceDto dto)
    {
        try
        {
            if (!await WeighbridgeExists(id))
            {
                return NotFound(ApiResponse<WeighbridgeMaintenanceDto>.ErrorResponse("Weighbridge not found"));
            }

            var maintenance = new WeighbridgeMaintenance
            {
                Id = Guid.NewGuid(),
                WeighbridgeId = id,
                MaintenanceDate = dto.MaintenanceDate,
                MaintenanceType = dto.MaintenanceType,
                ServiceProvider = dto.ServiceProvider,
                WorkOrderNumber = dto.WorkOrderNumber,
                WorkPerformed = dto.WorkPerformed,
                PartsReplaced = dto.PartsReplaced,
                ServiceCost = dto.ServiceCost,
                PartsCost = dto.PartsCost,
                TotalCost = dto.TotalCost,
                Status = dto.Status,
                NextMaintenanceDate = dto.NextMaintenanceDate,
                Warranty = dto.Warranty,
                TechnicianName = dto.TechnicianName,
                Notes = dto.Notes
            };

            _context.WeighbridgeMaintenances.Add(maintenance);
            
            // Update weighbridge last maintenance date
            var weighbridge = await _context.Weighbridges.FindAsync(id);
            if (weighbridge != null)
            {
                weighbridge.LastMaintenanceDate = dto.MaintenanceDate;
                weighbridge.NextMaintenanceDate = dto.NextMaintenanceDate;
            }

            await _context.SaveChangesAsync();

            var responseDto = new WeighbridgeMaintenanceDto
            {
                Id = maintenance.Id,
                WeighbridgeId = maintenance.WeighbridgeId,
                MaintenanceDate = maintenance.MaintenanceDate,
                MaintenanceType = maintenance.MaintenanceType,
                ServiceProvider = maintenance.ServiceProvider,
                WorkOrderNumber = maintenance.WorkOrderNumber,
                WorkPerformed = maintenance.WorkPerformed,
                PartsReplaced = maintenance.PartsReplaced,
                ServiceCost = maintenance.ServiceCost,
                PartsCost = maintenance.PartsCost,
                TotalCost = maintenance.TotalCost,
                Status = maintenance.Status,
                NextMaintenanceDate = maintenance.NextMaintenanceDate,
                Warranty = maintenance.Warranty,
                TechnicianName = maintenance.TechnicianName,
                Notes = maintenance.Notes,
                CreatedAt = maintenance.CreatedAt,
                IsWarrantyActive = !string.IsNullOrEmpty(maintenance.Warranty)
            };

            return Ok(ApiResponse<WeighbridgeMaintenanceDto>.SuccessResponse(responseDto, "Weighbridge maintenance record created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WeighbridgeMaintenanceDto>.ErrorResponse("Error creating weighbridge maintenance record", ex.Message));
        }
    }

    /// <summary>
    /// Get weighbridge documents
    /// </summary>
    [HttpGet("{id}/documents")]
    public async Task<ActionResult<ApiResponse<PagedResult<WeighbridgeDocumentDto>>>> GetWeighbridgeDocuments(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.WeighbridgeDocuments
                .Where(d => d.WeighbridgeId == id)
                .Select(d => new WeighbridgeDocumentDto
                {
                    Id = d.Id,
                    WeighbridgeId = d.WeighbridgeId,
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
                    UploadedBy = d.UploadedBy,
                    UploadedAt = d.UploadedAt,
                    IsExpired = d.ExpiryDate.HasValue && d.ExpiryDate < DateTime.Today
                })
                .OrderByDescending(d => d.UploadedAt)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<WeighbridgeDocumentDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<WeighbridgeDocumentDto>>.ErrorResponse("Error retrieving weighbridge documents", ex.Message));
        }
    }

    /// <summary>
    /// Add document to weighbridge
    /// </summary>
    [HttpPost("{id}/documents")]
    public async Task<ActionResult<ApiResponse<WeighbridgeDocumentDto>>> CreateWeighbridgeDocument(Guid id, CreateWeighbridgeDocumentDto dto)
    {
        try
        {
            if (!await WeighbridgeExists(id))
            {
                return NotFound(ApiResponse<WeighbridgeDocumentDto>.ErrorResponse("Weighbridge not found"));
            }

            var document = new WeighbridgeDocument
            {
                Id = Guid.NewGuid(),
                WeighbridgeId = id,
                FileName = dto.FileName,
                OriginalFileName = dto.OriginalFileName,
                ContentType = dto.ContentType,
                FilePath = dto.FilePath,
                FileUrl = dto.FileUrl,
                FileSize = dto.FileSize,
                Category = dto.Category,
                Description = dto.Description,
                ExpiryDate = dto.ExpiryDate,
                UploadedBy = dto.UploadedBy,
                UploadedAt = DateTime.UtcNow
            };

            _context.WeighbridgeDocuments.Add(document);
            await _context.SaveChangesAsync();

            var responseDto = new WeighbridgeDocumentDto
            {
                Id = document.Id,
                WeighbridgeId = document.WeighbridgeId,
                FileName = document.FileName,
                OriginalFileName = document.OriginalFileName,
                ContentType = document.ContentType,
                FilePath = document.FilePath,
                FileUrl = document.FileUrl,
                FileSize = document.FileSize,
                Category = document.Category,
                Description = document.Description,
                ExpiryDate = document.ExpiryDate,
                IsActive = document.IsActive,
                UploadedBy = document.UploadedBy,
                UploadedAt = document.UploadedAt,
                IsExpired = document.ExpiryDate.HasValue && document.ExpiryDate < DateTime.Today
            };

            return Ok(ApiResponse<WeighbridgeDocumentDto>.SuccessResponse(responseDto, "Weighbridge document uploaded successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WeighbridgeDocumentDto>.ErrorResponse("Error uploading weighbridge document", ex.Message));
        }
    }

    private async Task<bool> WeighbridgeExists(Guid id)
    {
        return await _context.Weighbridges.AnyAsync(e => e.Id == id);
    }
}