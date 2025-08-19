namespace UserService.Core.Entities;

public class BackupMetadata
{
    public List<BackupChain> Chains { get; set; } = new List<BackupChain>();

}