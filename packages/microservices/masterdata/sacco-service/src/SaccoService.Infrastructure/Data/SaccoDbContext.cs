using Microsoft.EntityFrameworkCore;
using SaccoService.Core.Entities;

namespace SaccoService.Infrastructure.Data;

public class SaccoDbContext : DbContext
{
    public SaccoDbContext(DbContextOptions<SaccoDbContext> options) : base(options)
    {
    }

    public DbSet<Sacco> Saccos { get; set; }
    public DbSet<SaccoMember> SaccoMembers { get; set; }
    public DbSet<SaccoMembership> SaccoMemberships { get; set; }
    public DbSet<SaccoCommittee> SaccoCommittees { get; set; }
    public DbSet<SaccoMeeting> SaccoMeetings { get; set; }
    public DbSet<SaccoFinancial> SaccoFinancials { get; set; }
    public DbSet<SaccoShare> SaccoShares { get; set; }
    public DbSet<SaccoLoan> SaccoLoans { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Sacco entity
        modelBuilder.Entity<Sacco>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.RegistrationNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ContactEmail).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ShareCapitalMinimum).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ShareValue).HasColumnType("decimal(18,2)");
            
            entity.HasIndex(e => e.RegistrationNumber).IsUnique();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure SaccoMember entity
        modelBuilder.Entity<SaccoMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.MemberNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.IdNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ContactEmail).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SharesOwned).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CurrentSavings).HasColumnType("decimal(18,2)");
            
            entity.HasIndex(e => e.MemberNumber).IsUnique();
            entity.HasIndex(e => e.IdNumber).IsUnique();
            
            entity.HasOne(e => e.Sacco)
                  .WithMany(s => s.Members)
                  .HasForeignKey(e => e.SaccoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure SaccoMembership entity
        modelBuilder.Entity<SaccoMembership>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RegistrationFee).HasColumnType("decimal(18,2)");
            entity.Property(e => e.AnnualFee).HasColumnType("decimal(18,2)");
            entity.Property(e => e.OutstandingFees).HasColumnType("decimal(18,2)");
            
            entity.HasOne(e => e.Sacco)
                  .WithMany()
                  .HasForeignKey(e => e.SaccoId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Member)
                  .WithMany(m => m.Memberships)
                  .HasForeignKey(e => e.SaccoMemberId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure SaccoCommittee entity
        modelBuilder.Entity<SaccoCommittee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Allowance).HasColumnType("decimal(18,2)");
            
            entity.HasOne(e => e.Sacco)
                  .WithMany(s => s.Committees)
                  .HasForeignKey(e => e.SaccoId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Member)
                  .WithMany()
                  .HasForeignKey(e => e.MemberId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure SaccoMeeting entity
        modelBuilder.Entity<SaccoMeeting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Location).IsRequired().HasMaxLength(300);
            
            entity.HasOne(e => e.Sacco)
                  .WithMany(s => s.Meetings)
                  .HasForeignKey(e => e.SaccoId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Chairperson)
                  .WithMany()
                  .HasForeignKey(e => e.ChairpersonId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasOne(e => e.Secretary)
                  .WithMany()
                  .HasForeignKey(e => e.SecretaryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure SaccoFinancial entity
        modelBuilder.Entity<SaccoFinancial>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalAssets).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalLiabilities).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ShareCapital).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ReservesFunds).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalSavings).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalLoansOutstanding).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CashAtBank).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CashAtHand).HasColumnType("decimal(18,2)");
            entity.Property(e => e.AnnualIncome).HasColumnType("decimal(18,2)");
            entity.Property(e => e.AnnualExpenses).HasColumnType("decimal(18,2)");
            entity.Property(e => e.NetSurplus).HasColumnType("decimal(18,2)");
            entity.Property(e => e.InterestRateOnLoans).HasColumnType("decimal(5,2)");
            entity.Property(e => e.InterestRateOnSavings).HasColumnType("decimal(5,2)");
            
            entity.HasOne(e => e.Sacco)
                  .WithOne(s => s.Financial)
                  .HasForeignKey<SaccoFinancial>(e => e.SaccoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure SaccoShare entity
        modelBuilder.Entity<SaccoShare>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ShareCertificateNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ShareValue).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalValue).HasColumnType("decimal(18,2)");
            
            entity.HasIndex(e => e.ShareCertificateNumber).IsUnique();
            
            entity.HasOne(e => e.Sacco)
                  .WithMany(s => s.Shares)
                  .HasForeignKey(e => e.SaccoId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Member)
                  .WithMany(m => m.Shares)
                  .HasForeignKey(e => e.MemberId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure SaccoLoan entity
        modelBuilder.Entity<SaccoLoan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LoanNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PrincipalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.InterestRate).HasColumnType("decimal(5,2)");
            entity.Property(e => e.MonthlyInstallment).HasColumnType("decimal(18,2)");
            entity.Property(e => e.OutstandingBalance).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CollateralValue).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Purpose).IsRequired().HasMaxLength(500);
            
            entity.HasIndex(e => e.LoanNumber).IsUnique();
            
            entity.HasOne(e => e.Sacco)
                  .WithMany(s => s.Loans)
                  .HasForeignKey(e => e.SaccoId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Member)
                  .WithMany(m => m.Loans)
                  .HasForeignKey(e => e.MemberId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.ApprovedBy)
                  .WithMany()
                  .HasForeignKey(e => e.ApprovedById)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}