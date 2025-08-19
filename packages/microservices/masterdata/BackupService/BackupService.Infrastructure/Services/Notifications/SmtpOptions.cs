namespace BackupService.Infrastructure.Services.Notifications;

public class SmtpOptions
{
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; } = 25;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "backups@example.com";
}