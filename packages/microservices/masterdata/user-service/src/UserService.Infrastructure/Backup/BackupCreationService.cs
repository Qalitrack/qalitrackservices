using System.Diagnostics;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Text.RegularExpressions;
using Messaging.Contracts.Messaging.contracts;
using Messaging.Contracts.Messaging.contracts.Enums;
using Messaging.Contracts.Messaging.contracts.Events.BackupEvents;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace UserService.Infrastructure.Backup;

/// <summary>
/// Service responsible for creating PostgreSQL database backups using native PostgreSQL tools.
/// Supports both full backups (pg_basebackup) and incremental backups (WAL files).
/// Manages backup chains, retention policies, and ensures backup integrity.
/// </summary>
public class BackupCreationService : IBackupCreationService, IAsyncDisposable
{
    private const int COMMAND_TIMEOUT_SECONDS = 3600;
    private const int CACHE_EXPIRATION_HOURS = 24;
    private const long MINIMUM_DISK_SPACE_BYTES = 1_000_000_000; // 1GB
    private const int MAX_RETRY_ATTEMPTS = 3;
    private const int RETRY_DELAY_MS = 1000;

    private readonly ILogger<BackupCreationService> _logger;
    private readonly IOptions<BackupOptions> _options;
    private readonly IDistributedCache _cache;
    private readonly IBackupMetadataService _metadataService;
    private readonly IBackupVerificationService _verificationService;
    private readonly IFileSystem _fileSystem;
    private readonly string _connectionString;
    private readonly string _pgBinPath;
    private readonly string _walArchivePath;
    private readonly string _hostBackupPath;
    private readonly string _pgHost;
    private readonly string _pgPort;
    private readonly string _pgDatabase;
    private readonly string _pgUsername;
    private readonly string _pgPassword;
    private readonly SemaphoreSlim _operationLock = new(1, 1);

    public BackupCreationService(
        IConfiguration config,
        ILogger<BackupCreationService> logger,
        IOptions<BackupOptions> options,
        IDistributedCache cache,
        IBackupMetadataService metadataService,
        IBackupVerificationService verificationService,
        IFileSystem fileSystem)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
        _verificationService = verificationService ?? throw new ArgumentNullException(nameof(verificationService));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

        _connectionString = config?.GetConnectionString("DefaultConnection") ??
            throw new InvalidOperationException("DefaultConnection string is not configured");
        
        // Parse connection string to extract PostgreSQL connection parameters
        var connectionParams = ParseConnectionString(_connectionString);
        _pgHost = connectionParams.Host;
        _pgPort = connectionParams.Port;
        _pgDatabase = connectionParams.Database;
        _pgUsername = connectionParams.Username;
        _pgPassword = connectionParams.Password;

        _pgBinPath = config["Postgres:BinPath"] ?? "/usr/lib/postgresql/16/bin";
        _walArchivePath = config["Backup:WalArchivePath"] ??
            Environment.GetEnvironmentVariable("Backup__WalArchivePath") ??
            throw new InvalidOperationException("WAL archive path not configured");
        _hostBackupPath = config["Backup:Path"] ??
            Environment.GetEnvironmentVariable("Backup__Path") ??
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "UserService", "Backups");

        // Log configuration values for debugging
        _logger.LogInformation("Backup configuration: Path={BackupPath}, WalArchivePath={WalArchivePath}, PgHost={PgHost}, PgPort={PgPort}",
            _hostBackupPath, _walArchivePath, _pgHost, _pgPort);

        EnsureBackupDirectoryExists();
        VerifyDiskSpace();
    }

    /// <summary>
    /// Parses PostgreSQL connection string to extract connection parameters.
    /// </summary>
    private (string Host, string Port, string Database, string Username, string Password) ParseConnectionString(string connectionString)
    {
        var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
        return (
            Host: builder.Host ?? "localhost",
            Port: builder.Port.ToString(),
            Database: builder.Database ?? throw new InvalidOperationException("Database name not found in connection string"),
            Username: builder.Username ?? throw new InvalidOperationException("Username not found in connection string"),
            Password: builder.Password ?? throw new InvalidOperationException("Password not found in connection string")
        );
    }

    /// <summary>
    /// Creates a database backup of the specified type (Full or Incremental).
    /// Full backups use pg_basebackup, incremental backups collect WAL files since the last full backup.
    /// </summary>
    /// <param name="backupType">Type of backup to create (Full or Incremental)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>BackupResult containing metadata about the created backup</returns>
    public async Task<BackupResult> CreateBackupAsync(BackupType backupType, CancellationToken cancellationToken = default)
        {
            await _operationLock.WaitAsync(cancellationToken);
            
            try
            {
                VerifyDiskSpace(); // Check before starting backup
                
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var backupTypeStr = backupType == BackupType.Full ? "full" : "incremental";
                var backupFileName = $"UserService_{backupTypeStr}_{timestamp}.tar.gz";
                var backupPath = Path.Combine(_hostBackupPath, backupFileName);

                _logger.LogInformation("Creating {BackupType} backup: {BackupFileName}", backupType, backupFileName);

                // Cache current database state for continuity during backup
                await CacheCurrentDatabaseStateAsync(cancellationToken);

                try
                {
                    if (backupType == BackupType.Full)
                    {
                        await CreateFullBackupAsync(backupPath, cancellationToken);
                        await _metadataService.UpdateMetadataWithFullBackupAsync(backupFileName, cancellationToken);
                    }
                    else
                    {
                        await CreateIncrementalBackupAsync(backupPath, cancellationToken);
                        // Check if the backup file was actually created
                        if (!_fileSystem.File.Exists(backupPath))
                        {
                            _logger.LogInformation("No new changes detected; incremental backup not created for {BackupFileName}", backupFileName);
                            return new BackupResult
                            {
                                BackupId = Path.GetFileNameWithoutExtension(backupFileName),
                                FileName = backupFileName,
                                BackupType = backupType,
                                CreatedAt = DateTime.UtcNow,
                                FilePath = null, // No file created
                                FileSizeBytes = 0,
                                IsValid = true, // Valid operation, no changes to back up
                                ServiceName = "UserService",
                                ChainId = null, // Adjust if metadata requires a chain ID
                                ChainInfo = null
                            };
                        }
                        await _metadataService.UpdateMetadataWithIncrementalBackupAsync(backupFileName, cancellationToken);
                    }

                    await _verificationService.VerifyBackupIntegrityAsync(backupPath, backupType, cancellationToken);
                    await CleanupOldBackupsAsync(cancellationToken);
                    
                    var fileInfo = _fileSystem.FileInfo.FromFileName(backupPath);
                    var result = new BackupResult
                    {
                        BackupId = Path.GetFileNameWithoutExtension(backupFileName),
                        FileName = backupFileName,
                        BackupType = backupType,
                        CreatedAt = DateTime.UtcNow,
                        FilePath = backupPath,
                        FileSizeBytes = fileInfo.Length,
                        IsValid = true,
                        ServiceName = "UserService"
                    };

                    // Get updated chain information
                    var chain = await _metadataService.GetBackupChainAsync(backupFileName);
                    if (chain != null)
                    {
                        result.ChainId = _metadataService.GetChainId(chain);
                        result.ChainInfo = _metadataService.MapToBackupChainInfo(chain);
                    }
                    
                    _logger.LogInformation("Successfully created {BackupType} backup: {BackupId} ({FileSizeBytes} bytes)", 
                        backupType, result.BackupId, result.FileSizeBytes);
                    
                    return result;
                }
                catch (Exception ex)
                {
                    SafeDeleteFile(backupPath);
                    _logger.LogError(ex, "Error creating {BackupType} backup", backupType);
                    throw new InvalidOperationException($"Failed to create {backupType} backup", ex);
                }
                finally
                {
                    await ClearBackupCacheAsync(cancellationToken);
                }
            }
            finally
            {
                _operationLock.Release();
            }
        }
    /// <summary>
    /// Creates a full database backup using pg_basebackup command directly.
    /// </summary>
    private async Task CreateFullBackupAsync(string backupPath, CancellationToken cancellationToken)
    {
        var tempBackupDir = Path.Combine(Path.GetTempPath(), $"pg_backup_{Guid.NewGuid():N}");
        
        try
        {
            _logger.LogDebug("Starting full backup creation to {BackupPath}", backupPath);
            
            // Create temporary backup directory
            _fileSystem.Directory.CreateDirectory(tempBackupDir);
            
            // Find pg_basebackup executable
            var pgBasebackupPath = await FindPostgreSQLExecutableAsync("pg_basebackup");
            var arguments = $"-D \"{tempBackupDir}\" -F tar -z -v --wal-method=fetch -h {_pgHost} -p {_pgPort} -U {_pgUsername}";
            
            await ExecuteWithRetryAsync(() => 
                ExecutePostgreSQLCommandAsync(pgBasebackupPath, arguments, cancellationToken),
                "create base backup", cancellationToken);
            
            // Move the generated backup file to final location
            var generatedBackupFile = Path.Combine(tempBackupDir, "base.tar.gz");
            if (_fileSystem.File.Exists(generatedBackupFile))
            {
                _fileSystem.File.Move(generatedBackupFile, backupPath);
            }
            else
            {
                throw new FileNotFoundException($"Expected backup file not found: {generatedBackupFile}");
            }

            _logger.LogDebug("Full backup created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create full backup");
            throw;
        }
        finally
        {
            // Cleanup temporary directory
            try
            {
                if (_fileSystem.Directory.Exists(tempBackupDir))
                {
                    _fileSystem.Directory.Delete(tempBackupDir, recursive: true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cleanup temporary backup directory: {TempDir}", tempBackupDir);
            }
        }
    }

    /// <summary>
    /// Finds PostgreSQL executable by checking common paths and PATH environment variable.
    /// </summary>
    private async Task<string> FindPostgreSQLExecutableAsync(string executableName)
    {
        // First try the configured path
        var configuredPath = Path.Combine(_pgBinPath, executableName);
        if (_fileSystem.File.Exists(configuredPath))
        {
            _logger.LogDebug("Found PostgreSQL executable at configured path: {Path}", configuredPath);
            return configuredPath;
        }

        // Try common PostgreSQL installation paths
        var commonPaths = new[]
        {
            $"/usr/bin/{executableName}",
            $"/usr/local/bin/{executableName}",
            $"/usr/local/pgsql/bin/{executableName}",
            $"/opt/postgresql/bin/{executableName}",
            executableName // Try PATH
        };

        foreach (var path in commonPaths)
        {
            try
            {
                // Test if executable exists and is accessible
                var testResult = await ExecuteSystemCommandAsync("which", path, CancellationToken.None);
                if (!string.IsNullOrWhiteSpace(testResult))
                {
                    _logger.LogDebug("Found PostgreSQL executable via 'which': {Path}", path);
                    return path;
                }
            }
            catch
            {
                // Continue searching
            }
        }

        // If 'which' doesn't work, try direct execution test
        foreach (var path in commonPaths)
        {
            try
            {
                var testStartInfo = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var testProcess = new Process { StartInfo = testStartInfo };
                testProcess.Start();
                await testProcess.WaitForExitAsync(CancellationToken.None);

                if (testProcess.ExitCode == 0)
                {
                    _logger.LogDebug("Found PostgreSQL executable via direct test: {Path}", path);
                    return path;
                }
            }
            catch
            {
                // Continue searching
            }
        }

        throw new FileNotFoundException($"PostgreSQL executable '{executableName}' not found. Please ensure PostgreSQL client tools are installed in the container.");
    }

    /// <summary>
    /// Creates an incremental backup by collecting WAL files since the last full backup.
    /// </summary>
    private async Task CreateIncrementalBackupAsync(string backupPath, CancellationToken cancellationToken)
    {
        var latestChain = await _metadataService.GetLatestFullBackupChainAsync();
        if (latestChain == null)
        {
            throw new InvalidOperationException("Cannot create incremental backup: No full backup exists. Create a full backup first.");
        }

        var lastFullTime = latestChain.Timestamp;
        var tempWalDir = Path.Combine(Path.GetTempPath(), $"wal_backup_{Guid.NewGuid():N}");
        
        try
        {
            _logger.LogDebug("Starting incremental backup since {LastFullTime}", lastFullTime);
            
            // Create temporary WAL directory
            _fileSystem.Directory.CreateDirectory(tempWalDir);
            
            // Find WAL files since last full backup
            var walFiles = _fileSystem.Directory.GetFiles(_walArchivePath, "*.wal")
                .Where(f => _fileSystem.File.GetCreationTimeUtc(f) > lastFullTime)
                .ToList();
            
            if (!walFiles.Any())
            {
                _logger.LogInformation("No new WAL files found since last full backup ({LastFullTime}). Incremental backup not needed.", lastFullTime);
                return;
            }

            _logger.LogInformation("Found {WalCount} WAL files for incremental backup since {LastFullTime}", walFiles.Count, lastFullTime);
            
            // Copy WAL files to temporary directory
            foreach (var walFile in walFiles)
            {
                var fileName = Path.GetFileName(walFile);
                var destPath = Path.Combine(tempWalDir, fileName);
                _fileSystem.File.Copy(walFile, destPath);
            }
            
            // Create compressed archive of WAL files using tar (if available) or built-in compression
            await ExecuteWithRetryAsync(() => 
                CreateCompressedArchiveAsync(tempWalDir, backupPath, cancellationToken),
                "create WAL archive", cancellationToken);

            _logger.LogDebug("Incremental backup created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create incremental backup");
            throw;
        }
        finally
        {
            // Cleanup temporary directory
            try
            {
                if (_fileSystem.Directory.Exists(tempWalDir))
                {
                    _fileSystem.Directory.Delete(tempWalDir, recursive: true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cleanup temporary WAL directory: {TempDir}", tempWalDir);
            }
        }
    }

    /// <summary>
    /// Executes a PostgreSQL command directly with proper environment setup.
    /// </summary>
    private async Task<string> ExecutePostgreSQLCommandAsync(string command, string arguments, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        // Set PostgreSQL environment variables for authentication
        startInfo.Environment["PGPASSWORD"] = _pgPassword;
        startInfo.Environment["PGHOST"] = _pgHost;
        startInfo.Environment["PGPORT"] = _pgPort;
        startInfo.Environment["PGUSER"] = _pgUsername;
        startInfo.Environment["PGDATABASE"] = _pgDatabase;

        using var process = new Process { StartInfo = startInfo };
        _logger.LogDebug("Executing PostgreSQL command: {Command} {Arguments}", command, arguments);
        
        try
        {
            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            // Apply timeout to the process
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(COMMAND_TIMEOUT_SECONDS));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var processTask = process.WaitForExitAsync(combinedCts.Token);
            
            await Task.WhenAll(outputTask, errorTask, processTask);

            var output = await outputTask;
            var error = await errorTask;

            if (process.ExitCode != 0)
            {
                _logger.LogError("PostgreSQL command failed with exit code {ExitCode}. Command: {Command}, Error: {Error}", 
                    process.ExitCode, command, error);
                throw new InvalidOperationException($"PostgreSQL command failed with exit code {process.ExitCode}: {command}\nError: {error}");
            }

            _logger.LogDebug("PostgreSQL command completed successfully");
            return output;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("PostgreSQL command was cancelled: {Command}", command);
            if (!process.HasExited)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to kill PostgreSQL process after cancellation");
                }
            }
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("PostgreSQL command timed out after {TimeoutSeconds} seconds: {Command}", COMMAND_TIMEOUT_SECONDS, command);
            if (!process.HasExited)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to kill PostgreSQL process after timeout");
                }
            }
            throw new TimeoutException($"PostgreSQL command timed out: {command}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error executing PostgreSQL command: {Command}", command);
            throw;
        }
    }

    /// <summary>
    /// Creates a compressed archive from the source directory.
    /// </summary>
    private async Task<string> CreateCompressedArchiveAsync(string sourceDir, string outputPath, CancellationToken cancellationToken)
    {
        // Try to use tar if available (Linux/Unix), otherwise use built-in compression
        try
        {
            var tarCommand = "tar";
            var tarArguments = $"-czf \"{outputPath}\" -C \"{sourceDir}\" .";
            return await ExecuteSystemCommandAsync(tarCommand, tarArguments, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "tar command not available, falling back to built-in compression");
            
            // Fallback to built-in .NET compression
            using var archive = System.IO.Compression.ZipFile.Open(outputPath.Replace(".tar.gz", ".zip"), System.IO.Compression.ZipArchiveMode.Create);
            
            var files = _fileSystem.Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var relativePath = Path.GetRelativePath(sourceDir, file);
                archive.CreateEntryFromFile(file, relativePath);
            }
            
            return "Archive created using built-in compression";
        }
    }

    /// <summary>
    /// Executes a system command with timeout and cancellation support.
    /// </summary>
    private async Task<string> ExecuteSystemCommandAsync(string command, string arguments, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        _logger.LogDebug("Executing system command: {Command} {Arguments}", command, arguments);
        
        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(COMMAND_TIMEOUT_SECONDS));
        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        var processTask = process.WaitForExitAsync(combinedCts.Token);
        
        await Task.WhenAll(outputTask, errorTask, processTask);

        var output = await outputTask;
        var error = await errorTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Command failed with exit code {process.ExitCode}: {command}\nError: {error}");
        }

        return output;
    }

    /// <summary>
    /// Executes an operation with retry logic for transient failures.
    /// </summary>
    private async Task<string> ExecuteWithRetryAsync(Func<Task<string>> operation, string operationName, CancellationToken cancellationToken)
    {
        var lastException = new Exception();
        
        for (var attempt = 1; attempt <= MAX_RETRY_ATTEMPTS; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (attempt < MAX_RETRY_ATTEMPTS)
            {
                lastException = ex;
                _logger.LogWarning(ex, "Attempt {Attempt} failed for {OperationName}, retrying in {DelayMs}ms", 
                    attempt, operationName, RETRY_DELAY_MS);
                
                await Task.Delay(RETRY_DELAY_MS, cancellationToken);
            }
        }
        
        _logger.LogError(lastException, "All {MaxAttempts} attempts failed for {OperationName}", MAX_RETRY_ATTEMPTS, operationName);
        throw new InvalidOperationException($"Operation '{operationName}' failed after {MAX_RETRY_ATTEMPTS} attempts", lastException);
    }

    /// <summary>
    /// Verifies that sufficient disk space is available for backup operations.
    /// </summary>
    private void VerifyDiskSpace()
    {
        try
        {
            var drive = _fileSystem.DriveInfo.FromDriveName(_hostBackupPath);
            if (drive.AvailableFreeSpace < MINIMUM_DISK_SPACE_BYTES)
            {
                throw new InvalidOperationException(
                    $"Insufficient disk space for backup. Required: {MINIMUM_DISK_SPACE_BYTES:N0} bytes, Available: {drive.AvailableFreeSpace:N0} bytes");
            }
            
            _logger.LogDebug("Disk space verified: {AvailableSpace:N0} bytes available", drive.AvailableFreeSpace);
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            _logger.LogWarning(ex, "Could not verify disk space, continuing with backup operation");
        }
    }

    /// <summary>
    /// Caches the current database state during backup operations for continuity.
    /// </summary>
    private async Task CacheCurrentDatabaseStateAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _cache.SetStringAsync("backup:state", "active", new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CACHE_EXPIRATION_HOURS)
            }, cancellationToken);
            _logger.LogDebug("Cached current database state for backup operation");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache database state, continuing without cache");
        }
    }

    /// <summary>
    /// Clears the backup operation cache.
    /// </summary>
    private async Task ClearBackupCacheAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _cache.RemoveAsync("backup:state", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to clear backup cache");
        }
    }

    /// <summary>
    /// Removes old backup chains based on the configured retention policy.
    /// Always keeps at least one full backup chain.
    /// </summary>
    private async Task CleanupOldBackupsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var retentionDays = _options.Value.RetentionDays ?? 30;
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

            var metadata = await _metadataService.LoadMetadataAsync();
            var chainsToRemove = metadata.Chains
                .Where(c => c.Timestamp < cutoffDate)
                .OrderBy(c => c.Timestamp)
                .ToList();
            
            // Always keep at least one full backup chain (the most recent)
            if (chainsToRemove.Count >= metadata.Chains.Count)
            {
                var mostRecentChain = metadata.Chains.MaxBy(c => c.Timestamp);
                if (mostRecentChain != null)
                {
                    chainsToRemove.Remove(mostRecentChain);
                    _logger.LogInformation("Keeping most recent backup chain: {FullBackup} (created {Timestamp})", 
                        mostRecentChain.FullBackupFile, mostRecentChain.Timestamp);
                }
            }

            foreach (var chain in chainsToRemove)
            {
                _logger.LogInformation("Removing expired backup chain: {FullBackup} with {IncrementalCount} incrementals", 
                    chain.FullBackupFile, chain.Incrementals.Count);

                SafeDeleteFile(Path.Combine(_hostBackupPath, chain.FullBackupFile));

                foreach (var incFile in chain.Incrementals)
                {
                    SafeDeleteFile(Path.Combine(_hostBackupPath, incFile));
                }

                metadata.Chains.Remove(chain);
            }

            if (chainsToRemove.Any())
            {
                await _metadataService.SaveMetadataAsync(metadata);
                _logger.LogInformation("Cleaned up {Count} old backup chains. {RemainingCount} chains remain.", 
                    chainsToRemove.Count, metadata.Chains.Count);
            }
            else
            {
                _logger.LogDebug("No backup chains exceeded retention period ({RetentionDays} days)", retentionDays);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cleanup old backups");
        }
    }

    /// <summary>
    /// Ensures the backup directory exists and is accessible.
    /// </summary>
    private void EnsureBackupDirectoryExists()
    {
        try
        {
            if (!_fileSystem.Directory.Exists(_hostBackupPath))
            {
                _fileSystem.Directory.CreateDirectory(_hostBackupPath);
                _logger.LogInformation("Created backup directory: {BackupPath}", _hostBackupPath);
            }

            var dirInfo = _fileSystem.DirectoryInfo.FromDirectoryName(_hostBackupPath);
            if (!dirInfo.Exists)
            {
                throw new DirectoryNotFoundException($"Could not create backup directory: {_hostBackupPath}");
            }

            _logger.LogInformation("Backup directory verified: {BackupPath}", _hostBackupPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create backup directory: {BackupPath}", _hostBackupPath);
            throw new InvalidOperationException($"Cannot access backup directory: {_hostBackupPath}", ex);
        }
    }

    /// <summary>
    /// Safely deletes a file, logging warnings if the operation fails.
    /// </summary>
    private void SafeDeleteFile(string filePath)
    {
        try
        {
            if (_fileSystem.File.Exists(filePath))
            {
                _fileSystem.File.Delete(filePath);
                _logger.LogDebug("Deleted file: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete file: {FilePath}", filePath);
        }
    }

    public async ValueTask DisposeAsync()
    {
        _operationLock.Dispose();
    }

    public void Dispose()
    {
        _metadataService?.Dispose();
        _operationLock?.Dispose();
    }
}