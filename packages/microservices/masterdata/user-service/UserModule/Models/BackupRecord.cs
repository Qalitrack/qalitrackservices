using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserModule.Models;

[Table("BackupRecords")]
public class BackupRecord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(20)]
    public string BackupType { get; set; } = string.Empty; // 'FULL' or 'INCREMENTAL'

    [Required]
    [StringLength(500)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string FilePath { get; set; } = string.Empty;

    public long? FileSize { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "IN_PROGRESS"; // 'COMPLETED', 'FAILED'

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public Guid? CreatedBy { get; set; }
    public Guid? ParentBackupId { get; set; }

    [StringLength(255)]
    public string? ChecksumHash { get; set; }

    // Navigation Properties
    [ForeignKey("CreatedBy")]
    public virtual User? CreatedByUser { get; set; }

    [ForeignKey("ParentBackupId")]
    public virtual BackupRecord? ParentBackup { get; set; }

    public virtual ICollection<BackupRecord> ChildBackups { get; set; } = new List<BackupRecord>();
}
