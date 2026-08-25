using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserService.Core.Interfaces;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Interfaces.Services;

namespace UserService.Core.Services
{
    // Registered as a singleton hosted service (see CoreServiceRegistration) and
    // also exposed as IUserStatusService via that same instance, so the queue
    // this class owns actually has a reader running. It must not depend
    // directly on IUserStatusRepository (scoped, DbContext-backed) — that
    // would capture one scoped instance for the app's entire lifetime.
    // Instead it resolves a fresh scope per processed update, matching this
    // codebase's other background services (ShiftInstanceBackgroundService,
    // EmailProcessorService).
    public class UserStatusService : BackgroundService, IUserStatusService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UserStatusService> _logger;
        private readonly Channel<UserStatusUpdate> _channel;
        private readonly ChannelWriter<UserStatusUpdate> _writer;

        public UserStatusService(
            IServiceProvider serviceProvider,
            ILogger<UserStatusService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Create a channel for queuing status updates
            var options = new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            };

            _channel = Channel.CreateBounded<UserStatusUpdate>(options);
            _writer = _channel.Writer;
        }

        // Enqueue status update
        public void EnqueueStatusUpdate(string userId, bool isActive)
        {
            var update = new UserStatusUpdate(userId, isActive);

            if (!_writer.TryWrite(update))
            {
                _logger.LogWarning("Failed to enqueue status update for user {UserId}", userId);
            }
        }

        // Execute async for background task processing
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var update in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await ProcessStatusUpdate(update);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing status update for user {UserId}", update.UserId);
                }
            }
        }

        // Process the status update
        private async Task ProcessStatusUpdate(UserStatusUpdate update)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var userStatusRepository = scope.ServiceProvider.GetRequiredService<IUserStatusRepository>();
                await userStatusRepository.UpdateUserStatusAsync(update.UserId, update.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user {UserId} status", update.UserId);
            }
        }

        // Dispose resources
        public override void Dispose()
        {
            _writer.Complete();
            base.Dispose();
        }
    }

    // Record for holding user status update data
    public record UserStatusUpdate(string UserId, bool IsActive);
}
