// using System.Net;
// using System.Net.Mail;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;
// using BackupService.Core.Dtos;
// using BackupService.Core.Interfaces.Services;
// using BackupService.Core.Options;
// using BackupService.Infrastructure.Services.Notifications;
//
// namespace BackupService.Infrastructure.Services;
//
// public class EmailNotificationService : IEmailNotificationService
// {
//     private readonly ILogger<EmailNotificationService> _logger;
//     private readonly string[] _adminEmails;
//     private readonly SmtpClient _smtpClient;
//     private readonly string _fromAddress;
//     
//     public EmailNotificationService(
//         IOptions<BackupInitiatorOptions> options,
//         IOptions<SmtpOptions> smtpOptions,
//         ILogger<EmailNotificationService> logger)
//     {
//         if (options?.Value == null)
//             throw new ArgumentNullException(nameof(options));
//         
//         if (smtpOptions?.Value == null)
//             throw new ArgumentNullException(nameof(smtpOptions));
//             
//         _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//         _adminEmails = options.Value.AdminEmails ?? Array.Empty<string>();
//         
//         if (!_adminEmails.Any())
//         {
//             _logger.LogWarning("No admin emails configured for notifications");
//         }
//         
//         var smtp = smtpOptions.Value;
//         _fromAddress = smtp.FromAddress;
//         
//         _smtpClient = new SmtpClient
//         {
//             Host = smtp.Server,
//             Port = smtp.Port,
//             EnableSsl = smtp.EnableSsl,
//             DeliveryMethod = SmtpDeliveryMethod.Network,
//             UseDefaultCredentials = false,
//             Credentials = new NetworkCredential(smtp.Username, smtp.Password)
//         };
//     }
//     
//     public async Task SendBackupSuccessNotificationAsync(BackupStatusDto status)
//     {
//         if (!_adminEmails.Any()) return;
//         
//         try
//         {
//             var subject = $"Backup Success: {status.ServiceId}";
//             var body = new System.Text.StringBuilder();
//             body.AppendLine($"<h2>Backup Completed Successfully</h2>");
//             body.AppendLine("<p>Details:</p>");
//             body.AppendLine("<ul>");
//             body.AppendLine($"<li><strong>Service:</strong> {status.ServiceId}</li>");
//             body.AppendLine($"<li><strong>Backup Type:</strong> {status.BackupType}</li>");
//             body.AppendLine($"<li><strong>Backup ID:</strong> {status.BackupId}</li>");
//             body.AppendLine($"<li><strong>Path:</strong> {status.BackupPath}</li>");
//             body.AppendLine($"<li><strong>Size:</strong> {FormatFileSize(status.SizeInBytes ?? 0)}</li>");
//             body.AppendLine($"<li><strong>Start Time:</strong> {status.RequestTime.ToString("yyyy-MM-dd HH:mm:ss")}</li>");
//             body.AppendLine($"<li><strong>Completion Time:</strong> {status.CompletionTime?.ToString("yyyy-MM-dd HH:mm:ss")}</li>");
//             body.AppendLine($"<li><strong>Duration:</strong> {FormatDuration(status.RequestTime, status.CompletionTime ?? DateTime.UtcNow)}</li>");
//             body.AppendLine("</ul>");
//             
//             await SendEmailAsync(_adminEmails, subject, body.ToString(), true);
//             _logger.LogInformation("Sent backup success notification for service {ServiceId}", status.ServiceId);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Failed to send backup success notification for service {ServiceId}", status.ServiceId);
//         }
//     }
//     
//     public async Task SendBackupFailureNotificationAsync(BackupStatusDto status)
//     {
//         if (!_adminEmails.Any()) return;
//         
//         try
//         {
//             var subject = $"Backup Failure: {status.ServiceId}";
//             var body = new System.Text.StringBuilder();
//             body.AppendLine($"<h2 style='color: red;'>Backup Failed</h2>");
//             body.AppendLine("<p>Details:</p>");
//             body.AppendLine("<ul>");
//             body.AppendLine($"<li><strong>Service:</strong> {status.ServiceId}</li>");
//             body.AppendLine($"<li><strong>Backup Type:</strong> {status.BackupType}</li>");
//             body.AppendLine($"<li><strong>Start Time:</strong> {status.RequestTime.ToString("yyyy-MM-dd HH:mm:ss")}</li>");
//             body.AppendLine($"<li><strong>Failure Time:</strong> {status.CompletionTime?.ToString("yyyy-MM-dd HH:mm:ss")}</li>");
//             body.AppendLine($"<li><strong>Error Message:</strong> {status.ErrorMessage}</li>");
//             body.AppendLine("</ul>");
//             body.AppendLine("<p>Please investigate and resolve this issue promptly.</p>");
//             
//             await SendEmailAsync(_adminEmails, subject, body.ToString(), true);
//             _logger.LogInformation("Sent backup failure notification for service {ServiceId}", status.ServiceId);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Failed to send backup failure notification for service {ServiceId}", status.ServiceId);
//         }
//     }
//     
//     private async Task SendEmailAsync(string[] recipients, string subject, string body, bool isHtml = false)
//     {
//         var message = new MailMessage
//         {
//             From = new MailAddress(_fromAddress),
//             Subject = subject,
//             Body = body,
//             IsBodyHtml = isHtml
//         };
//         
//         foreach (var recipient in recipients)
//         {
//             message.To.Add(recipient);
//         }
//         
//         await _smtpClient.SendMailAsync(message);
//     }
//     
//     private static string FormatFileSize(long bytes)
//     {
//         string[] sizes = { "B", "KB", "MB", "GB", "TB" };
//         double len = bytes;
//         int order = 0;
//         
//         while (len >= 1024 && order < sizes.Length - 1)
//         {
//             order++;
//             len /= 1024;
//         }
//         
//         return $"{len:0.##} {sizes[order]}";
//     }
//     
//     private static string FormatDuration(DateTime start, DateTime end)
//     {
//         var duration = end - start;
//         
//         if (duration.TotalHours >= 1)
//         {
//             return $"{duration.TotalHours:0.#} hours";
//         }
//         
//         if (duration.TotalMinutes >= 1)
//         {
//             return $"{duration.TotalMinutes:0.#} minutes";
//         }
//         
//         return $"{duration.TotalSeconds:0.#} seconds";
//     }
// }