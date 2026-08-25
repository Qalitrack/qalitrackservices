using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Core.Entities;

namespace UserService.Infrastructure.Data.Configurations;

public class EmailSettingsConfiguration : IEntityTypeConfiguration<EmailSettings>
{
    public void Configure(EntityTypeBuilder<EmailSettings> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.SmtpHost).HasMaxLength(255);
        builder.Property(e => e.SmtpUsername).HasMaxLength(255);
        builder.Property(e => e.SmtpPassword).HasMaxLength(500);
        builder.Property(e => e.FromEmail).HasMaxLength(255);
        builder.Property(e => e.FromName).HasMaxLength(100);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);
    }
}
