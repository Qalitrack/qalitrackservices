//
// using System.Data;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.Logging;
//
// namespace UserService.Core.Services
// {
//     public class RestoreService(IConfiguration config, ILogger<RestoreService> logger)
//     {
//         private readonly IConfiguration _config = config ?? throw new ArgumentNullException(nameof(config));
//         private readonly ILogger<RestoreService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//
//         public async Task RestoreDatabaseAsync(string backupPath, CancellationToken cancellationToken = default)
//         {
//             if (string.IsNullOrEmpty(backupPath))
//                 throw new ArgumentException("Backup path cannot be null or empty", nameof(backupPath));
//
//             if (!File.Exists(backupPath))
//                 throw new FileNotFoundException("Backup file not found", backupPath);
//
//             try
//             {
//                 var connectionString = _config.GetConnectionString("DefaultConnection");
//                 var databasePath = connectionString!.Replace("Data Source=", "").Trim(';');
//                 var backupDir = Path.GetDirectoryName(databasePath);
//                 
//                 // Ensure the directory exists
//                 if (!string.IsNullOrEmpty(backupDir) && !Directory.Exists(backupDir))
//                 {
//                     Directory.CreateDirectory(backupDir);
//                 }
//
//                 // Create a backup of the current database if it exists
//                 if (File.Exists(databasePath))
//                 {
//                     var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
//                     var backupFile = Path.Combine(
//                         Path.GetDirectoryName(databasePath) ?? string.Empty,
//                         $"pre_restore_{timestamp}_{Path.GetFileName(databasePath)}");
//                     
//                     File.Copy(databasePath, backupFile, true);
//                     _logger.LogInformation("Created pre-restore backup at {BackupFile}", backupFile);
//                 }
//
//                 // Copy the backup file to the database location
//                 File.Copy(backupPath, databasePath, overwrite: true);
//                 
//                 // Verify the database is valid by trying to open it
//
//                 using (var connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString)){
//                     await connection.OpenAsync(cancellationToken);
//                     using var command = connection.CreateCommand();
//                     command.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
//                     await command.ExecuteScalarAsync(cancellationToken);
//                 }
//
//                 _logger.LogInformation("Database restored successfully from {BackupPath}", backupPath);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error restoring database from {BackupPath}", backupPath);
//                 throw new InvalidOperationException($"Failed to restore database: {ex.Message}", ex);
//             }
//         }
//     }
// }