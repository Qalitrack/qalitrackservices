using UserService.Core.DTOs.Common;

namespace UserService.Core.Interfaces;

public interface IEmailQueueService
{
    Task EnqueueEmailAsync(EmailRequest emailRequest);
    Task EnqueueEmailAsync(string to, string subject, string body);
}