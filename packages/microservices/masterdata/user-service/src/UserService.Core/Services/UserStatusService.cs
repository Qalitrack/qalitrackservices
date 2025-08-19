using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserService.Core.Interfaces;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Interfaces.Services;

namespace UserService.Core.Services
{
    public class UserStatusService : BackgroundService, IUserStatusService
    {
        private readonly IUserStatusRepository _userStatusRepository;  // Inject repository
        private readonly ILogger<UserStatusService> _logger;
        private readonly Channel<UserStatusUpdate> _channel;
        private readonly ChannelWriter<UserStatusUpdate> _writer;

        public UserStatusService(
            IUserStatusRepository userStatusRepository,  // Use repository via DI
            ILogger<UserStatusService> logger)
        {
            _userStatusRepository = userStatusRepository ?? throw new ArgumentNullException(nameof(userStatusRepository));
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
                // Call the repository to update the user status
                await _userStatusRepository.UpdateUserStatusAsync(update.UserId, update.IsActive);
                _logger.LogInformation("Successfully updated user {UserId} status to {Status}", 
                    update.UserId, update.IsActive ? "online" : "offline");
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
