using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserService.Core.DTOs.Common;
using UserService.Core.Interfaces.Emails;

namespace UserService.Infrastructure.Services;

public class EmailProcessorService(
    IServiceProvider serviceProvider,
    EmailQueueService emailQueueService,
    ILogger<EmailProcessorService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Email processor service started");

        await foreach (var emailRequest in emailQueueService.GetEmailReader().ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessEmailAsync(emailRequest);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing email for {To}", emailRequest.To);
            }
        }

        logger.LogInformation("Email processor service stopped");
    }

    private async Task ProcessEmailAsync(EmailRequest emailRequest)
    {
        using var scope = serviceProvider.CreateScope();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        try
        {
            await emailService.SendEmailAsync(emailRequest.To, emailRequest.Subject, emailRequest.Body);
        }
        catch (Exception ex)
        {
            emailRequest.RetryCount++;
            
            if (emailRequest.RetryCount < emailRequest.MaxRetries)
            {
                logger.LogWarning(ex, "Failed to send email to {To}. Retry {RetryCount}/{MaxRetries}", 
                    emailRequest.To, emailRequest.RetryCount, emailRequest.MaxRetries);
                
                // Re-queue with delay for retry
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, emailRequest.RetryCount)), CancellationToken.None);
                await emailQueueService.EnqueueEmailAsync(emailRequest);
            }
            else
            {
                logger.LogError(ex, "Failed to send email to {To} after {MaxRetries} attempts. Giving up.", 
                    emailRequest.To, emailRequest.MaxRetries);
            }
        }
    }
}