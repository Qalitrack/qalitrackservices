namespace UserService.Core.DTOs.Email;

// Password is deliberately never included here — the settings form is
// write-only for it (see IsPasswordSet).
public class EmailSettingsDto
{
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public bool EnableSsl { get; set; }
    public bool IsPasswordSet { get; set; }
    public bool IsConfigured { get; set; }
}

public class UpdateEmailSettingsDto
{
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUsername { get; set; }

    // Null/blank = keep the currently saved password.
    public string? SmtpPassword { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public bool EnableSsl { get; set; } = true;
}

public class SendTestEmailDto
{
    public string ToEmail { get; set; } = string.Empty;
}
