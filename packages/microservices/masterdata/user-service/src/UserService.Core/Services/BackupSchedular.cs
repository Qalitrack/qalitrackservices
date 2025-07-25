//
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;
// using UserService.Core.Enums;
// using UserService.Core.Interfaces;
// using UserService.Core.Options;
//
// namespace UserService.Core.Services
// {
//     public class BackupScheduler : BackgroundService
//     {
//         private readonly IServiceProvider _serviceProvider;
//         private readonly ILogger<BackupScheduler> _logger;
//         private readonly BackupOptions _options;
//         private DateTime _lastFullBackup = DateTime.MinValue;
//
//         public BackupScheduler(
//             IServiceProvider serviceProvider, 
//             ILogger<BackupScheduler> logger,
//             IOptions<BackupOptions> options)
//         {
//             _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
//             _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//             _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
//             
//             if (_options.FullBackupInterval <= TimeSpan.Zero)
//                 throw new ArgumentException("Full backup interval must be greater than zero");
//                 
//             if (_options.IncrementalBackupInterval <= TimeSpan.Zero)
//                 throw new ArgumentException("Incremental backup interval must be greater than zero");
//         }
//
//         protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//         {
//             _logger.LogInformation("Backup Scheduler is starting.");
//
//             // Wait for the application to fully start
//             await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
//             
//             while (!stoppingToken.IsCancellationRequested)
//             {
//                 try
//                 {
//                     using var scope = _serviceProvider.CreateScope();
//                     var backupService = scope.ServiceProvider.GetRequiredService<IDatabaseBackupService>();
//                     
//                     var now = DateTime.UtcNow;
//                     var timeSinceLastFullBackup = now - _lastFullBackup;
//                     
//                     if (_lastFullBackup == DateTime.MinValue || timeSinceLastFullBackup >= _options.FullBackupInterval)
//                     {
//                         _logger.LogInformation("Starting full backup...");
//                         var backupPath = await backupService.CreateBackupAsync(BackupType.Full, stoppingToken);
//                         _lastFullBackup = now;
//                         _logger.LogInformation("Full backup completed successfully at {BackupPath}", backupPath);
//                     }
//                     else
//                     {
//                         _logger.LogInformation("Starting incremental backup...");
//                         var backupPath = await backupService.CreateBackupAsync(BackupType.Incremental, stoppingToken);
//                         _logger.LogInformation("Incremental backup completed successfully at {BackupPath}", backupPath);
//                     }
//                 }
//                 catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
//                 {
//                     _logger.LogInformation("Backup Scheduler is stopping.");
//                     break;
//                 }
//                 catch (Exception ex)
//                 {
//                     _logger.LogError(ex, "Error in backup scheduler");
//                     // Wait before retrying to prevent tight loop on failure
//                     await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
//                 }
//                 
//                 // Wait for the next interval
//                 await Task.Delay(_options.IncrementalBackupInterval, stoppingToken);
//             }
//         }
//     }
// }