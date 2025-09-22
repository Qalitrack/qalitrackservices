using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Core.Entities;
using UserService.Core.Enums;

namespace UserService.Infrastructure.Data.Configurations;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(1000);
        
        // Configure enums with sentinel values
        builder.Property(e => e.Status)
            .HasDefaultValue(ShiftStatus.Active)
            .HasSentinel(ShiftStatus.Active);
            
        builder.Property(e => e.Type)
            .HasDefaultValue(ShiftType.Recurring)
            .HasSentinel(ShiftType.Recurring);
            
        builder.Property(e => e.RecurrenceType).HasDefaultValue(RecurrenceType.None);
        builder.Property(e => e.RecurrenceInterval).HasDefaultValue(1);
        builder.Property(e => e.RequiredStaffCount).HasDefaultValue(1);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);
        
        // Configure relationships with query filters
        builder.HasMany(s => s.ShiftInstances)
            .WithOne(si => si.Shift)
            .HasForeignKey(si => si.ShiftId)
            .IsRequired(false) // Make relationship optional
            .OnDelete(DeleteBehavior.Cascade);
            
        // Add query filter
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
