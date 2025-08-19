using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using UserService.Core.DTOs.Common;
using UserService.Core.Interfaces;
using UserService.Core.Interfaces.Emails;

namespace UserService.Infrastructure.Services;

public class EmailQueueService : IEmailQueueService
{
    private readonly Channel<EmailRequest> _emailChannel;
    private readonly ILogger<EmailQueueService> _logger;

    public EmailQueueService(ILogger<EmailQueueService> logger)
    {
        _logger = logger;
        
        // Create bounded channel for email queue (1000 capacity for high load)
        var options = new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        };
        
        _emailChannel = Channel.CreateBounded<EmailRequest>(options);
    }

    public async Task EnqueueEmailAsync(EmailRequest emailRequest)
    {
        try
        {
            await _emailChannel.Writer.WriteAsync(emailRequest);
            _logger.LogDebug("Email queued for {To} with subject '{Subject}'", emailRequest.To, emailRequest.Subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue email for {To}", emailRequest.To);
            throw;
        }
    }

    public async Task EnqueueEmailAsync(string to, string subject, string body)
    {
        var emailRequest = new EmailRequest
        {
            To = to,
            Subject = subject,
            Body = body,
            QueuedAt = DateTime.UtcNow
        };

        await EnqueueEmailAsync(emailRequest);
    }

    public ChannelReader<EmailRequest> GetEmailReader()
    {
        return _emailChannel.Reader;
    }
}