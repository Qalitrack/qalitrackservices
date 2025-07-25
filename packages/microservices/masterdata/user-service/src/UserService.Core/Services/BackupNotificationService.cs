// using System;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;
// using UserService.Core.Interfaces;
// using UserService.Core.Options;
// using UserService.Infrastructure.Interfaces;
//
// namespace UserService.Core.Services;
//
// public class BackupNotificationService : IBackupNotificationService
// {
//     private readonly ILogger<BackupNotificationService> _logger;
//     private readonly IEmailService _emailService;
//     private readonly string[] _adminEmails;
//
//     public BackupNotificationService(
//         IOptions<BackupOptions> options,
//         ILogger<BackupNotificationService> logger,
//         IEmailService emailService)
//     {
//         _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//         _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
//         
//         if (options?.Value == null)
//             throw new ArgumentNullException(nameof(options));
//             
//         _adminEmails = options.Value.AdminEmails ?? Array.Empty<string>();
//         
//         if (!_adminEmails.Any())
//         {
//             _logger.LogWarning("No admin emails configured for backup notifications");
//         }
//     }
//
//     public async Task NotifyBackupSuccessAsync(string backupPath, long sizeInBytes)
//     {
//         try
//         {
//             var subject = "Backup Completed Successfully";
//             var body = $"Backup created successfully at {DateTime.UtcNow:u}\n" +
//                       $"Location: {backupPath}\n" +
//                       $"Size: {FormatFileSize(sizeInBytes)}";
//
//             foreach (var email in _adminEmails)
//             {
//                 await _emailService.SendEmailAsync(email, subject, body);
//             }
//             
//             _logger.LogInformation("Sent backup success notifications");
//         }
//         
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error sending backup success notification");
//         }
//     }
//
//     public async Task NotifyBackupFailureAsync(string error)
//     {
//         try
//         {
//             var subject = "Backup Failed";
//             var body = $"Backup failed at {DateTime.UtcNow:u}\n" +
//                       $"Error: {error}";
//
//             foreach (var email in _adminEmails)
//             {
//                 await _emailService.SendEmailAsync(email, subject, body);
//             }
//             
//             _logger.LogError("Sent backup failure notifications");
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error sending backup failure notification");
//         }
//     }
//
//     private static string FormatFileSize(long bytes)
//     {
//         string[] sizes = { "B", "KB", "MB", "GB", "TB" };
//         var order = 0;
//         double len = bytes;
//         while (len >= 1024 && order < sizes.Length - 1)
//         {
//             order++;
//             len /= 1024;
//         }
//         return $"{len:0.##} {sizes[order]}";
//     }
// }