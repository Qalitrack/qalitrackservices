using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Core.Entities;

namespace UserService.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EntityType).HasMaxLength(100);
        builder.Property(e => e.EntityId).HasMaxLength(100);
        builder.Property(e => e.Action).IsRequired().HasMaxLength(20);
        builder.Property(e => e.PreviousHash).IsRequired().HasMaxLength(64);
        builder.Property(e => e.Hash).IsRequired().HasMaxLength(64);

        // DB-assigned identity — AuditLog.Id (BaseEntity) is a client-generated
        // GUID and isn't ordered, so this is the real hash-chain order.
        builder.Property(e => e.SequenceNumber).UseIdentityAlwaysColumn();
        builder.HasIndex(e => e.SequenceNumber).IsUnique();
        builder.HasIndex(e => e.EntityType);
    }
}
