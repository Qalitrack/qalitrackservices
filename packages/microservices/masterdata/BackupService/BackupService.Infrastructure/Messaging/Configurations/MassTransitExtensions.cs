using BackupService.Core.Services;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace BackupService.Infrastructure.Messaging.Configurations
{
    public static class MassTransitExtensions
    {
        public static IServiceCollection AddMessaging(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure RabbitMQ options
            var rabbitMqOptions = configuration.GetSection("RabbitMQ").Get<RabbitMqOptions>();

            services.AddMassTransit(x =>
            {
                // Register consumers
                x.AddConsumer<BackupInitiationService>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqOptions?.Host ?? "rabbitmq", rabbitMqOptions?.VirtualHost ?? "/", h =>
                    {
                        h.Username(rabbitMqOptions?.Username ?? "admin");
                        h.Password(rabbitMqOptions?.Password ?? "securepassword");
                    });

                    // Configure consumer endpoint using the "backup.results" queue
                    cfg.ReceiveEndpoint("backup.results", e =>
                    {
                        e.ConfigureConsumer<BackupInitiationService>(context);
                    });

                    // Configure message retry policy
                    cfg.UseMessageRetry(r => r.Intervals(100, 500, 1000));
                });
            });

            // Register the command sender to maintain existing queue publishing approach
            services.AddScoped<BackupCommandSender>();

            return services;
        }
    }

    // Publisher helper
    public class BackupCommandSender
    {
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public BackupCommandSender(ISendEndpointProvider sendEndpointProvider)
        {
            _sendEndpointProvider = sendEndpointProvider;
        }

        public async Task SendInitiateBackupAsync(object message)
        {
            // Send directly to the queue, bypassing type name matching
            var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:backup.initiate.commands"));
            await endpoint.Send(message, context =>
            {
                // Always set correlation ID
                if (!context.CorrelationId.HasValue)
                    context.CorrelationId = Guid.NewGuid();
            });
        }
    }
}