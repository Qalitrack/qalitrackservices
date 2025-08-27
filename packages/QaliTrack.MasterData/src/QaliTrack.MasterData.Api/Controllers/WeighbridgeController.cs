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
    /// Partially update weighbridge
    /// </summary>
    [HttpPatch("{id}")]
    public async Task<ActionResult<ApiResponse<WeighbridgeDetailDto>>> PatchWeighbridge(Guid id, PatchWeighbridgeDto dto)
    {
        try
        {
            var weighbridge = await _context.Weighbridges.FindAsync(id);
            if (weighbridge == null)
            {
                return NotFound(ApiResponse<WeighbridgeDetailDto>.ErrorResponse("Weighbridge not found"));
            }

            if (dto.Name != null) weighbridge.Name = dto.Name;
            if (dto.Location != null) weighbridge.Location = dto.Location;
            if (dto.Latitude.HasValue) weighbridge.Latitude = dto.Latitude.Value;
            if (dto.Longitude.HasValue) weighbridge.Longitude = dto.Longitude.Value;
            if (dto.Status != null) weighbridge.Status = dto.Status;
            if (dto.Type != null) weighbridge.Type = dto.Type;
            if (dto.MaxCapacity.HasValue) weighbridge.MaxCapacity = dto.MaxCapacity.Value;
            if (dto.MinCapacity.HasValue) weighbridge.MinCapacity = dto.MinCapacity.Value;
            if (dto.Accuracy.HasValue) weighbridge.Accuracy = dto.Accuracy.Value;
            if (dto.Manufacturer != null) weighbridge.Manufacturer = dto.Manufacturer;
            if (dto.Model != null) weighbridge.Model = dto.Model;
            if (dto.CertificateNumber != null) weighbridge.CertificateNumber = dto.CertificateNumber;
            if (dto.CertificateExpiryDate.HasValue) weighbridge.CertificateExpiryDate = dto.CertificateExpiryDate;
            if (dto.CalibrationAuthority != null) weighbridge.CalibrationAuthority = dto.CalibrationAuthority;
            if (dto.OperatingHours != null) weighbridge.OperatingHours = dto.OperatingHours;
            if (dto.ContactPerson != null) weighbridge.ContactPerson = dto.ContactPerson;
            if (dto.ContactPhone != null) weighbridge.ContactPhone = dto.ContactPhone;
            if (dto.ServiceFee.HasValue) weighbridge.ServiceFee = dto.ServiceFee;
            if (dto.Currency != null) weighbridge.Currency = dto.Currency;
            if (dto.IsActive.HasValue) weighbridge.IsActive = dto.IsActive.Value;
            if (dto.Notes != null) weighbridge.Notes = dto.Notes;
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
    /// Get single weighbridge transaction
    /// </summary>
    [HttpGet("{id}/transactions/{transactionId}")]
    public async Task<ActionResult<ApiResponse<WeighbridgeTransactionDto>>> GetWeighbridgeTransaction(Guid id, Guid transactionId)
    {
        try
        {
            var transaction = await _context.WeighbridgeTransactions
                .Where(t => t.Id == transactionId && t.WeighbridgeId == id)
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
                .FirstOrDefaultAsync();

            if (transaction == null)
            {
                return NotFound(ApiResponse<WeighbridgeTransactionDto>.ErrorResponse("Transaction not found"));
            }

            return Ok(ApiResponse<WeighbridgeTransactionDto>.SuccessResponse(transaction));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WeighbridgeTransactionDto>.ErrorResponse("Error retrieving transaction", ex.Message));
        }
    }

    /// <summary>
    /// Update weighbridge transaction
    /// </summary>
    [HttpPut("{id}/transactions/{transactionId}")]
    public async Task<ActionResult<ApiResponse<WeighbridgeTransactionDto>>> UpdateWeighbridgeTransaction(Guid id, Guid transactionId, UpdateWeighbridgeTransactionDto dto)
    {
        try
        {
            var transaction = await _context.WeighbridgeTransactions
                .FirstOrDefaultAsync(t => t.Id == transactionId && t.WeighbridgeId == id);

            if (transaction == null)
            {
                return NotFound(ApiResponse<WeighbridgeTransactionDto>.ErrorResponse("Transaction not found"));
            }

            transaction.TicketNumber = dto.TicketNumber;
            transaction.TransactionDate = dto.TransactionDate;
            transaction.TransactionType = dto.TransactionType;
            transaction.VehicleNumber = dto.VehicleNumber;
            transaction.DriverName = dto.DriverName;
            transaction.CustomerName = dto.CustomerName;
            transaction.ProductType = dto.ProductType;
            transaction.GrossWeight = dto.GrossWeight;
            transaction.TareWeight = dto.TareWeight;
            transaction.NetWeight = dto.NetWeight;
            transaction.WeightUnit = dto.WeightUnit;
            transaction.ServiceFee = dto.ServiceFee;
            transaction.PaymentMethod = dto.PaymentMethod;
            transaction.OperatorName = dto.OperatorName;
            transaction.Comments = dto.Comments;

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

            return Ok(ApiResponse<WeighbridgeTransactionDto>.SuccessResponse(responseDto, "Transaction updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WeighbridgeTransactionDto>.ErrorResponse("Error updating transaction", ex.Message));
        }
    }

    /// <summary>
    /// Partially update weighbridge transaction
    /// </summary>
    [HttpPatch("{id}/transactions/{transactionId}")]
    public async Task<ActionResult<ApiResponse<WeighbridgeTransactionDto>>> PatchWeighbridgeTransaction(Guid id, Guid transactionId, PatchWeighbridgeTransactionDto dto)
    {
        try
        {
            var transaction = await _context.WeighbridgeTransactions
                .FirstOrDefaultAsync(t => t.Id == transactionId && t.WeighbridgeId == id);

            if (transaction == null)
            {
                return NotFound(ApiResponse<WeighbridgeTransactionDto>.ErrorResponse("Transaction not found"));
            }

            if (dto.TicketNumber != null) transaction.TicketNumber = dto.TicketNumber;
            if (dto.TransactionDate.HasValue) transaction.TransactionDate = dto.TransactionDate.Value;
            if (dto.TransactionType != null) transaction.TransactionType = dto.TransactionType;
            if (dto.VehicleNumber != null) transaction.VehicleNumber = dto.VehicleNumber;
            if (dto.DriverName != null) transaction.DriverName = dto.DriverName;
            if (dto.CustomerName != null) transaction.CustomerName = dto.CustomerName;
            if (dto.ProductType != null) transaction.ProductType = dto.ProductType;
            if (dto.GrossWeight.HasValue) transaction.GrossWeight = dto.GrossWeight.Value;
            if (dto.TareWeight.HasValue) transaction.TareWeight = dto.TareWeight.Value;
            if (dto.NetWeight.HasValue) transaction.NetWeight = dto.NetWeight.Value;
            if (dto.WeightUnit != null) transaction.WeightUnit = dto.WeightUnit;
            if (dto.ServiceFee.HasValue) transaction.ServiceFee = dto.ServiceFee;
            if (dto.PaymentMethod != null) transaction.PaymentMethod = dto.PaymentMethod;
            if (dto.OperatorName != null) transaction.OperatorName = dto.OperatorName;
            if (dto.Comments != null) transaction.Comments = dto.Comments;

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

            return Ok(ApiResponse<WeighbridgeTransactionDto>.SuccessResponse(responseDto, "Transaction updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WeighbridgeTransactionDto>.ErrorResponse("Error updating transaction", ex.Message));
        }
    }

    /// <summary>
    /// Delete weighbridge transaction
    /// </summary>
    [HttpDelete("{id}/transactions/{transactionId}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteWeighbridgeTransaction(Guid id, Guid transactionId)
    {
        try
        {
            var transaction = await _context.WeighbridgeTransactions
                .FirstOrDefaultAsync(t => t.Id == transactionId && t.WeighbridgeId == id);

            if (transaction == null)
            {
                return NotFound(ApiResponse.CreateError("Transaction not found"));
            }

            _context.WeighbridgeTransactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Transaction deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting transaction", ex.Message));
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
    /// Get single weighbridge calibration
    /// </summary>
    [HttpGet("{id}/calibrations/{calibrationId}")]
    public async Task<ActionResult<ApiResponse<WeighbridgeCalibrationDto>>> GetWeighbridgeCalibration(Guid id, Guid calibrationId)
    {
        try
        {
            var calibration = await _context.WeighbridgeCalibrations
                .Where(c => c.Id == calibrationId && c.WeighbridgeId == id)
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
                .FirstOrDefaultAsync();

            if (calibration == null)
            {
                return NotFound(ApiResponse<WeighbridgeCalibrationDto>.ErrorResponse("Calibration record not found"));
            }

            return Ok(ApiResponse<WeighbridgeCalibrationDto>.SuccessResponse(calibration));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WeighbridgeCalibrationDto>.ErrorResponse("Error retrieving calibration record", ex.Message));
        }
    }

    /// <summary>
    /// Update weighbridge calibration
    /// </summary>
    [HttpPut("{id}/calibrations/{calibrationId}")]
    public async Task<ActionResult<ApiResponse<WeighbridgeCalibrationDto>>> UpdateWeighbridgeCalibration(Guid id, Guid calibrationId, UpdateWeighbridgeCalibrationDto dto)
    {
        try
        {
            var calibration = await _context.WeighbridgeCalibrations
                .FirstOrDefaultAsync(c => c.Id == calibrationId && c.WeighbridgeId == id);

            if (calibration == null)
            {
                return NotFound(ApiResponse<WeighbridgeCalibrationDto>.ErrorResponse("Calibration record not found"));
            }

            calibration.CalibrationDate = dto.CalibrationDate;
            calibration.CalibrationBy = dto.CalibrationBy;
            calibration.CertificateNumber = dto.CertificateNumber;
            calibration.CertificateExpiryDate = dto.CertificateExpiryDate;
            calibration.CalibrationAuthority = dto.CalibrationAuthority;
            calibration.Status = dto.Status;
            calibration.AccuracyAchieved = dto.AccuracyAchieved;
            calibration.TestWeights = dto.TestWeights;
            calibration.TestResults = dto.TestResults;
            calibration.Adjustments = dto.Adjustments;
            calibration.CalibrationCost = dto.CalibrationCost;
            calibration.NextCalibrationDate = dto.NextCalibrationDate;
            calibration.Notes = dto.Notes;

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

            return Ok(ApiResponse<WeighbridgeCalibrationDto>.SuccessResponse(responseDto, "Calibration record updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WeighbridgeCalibrationDto>.ErrorResponse("Error updating calibration record", ex.Message));
        }
    }

    /// <summary>
    /// Partially update weighbridge calibration
    /// </summary>
    [HttpPatch("{id}/calibrations/{calibrationId}")]
    public async Task<ActionResult<ApiResponse<WeighbridgeCalibrationDto>>> PatchWeighbridgeCalibration(Guid id, Guid calibrationId, PatchWeighbridgeCalibrationDto dto)
    {
        try
        {
            var calibration = await _context.WeighbridgeCalibrations
                .FirstOrDefaultAsync(c => c.Id == calibrationId && c.WeighbridgeId == id);

            if (calibration == null)
            {
                return NotFound(ApiResponse<WeighbridgeCalibrationDto>.ErrorResponse("Calibration record not found"));
            }

            if (dto.CalibrationDate.HasValue) calibration.CalibrationDate = dto.CalibrationDate.Value;
            if (dto.CalibrationBy != null) calibration.CalibrationBy = dto.CalibrationBy;
            if (dto.CertificateNumber != null) calibration.CertificateNumber = dto.CertificateNumber;
            if (dto.CertificateExpiryDate.HasValue) calibration.CertificateExpiryDate = dto.CertificateExpiryDate.Value;
            if (dto.CalibrationAuthority != null) calibration.CalibrationAuthority = dto.CalibrationAuthority;
            if (dto.Status != null) calibration.Status = dto.Status;
            if (dto.CalibrationCost.HasValue) calibration.CalibrationCost = dto.CalibrationCost.Value;
            if (dto.AccuracyAchieved.HasValue) calibration.AccuracyAchieved = dto.AccuracyAchieved;
            if (dto.TestWeights != null) calibration.TestWeights = dto.TestWeights;
            if (dto.TestResults != null) calibration.TestResults = dto.TestResults;
            if (dto.Adjustments != null) calibration.Adjustments = dto.Adjustments;
            if (dto.NextCalibrationDate.HasValue) calibration.NextCalibrationDate = dto.NextCalibrationDate;
            if (dto.Notes != null) calibration.Notes = dto.Notes;

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

            return Ok(ApiResponse<WeighbridgeCalibrationDto>.SuccessResponse(responseDto, "Calibration record updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WeighbridgeCalibrationDto>.ErrorResponse("Error updating calibration record", ex.Message));
        }
    }

    /// <summary>
    /// Delete weighbridge calibration
    /// </summary>
    [HttpDelete("{id}/calibrations/{calibrationId}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteWeighbridgeCalibration(Guid id, Guid calibrationId)
    {
        try
        {
            var calibration = await _context.WeighbridgeCalibrations
                .FirstOrDefaultAsync(c => c.Id == calibrationId && c.WeighbridgeId == id);

            if (calibration == null)
            {
                return NotFound(ApiResponse.CreateError("Calibration record not found"));
            }

            _context.WeighbridgeCalibrations.Remove(calibration);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Calibration record deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting calibration record", ex.Message));
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
    /// Get single weighbridge maintenance record
    /// </summary>
    [HttpGet("{id}/maintenance/{maintenanceId}")]
    public async Task<ActionResult<ApiResponse<WeighbridgeMaintenanceDto>>> GetWeighbridgeMaintenanceRecord(Guid id, Guid maintenanceId)
    {
        try
        {
            var maintenance = await _context.WeighbridgeMaintenances
                .Where(m => m.Id == maintenanceId && m.WeighbridgeId == id)
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
                .FirstOrDefaultAsync();

            if (maintenance == null)
            {
                return NotFound(ApiResponse<WeighbridgeMaintenanceDto>.ErrorResponse("Maintenance record not found"));
            }

            return Ok(ApiResponse<WeighbridgeMaintenanceDto>.SuccessResponse(maintenance));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WeighbridgeMaintenanceDto>.ErrorResponse("Error retrieving maintenance record", ex.Message));
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
    /// Update weighbridge maintenance record
    /// </summary>
    [HttpPut("{id}/maintenance/{maintenanceId}")]
    public async Task<ActionResult<ApiResponse<WeighbridgeMaintenanceDto>>> UpdateWeighbridgeMaintenance(Guid id, Guid maintenanceId, UpdateWeighbridgeMaintenanceDto dto)
    {
        try
        {
            var maintenance = await _context.WeighbridgeMaintenances
                .FirstOrDefaultAsync(m => m.Id == maintenanceId && m.WeighbridgeId == id);

            if (maintenance == null)
            {
                return NotFound(ApiResponse<WeighbridgeMaintenanceDto>.ErrorResponse("Maintenance record not found"));
            }

            maintenance.MaintenanceDate = dto.MaintenanceDate;
            maintenance.MaintenanceType = dto.MaintenanceType;
            maintenance.ServiceProvider = dto.ServiceProvider;
            maintenance.Status = dto.Status;
            maintenance.WorkOrderNumber = dto.WorkOrderNumber;
            maintenance.WorkPerformed = dto.WorkPerformed;
            maintenance.PartsReplaced = dto.PartsReplaced;
            maintenance.ServiceCost = dto.ServiceCost;
            maintenance.PartsCost = dto.PartsCost;
            maintenance.TotalCost = dto.TotalCost;
            maintenance.NextMaintenanceDate = dto.NextMaintenanceDate;
            maintenance.Warranty = dto.Warranty;
            maintenance.TechnicianName = dto.TechnicianName;
            maintenance.Notes = dto.Notes;

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

            return Ok(ApiResponse<WeighbridgeMaintenanceDto>.SuccessResponse(responseDto, "Maintenance record updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WeighbridgeMaintenanceDto>.ErrorResponse("Error updating maintenance record", ex.Message));
        }
    }

    /// <summary>
    /// Partially update weighbridge maintenance record
    /// </summary>
    [HttpPatch("{id}/maintenance/{maintenanceId}")]
    public async Task<ActionResult<ApiResponse<WeighbridgeMaintenanceDto>>> PatchWeighbridgeMaintenance(Guid id, Guid maintenanceId, PatchWeighbridgeMaintenanceDto dto)
    {
        try
        {
            var maintenance = await _context.WeighbridgeMaintenances
                .FirstOrDefaultAsync(m => m.Id == maintenanceId && m.WeighbridgeId == id);

            if (maintenance == null)
            {
                return NotFound(ApiResponse<WeighbridgeMaintenanceDto>.ErrorResponse("Maintenance record not found"));
            }

            if (dto.MaintenanceDate.HasValue) maintenance.MaintenanceDate = dto.MaintenanceDate.Value;
            if (dto.MaintenanceType != null) maintenance.MaintenanceType = dto.MaintenanceType;
            if (dto.ServiceProvider != null) maintenance.ServiceProvider = dto.ServiceProvider;
            if (dto.Status != null) maintenance.Status = dto.Status;
            if (dto.WorkOrderNumber != null) maintenance.WorkOrderNumber = dto.WorkOrderNumber;
            if (dto.WorkPerformed != null) maintenance.WorkPerformed = dto.WorkPerformed;
            if (dto.PartsReplaced != null) maintenance.PartsReplaced = dto.PartsReplaced;
            if (dto.ServiceCost.HasValue) maintenance.ServiceCost = dto.ServiceCost.Value;
            if (dto.PartsCost.HasValue) maintenance.PartsCost = dto.PartsCost.Value;
            if (dto.TotalCost.HasValue) maintenance.TotalCost = dto.TotalCost.Value;
            if (dto.NextMaintenanceDate.HasValue) maintenance.NextMaintenanceDate = dto.NextMaintenanceDate;
            if (dto.Warranty != null) maintenance.Warranty = dto.Warranty;
            if (dto.TechnicianName != null) maintenance.TechnicianName = dto.TechnicianName;
            if (dto.Notes != null) maintenance.Notes = dto.Notes;

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

            return Ok(ApiResponse<WeighbridgeMaintenanceDto>.SuccessResponse(responseDto, "Maintenance record updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WeighbridgeMaintenanceDto>.ErrorResponse("Error updating maintenance record", ex.Message));
        }
    }

    /// <summary>
    /// Delete weighbridge maintenance record
    /// </summary>
    [HttpDelete("{id}/maintenance/{maintenanceId}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteWeighbridgeMaintenance(Guid id, Guid maintenanceId)
    {
        try
        {
            var maintenance = await _context.WeighbridgeMaintenances
                .FirstOrDefaultAsync(m => m.Id == maintenanceId && m.WeighbridgeId == id);

            if (maintenance == null)
            {
                return NotFound(ApiResponse.CreateError("Maintenance record not found"));
            }

            _context.WeighbridgeMaintenances.Remove(maintenance);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Maintenance record deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting maintenance record", ex.Message));
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

    /// <summary>
    /// Update weighbridge document
    /// </summary>
    [HttpPut("{id}/documents/{documentId}")]
    public async Task<ActionResult<ApiResponse<WeighbridgeDocumentDto>>> UpdateWeighbridgeDocument(Guid id, Guid documentId, UpdateWeighbridgeDocumentDto dto)
    {
        try
        {
            var document = await _context.WeighbridgeDocuments
                .FirstOrDefaultAsync(d => d.Id == documentId && d.WeighbridgeId == id);

            if (document == null)
            {
                return NotFound(ApiResponse<WeighbridgeDocumentDto>.ErrorResponse("Document not found"));
            }

            document.FileName = dto.FileName;
            document.OriginalFileName = dto.OriginalFileName;
            document.ContentType = dto.ContentType;
            document.FilePath = dto.FilePath;
            document.FileUrl = dto.FileUrl;
            document.FileSize = dto.FileSize;
            document.Category = dto.Category;
            document.Description = dto.Description;
            document.ExpiryDate = dto.ExpiryDate;
            document.IsActive = dto.IsActive;
            document.UploadedBy = dto.UploadedBy;

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

            return Ok(ApiResponse<WeighbridgeDocumentDto>.SuccessResponse(responseDto, "Document updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WeighbridgeDocumentDto>.ErrorResponse("Error updating document", ex.Message));
        }
    }

    /// <summary>
    /// Partially update weighbridge document
    /// </summary>
    [HttpPatch("{id}/documents/{documentId}")]
    public async Task<ActionResult<ApiResponse<WeighbridgeDocumentDto>>> PatchWeighbridgeDocument(Guid id, Guid documentId, PatchWeighbridgeDocumentDto dto)
    {
        try
        {
            var document = await _context.WeighbridgeDocuments
                .FirstOrDefaultAsync(d => d.Id == documentId && d.WeighbridgeId == id);

            if (document == null)
            {
                return NotFound(ApiResponse<WeighbridgeDocumentDto>.ErrorResponse("Document not found"));
            }

            if (dto.FileName != null) document.FileName = dto.FileName;
            if (dto.OriginalFileName != null) document.OriginalFileName = dto.OriginalFileName;
            if (dto.ContentType != null) document.ContentType = dto.ContentType;
            if (dto.FilePath != null) document.FilePath = dto.FilePath;
            if (dto.FileUrl != null) document.FileUrl = dto.FileUrl;
            if (dto.FileSize.HasValue) document.FileSize = dto.FileSize.Value;
            if (dto.Category != null) document.Category = dto.Category;
            if (dto.Description != null) document.Description = dto.Description;
            if (dto.ExpiryDate.HasValue) document.ExpiryDate = dto.ExpiryDate;
            if (dto.IsActive.HasValue) document.IsActive = dto.IsActive.Value;
            if (dto.UploadedBy != null) document.UploadedBy = dto.UploadedBy;

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

            return Ok(ApiResponse<WeighbridgeDocumentDto>.SuccessResponse(responseDto, "Document updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WeighbridgeDocumentDto>.ErrorResponse("Error updating document", ex.Message));
        }
    }

    /// <summary>
    /// Delete weighbridge document
    /// </summary>
    [HttpDelete("{id}/documents/{documentId}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteWeighbridgeDocument(Guid id, Guid documentId)
    {
        try
        {
            var document = await _context.WeighbridgeDocuments
                .FirstOrDefaultAsync(d => d.Id == documentId && d.WeighbridgeId == id);

            if (document == null)
            {
                return NotFound(ApiResponse.CreateError("Document not found"));
            }

            _context.WeighbridgeDocuments.Remove(document);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Document deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting document", ex.Message));
        }
    }

    private async Task<bool> WeighbridgeExists(Guid id)
    {
        return await _context.Weighbridges.AnyAsync(e => e.Id == id);
    }
}