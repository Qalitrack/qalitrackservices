using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Core.Entities;

namespace UserService.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(255);
        builder.HasIndex(e => e.Email).IsUnique();
        builder.Property(e => e.Password).IsRequired();
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.IsActive).HasDefaultValue(false);
        builder.Property(e => e.IsFirstLogin).HasDefaultValue(false);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);
        
        // Configure relationships with query filters
        builder.HasMany(u => u.PersonalAccessTokens)
            .WithOne(pat => pat.User)  // Add the navigation property
            .HasForeignKey(pat => pat.UserId)
            .IsRequired(true)  // This matches the [Required] attribute
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.UserShifts)
            .WithOne(us => us.User)
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Add query filter
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
