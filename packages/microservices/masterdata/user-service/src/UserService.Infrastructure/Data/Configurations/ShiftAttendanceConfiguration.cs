using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Core.Entities;
using UserService.Core.Enums;

namespace UserService.Infrastructure.Data.Configurations;

public class ShiftAttendanceConfiguration : IEntityTypeConfiguration<ShiftAttendance>
{
    public void Configure(EntityTypeBuilder<ShiftAttendance> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ShiftInstanceId).IsRequired();
        builder.Property(e => e.EmployeeId).IsRequired();
        
        // Configure enum with sentinel value
        builder.Property(e => e.Status)
            .HasDefaultValue(AttendanceStatus.Scheduled)
            .HasSentinel(AttendanceStatus.Scheduled);
            
        builder.Property(e => e.Notes).HasMaxLength(1000);
        builder.Property(e => e.IsLate).HasDefaultValue(false);
        builder.Property(e => e.IsEarlyDeparture).HasDefaultValue(false);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);

        // Foreign key relationships
        builder.HasOne(sa => sa.ShiftInstance)
            .WithMany(si => si.Attendances)
            .HasForeignKey(sa => sa.ShiftInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sa => sa.Employee)
            .WithMany()
            .HasForeignKey(sa => sa.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint: one attendance record per employee per shift instance
        builder.HasIndex(sa => new { sa.ShiftInstanceId, sa.EmployeeId })
            .IsUnique();

        // Indexes for better query performance
        builder.HasIndex(sa => sa.ShiftInstanceId);
        builder.HasIndex(sa => sa.EmployeeId);
        builder.HasIndex(sa => sa.Status);
        builder.HasIndex(sa => sa.ClockInTime);
        builder.HasIndex(sa => sa.ClockOutTime);
    }
}
