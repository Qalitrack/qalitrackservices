using BackupService.Core.Interfaces;

namespace BackupService.Tests;

/// <summary>
/// Records every invocation and returns a scripted result per call (FIFO), or a default
/// success result if nothing was scripted — avoids shelling out to real docker/pgbackrest in tests.
/// </summary>
public class FakeProcessRunner : IProcessRunner
{
    public record Invocation(string FileName, IReadOnlyList<string> Arguments);

    public List<Invocation> Invocations { get; } = new();
    private readonly Queue<ProcessRunResult> _scriptedResults = new();
    public ProcessRunResult DefaultResult { get; set; } = new(0, string.Empty, string.Empty);

    public void Enqueue(ProcessRunResult result) => _scriptedResults.Enqueue(result);

    public Task<ProcessRunResult> RunAsync(string fileName, IReadOnlyList<string> arguments, TimeSpan timeout, CancellationToken ct = default)
    {
        Invocations.Add(new Invocation(fileName, arguments));
        var result = _scriptedResults.Count > 0 ? _scriptedResults.Dequeue() : DefaultResult;
        return Task.FromResult(result);
    }
}
