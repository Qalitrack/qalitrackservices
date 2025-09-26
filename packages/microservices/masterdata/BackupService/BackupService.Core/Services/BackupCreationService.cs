using System.Diagnostics;
using System.IO.Abstractions;
using System.Text;
using BackupService.Core.Dtos;
using BackupService.Core.Entities;
using BackupService.Core.Enums;
using BackupService.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackupService.Core.Services;

public class BackupCreationService : IBackupCreationService
{
    private readonly IMicroserviceRepository _microserviceRepository;
    private readonly IBackupMetadataService _backupMetadataService;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<BackupCreationService> _logger;
    private readonly SemaphoreSlim _operationLock = new(1, 1);
    private readonly int _commandTimeoutMinutes;
    private readonly int _connectionTestTimeoutSeconds;

    private static class PostgreSQLCommands
    {
        public const string Verbose = "--verbose";
        public const string Compress = "--compress=9";
        public const string Quiet = "--quiet";
        public const string TuplesOnly = "--tuples-only";
        public const string NoPassword = "--no-password";
        public const string Format = "--format=custom";
        public const string Blobs = "--blobs";
        public const string CreateDb = "--create";
        public const string CleanFirst = "--clean";
        public const string IfExists = "--if-exists";
    }

    public BackupCreationService(
        IMicroserviceRepository microserviceRepository,
        IBackupMetadataService backupMetadataService,
        IFileSystem fileSystem,
        ILogger<BackupCreationService> logger,
        IConfiguration configuration)
    {
        _microserviceRepository = microserviceRepository;
        _backupMetadataService = backupMetadataService;
        _fileSystem = fileSystem;
        _logger = logger;
        _commandTimeoutMinutes = configuration.GetValue<int>("Backup:CommandTimeoutMinutes", 30);
        _connectionTestTimeoutSeconds = configuration.GetValue<int>("Backup:ConnectionTestTimeoutSeconds", 10);
    }

    public async Task<BackupResult> CreateBackupAsync(BackupType backupType, string microservice, CancellationToken ct = default)
    {
        // ...existing code...
        // Use configured storage path internally
        var saveLocation = "/app/backups"; // Or get from config if needed
        await CheckPostgreSqlToolVersionsAsync(ct);
        if (backupType != BackupType.Full)
        {
            throw new NotSupportedException("Only full backups are supported. Incremental backups have been removed.");
        }
        await _operationLock.WaitAsync(ct);
        try
        {
            ct.ThrowIfCancellationRequested();
            var ms = await ValidateMicroserviceAsync(microservice, ct);
            await EnsureBackupDirectoryAsync(saveLocation, ct);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"{microservice}_full_{timestamp}.dump";
            var backupPath = Path.Combine(saveLocation, backupFileName);
            _logger.LogInformation("Creating SQL dump backup for {Microservice}: {BackupFileName}", microservice, backupFileName);
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var result = await CreateSqlDumpBackupAsync(backupPath, ms, ct);
                await _backupMetadataService.UpdateMetadataWithFullBackupAsync(result, microservice, ct);
                ms.LastBackupAt = DateTime.UtcNow;
                ms.Status = MicroserviceStatus.Active;
                ms.UpdatedAt = DateTime.UtcNow;
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
                _logger.LogInformation("SQL dump backup completed successfully for {Microservice}. Size: {SizeBytes} bytes", microservice, result.FileSizeBytes);
                return result;
            }, ms, microservice, backupPath, ct);
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

    private async Task<Microservice> ValidateMicroserviceAsync(string microservice, CancellationToken ct)
    {
        var ms = await _microserviceRepository.GetMicroserviceAsync(microservice, ct)
            ?? throw new KeyNotFoundException($"Microservice {microservice} not found");

        if (ms.Status != MicroserviceStatus.Active)
        {
            _logger.LogWarning("Microservice {Name} is {Status}, cannot backup", microservice, ms.Status);
            throw new InvalidOperationException($"Microservice {microservice} is not Active");
        }

        if (string.IsNullOrWhiteSpace(ms.ConnectionString))
        {
            _logger.LogError("Connection string is missing for microservice {Name}", microservice);
            throw new InvalidOperationException($"Connection string is not configured for microservice {microservice}");
        }

        var connParams = ParseConnectionString(ms.ConnectionString);
        if (!connParams.ContainsKey("PGHOST") || !connParams.ContainsKey("PGDATABASE"))
        {
            _logger.LogError("Connection string for {Microservice} is missing required parameters (host or database)", microservice);
            throw new InvalidOperationException("Connection string is missing required parameters");
        }

        await ExecuteWithRetryAsync(async () =>
        {
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
            return true;
        }, "DatabaseConnectionValidation", ct: ct);

        return ms;
    }

    private async Task<BackupResult> CreateSqlDumpBackupAsync(string backupPath, Microservice ms, CancellationToken ct)
    {
        if (ms.Status != MicroserviceStatus.Active || string.IsNullOrWhiteSpace(ms.ConnectionString))
            throw new InvalidOperationException($"Microservice {ms.Name} is not Active or has invalid connection string");

        var connParams = ParseConnectionString(ms.ConnectionString);
        var databaseName = connParams["PGDATABASE"];

        var arguments = new List<string>
        {
            PostgreSQLCommands.Format,
            PostgreSQLCommands.Compress,
            PostgreSQLCommands.Verbose,
            PostgreSQLCommands.Blobs,
            PostgreSQLCommands.CreateDb,
            PostgreSQLCommands.CleanFirst,
            PostgreSQLCommands.IfExists,
            PostgreSQLCommands.NoPassword,
            "--file", backupPath,
            databaseName
        };

        _logger.LogDebug("Creating SQL dump with arguments: {Arguments}", 
            string.Join(" ", arguments.Where(a => !a.Contains("password", StringComparison.OrdinalIgnoreCase))));

        await ExecuteWithRetryAsync(async () =>
            await ExecutePostgreSQLCommandAsync("pg_dump", arguments, ms.ConnectionString, ct),
            "SqlDumpBackup", ct: ct);

        var fileInfo = _fileSystem.FileInfo.New(backupPath);
        if (!fileInfo.Exists)
        {
            throw new InvalidOperationException($"SQL dump file was not created at {backupPath}");
        }

        var chainId = Guid.NewGuid().ToString();

        _logger.LogInformation("SQL dump backup created successfully for {Microservice}. File: {BackupPath}, Size: {SizeBytes} bytes", 
            ms.Name, backupPath, fileInfo.Length);

        return new BackupResult
        {
            BackupId = Path.GetFileNameWithoutExtension(Path.GetFileName(backupPath)),
            FileName = Path.GetFileName(backupPath),
            BackupType = BackupType.Full,
            Timestamp = DateTime.UtcNow,
            FilePath = backupPath,
            FileSizeBytes = fileInfo.Length,
            IsValid = true,
            ServiceName = ms.Name,
            ChainId = chainId,
        };
    }

    private async Task ExecuteWithRetryAsync(Func<Task> operation, string operationName, int maxRetries = 3, CancellationToken ct = default)
    {
        int retryCount = 0;
        while (true)
        {
            try
            {
                await operation();
                return;
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is TimeoutException)
            {
                retryCount++;
                if (retryCount >= maxRetries)
                {
                    _logger.LogError(ex, "Failed to execute {OperationName} after {MaxRetries} retries", operationName, maxRetries);
                    throw;
                }
                var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                _logger.LogWarning(ex, "Retry {RetryCount}/{MaxRetries} for {OperationName} after {DelaySeconds}s", 
                    retryCount, maxRetries, operationName, delay.TotalSeconds);
                await Task.Delay(delay, ct);
            }
        }
    }

    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, string operationName, int maxRetries = 3, CancellationToken ct = default)
    {
        int retryCount = 0;
        while (true)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is TimeoutException)
            {
                retryCount++;
                if (retryCount >= maxRetries)
                {
                    _logger.LogError(ex, "Failed to execute {OperationName} after {MaxRetries} retries", operationName, maxRetries);
                    throw;
                }
                var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                _logger.LogWarning(ex, "Retry {RetryCount}/{MaxRetries} for {OperationName} after {DelaySeconds}s", 
                    retryCount, maxRetries, operationName, delay.TotalSeconds);
                await Task.Delay(delay, ct);
            }
        }
    }

    private async Task ExecutePostgreSQLCommandAsync(string command, List<string> arguments, 
        string connectionString, CancellationToken ct)
    {
        var connParams = ParseConnectionString(connectionString);
        if (!connParams.ContainsKey("PGHOST") || !connParams.ContainsKey("PGDATABASE"))
        {
            throw new InvalidOperationException("Connection string is missing required parameters (host or database)");
        }

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

        foreach (var arg in arguments)
        {
            processInfo.ArgumentList.Add(arg);
        }

        foreach (var (key, value) in connParams)
        {
            processInfo.Environment[key] = value;
        }

        var safeArguments = arguments.Any(a => a.ToLowerInvariant().Contains("password"))
            ? string.Join(" ", arguments.Select(a => a.ToLowerInvariant().Contains("password") ? "[HIDDEN]" : a))
            : string.Join(" ", arguments);
        
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

    private async Task<T> ExecuteWithErrorHandlingAsync<T>(Func<Task<T>> operation, Microservice ms, 
        string microservice, string backupPath, CancellationToken ct)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL dump backup failed for {Microservice}. Error: {Error}", microservice, ex.Message);
            
            if (_fileSystem.File.Exists(backupPath))
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    _fileSystem.File.Delete(backupPath);
                    _logger.LogDebug("Cleaned up failed backup file: {BackupPath}", backupPath);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogWarning(cleanupEx, "Failed to clean up backup file: {BackupPath}", backupPath);
                }
            }

            ms.Status = MicroserviceStatus.Paused;
            ms.UpdatedAt = DateTime.UtcNow;

            try
            {
                ct.ThrowIfCancellationRequested();
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
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Failed to update microservice status after backup failure for {Microservice}", microservice);
            }

            throw new InvalidOperationException($"SQL dump backup failed for {microservice}: {ex.Message}", ex);
        }
    }

    private Task EnsureBackupDirectoryAsync(string saveLocation, CancellationToken ct)
    {
        _fileSystem.Directory.CreateDirectory(saveLocation);
        return Task.CompletedTask;
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
}