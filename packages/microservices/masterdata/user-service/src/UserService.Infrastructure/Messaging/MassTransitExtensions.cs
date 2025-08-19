using MassTransit;
   using Messaging.Contracts.Messaging.contracts;
   using Messaging.Contracts.Messaging.contracts.Events.BackupEvents;
   using Microsoft.Extensions.Configuration;
   using Microsoft.Extensions.DependencyInjection;
   using Serilog;
   using UserService.Infrastructure.Backup;
   using UserService.Infrastructure.Messaging.Events;
   
   namespace UserService.Infrastructure.Messaging;
   
   public static class MassTransitExtensions
   {
       public static void ConfigureMassTransit(
           this IServiceCollection services,
           IConfiguration configuration,
           ILogger logger)
       {
           try
           {
               logger.Information("Starting MassTransit configuration");
   
               // Get and validate configuration values
               var host = GetRequiredConfigValue(configuration, "RabbitMQ:Host", "RABBITMQ_HOST", logger);
               var port = configuration.GetValue<int>("RabbitMQ:Port", 5672);
               var username = GetRequiredConfigValue(configuration, "RabbitMQ:Username", "RABBITMQ_USERNAME", logger);
               var password = GetRequiredConfigValue(configuration, "RabbitMQ:Password", "RABBITMQ_PASSWORD", logger);
               var virtualHost = configuration.GetValue<string>("RabbitMQ:VirtualHost", "/");
   
               // Get queue configurations
               var backupCommandsQueue = configuration["RabbitMQ:Queues:BackupCommands"] ?? "backup.commands";
               var backupResultsQueue = configuration["RabbitMQ:Queues:BackupResults"] ?? "backup.results";
   
               logger.Information("RabbitMQ Configuration: Host={Host}, Port={Port}, VirtualHost={VirtualHost}", 
                   host, port, virtualHost);
               logger.Information("Queue Configuration: BackupCommandsQueue={BackupCommandsQueue}, BackupResultsQueue={BackupResultsQueue}",
                   backupCommandsQueue, backupResultsQueue);
   
               services.AddMassTransit(x =>
               {
                   // Register BackupEventConsumer
                   x.AddConsumer<BackupEventConsumer>();
   
                   x.UsingRabbitMq((context, cfg) =>
                   {
                       try
                       {
                           logger.Debug("Configuring RabbitMQ connection");
                           cfg.UseNewtonsoftJsonSerializer();
   
                           // Configure RabbitMQ host connection
                           cfg.Host(host, (ushort)port, virtualHost, h =>
                           {
                               h.Username(username);
                               h.Password(password);
                           });
   
                           // Configure backup commands queue endpoint
                           cfg.ReceiveEndpoint(backupCommandsQueue, e =>
                           {
                               // Configure for CrossServiceBackupCommand consumer
                               e.Consumer<BackupEventConsumer>(context, c =>
                               {
                                   // Use partitioning to ensure scalability
                                   c.Message<CrossServiceBackupCommand>(m => m.UsePartitioner(16, p => p.Message.CommandId));
                               });
   
                               // Apply standard configuration
                               e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                               e.PrefetchCount = 16;
                               e.ConcurrentMessageLimit = 8;
   
                               logger.Information("Successfully configured endpoint for queue {QueueName} with BackupEventConsumer", backupCommandsQueue);
                           });
   
                           // Configure the publish endpoint for backup results
                           cfg.Publish<BackupOrchestrationResult>(p => 
                           {
                               p.ExchangeType = "fanout";
                               p.BindQueue(backupResultsQueue, backupResultsQueue);
                           });
   
                           logger.Information("MassTransit RabbitMQ configuration completed successfully");
                       }
                       catch (Exception ex)
                       {
                           logger.Error(ex, "Failed to configure RabbitMQ connection");
                           throw;
                       }
                   });
               });
   
               
               // Add the MassTransit hosted service
               logger.Information("MassTransit configuration completed successfully");
           }
           catch (Exception ex)
           {
               logger.Error(ex, "MassTransit configuration failed");
               throw;
           }
       }
   
       private static string GetRequiredConfigValue(
           IConfiguration configuration, 
           string configPath, 
           string envVarName,
           ILogger logger)
       {
           var value = configuration[configPath];
           
           if (string.IsNullOrWhiteSpace(value))
           {
               logger.Error("Configuration value for {ConfigPath} is missing or empty. Check environment variable {EnvVarName}", 
                   configPath, envVarName);
               throw new InvalidOperationException($"Required configuration value '{configPath}' is missing");
           }
   
           logger.Debug("Configuration value {ConfigPath}={Value}", configPath, value);
           return value;
       }
   }