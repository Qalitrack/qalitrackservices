namespace UserService.Core.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body);
    Task<bool> SendEmailConfirmationAsync(string email, string confirmationLink);
    Task<bool> SendPasswordResetAsync(string email, string resetLink);
    Task<bool> SendUserInvitationAsync(string email, string invitationLink, string organizationName);
    Task<bool> SendWelcomeEmailAsync(string email, string firstName, string organizationName);
    Task<bool> SendAccountLockedEmailAsync(string email, string firstName);
    Task<bool> SendPasswordChangedEmailAsync(string email, string firstName);
}