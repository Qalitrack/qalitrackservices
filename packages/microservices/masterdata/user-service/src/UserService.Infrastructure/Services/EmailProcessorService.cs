using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserService.Core.DTOs.Common;
using UserService.Infrastructure.Interfaces;

namespace UserService.Infrastructure.Services;

public class EmailProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly EmailQueueService _emailQueueService;
    private readonly ILogger<EmailProcessorService> _logger;

    public EmailProcessorService(
        IServiceProvider serviceProvider,
        EmailQueueService emailQueueService,
        ILogger<EmailProcessorService> logger)
    {
        _serviceProvider = serviceProvider;
        _emailQueueService = emailQueueService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email processor service started");

        await foreach (var emailRequest in _emailQueueService.GetEmailReader().ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessEmailAsync(emailRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing email for {To}", emailRequest.To);
            }
        }

        _logger.LogInformation("Email processor service stopped");
    }

    private async Task ProcessEmailAsync(EmailRequest emailRequest)
    {
        using var scope = _serviceProvider.CreateScope();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        try
        {
            await emailService.SendEmailAsync(emailRequest.To, emailRequest.Subject, emailRequest.Body);
            _logger.LogInformation("Email sent successfully to {To} with subject '{Subject}'", 
                emailRequest.To, emailRequest.Subject);
        }
        catch (Exception ex)
        {
            emailRequest.RetryCount++;
            
            if (emailRequest.RetryCount < emailRequest.MaxRetries)
            {
                _logger.LogWarning(ex, "Failed to send email to {To}. Retry {RetryCount}/{MaxRetries}", 
                    emailRequest.To, emailRequest.RetryCount, emailRequest.MaxRetries);
                
                // Re-queue with delay for retry
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, emailRequest.RetryCount)), CancellationToken.None);
                await _emailQueueService.EnqueueEmailAsync(emailRequest);
            }
            else
            {
                _logger.LogError(ex, "Failed to send email to {To} after {MaxRetries} attempts. Giving up.", 
                    emailRequest.To, emailRequest.MaxRetries);
            }
        }
    }
}