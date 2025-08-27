using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.Sacco.Entities;
using QaliTrack.MasterData.Core.Modules.Sacco.DTOs;
using QaliTrack.MasterData.Core.Modules.Relationships.DTOs;
using QaliTrack.MasterData.Core.Modules.Relationships.Entities;
using QaliTrack.MasterData.Infrastructure.Data;

namespace QaliTrack.MasterData.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Tags("SACCO Module")]
public class SaccoController : ControllerBase
{
    private readonly MasterDataDbContext _context;

    public SaccoController(MasterDataDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get SACCOs with Django-style filtering, searching, and pagination
    /// </summary>
    /// <param name="queryParams">Query parameters for filtering, search, and pagination</param>
    /// <returns>Paginated list of SACCOs</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<SaccoSummaryDto>>>> GetSaccos(
        [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.Saccos
                .Select(s => new SaccoSummaryDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Code = s.Code,
                    RegistrationNumber = s.RegistrationNumber,
                    ContactEmail = s.ContactEmail,
                    SaccoType = s.SaccoType,
                    Status = s.Status,
                    MembershipCapacity = s.MembershipCapacity,
                    CurrentMemberCount = s.CurrentMemberCount,
                    EstablishedDate = s.EstablishedDate,
                    City = s.City,
                    State = s.State,
                    CreatedAt = s.CreatedAt
                })
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<SaccoSummaryDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<SaccoSummaryDto>>.ErrorResponse("Error retrieving SACCOs", ex.Message));
        }
    }

    /// <summary>
    /// Get SACCO by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SaccoDetailDto>>> GetSacco(Guid id)
    {
        try
        {
            var sacco = await _context.Saccos
                .Include(s => s.Members)
                .Include(s => s.Committees)
                .Include(s => s.Financial)
                .Include(s => s.Services)
                .Where(s => s.Id == id)
                .Select(s => new SaccoDetailDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Code = s.Code,
                    RegistrationNumber = s.RegistrationNumber,
                    LicenseNumber = s.LicenseNumber,
                    ContactEmail = s.ContactEmail,
                    ContactPhone = s.ContactPhone,
                    Address = s.Address,
                    City = s.City,
                    State = s.State,
                    Country = s.Country,
                    PostalCode = s.PostalCode,
                    EstablishedDate = s.EstablishedDate,
                    SaccoType = s.SaccoType,
                    MembershipCapacity = s.MembershipCapacity,
                    CurrentMemberCount = s.CurrentMemberCount,
                    Status = s.Status,
                    Description = s.Description,
                    Website = s.Website,
                    ShareCapitalMinimum = s.ShareCapitalMinimum,
                    ShareValue = s.ShareValue,
                    LogoUrl = s.LogoUrl,
                    RegulatoryAuthority = s.RegulatoryAuthority,
                    Notes = s.Notes,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    HasFinancialData = s.Financial != null,
                    HasActiveMembers = s.Members.Any(m => m.Status == "Active"),
                    HasCommittees = s.Committees.Any(),
                    HasServices = s.Services.Any(sv => sv.IsActive)
                })
                .FirstOrDefaultAsync();

            if (sacco == null)
            {
                return NotFound(ApiResponse<SaccoDetailDto>.ErrorResponse("SACCO not found"));
            }

            return Ok(ApiResponse<SaccoDetailDto>.SuccessResponse(sacco));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<SaccoDetailDto>.ErrorResponse("Error retrieving SACCO", ex.Message));
        }
    }

    /// <summary>
    /// Create new SACCO
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<SaccoDetailDto>>> CreateSacco(CreateSaccoDto dto)
    {
        try
        {
            var sacco = new Sacco
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Code = dto.Code,
                RegistrationNumber = dto.RegistrationNumber,
                LicenseNumber = dto.LicenseNumber,
                ContactEmail = dto.ContactEmail,
                ContactPhone = dto.ContactPhone,
                Address = dto.Address,
                City = dto.City,
                State = dto.State,
                Country = dto.Country,
                PostalCode = dto.PostalCode,
                EstablishedDate = dto.EstablishedDate,
                SaccoType = dto.SaccoType,
                MembershipCapacity = dto.MembershipCapacity,
                ShareCapitalMinimum = dto.ShareCapitalMinimum,
                ShareValue = dto.ShareValue,
                Description = dto.Description,
                Website = dto.Website,
                RegulatoryAuthority = dto.RegulatoryAuthority,
                Status = "Active"
            };

            _context.Saccos.Add(sacco);
            await _context.SaveChangesAsync();

            // Return detailed response
            var responseDto = new SaccoDetailDto
            {
                Id = sacco.Id,
                Name = sacco.Name,
                Code = sacco.Code,
                RegistrationNumber = sacco.RegistrationNumber,
                LicenseNumber = sacco.LicenseNumber,
                ContactEmail = sacco.ContactEmail,
                ContactPhone = sacco.ContactPhone,
                Address = sacco.Address,
                City = sacco.City,
                State = sacco.State,
                Country = sacco.Country,
                PostalCode = sacco.PostalCode,
                EstablishedDate = sacco.EstablishedDate,
                SaccoType = sacco.SaccoType,
                MembershipCapacity = sacco.MembershipCapacity,
                CurrentMemberCount = 0,
                Status = sacco.Status,
                Description = sacco.Description,
                Website = sacco.Website,
                ShareCapitalMinimum = sacco.ShareCapitalMinimum,
                ShareValue = sacco.ShareValue,
                LogoUrl = sacco.LogoUrl,
                RegulatoryAuthority = sacco.RegulatoryAuthority,
                Notes = sacco.Notes,
                CreatedAt = sacco.CreatedAt,
                UpdatedAt = sacco.UpdatedAt,
                HasFinancialData = false,
                HasActiveMembers = false,
                HasCommittees = false,
                HasServices = false
            };

            return CreatedAtAction(nameof(GetSacco), 
                new { id = sacco.Id }, 
                ApiResponse<SaccoDetailDto>.SuccessResponse(responseDto, "SACCO created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<SaccoDetailDto>.ErrorResponse("Error creating SACCO", ex.Message));
        }
    }

    /// <summary>
    /// Update SACCO
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<SaccoDetailDto>>> UpdateSacco(Guid id, UpdateSaccoDto dto)
    {
        try
        {
            var sacco = await _context.Saccos.FindAsync(id);
            if (sacco == null)
            {
                return NotFound(ApiResponse<SaccoDetailDto>.ErrorResponse("SACCO not found"));
            }

            // Update properties from DTO
            sacco.Name = dto.Name;
            sacco.ContactEmail = dto.ContactEmail;
            sacco.Address = dto.Address;
            sacco.Status = dto.Status;
            sacco.MembershipCapacity = dto.MembershipCapacity;
            sacco.ShareCapitalMinimum = dto.ShareCapitalMinimum;
            sacco.ShareValue = dto.ShareValue;
            sacco.ContactPhone = dto.ContactPhone;
            sacco.City = dto.City;
            sacco.State = dto.State;
            sacco.Country = dto.Country;
            sacco.PostalCode = dto.PostalCode;
            sacco.Description = dto.Description;
            sacco.Website = dto.Website;
            sacco.LogoUrl = dto.LogoUrl;
            sacco.Notes = dto.Notes;
            sacco.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Return updated SACCO details
            var responseDto = await _context.Saccos
                .Include(s => s.Members)
                .Include(s => s.Committees)
                .Include(s => s.Financial)
                .Include(s => s.Services)
                .Where(s => s.Id == id)
                .Select(s => new SaccoDetailDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Code = s.Code,
                    RegistrationNumber = s.RegistrationNumber,
                    LicenseNumber = s.LicenseNumber,
                    ContactEmail = s.ContactEmail,
                    ContactPhone = s.ContactPhone,
                    Address = s.Address,
                    City = s.City,
                    State = s.State,
                    Country = s.Country,
                    PostalCode = s.PostalCode,
                    EstablishedDate = s.EstablishedDate,
                    SaccoType = s.SaccoType,
                    MembershipCapacity = s.MembershipCapacity,
                    CurrentMemberCount = s.CurrentMemberCount,
                    Status = s.Status,
                    Description = s.Description,
                    Website = s.Website,
                    ShareCapitalMinimum = s.ShareCapitalMinimum,
                    ShareValue = s.ShareValue,
                    LogoUrl = s.LogoUrl,
                    RegulatoryAuthority = s.RegulatoryAuthority,
                    Notes = s.Notes,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    HasFinancialData = s.Financial != null,
                    HasActiveMembers = s.Members.Any(m => m.Status == "Active"),
                    HasCommittees = s.Committees.Any(),
                    HasServices = s.Services.Any(sv => sv.IsActive)
                })
                .FirstOrDefaultAsync();

            return Ok(ApiResponse<SaccoDetailDto>.SuccessResponse(responseDto!, "SACCO updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<SaccoDetailDto>.ErrorResponse("Error updating SACCO", ex.Message));
        }
    }

    /// <summary>
    /// Delete SACCO (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteSacco(Guid id)
    {
        try
        {
            var sacco = await _context.Saccos.FindAsync(id);
            if (sacco == null)
            {
                return NotFound(ApiResponse.CreateError("SACCO not found"));
            }

            sacco.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("SACCO deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting SACCO", ex.Message));
        }
    }

    /// <summary>
    /// Get SACCO members
    /// </summary>
    [HttpGet("{id}/members")]
    public async Task<ActionResult<ApiResponse<PagedResult<SaccoMemberDto>>>> GetSaccoMembers(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.SaccoMembers
                .Where(m => m.SaccoId == id)
                .Select(m => new SaccoMemberDto
                {
                    Id = m.Id,
                    SaccoId = m.SaccoId,
                    MemberNumber = m.MemberNumber,
                    FirstName = m.FirstName,
                    LastName = m.LastName,
                    MiddleName = m.MiddleName,
                    FullName = m.FullName,
                    Email = m.Email,
                    PhoneNumber = m.PhoneNumber,
                    Address = m.Address,
                    JoinDate = m.JoinDate,
                    ExitDate = m.ExitDate,
                    MembershipType = m.MembershipType,
                    Status = m.Status,
                    SharesOwned = m.SharesOwned,
                    ShareValue = m.ShareValue,
                    TotalContributions = m.TotalContributions,
                    Occupation = m.Occupation,
                    EmergencyContact = m.EmergencyContact,
                    EmergencyPhone = m.EmergencyPhone,
                    Notes = m.Notes,
                    CreatedAt = m.CreatedAt
                })
                .OrderByDescending(m => m.JoinDate)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<SaccoMemberDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<SaccoMemberDto>>.ErrorResponse("Error retrieving SACCO members", ex.Message));
        }
    }

    /// <summary>
    /// Create SACCO member
    /// </summary>
    [HttpPost("{id}/members")]
    public async Task<ActionResult<ApiResponse<SaccoMemberDto>>> CreateSaccoMember(Guid id, CreateSaccoMemberDto dto)
    {
        try
        {
            var member = new SaccoMember
            {
                Id = Guid.NewGuid(),
                SaccoId = id,
                MemberNumber = dto.MemberNumber,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                MiddleName = dto.MiddleName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                JoinDate = dto.JoinDate,
                MembershipType = dto.MembershipType,
                Occupation = dto.Occupation,
                EmergencyContact = dto.EmergencyContact,
                EmergencyPhone = dto.EmergencyPhone,
                Status = "Active"
            };

            _context.SaccoMembers.Add(member);
            await _context.SaveChangesAsync();

            var responseDto = new SaccoMemberDto
            {
                Id = member.Id,
                SaccoId = member.SaccoId,
                MemberNumber = member.MemberNumber,
                FirstName = member.FirstName,
                LastName = member.LastName,
                MiddleName = member.MiddleName,
                FullName = member.FullName,
                Email = member.Email,
                PhoneNumber = member.PhoneNumber,
                Address = member.Address,
                JoinDate = member.JoinDate,
                MembershipType = member.MembershipType,
                Status = member.Status,
                SharesOwned = member.SharesOwned,
                ShareValue = member.ShareValue,
                TotalContributions = member.TotalContributions,
                Occupation = member.Occupation,
                EmergencyContact = member.EmergencyContact,
                EmergencyPhone = member.EmergencyPhone,
                CreatedAt = member.CreatedAt
            };

            return Ok(ApiResponse<SaccoMemberDto>.SuccessResponse(responseDto, "SACCO member created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<SaccoMemberDto>.ErrorResponse("Error creating SACCO member", ex.Message));
        }
    }

    /// <summary>
    /// Get SACCO financial information
    /// </summary>
    [HttpGet("{id}/financial")]
    public async Task<ActionResult<ApiResponse<SaccoFinancialDto>>> GetSaccoFinancial(Guid id)
    {
        try
        {
            var financial = await _context.SaccoFinancials
                .Where(f => f.SaccoId == id)
                .Select(f => new SaccoFinancialDto
                {
                    Id = f.Id,
                    SaccoId = f.SaccoId,
                    TotalShares = f.TotalShares,
                    ShareCapital = f.ShareCapital,
                    ReservesFund = f.ReservesFund,
                    LoansFund = f.LoansFund,
                    TotalAssets = f.TotalAssets,
                    TotalLiabilities = f.TotalLiabilities,
                    NetWorth = f.NetWorth,
                    OutstandingLoans = f.OutstandingLoans,
                    BadDebts = f.BadDebts,
                    AnnualIncome = f.AnnualIncome,
                    OperatingExpenses = f.OperatingExpenses,
                    NetProfit = f.NetProfit,
                    LastAuditDate = f.LastAuditDate,
                    AuditorName = f.AuditorName,
                    FinancialStatus = f.FinancialStatus,
                    UpdatedDate = f.UpdatedDate,
                    Notes = f.Notes,
                    CreatedAt = f.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (financial == null)
            {
                return NotFound(ApiResponse<SaccoFinancialDto>.ErrorResponse("SACCO financial data not found"));
            }

            return Ok(ApiResponse<SaccoFinancialDto>.SuccessResponse(financial));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<SaccoFinancialDto>.ErrorResponse("Error retrieving SACCO financial data", ex.Message));
        }
    }

    /// <summary>
    /// Create or update SACCO financial information
    /// </summary>
    [HttpPost("{id}/financial")]
    public async Task<ActionResult<ApiResponse<SaccoFinancialDto>>> CreateSaccoFinancial(Guid id, CreateSaccoFinancialDto dto)
    {
        try
        {
            var financial = new SaccoFinancial
            {
                Id = Guid.NewGuid(),
                SaccoId = id,
                TotalShares = dto.TotalShares,
                ShareCapital = dto.ShareCapital,
                ReservesFund = dto.ReservesFund,
                LoansFund = dto.LoansFund,
                TotalAssets = dto.TotalAssets,
                TotalLiabilities = dto.TotalLiabilities,
                NetWorth = dto.TotalAssets - dto.TotalLiabilities,
                OutstandingLoans = dto.OutstandingLoans,
                AnnualIncome = dto.AnnualIncome,
                OperatingExpenses = dto.OperatingExpenses,
                NetProfit = dto.AnnualIncome - dto.OperatingExpenses,
                LastAuditDate = dto.LastAuditDate,
                AuditorName = dto.AuditorName,
                FinancialStatus = "Stable"
            };

            _context.SaccoFinancials.Add(financial);
            await _context.SaveChangesAsync();

            var responseDto = new SaccoFinancialDto
            {
                Id = financial.Id,
                SaccoId = financial.SaccoId,
                TotalShares = financial.TotalShares,
                ShareCapital = financial.ShareCapital,
                ReservesFund = financial.ReservesFund,
                LoansFund = financial.LoansFund,
                TotalAssets = financial.TotalAssets,
                TotalLiabilities = financial.TotalLiabilities,
                NetWorth = financial.NetWorth,
                OutstandingLoans = financial.OutstandingLoans,
                BadDebts = financial.BadDebts,
                AnnualIncome = financial.AnnualIncome,
                OperatingExpenses = financial.OperatingExpenses,
                NetProfit = financial.NetProfit,
                LastAuditDate = financial.LastAuditDate,
                AuditorName = financial.AuditorName,
                FinancialStatus = financial.FinancialStatus,
                UpdatedDate = financial.UpdatedDate,
                CreatedAt = financial.CreatedAt
            };

            return Ok(ApiResponse<SaccoFinancialDto>.SuccessResponse(responseDto, "SACCO financial data created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<SaccoFinancialDto>.ErrorResponse("Error creating SACCO financial data", ex.Message));
        }
    }

    /// <summary>
    /// Get SACCO committees
    /// </summary>
    [HttpGet("{id}/committees")]
    public async Task<ActionResult<ApiResponse<PagedResult<SaccoCommitteeDto>>>> GetSaccoCommittees(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.SaccoCommittees
                .Where(c => c.SaccoId == id)
                .Select(c => new SaccoCommitteeDto
                {
                    Id = c.Id,
                    SaccoId = c.SaccoId,
                    Name = c.Name,
                    Description = c.Description,
                    CommitteeType = c.CommitteeType,
                    EstablishedDate = c.EstablishedDate,
                    Status = c.Status,
                    MaxMembers = c.MaxMembers,
                    CurrentMemberCount = c.CommitteeMembers.Count,
                    Responsibilities = c.Responsibilities,
                    MeetingSchedule = c.MeetingSchedule,
                    Notes = c.Notes,
                    CreatedAt = c.CreatedAt
                })
                .OrderBy(c => c.Name)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<SaccoCommitteeDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<SaccoCommitteeDto>>.ErrorResponse("Error retrieving SACCO committees", ex.Message));
        }
    }

    /// <summary>
    /// Get SACCO services
    /// </summary>
    [HttpGet("{id}/services")]
    public async Task<ActionResult<ApiResponse<PagedResult<SaccoServiceDto>>>> GetSaccoServices(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.SaccoServices
                .Where(s => s.SaccoId == id)
                .Select(s => new SaccoServiceDto
                {
                    Id = s.Id,
                    SaccoId = s.SaccoId,
                    ServiceName = s.ServiceName,
                    Description = s.Description,
                    ServiceType = s.ServiceType,
                    ServiceFee = s.ServiceFee,
                    Eligibility = s.Eligibility,
                    Requirements = s.Requirements,
                    ProcessingTime = s.ProcessingTime,
                    IsActive = s.IsActive,
                    LaunchDate = s.LaunchDate,
                    ResponsibleCommittee = s.ResponsibleCommittee,
                    Notes = s.Notes,
                    CreatedAt = s.CreatedAt
                })
                .OrderBy(s => s.ServiceName)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<SaccoServiceDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<SaccoServiceDto>>.ErrorResponse("Error retrieving SACCO services", ex.Message));
        }
    }

    /// <summary>
    /// Get SACCO loans
    /// </summary>
    [HttpGet("{id}/loans")]
    public async Task<ActionResult<ApiResponse<PagedResult<SaccoLoanDto>>>> GetSaccoLoans(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.SaccoLoans
                .Where(l => l.SaccoId == id)
                .Include(l => l.Member)
                .Select(l => new SaccoLoanDto
                {
                    Id = l.Id,
                    SaccoId = l.SaccoId,
                    SaccoMemberId = l.SaccoMemberId,
                    MemberName = l.Member.FullName,
                    LoanNumber = l.LoanNumber,
                    LoanType = l.LoanType,
                    LoanAmount = l.LoanAmount,
                    InterestRate = l.InterestRate,
                    RepaymentPeriodMonths = l.RepaymentPeriodMonths,
                    MonthlyPayment = l.MonthlyPayment,
                    OutstandingBalance = l.OutstandingBalance,
                    ApplicationDate = l.ApplicationDate,
                    ApprovalDate = l.ApprovalDate,
                    DisbursementDate = l.DisbursementDate,
                    MaturityDate = l.MaturityDate,
                    Status = l.Status,
                    Purpose = l.Purpose,
                    Collateral = l.Collateral,
                    Guarantors = l.Guarantors,
                    ApprovedBy = l.ApprovedBy,
                    RepaymentHistory = l.RepaymentHistory,
                    IsOverdue = l.MaturityDate.HasValue && l.MaturityDate < DateTime.Today && l.OutstandingBalance > 0,
                    Notes = l.Notes,
                    CreatedAt = l.CreatedAt
                })
                .OrderByDescending(l => l.ApplicationDate)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<SaccoLoanDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<SaccoLoanDto>>.ErrorResponse("Error retrieving SACCO loans", ex.Message));
        }
    }

    /// <summary>
    /// Get all drivers who are members of this SACCO
    /// </summary>
    [HttpGet("{id}/drivers")]
    public async Task<ActionResult<ApiResponse<PagedResult<DriverSaccoMembershipDto>>>> GetSaccoDrivers(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            if (!await SaccoExists(id))
            {
                return NotFound(ApiResponse<PagedResult<DriverSaccoMembershipDto>>.ErrorResponse("SACCO not found"));
            }

            var query = _context.DriverSaccoMemberships
                .Where(m => m.SaccoId == id)
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
                    DriverName = "Driver Name", // TODO: Join with Driver entity when implemented
                    SaccoName = "SACCO Name" // TODO: Join with SACCO entity when implemented
                })
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<DriverSaccoMembershipDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<DriverSaccoMembershipDto>>.ErrorResponse("Error retrieving SACCO drivers", ex.Message));
        }
    }

    /// <summary>
    /// Add a driver membership to this SACCO
    /// </summary>
    [HttpPost("{id}/drivers")]
    public async Task<ActionResult<ApiResponse<DriverSaccoMembershipDto>>> AddDriverToSacco(
        Guid id, CreateDriverSaccoMembershipDto dto)
    {
        try
        {
            if (!await SaccoExists(id))
            {
                return NotFound(ApiResponse<DriverSaccoMembershipDto>.ErrorResponse("SACCO not found"));
            }

            if (dto.SaccoId != id)
            {
                return BadRequest(ApiResponse<DriverSaccoMembershipDto>.ErrorResponse("SACCO ID mismatch"));
            }

            // Check if driver is already a member
            var existingMembership = await _context.DriverSaccoMemberships
                .FirstOrDefaultAsync(m => m.DriverId == dto.DriverId && m.SaccoId == id && m.IsActive);
            
            if (existingMembership != null)
            {
                return BadRequest(ApiResponse<DriverSaccoMembershipDto>.ErrorResponse("Driver is already a member of this SACCO"));
            }

            var membership = new DriverSaccoMembership
            {
                Id = Guid.NewGuid(),
                DriverId = dto.DriverId,
                SaccoId = dto.SaccoId,
                MembershipDate = dto.MembershipDate,
                MembershipNumber = dto.MembershipNumber,
                ShareContribution = dto.ShareContribution,
                Status = dto.Status,
                ExpiryDate = dto.ExpiryDate,
                MonthlyContribution = dto.MonthlyContribution,
                MembershipType = dto.MembershipType,
                Benefits = dto.Benefits,
                Notes = dto.Notes
            };

            _context.DriverSaccoMemberships.Add(membership);
            await _context.SaveChangesAsync();

            var responseDto = new DriverSaccoMembershipDto
            {
                Id = membership.Id,
                DriverId = membership.DriverId,
                SaccoId = membership.SaccoId,
                MembershipDate = membership.MembershipDate,
                ExpiryDate = membership.ExpiryDate,
                MembershipNumber = membership.MembershipNumber,
                Status = membership.Status,
                ShareContribution = membership.ShareContribution,
                MonthlyContribution = membership.MonthlyContribution,
                MembershipType = membership.MembershipType,
                Benefits = membership.Benefits,
                IsActive = membership.IsActive,
                Notes = membership.Notes,
                CreatedAt = membership.CreatedAt,
                UpdatedAt = membership.UpdatedAt,
                IsExpired = membership.ExpiryDate.HasValue && membership.ExpiryDate < DateTime.Today,
                DriverName = "Driver Name", // TODO: Join with Driver entity when implemented
                SaccoName = "SACCO Name" // TODO: Join with SACCO entity when implemented
            };

            return Ok(ApiResponse<DriverSaccoMembershipDto>.SuccessResponse(responseDto, "Driver added to SACCO successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DriverSaccoMembershipDto>.ErrorResponse("Error adding driver to SACCO", ex.Message));
        }
    }

    /// <summary>
    /// Remove a driver membership from this SACCO
    /// </summary>
    [HttpDelete("{id}/drivers/{driverId}")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveDriverFromSacco(Guid id, Guid driverId)
    {
        try
        {
            if (!await SaccoExists(id))
            {
                return NotFound(ApiResponse.CreateError("SACCO not found"));
            }

            var membership = await _context.DriverSaccoMemberships
                .FirstOrDefaultAsync(m => m.DriverId == driverId && m.SaccoId == id && m.IsActive);

            if (membership == null)
            {
                return NotFound(ApiResponse.CreateError("Driver membership not found"));
            }

            membership.IsActive = false;
            membership.Status = "Inactive";
            membership.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Driver removed from SACCO successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error removing driver from SACCO", ex.Message));
        }
    }

    /// <summary>
    /// Get all vehicles registered under this SACCO
    /// </summary>
    [HttpGet("{id}/vehicles")]
    public async Task<ActionResult<ApiResponse<PagedResult<VehicleSaccoRegistrationDto>>>> GetSaccoVehicles(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            if (!await SaccoExists(id))
            {
                return NotFound(ApiResponse<PagedResult<VehicleSaccoRegistrationDto>>.ErrorResponse("SACCO not found"));
            }

            var query = _context.VehicleSaccoRegistrations
                .Where(r => r.SaccoId == id)
                .Select(r => new VehicleSaccoRegistrationDto
                {
                    Id = r.Id,
                    VehicleId = r.VehicleId,
                    SaccoId = r.SaccoId,
                    RegistrationDate = r.RegistrationDate,
                    ExpiryDate = r.ExpiryDate,
                    RegistrationNumber = r.RegistrationNumber,
                    Status = r.Status,
                    RegistrationFee = r.RegistrationFee,
                    CertificateNumber = r.CertificateNumber,
                    Conditions = r.Conditions,
                    IsActive = r.IsActive,
                    Notes = r.Notes,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    IsExpired = r.ExpiryDate.HasValue && r.ExpiryDate < DateTime.Today,
                    VehicleRegistrationNumber = "Vehicle Reg", // TODO: Join with Vehicle entity when implemented
                    SaccoName = "SACCO Name" // TODO: Join with SACCO entity when implemented
                })
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<VehicleSaccoRegistrationDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<VehicleSaccoRegistrationDto>>.ErrorResponse("Error retrieving SACCO vehicles", ex.Message));
        }
    }

    private async Task<bool> SaccoExists(Guid id)
    {
        return await _context.Saccos.AnyAsync(e => e.Id == id);
    }
}