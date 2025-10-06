using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UserService.Core.Enums;
using UserService.Core.Interfaces.Emails;
using UserService.Core.Interfaces.Services;

namespace UserService.Core.Services
{
    public class ShiftNotificationService : IShiftNotificationService
    {
        private readonly IEmailQueueService _emailQueueService;
        private readonly ILogger<ShiftNotificationService> _logger;
        private static readonly TimeZoneInfo _nairobiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("East Africa Standard Time");

        public ShiftNotificationService(
            IEmailQueueService emailQueueService,
            ILogger<ShiftNotificationService> logger)
        {
            _emailQueueService = emailQueueService ?? throw new ArgumentNullException(nameof(emailQueueService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task<bool> SendEmailAsync(string email, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Email address is null or empty");
                return false;
            }

            try
            {
                await _emailQueueService.EnqueueEmailAsync(email, subject, body);
                _logger.LogInformation("Successfully queued email to {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error queuing email to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendShiftNotificationsAsync(
            IEnumerable<(string Email, string FullName)> users,
            string shiftInstanceId,
            DateTime startTime,
            DateTime endTime,
            string shiftName,
            NotificationType type,
            string? reason = null,
            string? changes = null)
        {
            if (users == null || !users.Any())
                throw new ArgumentException("Users list cannot be null or empty", nameof(users));
            if (string.IsNullOrWhiteSpace(shiftInstanceId))
                throw new ArgumentException("Shift instance ID cannot be null or empty", nameof(shiftInstanceId));
            if (string.IsNullOrWhiteSpace(shiftName))
                throw new ArgumentException("Shift name cannot be null or empty", nameof(shiftName));

            var successCount = 0;
            var subject = GetEmailSubject(type);
            var currentTime = DateTime.UtcNow;

            foreach (var user in users)
            {
                if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.FullName))
                {
                    _logger.LogWarning("Skipping user with invalid email or name for shift {ShiftInstanceId}", shiftInstanceId);
                    continue;
                }

                var body = GetEmailBody(type, user.FullName, shiftName, startTime, endTime, reason, changes);
                var success = await SendEmailAsync(user.Email, subject, body);
                if (success)
                    successCount++;
            }

        
            return successCount > 0;
        }

        private string GetEmailBody(NotificationType type, string fullName, string shiftName, DateTime startTime, DateTime endTime, string? reason, string? changes)
        {
            var nairobiStartTime = TimeZoneInfo.ConvertTimeFromUtc(startTime, _nairobiTimeZone);
            var nairobiEndTime = TimeZoneInfo.ConvertTimeFromUtc(endTime, _nairobiTimeZone);

            var formattedDate = nairobiStartTime.ToString("dddd, MMMM d, yyyy");
            var formattedStartTime = nairobiStartTime.ToString("hh:mm tt");
            var formattedEndTime = nairobiEndTime.ToString("hh:mm tt");

            var shiftDetails = $"""
                <strong>Shift Details:</strong><br>
                Date: {formattedDate}<br>
                Time: {formattedStartTime} - {formattedEndTime}
                """;

            return type switch
            {
                NotificationType.ShiftReminder => $"""
                    <p>Dear {fullName},</p>
                    <p>This is a reminder that your shift '{shiftName}' starts in 10 minutes.</p>
                    <p>{shiftDetails}</p>
                    <p>Please be prepared to start your shift on time.</p>
                    <p>Best regards,<br>Your HR Team</p>
                    """,
                NotificationType.ShiftCancellation => $"""
                    <p>Dear {fullName},</p>
                    <p>We regret to inform you that your shift '{shiftName}' has been cancelled.</p>
                    <p>{shiftDetails}<br>
                    Reason: {reason ?? "No reason provided"}</p>
                    <p>We apologize for any inconvenience this may cause. Please check your schedule for updates.</p>
                    <p>Best regards,<br>Your HR Team</p>
                    """,
                NotificationType.ShiftModification => $"""
                    <p>Dear {fullName},</p>
                    <p>Your shift '{shiftName}' has been modified.</p>
                    <p><strong>Updated Shift Details:</strong><br>
                    {shiftDetails}<br>
                    Changes: {(changes ?? "No specific changes provided").Replace("\n", "<br>")}</p>
                    <p>Please make note of these changes to your schedule.</p>
                    <p>Best regards,<br>Your HR Team</p>
                    """,
                NotificationType.ShiftEndingAlert => $"""
                    <p>Dear {fullName},</p>
                    <p>This is a reminder that your shift '{shiftName}' will end in 10 minutes.</p>
                    <p>{shiftDetails}</p>
                    <p>Please ensure all tasks are completed and prepare to end your shift.</p>
                    <p>Best regards,<br>Your HR Team</p>
                    """,
                _ => throw new ArgumentException($"Unsupported notification type: {type}", nameof(type))
            };
        }

        private static string GetEmailSubject(NotificationType type)
        {
            return type switch
            {
                NotificationType.ShiftReminder => "Shift Reminder: Starts in 10 Minutes",
                NotificationType.ShiftCancellation => "Shift Cancellation Notice",
                NotificationType.ShiftModification => "Shift Update Notification",
                NotificationType.ShiftEndingAlert => "Shift Ending Alert: 10 Minutes Remaining",
                _ => throw new ArgumentException($"Unsupported notification type: {type}", nameof(type))
            };
        }
    }
}