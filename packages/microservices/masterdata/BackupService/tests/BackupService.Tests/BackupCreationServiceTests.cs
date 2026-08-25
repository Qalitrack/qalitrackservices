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

public class BackupCreationServiceTests
{
    private readonly Mock<IMicroserviceRepository> _repo = new();
    private readonly Mock<IBackupMetadataService> _metadata = new();
    private readonly Mock<IPgBackRestClient> _pgBackRest = new();
    private readonly BackupCreationService _sut;

    public BackupCreationServiceTests()
    {
        _sut = new BackupCreationService(_repo.Object, _metadata.Object, _pgBackRest.Object, NullLogger<BackupCreationService>.Instance);
    }

    private static Microservice ActiveMicroservice(string name = "masterdata") => new()
    {
        Id = 1,
        Name = name,
        ConnectionString = "unused-for-physical-backups",
        Status = MicroserviceStatus.Active,
    };

    [Fact]
    public async Task CreateBackupAsync_UnknownMicroservice_ThrowsKeyNotFound()
    {
        _repo.Setup(r => r.GetMicroserviceAsync("ghost", It.IsAny<CancellationToken>())).ReturnsAsync((Microservice?)null);

        var act = async () => await _sut.CreateBackupAsync(BackupType.Full, "ghost");

        await act.Should().ThrowAsync<KeyNotFoundException>();
        _pgBackRest.Verify(p => p.BackupAsync(It.IsAny<BackupType>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateBackupAsync_PausedMicroservice_ThrowsInvalidOperation()
    {
        var ms = ActiveMicroservice();
        ms.Status = MicroserviceStatus.Paused;
        _repo.Setup(r => r.GetMicroserviceAsync("masterdata", It.IsAny<CancellationToken>())).ReturnsAsync(ms);

        var act = async () => await _sut.CreateBackupAsync(BackupType.Full, "masterdata");

        await act.Should().ThrowAsync<InvalidOperationException>();
        _pgBackRest.Verify(p => p.BackupAsync(It.IsAny<BackupType>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateBackupAsync_Full_RecordsFullChainAndActivatesMicroservice()
    {
        var ms = ActiveMicroservice();
        _repo.Setup(r => r.GetMicroserviceAsync("masterdata", It.IsAny<CancellationToken>())).ReturnsAsync(ms);
        _pgBackRest.Setup(p => p.BackupAsync(BackupType.Full, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PgBackRestBackupOutcome(true, "20260825-020000F", BackupType.Full, DateTime.UtcNow, 123456, "0/20000A0", ""));

        var result = await _sut.CreateBackupAsync(BackupType.Full, "masterdata");

        result.Success.Should().BeTrue();
        result.FileName.Should().Be("20260825-020000F");
        result.FileSizeBytes.Should().Be(123456);

        _metadata.Verify(m => m.UpdateMetadataWithFullBackupAsync(
            It.Is<BackupResult>(r => r.FileName == "20260825-020000F"), "masterdata", It.IsAny<CancellationToken>()), Times.Once);
        _metadata.Verify(m => m.UpdateMetadataWithIncrementalBackupAsync(It.IsAny<BackupResult>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        _repo.Verify(r => r.UpdateMicroserviceAsync("masterdata",
            It.Is<MicroserviceRequest>(req => req.Status == MicroserviceStatus.Active), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBackupAsync_Incremental_RecordsIncrementalOnExistingChain()
    {
        var ms = ActiveMicroservice();
        _repo.Setup(r => r.GetMicroserviceAsync("masterdata", It.IsAny<CancellationToken>())).ReturnsAsync(ms);
        _pgBackRest.Setup(p => p.BackupAsync(BackupType.Incremental, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PgBackRestBackupOutcome(true, "20260825-020000F_20260826-020000I", BackupType.Incremental, DateTime.UtcNow, 5000, "0/2500110", ""));

        var result = await _sut.CreateBackupAsync(BackupType.Incremental, "masterdata");

        result.BackupType.Should().Be(BackupType.Incremental);
        _metadata.Verify(m => m.UpdateMetadataWithIncrementalBackupAsync(
            It.Is<BackupResult>(r => r.FileName == "20260825-020000F_20260826-020000I"), "masterdata", It.IsAny<CancellationToken>()), Times.Once);
        _metadata.Verify(m => m.UpdateMetadataWithFullBackupAsync(It.IsAny<BackupResult>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateBackupAsync_PgBackRestFails_PausesMicroserviceAndWraps()
    {
        var ms = ActiveMicroservice();
        _repo.Setup(r => r.GetMicroserviceAsync("masterdata", It.IsAny<CancellationToken>())).ReturnsAsync(ms);
        _pgBackRest.Setup(p => p.BackupAsync(It.IsAny<BackupType>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("docker exec failed: no such container"));

        var act = async () => await _sut.CreateBackupAsync(BackupType.Full, "masterdata");

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*docker exec failed*");

        _repo.Verify(r => r.UpdateMicroserviceAsync("masterdata",
            It.Is<MicroserviceRequest>(req => req.Status == MicroserviceStatus.Paused), It.IsAny<CancellationToken>()), Times.Once);
    }
}
