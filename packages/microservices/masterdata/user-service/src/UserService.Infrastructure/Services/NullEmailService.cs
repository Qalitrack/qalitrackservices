using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UserService.Core.Interfaces;
using UserService.Core.Interfaces.Emails;

namespace UserService.Infrastructure.Services;

public class NullEmailService : IEmailService
{
    private readonly ILogger<NullEmailService> _logger;

    public NullEmailService(ILogger<NullEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogWarning("No email service configured. Email would have been sent to {To} with subject '{Subject}'", 
            to, subject);
        return Task.CompletedTask;
    }
}
