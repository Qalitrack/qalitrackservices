using System.IO.Abstractions.TestingHelpers;
using BackupService.Core.Dtos;
using BackupService.Core.Enums;
using BackupService.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BackupService.Tests;

public class BackupMetadataServiceTests
{
    private static JsonBackupMetadataService CreateService(MockFileSystem fileSystem) =>
        new(fileSystem, NullLogger<JsonBackupMetadataService>.Instance, "/app/backup-metadata", "/app/backups");

    private static BackupResult FullResult(string label, long sizeBytes = 1000) => new()
    {
        Success = true,
        FileName = label,
        BackupType = BackupType.Full,
        Timestamp = DateTime.UtcNow,
        Lsn = "0/20000A0",
        ServiceName = "masterdata",
        FileSizeBytes = sizeBytes,
    };

    private static BackupResult IncrementalResult(string label, long sizeBytes = 200) => new()
    {
        Success = true,
        FileName = label,
        BackupType = BackupType.Incremental,
        Timestamp = DateTime.UtcNow,
        Lsn = "0/2500110",
        ServiceName = "masterdata",
        FileSizeBytes = sizeBytes,
    };

    [Fact]
    public async Task UpdateMetadataWithFullBackupAsync_CreatesPhysicalChain()
    {
        var fs = new MockFileSystem();
        var svc = CreateService(fs);

        await svc.UpdateMetadataWithFullBackupAsync(FullResult("20260825-020000F"), "masterdata");

        var chains = await svc.GetAllBackupChainsAsync("masterdata");
        chains.Should().ContainSingle();
        chains[0].IsPhysical.Should().BeTrue();
        chains[0].FullBackupFile.Should().Be("20260825-020000F");
        chains[0].LastLsn.Should().Be("0/20000A0");
    }

    [Fact]
    public async Task UpdateMetadataWithIncrementalBackupAsync_AppendsToLatestPhysicalChain()
    {
        var fs = new MockFileSystem();
        var svc = CreateService(fs);
        await svc.UpdateMetadataWithFullBackupAsync(FullResult("20260825-020000F"), "masterdata");

        await svc.UpdateMetadataWithIncrementalBackupAsync(IncrementalResult("20260825-020000F_20260826-020000I"), "masterdata");

        var chains = await svc.GetAllBackupChainsAsync("masterdata");
        chains.Should().ContainSingle();
        chains[0].Incrementals.Should().ContainSingle().Which.Should().Be("20260825-020000F_20260826-020000I");
        chains[0].LastLsn.Should().Be("0/2500110");
    }

    [Fact]
    public async Task UpdateMetadataWithIncrementalBackupAsync_NoExistingChain_Throws()
    {
        var fs = new MockFileSystem();
        var svc = CreateService(fs);

        var act = async () => await svc.UpdateMetadataWithIncrementalBackupAsync(IncrementalResult("orphan-incr"), "masterdata");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetAvailableBackupsAsync_Incremental_UsesItsOwnTimestampNotTheFulls()
    {
        // Regression: incrementals used to inherit the full backup's CreatedAt, so a
        // caller resolving "latest" by OrderByDescending(CreatedAt) could tie-break back
        // to the full backup instead of the newer incremental — confirmed against a real
        // pgBackRest restore that silently restored to the full, dropping incremental data.
        var fs = new MockFileSystem();
        var svc = CreateService(fs);
        var fullTime = new DateTime(2026, 8, 25, 6, 59, 7, DateTimeKind.Utc);
        var incrTime = fullTime.AddMinutes(1);

        await svc.UpdateMetadataWithFullBackupAsync(
            new BackupResult { FileName = "F", BackupType = BackupType.Full, Timestamp = fullTime, ServiceName = "masterdata" }, "masterdata");
        await svc.UpdateMetadataWithIncrementalBackupAsync(
            new BackupResult { FileName = "F_I", BackupType = BackupType.Incremental, Timestamp = incrTime, ServiceName = "masterdata" }, "masterdata");

        var available = await svc.GetAvailableBackupsAsync("masterdata");

        var incrementalEntry = available.Single(b => b.BackupType == BackupType.Incremental);
        incrementalEntry.CreatedAt.Should().Be(incrTime);
        available.OrderByDescending(b => b.CreatedAt).First().Should().Be(incrementalEntry);
    }

    [Fact]
    public async Task GetAvailableBackupsAsync_PhysicalChain_DoesNotRequireFileOnDisk()
    {
        // Physical (pgBackRest) backups live in the repo volume, not as files this
        // service can see — availability must not depend on File.Exists for them.
        var fs = new MockFileSystem();
        var svc = CreateService(fs);
        await svc.UpdateMetadataWithFullBackupAsync(FullResult("20260825-020000F"), "masterdata");
        await svc.UpdateMetadataWithIncrementalBackupAsync(IncrementalResult("20260825-020000F_20260826-020000I"), "masterdata");

        var available = await svc.GetAvailableBackupsAsync("masterdata");

        available.Should().HaveCount(2);
        available.Should().OnlyContain(b => b.IsPhysical);
        available.Select(b => b.FileName).Should().Contain(new[]
        {
            "pgbackrest:20260825-020000F",
            "pgbackrest:20260825-020000F_20260826-020000I",
        });

        // Regression: sizes weren't tracked on the chain at all, so every physical
        // entry silently showed "0 Bytes" in the UI regardless of the real backup size.
        available.Single(b => b.BackupType == BackupType.Full).FileSizeBytes.Should().Be(1000);
        available.Single(b => b.BackupType == BackupType.Incremental).FileSizeBytes.Should().Be(200);
    }

    [Fact]
    public async Task GetAvailableBackupsAsync_LegacyChainWithMissingFile_IsSkipped()
    {
        // Regression guard: legacy pg_dump chains must keep requiring the file to
        // actually exist on disk — only physical chains bypass that check.
        var fs = new MockFileSystem();
        fs.AddDirectory("/app/backup-metadata");
        var svc = CreateService(fs);

        // Simulate a legacy (non-physical) chain by writing metadata directly, since
        // UpdateMetadataWithFullBackupAsync now always marks new chains physical.
        var legacyMetadataJson = """
        {
          "Chains": [
            { "Id": 1, "MicroserviceName": "masterdata", "FullBackupFile": "masterdata_full_20250101.dump", "Timestamp": "2025-01-01T00:00:00Z", "Incrementals": [], "IsPhysical": false }
          ]
        }
        """;
        fs.AddFile("/app/backup-metadata/backup_metadata.json", new MockFileData(legacyMetadataJson));

        var available = await svc.GetAvailableBackupsAsync("masterdata");

        available.Should().BeEmpty(); // file doesn't exist on the mock filesystem
    }
}
