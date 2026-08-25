namespace BackupService.Core.Interfaces;

public record ProcessRunResult(int ExitCode, string StandardOutput, string StandardError)
{
    public bool Succeeded => ExitCode == 0;
}

/// <summary>
/// Thin seam around external process execution (docker exec/run, pgbackrest, ...),
/// mirroring the IFileSystem abstraction already used for the filesystem so process
/// invocations can be faked in tests instead of shelling out for real.
/// </summary>
public interface IProcessRunner
{
    Task<ProcessRunResult> RunAsync(string fileName, IReadOnlyList<string> arguments, TimeSpan timeout, CancellationToken ct = default);
}
