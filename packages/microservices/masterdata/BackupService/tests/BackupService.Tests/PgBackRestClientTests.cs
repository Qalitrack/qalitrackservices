using BackupService.Core.Enums;
using BackupService.Core.Interfaces;
using BackupService.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BackupService.Tests;

public class PgBackRestClientTests
{
    private const string SampleInfoJson = """
    [
      {
        "name": "qalitrack",
        "status": {"code": 0, "message": "ok"},
        "backup": [
          {
            "label": "20260825-020000F",
            "type": "full",
            "timestamp": {"start": 1798250400, "stop": 1798250700},
            "info": {"size": 4200000000, "delta": 4200000000},
            "lsn": {"start": "0/2000028", "stop": "0/20000A0"}
          },
          {
            "label": "20260825-020000F_20260826-020000I",
            "type": "incr",
            "timestamp": {"start": 1798336800, "stop": 1798336900},
            "info": {"size": 4200000000, "delta": 180000000},
            "lsn": {"start": "0/20000A0", "stop": "0/2500110"}
          }
        ]
      }
    ]
    """;

    private static IConfiguration BuildConfig() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Backup:PgBackRestStanza"] = "qalitrack",
            ["Backup:PostgresContainerName"] = "qalitrack-postgres-prod",
            ["Backup:PostgresImage"] = "qalitrack-postgres-prod:latest",
            ["Backup:PostgresDataVolume"] = "postgres_prod_data",
            ["Backup:PgBackRestRepoVolume"] = "pgbackrest_repo",
        })
        .Build();

    private static PgBackRestClient CreateClient(FakeProcessRunner runner) =>
        new(runner, BuildConfig(), NullLogger<PgBackRestClient>.Instance);

    [Fact]
    public async Task BackupAsync_Full_ExecsDockerWithFullType()
    {
        var runner = new FakeProcessRunner();
        runner.Enqueue(new ProcessRunResult(0, "", "")); // backup command
        runner.Enqueue(new ProcessRunResult(0, SampleInfoJson, "")); // info command
        var client = CreateClient(runner);

        var outcome = await client.BackupAsync(BackupType.Full);

        outcome.Success.Should().BeTrue();
        outcome.Label.Should().Be("20260825-020000F_20260826-020000I"); // latest by timestamp
        runner.Invocations[0].FileName.Should().Be("docker");
        runner.Invocations[0].Arguments.Should().ContainInOrder("exec", "qalitrack-postgres-prod", "pgbackrest");
        runner.Invocations[0].Arguments.Should().Contain("--type=full");
    }

    [Fact]
    public async Task BackupAsync_Incremental_ExecsDockerWithIncrType()
    {
        var runner = new FakeProcessRunner();
        runner.Enqueue(new ProcessRunResult(0, "", ""));
        runner.Enqueue(new ProcessRunResult(0, SampleInfoJson, ""));
        var client = CreateClient(runner);

        await client.BackupAsync(BackupType.Incremental);

        runner.Invocations[0].Arguments.Should().Contain("--type=incr");
    }

    [Fact]
    public async Task BackupAsync_NonZeroExitCode_Throws()
    {
        var runner = new FakeProcessRunner();
        runner.Enqueue(new ProcessRunResult(1, "", "stanza does not exist"));
        var client = CreateClient(runner);

        var act = async () => await client.BackupAsync(BackupType.Full);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*stanza does not exist*");
    }

    [Fact]
    public async Task BackupAsync_SucceedsButInfoShowsNoBackups_Throws()
    {
        var runner = new FakeProcessRunner();
        runner.Enqueue(new ProcessRunResult(0, "", ""));
        runner.Enqueue(new ProcessRunResult(0, "[]", ""));
        var client = CreateClient(runner);

        var act = async () => await client.BackupAsync(BackupType.Full);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*none appear in*pgbackrest info*");
    }

    [Fact]
    public async Task GetInfoAsync_ParsesLabelsTypesAndSizes()
    {
        var runner = new FakeProcessRunner();
        runner.Enqueue(new ProcessRunResult(0, SampleInfoJson, ""));
        var client = CreateClient(runner);

        var info = await client.GetInfoAsync();

        info.Backups.Should().HaveCount(2);
        info.Backups[0].Label.Should().Be("20260825-020000F");
        info.Backups[0].Type.Should().Be("full");
        info.Backups[0].SizeBytes.Should().Be(4200000000);
        info.Backups[1].Type.Should().Be("incr");
        info.Backups[1].LsnStop.Should().Be("0/2500110");
    }

    [Fact]
    public async Task GetInfoAsync_EmptyArray_ReturnsNoBackups()
    {
        var runner = new FakeProcessRunner();
        runner.Enqueue(new ProcessRunResult(0, "[]", ""));
        var client = CreateClient(runner);

        var info = await client.GetInfoAsync();

        info.Backups.Should().BeEmpty();
    }

    [Fact]
    public async Task RestoreAsync_StopsRestoresThenStarts_InOrder()
    {
        var runner = new FakeProcessRunner();
        var client = CreateClient(runner);

        await client.RestoreAsync();

        runner.Invocations.Should().HaveCount(3);
        runner.Invocations[0].Arguments.Should().ContainInOrder("stop", "qalitrack-postgres-prod");
        runner.Invocations[1].Arguments.Should().Contain("run");
        runner.Invocations[1].Arguments.Should().Contain("restore");
        runner.Invocations[2].Arguments.Should().ContainInOrder("start", "qalitrack-postgres-prod");
    }

    [Fact]
    public async Task RestoreAsync_RestoreFails_StillStartsPostgresBackUp()
    {
        var runner = new FakeProcessRunner();
        runner.Enqueue(new ProcessRunResult(0, "", "")); // stop succeeds
        runner.Enqueue(new ProcessRunResult(1, "", "restore failed: missing WAL segment")); // restore fails
        var client = CreateClient(runner);

        var act = async () => await client.RestoreAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*restore failed*");
        runner.Invocations.Should().HaveCount(3); // stop, run (failed), start — start still attempted
        runner.Invocations[2].Arguments.Should().ContainInOrder("start", "qalitrack-postgres-prod");
    }
}
