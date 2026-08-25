using BackupService.Core.Dtos;
using BackupService.Core.Entities;
using BackupService.Core.Enums;
using BackupService.Core.Interfaces;
using BackupService.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BackupService.Tests;

public class BackupRestoreServiceTests
{
    private readonly Mock<IMicroserviceRepository> _repo = new();
    private readonly Mock<IPgBackRestClient> _pgBackRest = new();
    private readonly BackupRestoreService _sut;

    public BackupRestoreServiceTests()
    {
        _sut = new BackupRestoreService(_repo.Object, _pgBackRest.Object, NullLogger<BackupRestoreService>.Instance);
    }

    private static Microservice ActiveMicroservice(string name = "masterdata") => new()
    {
        Id = 1,
        Name = name,
        ConnectionString = "unused",
        Status = MicroserviceStatus.Active,
    };

    [Theory]
    [InlineData("masterdata_full_20260101_020000.dump")]
    [InlineData("")]
    public async Task RestoreBackupAsync_NonPgBackRestIdentifier_Throws(string backupFilePath)
    {
        _repo.Setup(r => r.GetMicroserviceAsync("masterdata", It.IsAny<CancellationToken>())).ReturnsAsync(ActiveMicroservice());

        var act = async () => await _sut.RestoreBackupAsync("masterdata", backupFilePath);

        await act.Should().ThrowAsync<Exception>(); // ArgumentException for empty, NotSupportedException otherwise
        _pgBackRest.Verify(p => p.RestoreAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RestoreBackupAsync_PgBackRestIdentifier_CallsPgBackRestRestore()
    {
        _repo.Setup(r => r.GetMicroserviceAsync("masterdata", It.IsAny<CancellationToken>())).ReturnsAsync(ActiveMicroservice());

        var result = await _sut.RestoreBackupAsync("masterdata", "pgbackrest:20260825-020000F");

        result.IsSuccessful.Should().BeTrue();
        result.FullBackupUsed.Should().Be("pgbackrest:20260825-020000F");
        // Must pass the specific label through, not just "restore something" — otherwise
        // restoring an older/non-latest chain entry would silently restore latest instead.
        _pgBackRest.Verify(p => p.RestoreAsync("20260825-020000F", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RestoreBackupAsync_IncrementalIdentifier_PassesIncrementalLabelThrough()
    {
        _repo.Setup(r => r.GetMicroserviceAsync("masterdata", It.IsAny<CancellationToken>())).ReturnsAsync(ActiveMicroservice());

        await _sut.RestoreBackupAsync("masterdata", "pgbackrest:20260825-020000F_20260826-020000I");

        _pgBackRest.Verify(p => p.RestoreAsync("20260825-020000F_20260826-020000I", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RestoreBackupAsync_InactiveMicroservice_Throws()
    {
        var ms = ActiveMicroservice();
        ms.Status = MicroserviceStatus.Inactive;
        _repo.Setup(r => r.GetMicroserviceAsync("masterdata", It.IsAny<CancellationToken>())).ReturnsAsync(ms);

        var act = async () => await _sut.RestoreBackupAsync("masterdata", "pgbackrest:20260825-020000F");

        await act.Should().ThrowAsync<InvalidOperationException>();
        _pgBackRest.Verify(p => p.RestoreAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RestoreBackupAsync_PgBackRestThrows_PausesMicroserviceAndWraps()
    {
        _repo.Setup(r => r.GetMicroserviceAsync("masterdata", It.IsAny<CancellationToken>())).ReturnsAsync(ActiveMicroservice());
        _pgBackRest.Setup(p => p.RestoreAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("pgBackRest restore failed: missing WAL segment"));

        var act = async () => await _sut.RestoreBackupAsync("masterdata", "pgbackrest:20260825-020000F");

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*missing WAL segment*");

        _repo.Verify(r => r.UpdateMicroserviceAsync("masterdata",
            It.Is<MicroserviceRequest>(req => req.Status == MicroserviceStatus.Paused), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void IsPgBackRestIdentifier_RecognizesPrefixOnly()
    {
        BackupRestoreService.IsPgBackRestIdentifier("pgbackrest:20260825-020000F").Should().BeTrue();
        BackupRestoreService.IsPgBackRestIdentifier("/app/backups/masterdata_full.dump").Should().BeFalse();
        BackupRestoreService.IsPgBackRestIdentifier("").Should().BeFalse();
    }

    [Fact]
    public async Task PreviewRestoreAsync_ReturnsChainFromPgBackRestInfo()
    {
        _repo.Setup(r => r.GetMicroserviceAsync("masterdata", It.IsAny<CancellationToken>())).ReturnsAsync(ActiveMicroservice());
        _pgBackRest.Setup(p => p.GetInfoAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new PgBackRestInfo
        {
            Stanza = "qalitrack",
            Backups = new List<PgBackRestBackupEntry>
            {
                new() { Label = "20260825-020000F", Type = "full", TimestampStop = 1798250700, SizeBytes = 1000 },
                new() { Label = "20260825-020000F_20260826-020000I", Type = "incr", TimestampStop = 1798336900, SizeBytes = 200 },
            }
        });

        var preview = await _sut.PreviewRestoreAsync("masterdata", "pgbackrest:latest");

        preview.FullBackupFile.Should().Be("20260825-020000F");
        preview.IncrementalFiles.Should().ContainSingle().Which.Should().Be("20260825-020000F_20260826-020000I");
        preview.TotalFilesToProcess.Should().Be(2);
        preview.RequiredSpaceBytes.Should().Be(1200);
    }
}
