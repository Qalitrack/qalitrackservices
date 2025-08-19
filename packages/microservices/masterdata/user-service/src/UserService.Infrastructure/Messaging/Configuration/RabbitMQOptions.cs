namespace UserService.Infrastructure.Messaging.Configuration;

public class RabbitMQOptions
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    
    public Dictionary<string, string> Queues { get; set; }
}