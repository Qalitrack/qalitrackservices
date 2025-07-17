using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserService.Core.Enums;
using UserService.Core.Interfaces;
using UserService.Core.Options;

namespace UserService.Core.Services
{
    public class DatabaseBackupService : IDatabaseBackupService, IDisposable
    {
        private const int FILE_OPERATION_RETRY_COUNT = 3;
        private const int FILE_OPERATION_RETRY_DELAY_MS = 1000;
        private const int MUTEX_TIMEOUT_MS = 30000; // 30 seconds
        
        private readonly ILogger<DatabaseBackupService> _logger;
        private readonly IOptions<BackupOptions> _options;
        private readonly string _connectionString;
        private readonly bool _useMutex;
        private bool _disposed;

        public DatabaseBackupService(
            IConfiguration config,
            ILogger<DatabaseBackupService> logger,
            IOptions<BackupOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _connectionString = config?.GetConnectionString("DefaultConnection") ?? 
                throw new InvalidOperationException("DefaultConnection string is not configured");
                
            _useMutex = OperatingSystem.IsWindows();
            
            if (!_useMutex)
            {
                _logger.LogWarning("Mutex-based file locking is only supported on Windows. " +
                    "Using best-effort file operations without cross-process locking.");
            }
        }

        public async Task<string> CreateBackupAsync(BackupType backupType, CancellationToken cancellationToken = default)
        {
            // Generate backup file name with timestamp
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var backupTypeStr = backupType == BackupType.Full ? "Full" : "Incremental";
            var backupFileName = $"UserService_{backupTypeStr}_{timestamp}.db";
            var backupPath = Path.Combine(_options.Value.Path, backupFileName);

            _logger.LogInformation("Creating {BackupType} backup at: {BackupPath}", backupType, backupPath);

            // Ensure backup directory exists with proper permissions
            EnsureBackupDirectory();

            // Get source database path
            var sourceDbPath = _connectionString
                .Replace("Data Source=", "")
                .Split(';')[0]
                .Trim();
            
            // Ensure source file exists and is accessible
            if (!File.Exists(sourceDbPath))
            {
                throw new FileNotFoundException("Source database file not found", sourceDbPath);
            }

            // Use a mutex to prevent concurrent access to the same database file (Windows only)
            Mutex mutex = null;
            bool mutexAcquired = false;
        
            if (_useMutex)
            {
                var mutexName = $"Global\\{Path.GetFileName(sourceDbPath).Replace(".", "_")}_backup_mutex";
                mutex = new Mutex(false, mutexName);
            
                try
                {
                    // Try to acquire the mutex with timeout
                    mutexAcquired = mutex.WaitOne(MUTEX_TIMEOUT_MS);
                    if (!mutexAcquired)
                    {
                        throw new InvalidOperationException(
                            "Failed to acquire mutex for database backup within the timeout period");
                    }
                }
                catch (AbandonedMutexException)
                {
                    // The mutex was abandoned, but we still own it
                    mutexAcquired = true;
                    _logger.LogWarning("Acquired mutex that was abandoned by another process");
                }
            }

            try
            {
                // Copy the database file with retry logic and verification
                await RetryFileOperationAsync(async () =>
                {
                    // Use FileStream with FileShare.None to prevent other processes from accessing the file during copy
                    using (var sourceStream = new FileStream(
                        sourceDbPath, 
                        FileMode.Open, 
                        FileAccess.Read, 
                        FileShare.Read, // Allow other readers but no writers
                        bufferSize: 81920, // 80KB buffer
                        useAsync: true))
                    using (var destStream = new FileStream(
                        backupPath, 
                        FileMode.Create, 
                        FileAccess.Write, 
                        FileShare.None, // Prevent any access to the backup file while writing
                        bufferSize: 81920,
                        useAsync: true))
                    {
                        await sourceStream.CopyToAsync(destStream, cancellationToken);
                        await destStream.FlushAsync(cancellationToken);
                    }

                    // Verify the backup was created and has content
                    var fileInfo = new FileInfo(backupPath);
                    if (fileInfo.Length == 0)
                    {
                        throw new InvalidOperationException("Backup file was created but is empty");
                    }

                    _logger.LogInformation(
                        "Successfully created {BackupType} backup: {BackupPath} ({FileSize} bytes)", 
                        backupType, backupPath, fileInfo.Length);
                    
                }, FILE_OPERATION_RETRY_COUNT, FILE_OPERATION_RETRY_DELAY_MS, cancellationToken, "create backup");

                // Verify backup integrity
                await VerifyBackupIntegrityAsync(backupPath, cancellationToken);
            
                // Clean up old backups
                await CleanupOldBackupsAsync(backupType, cancellationToken);

                return backupPath;
            }
            catch (Exception ex)
            {
                // Clean up any partially created backup file
                SafeDeleteFile(backupPath);
            
                _logger.LogError(ex, "Error creating {BackupType} backup", backupType);
                throw new InvalidOperationException(
                    $"Failed to create {backupType.ToString().ToLower()} backup", ex);
            }
            finally
            {
                // Release the mutex if we acquired it
                if (mutex != null && mutexAcquired)
                {
                    try
                    {
                        mutex.ReleaseMutex();
                        mutex.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error releasing mutex");
                    }
                }
            }
        }

        public async Task VerifyBackupIntegrityAsync(string backupPath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(backupPath))
                throw new ArgumentException("Backup path cannot be null or empty", nameof(backupPath));

            if (!File.Exists(backupPath))
                throw new FileNotFoundException("Backup file not found", backupPath);

            _logger.LogDebug("Verifying backup integrity: {BackupPath}", backupPath);

            try
            {
                // For SQLite, we'll verify the file is a valid database by trying to open it
                var tempConnectionString = $"Data Source={backupPath};Mode=ReadOnly";
                
                using var connection = new SqliteConnection(tempConnectionString);
                await connection.OpenAsync(cancellationToken);
                
                // Run a simple query to verify the database is not corrupted
                using var command = connection.CreateCommand();
                command.CommandText = "PRAGMA integrity_check;";
                var result = await command.ExecuteScalarAsync(cancellationToken);
                
                if (result?.ToString()?.Trim() != "ok")
                {
                    _logger.LogError("Backup integrity check failed: {Result}", result);
                    throw new InvalidDataException($"Backup integrity check failed: {result}");
                }
                
                _logger.LogDebug("Backup integrity verified: {BackupPath}", backupPath);
            }
            catch (Exception ex) when (ex is not (FileNotFoundException or InvalidDataException))
            {
                _logger.LogError(ex, "Error verifying backup integrity: {BackupPath}", backupPath);
                throw new InvalidDataException("Backup file is corrupted or invalid", ex);
            }
        }

        private void EnsureBackupDirectory()
        {
            var backupDirectory = Path.GetFullPath(_options.Value.Path);
            
            try
            {
                if (!Directory.Exists(backupDirectory))
                {
                    _logger.LogInformation("Creating backup directory: {BackupDirectory}", backupDirectory);
                    Directory.CreateDirectory(backupDirectory);
                }
                
                // Verify directory is writable
                var testFile = Path.Combine(backupDirectory, $"write_test_{Guid.NewGuid()}.tmp");
                try
                {
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Backup directory is not writable: {backupDirectory}", ex);
                }
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                throw new InvalidOperationException(
                    $"Failed to initialize backup directory: {backupDirectory}", ex);
            }
        }

        private async Task CleanupOldBackupsAsync(BackupType backupType, CancellationToken cancellationToken)
        {
            try
            {
                var retentionDays = backupType == BackupType.Full ? 
                    _options.Value.FullBackupRetentionDays : 
                    _options.Value.IncrementalBackupRetentionDays;
                    
                var searchPattern = backupType == BackupType.Full ? 
                    "UserService_Full_*.db" : 
                    "UserService_Incremental_*.db";
                    
                var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
                var backupFiles = Directory.GetFiles(_options.Value.Path, searchPattern)
                    .Select(f => new FileInfo(f))
                    .Where(f => f.LastWriteTimeUtc < cutoffDate)
                    .OrderBy(f => f.LastWriteTimeUtc)
                    .ToList();
                    
                if (!backupFiles.Any())
                {
                    _logger.LogDebug("No old {BackupType} backups to clean up", backupType);
                    return;
                }
                
                _logger.LogInformation(
                    "Cleaning up {Count} old {BackupType} backups older than {CutoffDate}", 
                    backupFiles.Count, backupType, cutoffDate);
                    
                foreach (var file in backupFiles)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Backup cleanup was cancelled");
                        break;
                    }
                    
                    try
                    {
                        await RetryFileOperationAsync(() => 
                        {
                            file.Delete();
                            return Task.CompletedTask;
                        }, 3, 500, cancellationToken, $"delete old backup {file.Name}");
                        
                        _logger.LogDebug("Deleted old backup: {BackupFile}", file.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to delete old backup: {BackupFile}", file.Name);
                        // Continue with next file
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup of old {BackupType} backups", backupType);
                // Don't rethrow - this is a non-critical operation
            }
        }
    
        private async Task RetryFileOperationAsync(
            Func<Task> operation,
            int maxRetries,
            int delayMs,
            CancellationToken cancellationToken,
            string operationDescription)
        {
            int attempt = 0;
            while (true)
            {
                try
                {
                    await operation();
                    return;
                }
                catch (Exception ex) when (attempt < maxRetries - 1 && 
                                         (ex is IOException or UnauthorizedAccessException))
                {
                    attempt++;
                    _logger.LogWarning(
                        "Attempt {Attempt}/{MaxRetries} failed for {Operation}. Retrying in {DelayMs}ms...", 
                        attempt, maxRetries, operationDescription, delayMs);
                        
                    await Task.Delay(delayMs, cancellationToken);
                    delayMs *= 2; // Exponential backoff
                }
            }
        }
    
        private void SafeDeleteFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;
                
            try
            {
                File.Delete(path);
                _logger.LogDebug("Cleaned up file: {Path}", path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clean up file: {Path}", path);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here
                }
                
                _disposed = true;
            }
        }

        ~DatabaseBackupService()
        {
            Dispose(false);
        }
    }
}