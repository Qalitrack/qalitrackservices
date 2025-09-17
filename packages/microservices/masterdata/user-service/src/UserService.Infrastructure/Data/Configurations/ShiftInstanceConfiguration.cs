using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Core.Entities;
using UserService.Core.Enums;

namespace UserService.Infrastructure.Data.Configurations;

public class ShiftInstanceConfiguration : IEntityTypeConfiguration<ShiftInstance>
{
    public void Configure(EntityTypeBuilder<ShiftInstance> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ShiftId).IsRequired();
        builder.Property(e => e.ScheduledDate).IsRequired();
        builder.Property(e => e.ScheduledStartTime).IsRequired();
        builder.Property(e => e.ScheduledEndTime).IsRequired();
        
        // Configure enum with sentinel value
        builder.Property(e => e.Status)
            .HasDefaultValue(ShiftInstanceStatus.Scheduled)
            .HasSentinel(ShiftInstanceStatus.Scheduled);
            
        builder.Property(e => e.Notes).HasMaxLength(1000);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);

        // Foreign key relationship with Shift
        builder.HasOne(si => si.Shift)
            .WithMany(s => s.ShiftInstances)
            .HasForeignKey(si => si.ShiftId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for better query performance
        builder.HasIndex(si => si.ScheduledDate);
        builder.HasIndex(si => si.ShiftId);
        builder.HasIndex(si => new { si.ShiftId, si.ScheduledDate });
    }
}
