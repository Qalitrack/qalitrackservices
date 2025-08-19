namespace BackupService.Infrastructure.Messaging.Configurations;

public class RabbitMqOptions
{
    public string Host { get; set; } = "localhost";
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public ushort Port { get; set; } = 5672;
}