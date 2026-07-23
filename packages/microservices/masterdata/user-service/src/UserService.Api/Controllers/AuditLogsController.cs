using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Core.DTOs;
using UserService.Core.DTOs.Common;
using UserService.Core.Entities;
using UserService.Infrastructure.Data;

namespace UserService.Api.Controllers
{
    // Pure data capture with no business logic (no validation rules, no
    // password hashing, nothing domain-specific) — unlike Users/Roles/
    // Permissions, a full Service+Repository layer would just be
    // pass-through ceremony here, so this talks to the DbContext directly.
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuditLogsController : ControllerBase
    {
        private readonly UserServiceDbContext _context;
        private readonly ILogger<AuditLogsController> _logger;

        public AuditLogsController(UserServiceDbContext context, ILogger<AuditLogsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Called only by the gateway (internal Docker network, not
        // externally reachable) immediately after every mutating request —
        // no user token to authorize against here, since this call
        // originates from the gateway itself, not from the end user's browser.
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CreateAuditLogDto dto)
        {
            try
            {
                var log = new AuditLog
                {
                    Method = dto.Method,
                    Path = dto.Path,
                    QueryString = dto.QueryString,
                    StatusCode = dto.StatusCode,
                    UserId = dto.UserId,
                    UserName = dto.UserName,
                    IpAddress = dto.IpAddress,
                    DurationMs = dto.DurationMs,
                };

                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();

                return Ok(new { Success = true });
            }
            catch (Exception ex)
            {
                // Never let an audit-write failure surface as a real error —
                // the caller (gateway) already fire-and-forgets this anyway.
                _logger.LogError(ex, "Failed to record audit log for {Method} {Path}", dto.Method, dto.Path);
                return Ok(new { Success = false });
            }
        }

        [HttpGet]
        [Authorize(Policy = "audit.view")]
        public async Task<IActionResult> GetPaged([FromQuery] PaginationParameters parameters)
        {
            var query = _context.AuditLogs.AsNoTracking().OrderByDescending(a => a.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    Method = a.Method,
                    Path = a.Path,
                    QueryString = a.QueryString,
                    StatusCode = a.StatusCode,
                    UserId = a.UserId,
                    UserName = a.UserName,
                    IpAddress = a.IpAddress,
                    DurationMs = a.DurationMs,
                    CreatedAt = a.CreatedAt,
                })
                .ToListAsync();

            return Ok(new PagedResult<AuditLogDto>
            {
                Items = items,
                Page = parameters.Page,
                PageSize = parameters.PageSize,
                TotalCount = totalCount,
            });
        }
    }
}
