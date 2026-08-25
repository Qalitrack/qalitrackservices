using System.Text.Json;
using BackupService.Core.Dtos;
using BackupService.Core.Enums;
using BackupService.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackupService.Core.Services;

public class PgBackRestClient : IPgBackRestClient
{
    private readonly IProcessRunner _processRunner;
    private readonly ILogger<PgBackRestClient> _logger;
    private readonly PgBackRestOptions _options;

    public PgBackRestClient(IProcessRunner processRunner, IConfiguration configuration, ILogger<PgBackRestClient> logger)
    {
        _processRunner = processRunner;
        _logger = logger;
        _options = new PgBackRestOptions
        {
            Stanza = configuration.GetValue<string>("Backup:PgBackRestStanza", "qalitrack")!,
            PostgresContainerName = configuration.GetValue<string>("Backup:PostgresContainerName", "qalitrack-postgres-prod")!,
            PostgresImage = configuration.GetValue<string>("Backup:PostgresImage", "qalitrack-postgres-prod:latest")!,
            PostgresDataVolume = configuration.GetValue<string>("Backup:PostgresDataVolume", "postgres_prod_data")!,
            PgBackRestRepoVolume = configuration.GetValue<string>("Backup:PgBackRestRepoVolume", "pgbackrest_repo")!,
            BackupTimeoutMinutes = configuration.GetValue<int>("Backup:PgBackRestBackupTimeoutMinutes", 60),
            RestoreTimeoutMinutes = configuration.GetValue<int>("Backup:PgBackRestRestoreTimeoutMinutes", 60),
            ContainerStopStartTimeoutSeconds = configuration.GetValue<int>("Backup:ContainerStopStartTimeoutSeconds", 120),
        };
    }

    public async Task<PgBackRestBackupOutcome> BackupAsync(BackupType type, CancellationToken ct = default)
    {
        var pgBackRestType = type switch
        {
            BackupType.Full => "full",
            BackupType.Incremental => "incr",
            _ => throw new NotSupportedException($"Unsupported backup type: {type}")
        };

        var args = DockerExecArgs("pgbackrest", $"--stanza={_options.Stanza}", $"--type={pgBackRestType}",
            "--log-level-console=info", "backup");

        _logger.LogInformation("Running pgBackRest {Type} backup for stanza {Stanza}", pgBackRestType, _options.Stanza);
        var result = await _processRunner.RunAsync("docker", args, TimeSpan.FromMinutes(_options.BackupTimeoutMinutes), ct);

        if (!result.Succeeded)
        {
            _logger.LogError("pgBackRest backup failed. Exit code {ExitCode}. Stderr: {Stderr}", result.ExitCode, result.StandardError);
            throw new InvalidOperationException($"pgBackRest backup failed with exit code {result.ExitCode}: {result.StandardError}");
        }

        var info = await GetInfoAsync(ct);
        var latest = info.Backups.OrderByDescending(b => b.TimestampStop).FirstOrDefault()
            ?? throw new InvalidOperationException("pgBackRest reported a successful backup but none appear in `pgbackrest info` — check the stanza/repo configuration.");

        _logger.LogInformation("pgBackRest {Type} backup completed: {Label} ({SizeBytes} bytes)", pgBackRestType, latest.Label, latest.SizeBytes);

        return new PgBackRestBackupOutcome(
            Success: true,
            Label: latest.Label,
            Type: type,
            Timestamp: DateTimeOffset.FromUnixTimeSeconds(latest.TimestampStop).UtcDateTime,
            SizeBytes: latest.SizeBytes,
            Lsn: latest.LsnStop,
            RawOutput: result.StandardOutput);
    }

    public async Task<PgBackRestInfo> GetInfoAsync(CancellationToken ct = default)
    {
        var args = DockerExecArgs("pgbackrest", $"--stanza={_options.Stanza}", "--output=json", "info");
        var result = await _processRunner.RunAsync("docker", args, TimeSpan.FromSeconds(30), ct);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"pgBackRest info failed with exit code {result.ExitCode}: {result.StandardError}");
        }

        return ParseInfo(result.StandardOutput);
    }

    public async Task RestoreAsync(CancellationToken ct = default)
    {
        _logger.LogWarning("Stopping {Container} to restore its data volume from pgBackRest — the whole stanza comes back together, there is no per-schema restore.", _options.PostgresContainerName);
        await RunDockerCommandAsync(new[] { "stop", _options.PostgresContainerName }, TimeSpan.FromSeconds(_options.ContainerStopStartTimeoutSeconds), ct);

        try
        {
            var restoreArgs = new List<string>
            {
                "run", "--rm",
                "-u", "postgres", // same root restriction as the exec path above
                "-v", $"{_options.PostgresDataVolume}:/var/lib/postgresql/data",
                "-v", $"{_options.PgBackRestRepoVolume}:/var/lib/pgbackrest",
                _options.PostgresImage,
                // --type=immediate stops recovery as soon as the restored backup set reaches
                // consistency. Without it, pgBackRest's default replays *every* WAL segment
                // archived since — since archive_command runs continuously regardless of when
                // backups happen, that silently recovers to "now", not "this backup" (verified
                // against a real pgbackrest run: without --type=immediate, data written after
                // the last backup survived a restore).
                "pgbackrest", $"--stanza={_options.Stanza}", "--delta", "--type=immediate", "--log-level-console=info", "restore"
            };

            _logger.LogInformation("Running pgBackRest restore via a one-off container from {Image}", _options.PostgresImage);
            var result = await _processRunner.RunAsync("docker", restoreArgs, TimeSpan.FromMinutes(_options.RestoreTimeoutMinutes), ct);

            if (!result.Succeeded)
            {
                _logger.LogError("pgBackRest restore failed. Exit code {ExitCode}. Stderr: {Stderr}", result.ExitCode, result.StandardError);
                throw new InvalidOperationException($"pgBackRest restore failed with exit code {result.ExitCode}: {result.StandardError}");
            }

            _logger.LogInformation("pgBackRest restore completed successfully");
        }
        finally
        {
            // Always try to bring Postgres back up, even if the restore itself failed —
            // leaving the whole stack down on a failed restore is worse than a restore
            // that failed but left the previous data in place for another attempt.
            _logger.LogInformation("Starting {Container} back up", _options.PostgresContainerName);
            await RunDockerCommandAsync(new[] { "start", _options.PostgresContainerName }, TimeSpan.FromSeconds(_options.ContainerStopStartTimeoutSeconds), ct);
        }
    }

    private async Task RunDockerCommandAsync(IReadOnlyList<string> args, TimeSpan timeout, CancellationToken ct)
    {
        var result = await _processRunner.RunAsync("docker", args, timeout, ct);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"docker {string.Join(' ', args)} failed with exit code {result.ExitCode}: {result.StandardError}");
        }
    }

    private List<string> DockerExecArgs(params string[] command)
    {
        // pgBackRest refuses to run as root (it must match the PGDATA owner) — `docker
        // exec` defaults to root regardless of the image's default user, so this must
        // be explicit rather than relying on the container's own default user.
        var args = new List<string> { "exec", "-u", "postgres", _options.PostgresContainerName };
        args.AddRange(command);
        return args;
    }

    internal static PgBackRestInfo ParseInfo(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
        {
            return new PgBackRestInfo();
        }

        var stanzaElement = root[0];
        var info = new PgBackRestInfo
        {
            Stanza = stanzaElement.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "",
            Status = stanzaElement.TryGetProperty("status", out var status) && status.TryGetProperty("message", out var msg)
                ? msg.GetString() ?? ""
                : ""
        };

        if (stanzaElement.TryGetProperty("backup", out var backups) && backups.ValueKind == JsonValueKind.Array)
        {
            foreach (var b in backups.EnumerateArray())
            {
                info.Backups.Add(new PgBackRestBackupEntry
                {
                    Label = b.GetProperty("label").GetString() ?? "",
                    Type = b.GetProperty("type").GetString() ?? "",
                    TimestampStop = b.GetProperty("timestamp").GetProperty("stop").GetInt64(),
                    SizeBytes = b.TryGetProperty("info", out var infoObj) && infoObj.TryGetProperty("size", out var size)
                        ? size.GetInt64()
                        : 0,
                    LsnStop = b.TryGetProperty("lsn", out var lsn) && lsn.TryGetProperty("stop", out var lsnStop)
                        ? lsnStop.GetString()
                        : null,
                });
            }
        }

        return info;
    }
}
