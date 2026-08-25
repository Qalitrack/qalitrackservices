using System.Data;
using System.Security.Cryptography;
using System.Text;
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
        private const string GenesisHash = "GENESIS";
        private const int MaxSerializationRetries = 3;

        // Arbitrary fixed key for a Postgres advisory lock scoped to
        // appending to the audit-log chain — see Create() for why this is
        // needed in addition to the Serializable transaction.
        private const long ChainAppendLockKey = 84671023950123L;

        private readonly UserServiceDbContext _context;
        private readonly ILogger<AuditLogsController> _logger;

        public AuditLogsController(UserServiceDbContext context, ILogger<AuditLogsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Called only by the gateway (internal Docker network, not
        // externally reachable) immediately after every mutating (and
        // sensitive-read) request — no user token to authorize against
        // here, since this call originates from the gateway itself, not
        // from the end user's browser.
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CreateAuditLogDto dto)
        {
            for (var attempt = 1; attempt <= MaxSerializationRetries; attempt++)
            {
                try
                {
                    // Serializable so two concurrent inserts can't both read the
                    // same "previous" row and produce two rows claiming the same
                    // PreviousHash — one of them will fail with SQLSTATE 40001
                    // and retry against the now-committed chain.
                    await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                    // The SequenceNumber identity column's advancement is NOT
                    // transactional and isn't tracked by Serializable's conflict
                    // detection — two concurrent transactions can get distinct
                    // sequence numbers (e.g. 5 and 6) with neither able to see
                    // the other's still-uncommitted row, so #6 looking up "#5's
                    // hash" finds nothing and wrongly falls back to GENESIS. A
                    // transaction-scoped advisory lock forces genuine one-at-a-
                    // time append ordering that Serializable alone doesn't
                    // provide here; it's released automatically on commit/rollback.
                    await _context.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock({0})", ChainAppendLockKey);

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
                        EntityType = dto.EntityType,
                        EntityId = dto.EntityId,
                        Action = dto.Action ?? dto.Method,
                        PreviousHash = string.Empty,
                        Hash = string.Empty,
                    };

                    _context.AuditLogs.Add(log);
                    await _context.SaveChangesAsync(); // assigns SequenceNumber (DB identity)

                    var previous = await _context.AuditLogs
                        .AsNoTracking()
                        .Where(a => a.SequenceNumber == log.SequenceNumber - 1)
                        .Select(a => a.Hash)
                        .FirstOrDefaultAsync();

                    log.PreviousHash = previous ?? GenesisHash;
                    log.Hash = ComputeHash(log);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return Ok(new { Success = true });
                }
                catch (Npgsql.PostgresException ex) when (ex.SqlState == "40001" && attempt < MaxSerializationRetries)
                {
                    _logger.LogWarning("[Audit] Serialization conflict writing audit log, retrying ({Attempt}/{Max})", attempt, MaxSerializationRetries);
                }
                catch (Exception ex)
                {
                    // Never let an audit-write failure surface as a real error —
                    // the caller (gateway) already fire-and-forgets this anyway.
                    _logger.LogError(ex, "Failed to record audit log for {Method} {Path}", dto.Method, dto.Path);
                    return Ok(new { Success = false });
                }
            }

            _logger.LogError("Failed to record audit log for {Method} {Path} after {Max} retries", dto.Method, dto.Path, MaxSerializationRetries);
            return Ok(new { Success = false });
        }

        [HttpGet]
        [Authorize(Policy = "audit.view")]
        public async Task<IActionResult> GetPaged([FromQuery] PaginationParameters parameters)
        {
            var query = _context.AuditLogs.AsNoTracking().OrderByDescending(a => a.SequenceNumber);

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
                    EntityType = a.EntityType,
                    EntityId = a.EntityId,
                    Action = a.Action,
                    SequenceNumber = a.SequenceNumber,
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

        private const int VerifyBatchSize = 1000;

        // Walks the full hash chain and recomputes each row's hash from its
        // stored fields + the previous row's hash, to prove (or disprove) that
        // nothing in the log has been altered since it was written. Walked in
        // fixed-size batches (carrying the running hash across batches)
        // instead of loading the whole table at once — this table only ever
        // grows, so an unbounded load here gets slower and more memory-hungry
        // forever.
        [HttpGet("verify")]
        [Authorize(Policy = "audit.view")]
        public async Task<IActionResult> VerifyChain()
        {
            var expectedPrevious = GenesisHash;
            long checkedCount = 0;
            long? lastSequence = null;

            while (true)
            {
                IQueryable<AuditLog> batchQuery = _context.AuditLogs.AsNoTracking();
                if (lastSequence.HasValue)
                {
                    batchQuery = batchQuery.Where(a => a.SequenceNumber > lastSequence.Value);
                }

                var batch = await batchQuery.OrderBy(a => a.SequenceNumber).Take(VerifyBatchSize).ToListAsync();
                if (batch.Count == 0)
                {
                    break;
                }

                foreach (var row in batch)
                {
                    checkedCount++;
                    if (row.PreviousHash != expectedPrevious || row.Hash != ComputeHash(row))
                    {
                        return Ok(new AuditLogVerifyResultDto
                        {
                            Valid = false,
                            CheckedCount = checkedCount,
                            FirstBrokenSequenceNumber = row.SequenceNumber,
                        });
                    }

                    expectedPrevious = row.Hash;
                    lastSequence = row.SequenceNumber;
                }

                if (batch.Count < VerifyBatchSize)
                {
                    break;
                }
            }

            return Ok(new AuditLogVerifyResultDto { Valid = true, CheckedCount = checkedCount });
        }

        private static string ComputeHash(AuditLog log)
        {
            var raw = string.Join('|',
                log.PreviousHash,
                log.SequenceNumber,
                log.CreatedAt.ToString("O"),
                log.Method,
                log.Path,
                log.QueryString,
                log.StatusCode,
                log.UserId,
                log.UserName,
                log.IpAddress,
                log.DurationMs,
                log.EntityType,
                log.EntityId,
                log.Action);

            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(bytes);
        }
    }
}
