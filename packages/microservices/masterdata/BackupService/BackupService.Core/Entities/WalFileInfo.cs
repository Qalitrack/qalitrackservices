using System.IO.Abstractions;

namespace BackupService.Core.Entities;

// Define this class somewhere in your file or project
public class WalFileInfo
{
    public string Path { get; set; } = string.Empty;
    public IFileInfo Info { get; set; } = default!;
}