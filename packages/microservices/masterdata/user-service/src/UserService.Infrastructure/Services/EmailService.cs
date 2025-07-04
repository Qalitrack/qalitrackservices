using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserService.Core.Interfaces;

namespace UserService.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _fromEmail = _configuration["Email:FromEmail"] ?? "noreply@qalitrack.com";
        _fromName = _configuration["Email:FromName"] ?? "QaliTrack System";
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            // In a real implementation, you would integrate with an email service like SendGrid, SMTP, etc.
            // For now, we'll just log the email and return true
            _logger.LogInformation("Sending email to {To} with subject: {Subject}", to, subject);
            _logger.LogDebug("Email body: {Body}", body);
            
            // Simulate email sending delay
            await Task.Delay(100);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To} with subject: {Subject}", to, subject);
            return false;
        }
    }

    public async Task<bool> SendEmailConfirmationAsync(string email, string confirmationLink)
    {
        var subject = "Confirm Your Email Address - QaliTrack";
        var body = $@"
            <html>
            <body>
                <h2>Email Confirmation</h2>
                <p>Thank you for creating your QaliTrack account!</p>
                <p>Please click the link below to confirm your email address:</p>
                <p><a href=""{confirmationLink}"" style=""background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;"">Confirm Email</a></p>
                <p>If you cannot click the link, please copy and paste the following URL into your browser:</p>
                <p>{confirmationLink}</p>
                <p>This link will expire in 24 hours.</p>
                <p>If you did not create this account, please ignore this email.</p>
                <br>
                <p>Best regards,<br>The QaliTrack Team</p>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendPasswordResetAsync(string email, string resetLink)
    {
        var subject = "Password Reset Request - QaliTrack";
        var body = $@"
            <html>
            <body>
                <h2>Password Reset Request</h2>
                <p>We received a request to reset your password for your QaliTrack account.</p>
                <p>Click the link below to reset your password:</p>
                <p><a href=""{resetLink}"" style=""background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;"">Reset Password</a></p>
                <p>If you cannot click the link, please copy and paste the following URL into your browser:</p>
                <p>{resetLink}</p>
                <p>This link will expire in 1 hour for security reasons.</p>
                <p>If you did not request a password reset, please ignore this email. Your password will remain unchanged.</p>
                <br>
                <p>Best regards,<br>The QaliTrack Team</p>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendUserInvitationAsync(string email, string invitationLink, string organizationName)
    {
        var subject = $"You're Invited to Join {organizationName} on QaliTrack";
        var body = $@"
            <html>
            <body>
                <h2>You're Invited!</h2>
                <p>You have been invited to join <strong>{organizationName}</strong> on QaliTrack.</p>
                <p>QaliTrack is a comprehensive weighbridge and transportation management system that helps organizations track and manage their operations efficiently.</p>
                <p>Click the link below to accept the invitation and create your account:</p>
                <p><a href=""{invitationLink}"" style=""background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;"">Accept Invitation</a></p>
                <p>If you cannot click the link, please copy and paste the following URL into your browser:</p>
                <p>{invitationLink}</p>
                <p>This invitation will expire in 7 days.</p>
                <p>If you have any questions, please contact the administrator of {organizationName}.</p>
                <br>
                <p>Best regards,<br>The QaliTrack Team</p>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendWelcomeEmailAsync(string email, string firstName, string organizationName)
    {
        var subject = $"Welcome to QaliTrack - {organizationName}";
        var body = $@"
            <html>
            <body>
                <h2>Welcome to QaliTrack, {firstName}!</h2>
                <p>Your account has been successfully created and you are now part of <strong>{organizationName}</strong>.</p>
                <p>QaliTrack provides you with powerful tools to manage weighbridge operations, track transportation, and maintain compliance with industry standards.</p>
                <h3>Getting Started:</h3>
                <ul>
                    <li>Complete your profile information</li>
                    <li>Familiarize yourself with the dashboard</li>
                    <li>Contact your administrator if you need additional permissions</li>
                </ul>
                <p>If you have any questions or need assistance, please don't hesitate to reach out to your organization administrator or our support team.</p>
                <br>
                <p>Best regards,<br>The QaliTrack Team</p>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendAccountLockedEmailAsync(string email, string firstName)
    {
        var subject = "Account Security Alert - QaliTrack";
        var body = $@"
            <html>
            <body>
                <h2>Account Security Alert</h2>
                <p>Hello {firstName},</p>
                <p>Your QaliTrack account has been temporarily locked due to multiple failed login attempts.</p>
                <p>This is a security measure to protect your account from unauthorized access.</p>
                <p>Your account will be automatically unlocked after a period of time, or you can contact your administrator to unlock it immediately.</p>
                <p>If you believe this was caused by suspicious activity, please contact your administrator immediately.</p>
                <br>
                <p>Best regards,<br>The QaliTrack Security Team</p>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendPasswordChangedEmailAsync(string email, string firstName)
    {
        var subject = "Password Changed Successfully - QaliTrack";
        var body = $@"
            <html>
            <body>
                <h2>Password Changed</h2>
                <p>Hello {firstName},</p>
                <p>Your QaliTrack account password has been successfully changed.</p>
                <p>If you made this change, no further action is required.</p>
                <p>If you did not change your password, please contact your administrator immediately and consider changing your password again.</p>
                <p>For security reasons, we recommend:</p>
                <ul>
                    <li>Using a strong, unique password</li>
                    <li>Not sharing your password with anyone</li>
                    <li>Changing your password regularly</li>
                </ul>
                <br>
                <p>Best regards,<br>The QaliTrack Security Team</p>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body);
    }
}