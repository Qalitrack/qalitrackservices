using System.Diagnostics;
using System.IO.Abstractions;
using System.Text;
using BackupService.Core.Dtos;
using BackupService.Core.Entities;
using BackupService.Core.Enums;
using BackupService.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BackupService.Core.Services;

public class BackupRestoreService : IBackupRestoreService
{
    private readonly IMicroserviceRepository _microserviceRepository;
    private readonly IBackupMetadataService _backupMetadataService;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<BackupRestoreService> _logger;
    private readonly SemaphoreSlim _operationLock = new(1, 1);
    private readonly int _connectionTestTimeoutSeconds = 10;
    private readonly int _commandTimeoutMinutes = 30;

    private static class PostgreSQLCommands
    {
        public const string Verbose = "--verbose";
        public const string Quiet = "--quiet";
        public const string NoPassword = "--no-password";
        public const string CreateDb = "--create";
        public const string CleanFirst = "--clean";
        public const string IfExists = "--if-exists";
        public const string SingleTransaction = "--single-transaction";
        public const string NoOwner = "--no-owner";
        public const string NoPrivileges = "--no-privileges";
    }

    public BackupRestoreService(
        IMicroserviceRepository microserviceRepository,
        IBackupMetadataService backupMetadataService,
        IFileSystem fileSystem,
        ILogger<BackupRestoreService> logger)
    {
        _microserviceRepository = microserviceRepository;
        _backupMetadataService = backupMetadataService;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task<RestoreResult> RestoreBackupAsync(string microservice, string backupFilePath, 
        CancellationToken ct = default)
    {
        // Check PostgreSQL tool versions
        await CheckPostgreSqlToolVersionsAsync(ct);

        if (string.IsNullOrWhiteSpace(backupFilePath))
            throw new ArgumentException("Backup file path must be provided", nameof(backupFilePath));

        if (!_fileSystem.File.Exists(backupFilePath))
            throw new FileNotFoundException($"Backup file not found: {backupFilePath}");

        await _operationLock.WaitAsync(ct);
        try
        {
            var ms = await ValidateMicroserviceAsync(microservice, ct);
            var startTime = DateTime.UtcNow;
            var originalDatabaseName = GetDatabaseName(ms.ConnectionString);
            var tempDatabaseName = $"{originalDatabaseName}_restore_temp_{DateTime.UtcNow:yyyyMMddHHmmss}";
            var backupDatabaseName = $"{originalDatabaseName}_backup_{DateTime.UtcNow:yyyyMMddHHmmss}";

            _logger.LogInformation("Starting restore for {Microservice} from {BackupFile}. Original DB: {OriginalDb}, Temp DB: {TempDb}", 
                microservice, Path.GetFileName(backupFilePath), originalDatabaseName, tempDatabaseName);

            try
            {
                // Step 1: Create temporary database and restore backup to it
                await CreateTemporaryDatabaseAsync(ms, tempDatabaseName, ct);
                
                try
                {
                    await RestoreToTemporaryDatabaseAsync(ms, backupFilePath, tempDatabaseName, ct);

                    // Step 2: Validate the restored database
                    await ValidateRestoredDatabaseAsync(ms, tempDatabaseName, ct);

                    // Step 3: Safely replace the original database
                    await SafelyReplaceDatabaseAsync(ms, originalDatabaseName, tempDatabaseName, backupDatabaseName, ct);

                    // Step 4: Update microservice status
                    await UpdateMicroserviceStatusAsync(ms, MicroserviceStatus.Active, ct);

                    var result = new RestoreResult
                    {
                        BackupId = Path.GetFileNameWithoutExtension(backupFilePath),
                        ServiceName = microservice,
                        StartedAt = startTime,
                        CompletedAt = DateTime.UtcNow,
                        IsSuccessful = true,
                        Message = $"SQL dump restore completed successfully. Database restored from {Path.GetFileName(backupFilePath)}",
                        FullBackupUsed = Path.GetFileName(backupFilePath),
                        IncrementalBackupsUsed = new List<string>(),
                        TotalFilesProcessed = 1,
                    };

                    _logger.LogInformation("Restore completed successfully for {Microservice}. Duration: {Duration}ms", 
                        microservice, (DateTime.UtcNow - startTime).TotalMilliseconds);

                    return result;
                }
                catch (Exception restoreEx)
                {
                    _logger.LogError(restoreEx, "Restore failed during database restoration for {Microservice}", microservice);
                    
                    await CleanupTemporaryDatabaseAsync(ms, tempDatabaseName, ct);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Restore failed for {Microservice}", microservice);
                
                await UpdateMicroserviceStatusAsync(ms, MicroserviceStatus.Paused, ct);
                
                throw new InvalidOperationException($"Restore failed for {microservice}: {ex.Message}", ex);
            }
        }
        finally
        {
            _operationLock.Release();
        }
    }

    private async Task CheckPostgreSqlToolVersionsAsync(CancellationToken ct)
    {
        try
        {
            var pgDumpVersion = await GetToolVersionAsync("pg_dump", ct);
            var pgRestoreVersion = await GetToolVersionAsync("pg_restore", ct);

            _logger.LogInformation("pg_dump version: {Version}", pgDumpVersion);
            _logger.LogInformation("pg_restore version: {Version}", pgRestoreVersion);

            if (pgDumpVersion != pgRestoreVersion)
            {
                _logger.LogWarning("Version mismatch detected: pg_dump ({PgDumpVersion}) and pg_restore ({PgRestoreVersion}) have different versions. This may cause compatibility issues during restore.", 
                    pgDumpVersion, pgRestoreVersion);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check PostgreSQL tool versions");
        }
    }

    private async Task<string> GetToolVersionAsync(string command, CancellationToken ct)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = "--version",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processInfo };
        var outputBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, e) => 
        {
            if (e.Data != null) outputBuilder.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"Failed to get version for {command}: {error}");
        }

        return outputBuilder.ToString().Trim();
    }

    public async Task<RestorePreviewResult> PreviewRestoreAsync(string microservice, string backupFilePath, 
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(backupFilePath))
            throw new ArgumentException("Backup file path must be provided", nameof(backupFilePath));

        if (!_fileSystem.File.Exists(backupFilePath))
            throw new FileNotFoundException($"Backup file not found: {backupFilePath}");

        var ms = await ValidateMicroserviceAsync(microservice, ct);
        var backupFileInfo = _fileSystem.FileInfo.New(backupFilePath);
        var originalDatabaseName = GetDatabaseName(ms.ConnectionString);

        return new RestorePreviewResult
        {
            BackupId = Path.GetFileNameWithoutExtension(backupFilePath),
            ChainId = "N/A",
            FullBackupFile = Path.GetFileName(backupFilePath),
            IncrementalFiles = new List<string>(),
            TotalFilesToProcess = 1,
            EstimatedRestoreTimeMinutes = CalculateEstimatedRestoreTime(backupFileInfo.Length),
            RequiredSpaceBytes = backupFileInfo.Length * 3,
            RestoreToTimestamp = backupFileInfo.LastWriteTimeUtc,
            ServiceName = microservice,
        };
    }

    private async Task<Microservice> ValidateMicroserviceAsync(string microservice, CancellationToken ct)
    {
        var ms = await _microserviceRepository.GetMicroserviceAsync(microservice, ct)
            ?? throw new KeyNotFoundException($"Microservice {microservice} not found");

        if (ms.Status == MicroserviceStatus.Inactive)
        {
            _logger.LogWarning("Microservice {Name} is Inactive, cannot restore", microservice);
            throw new InvalidOperationException($"Microservice {microservice} is Inactive");
        }

        if (string.IsNullOrWhiteSpace(ms.ConnectionString))
        {
            _logger.LogError("Connection string is missing for microservice {Name}", microservice);
            throw new InvalidOperationException($"Connection string is not configured for microservice {microservice}");
        }

        await ValidateDatabaseConnectionAsync(ms, ct);
        return ms;
    }

    private async Task CreateTemporaryDatabaseAsync(Microservice ms, string tempDatabaseName, CancellationToken ct)
    {
        _logger.LogInformation("Creating temporary database: {TempDatabaseName}", tempDatabaseName);

        var connParams = ParseConnectionString(ms.ConnectionString);
        var createDbSql = $"CREATE DATABASE \"{tempDatabaseName}\";";

        var arguments = new List<string>
        {
            PostgreSQLCommands.Quiet,
            PostgreSQLCommands.NoPassword,
            "--dbname=postgres",
            "-c",
            createDbSql
        };

        await ExecutePostgreSQLCommandAsync("psql", arguments, ms.ConnectionString, ct);
        _logger.LogDebug("Temporary database created successfully: {TempDatabaseName}", tempDatabaseName);
    }

    private async Task RestoreToTemporaryDatabaseAsync(Microservice ms, string backupFilePath, 
        string tempDatabaseName, CancellationToken ct)
    {
        _logger.LogInformation("Restoring backup {BackupFile} to temporary database {TempDatabaseName}", 
            Path.GetFileName(backupFilePath), tempDatabaseName);

        var arguments = new List<string>
        {
            PostgreSQLCommands.Verbose,
            PostgreSQLCommands.NoPassword,
            PostgreSQLCommands.SingleTransaction,
            PostgreSQLCommands.NoOwner,
            PostgreSQLCommands.NoPrivileges,
            $"--dbname={tempDatabaseName}",
            backupFilePath
        };

        await ExecutePostgreSQLCommandAsync("pg_restore", arguments, ms.ConnectionString, ct);
        _logger.LogInformation("Backup restored successfully to temporary database: {TempDatabaseName}", tempDatabaseName);
    }

    private async Task ValidateRestoredDatabaseAsync(Microservice ms, string tempDatabaseName, CancellationToken ct)
    {
        _logger.LogInformation("Validating restored database: {TempDatabaseName}", tempDatabaseName);

        var tempConnectionString = UpdateConnectionStringDatabase(ms.ConnectionString, tempDatabaseName);
        var connParams = ParseConnectionString(tempConnectionString);

        var processInfo = new ProcessStartInfo
        {
            FileName = "pg_isready",
            Arguments = $"--timeout={_connectionTestTimeoutSeconds}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var (key, value) in connParams)
        {
            processInfo.Environment[key] = value;
        }

        using var process = new Process { StartInfo = processInfo };
        process.Start();
        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"Restored database validation failed: {error}");
        }

        var testQuery = "SELECT 1;";
        var arguments = new List<string>
        {
            PostgreSQLCommands.Quiet,
            PostgreSQLCommands.NoPassword,
            $"--dbname={tempDatabaseName}",
            "-c",
            testQuery
        };

        await ExecutePostgreSQLCommandAsync("psql", arguments, ms.ConnectionString, ct);
        _logger.LogDebug("Restored database validation completed successfully: {TempDatabaseName}", tempDatabaseName);
    }

    private async Task SafelyReplaceDatabaseAsync(Microservice ms, string originalDatabaseName, 
        string tempDatabaseName, string backupDatabaseName, CancellationToken ct)
    {
        _logger.LogInformation("Safely replacing database {OriginalDb} with {TempDb}", 
            originalDatabaseName, tempDatabaseName);

        try
        {
            // Step 1: Terminate active connections to the original database
            await TerminateDatabaseConnectionsAsync(ms, originalDatabaseName, ct);

            // Step 2: Rename original database to backup
            await RenameDatabaseAsync(ms, originalDatabaseName, backupDatabaseName, ct);
            _logger.LogDebug("Original database renamed to backup: {BackupDatabaseName}", backupDatabaseName);

            // Step 3: Rename temporary database to original
            await RenameDatabaseAsync(ms, tempDatabaseName, originalDatabaseName, ct);
            _logger.LogDebug("Temporary database renamed to original: {OriginalDatabaseName}", originalDatabaseName);

            // Step 4: Drop the backup database
            await DropDatabaseAsync(ms, backupDatabaseName, ct);
            _logger.LogDebug("Backup database dropped: {BackupDatabaseName}", backupDatabaseName);

            _logger.LogInformation("Database replacement completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database replacement, attempting rollback");

            try
            {
                // Check if the backup database exists before attempting rename
                if (await DatabaseExistsAsync(ms, backupDatabaseName, ct))
                {
                    await TerminateDatabaseConnectionsAsync(ms, backupDatabaseName, ct);
                    await RenameDatabaseAsync(ms, backupDatabaseName, originalDatabaseName, ct);
                    _logger.LogInformation("Rollback successful: Restored {BackupDatabaseName} to {OriginalDatabaseName}", 
                        backupDatabaseName, originalDatabaseName);
                }
                else
                {
                    _logger.LogWarning("Rollback skipped: Backup database {BackupDatabaseName} does not exist", 
                        backupDatabaseName);
                }

                // Clean up temporary database if it exists
                if (await DatabaseExistsAsync(ms, tempDatabaseName, ct))
                {
                    await TerminateDatabaseConnectionsAsync(ms, tempDatabaseName, ct);
                    await DropDatabaseAsync(ms, tempDatabaseName, ct);
                    _logger.LogInformation("Cleaned up temporary database {TempDatabaseName}", tempDatabaseName);
                }
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Rollback failed - manual intervention required. Original DB may be at: {BackupDatabaseName}, Temp DB: {TempDatabaseName}", 
                    backupDatabaseName, tempDatabaseName);
                throw new InvalidOperationException(
                    $"Database replacement failed and rollback also failed. Manual intervention required. " +
                    $"Original DB may be at: {backupDatabaseName}, Temp DB: {tempDatabaseName}", ex);
            }

            throw new InvalidOperationException(
                $"Database replacement failed. Manual intervention may be required. Temp DB: {tempDatabaseName}", ex);
        }
    }

    private async Task TerminateDatabaseConnectionsAsync(Microservice ms, string databaseName, CancellationToken ct)
    {
        _logger.LogInformation("Terminating active connections to database {DatabaseName}", databaseName);

        // Quote the database name to handle special characters or case sensitivity
        string query = @"
            SELECT pg_terminate_backend(pg_stat_activity.pid)
            FROM pg_stat_activity
            WHERE pg_stat_activity.datname = '@DatabaseName'
            AND pid <> pg_backend_pid();";

        var arguments = new List<string>
        {
            PostgreSQLCommands.Quiet,
            PostgreSQLCommands.NoPassword,
            "--dbname=postgres",
            "-c",
            query.Replace("@DatabaseName", databaseName.Replace("'", "''")) // Escape single quotes
        };

        try
        {
            await ExecutePostgreSQLCommandAsync("psql", arguments, ms.ConnectionString, ct);
            _logger.LogInformation("Successfully terminated connections to {DatabaseName}", databaseName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to terminate connections to {DatabaseName}. Proceeding with replacement attempt.", 
                databaseName);
            // Continue even if termination fails, as some connections may not be critical
        }
    }

    private async Task<bool> DatabaseExistsAsync(Microservice ms, string databaseName, CancellationToken ct)
    {
        _logger.LogDebug("Checking if database {DatabaseName} exists", databaseName);

        string query = $"SELECT 1 FROM pg_database WHERE datname = '{databaseName.Replace("'", "''")}';";
        var arguments = new List<string>
        {
            PostgreSQLCommands.Quiet,
            "-t",
            "-A",
            "--dbname=postgres",
            "-c",
            query
        };

        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "psql",
                Arguments = string.Join(" ", arguments),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            foreach (var (key, value) in ParseConnectionString(ms.ConnectionString))
            {
                processInfo.Environment[key] = value;
            }

            using var process = new Process { StartInfo = processInfo };
            var outputBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    outputBuilder.AppendLine(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            await process.WaitForExitAsync(ct);

            if (process.ExitCode == 0 && outputBuilder.ToString().Trim() == "1")
            {
                _logger.LogDebug("Database {DatabaseName} exists", databaseName);
                return true;
            }

            _logger.LogDebug("Database {DatabaseName} does not exist", databaseName);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking existence of database {DatabaseName}", databaseName);
            return false;
        }
    }

    private async Task RenameDatabaseAsync(Microservice ms, string fromName, string toName, CancellationToken ct)
    {
        var renameSql = $"ALTER DATABASE \"{fromName}\" RENAME TO \"{toName}\";";
        
        var arguments = new List<string>
        {
            PostgreSQLCommands.Quiet,
            PostgreSQLCommands.NoPassword,
            "--dbname=postgres",
            "-c",
            renameSql
        };

        await ExecutePostgreSQLCommandAsync("psql", arguments, ms.ConnectionString, ct);
        _logger.LogDebug("Database renamed from {FromName} to {ToName}", fromName, toName);
    }

    private async Task DropDatabaseAsync(Microservice ms, string databaseName, CancellationToken ct)
    {
        var dropSql = $"DROP DATABASE IF EXISTS \"{databaseName}\";";
        
        var arguments = new List<string>
        {
            PostgreSQLCommands.Quiet,
            PostgreSQLCommands.NoPassword,
            "--dbname=postgres",
            "-c",
            dropSql
        };

        await ExecutePostgreSQLCommandAsync("psql", arguments, ms.ConnectionString, ct);
        _logger.LogDebug("Database dropped: {DatabaseName}", databaseName);
    }

    private async Task CleanupTemporaryDatabaseAsync(Microservice ms, string tempDatabaseName, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Cleaning up temporary database: {TempDatabaseName}", tempDatabaseName);
            await DropDatabaseAsync(ms, tempDatabaseName, ct);
            _logger.LogDebug("Temporary database cleanup completed: {TempDatabaseName}", tempDatabaseName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cleanup temporary database {TempDatabaseName} - may require manual cleanup", tempDatabaseName);
        }
    }

    private async Task UpdateMicroserviceStatusAsync(Microservice ms, MicroserviceStatus status, CancellationToken ct)
    {
        try
        {
            ms.Status = status;
            ms.UpdatedAt = DateTime.UtcNow;
            if (status == MicroserviceStatus.Active)
            {
                ms.LastBackupAt = DateTime.UtcNow;
            }

            await _microserviceRepository.UpdateMicroserviceAsync(
                ms.Name,
                new MicroserviceRequest
                {
                    Name = ms.Name,
                    ConnectionString = ms.ConnectionString,
                    Status = ms.Status,
                    LastBackupAt = ms.LastBackupAt
                },
                ct);

            _logger.LogDebug("Microservice status updated to {Status} for {Microservice}", status, ms.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update microservice status to {Status} for {Microservice}", status, ms.Name);
        }
    }

    private async Task ValidateDatabaseConnectionAsync(Microservice ms, CancellationToken ct)
    {
        var connParams = ParseConnectionString(ms.ConnectionString);
        
        if (!connParams.ContainsKey("PGHOST") || !connParams.ContainsKey("PGDATABASE"))
        {
            throw new InvalidOperationException("Connection string is missing required parameters (host or database)");
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(_connectionTestTimeoutSeconds));

        var processInfo = new ProcessStartInfo
        {
            FileName = "pg_isready",
            Arguments = $"--timeout={_connectionTestTimeoutSeconds}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var (key, value) in connParams)
        {
            processInfo.Environment[key] = value;
        }

        using var process = new Process { StartInfo = processInfo };
        process.Start();
        await process.WaitForExitAsync(timeoutCts.Token);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"Database connection test failed: {error}");
        }

        _logger.LogDebug("Database connection validated successfully");
    }

    private async Task ExecutePostgreSQLCommandAsync(string command, List<string> arguments, 
        string connectionString, CancellationToken ct)
    {
        var connParams = ParseConnectionString(connectionString);
        if (!connParams.ContainsKey("PGHOST"))
        {
            throw new InvalidOperationException("Connection string is missing host parameter");
        }

        // Filter out arguments containing transaction_timeout
        var filteredArguments = arguments.Where(arg => !arg.ToLowerInvariant().Contains("transaction_timeout")).ToList();

        // Log the original and filtered arguments for debugging
        _logger.LogDebug("Original arguments: {Arguments}", string.Join(" ", arguments));
        _logger.LogDebug("Filtered arguments: {FilteredArguments}", string.Join(" ", filteredArguments));

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromMinutes(_commandTimeoutMinutes));

        var processInfo = new ProcessStartInfo
        {
            FileName = command,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var arg in filteredArguments)
        {
            processInfo.ArgumentList.Add(arg);
        }

        foreach (var (key, value) in connParams)
        {
            processInfo.Environment[key] = value;
        }

        var safeArguments = filteredArguments.Any(a => a.ToLowerInvariant().Contains("password"))
            ? string.Join(" ", filteredArguments.Select(a => a.ToLowerInvariant().Contains("password") ? "[HIDDEN]" : a))
            : string.Join(" ", filteredArguments);
        
        _logger.LogDebug("Executing PostgreSQL command: {Command} {Arguments}", command, safeArguments);

        using var process = new Process { StartInfo = processInfo };
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                outputBuilder.AppendLine(e.Data);
                if (e.Data.Contains("ERROR") || e.Data.Contains("FATAL") || _logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("{Command} output: {Output}", command, e.Data);
                }
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                errorBuilder.AppendLine(e.Data);
                if (e.Data.Contains("ERROR") || e.Data.Contains("FATAL"))
                {
                    _logger.LogError("{Command} error: {Error}", command, e.Data);
                }
                else if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("{Command} info: {Info}", command, e.Data);
                }
            }
        };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            await process.WaitForExitAsync(timeoutCts.Token);

            if (process.ExitCode != 0)
            {
                var errorOutput = errorBuilder.ToString();
                var outputContent = outputBuilder.ToString();

                // Check if the error is related to transaction_timeout
                if (errorOutput.Contains("unrecognized configuration parameter \"transaction_timeout\""))
                {
                    _logger.LogWarning("Skipped error for unrecognized parameter 'transaction_timeout'. Continuing restore process.");
                    return;
                }

                _logger.LogError("PostgreSQL command failed with exit code {ExitCode}. Error: {Error}. Output: {Output}", 
                    process.ExitCode, errorOutput, outputContent);
                throw new InvalidOperationException($"Command {command} failed with exit code {process.ExitCode}. Error: {errorOutput}");
            }

            _logger.LogDebug("PostgreSQL command completed successfully");
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested && !ct.IsCancellationRequested)
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (Exception killEx)
            {
                _logger.LogWarning(killEx, "Failed to kill timed-out process");
            }
            throw new TimeoutException($"PostgreSQL command timed out after {_commandTimeoutMinutes} minutes");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (Exception killEx)
            {
                _logger.LogWarning(killEx, "Failed to kill cancelled process");
            }
            throw;
        }
    }

    private static string GetDatabaseName(string connectionString)
    {
        var connParams = ParseConnectionString(connectionString);
        return connParams.TryGetValue("PGDATABASE", out var dbName) ? dbName : 
            throw new InvalidOperationException("Database name not found in connection string");
    }

    private static string UpdateConnectionStringDatabase(string connectionString, string newDatabaseName)
    {
        if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(connectionString);
            var newUri = new UriBuilder(uri) { Path = "/" + newDatabaseName };
            return newUri.ToString();
        }
        else
        {
            var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var newParts = parts.Select(part =>
            {
                var keyValue = part.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
                if (keyValue.Length == 2 && keyValue[0].Trim().Equals("database", StringComparison.OrdinalIgnoreCase))
                {
                    return $"database={newDatabaseName}";
                }
                return part;
            });
            return string.Join(";", newParts);
        }
    }

    private static Dictionary<string, string> ParseConnectionString(string connectionString)
    {
        var envVars = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(connectionString))
            return envVars;

        if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(connectionString);
            envVars["PGHOST"] = uri.Host;
            envVars["PGPORT"] = uri.Port.ToString();
            envVars["PGDATABASE"] = uri.AbsolutePath.TrimStart('/');
            if (!string.IsNullOrEmpty(uri.UserInfo))
            {
                var userInfo = uri.UserInfo.Split(':');
                if (userInfo.Length > 0) envVars["PGUSER"] = userInfo[0];
                if (userInfo.Length > 1) envVars["PGPASSWORD"] = userInfo[1];
            }
        }
        else
        {
            var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var keyValue = part.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
                if (keyValue.Length != 2) continue;

                var key = keyValue[0].Trim();
                var value = keyValue[1].Trim();

                switch (key.ToLowerInvariant())
                {
                    case "host":
                    case "server":
                        envVars["PGHOST"] = value;
                        break;
                    case "port":
                        envVars["PGPORT"] = value;
                        break;
                    case "database":
                        envVars["PGDATABASE"] = value;
                        break;
                    case "username":
                    case "user":
                    case "user id":
                    case "userid":
                    case "uid":
                        envVars["PGUSER"] = value;
                        break;
                    case "password":
                    case "pwd":
                        envVars["PGPASSWORD"] = value;
                        break;
                }
            }
        }

        return envVars;
    }

    private static int CalculateEstimatedRestoreTime(long backupFileSize)
    {
        var sizeBasedMinutes = (int)Math.Ceiling(backupFileSize / (100.0 * 1024 * 1024));
        return Math.Max(2, sizeBasedMinutes);
    }
}