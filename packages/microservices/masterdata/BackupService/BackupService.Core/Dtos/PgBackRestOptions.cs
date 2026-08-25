namespace BackupService.Core.Dtos;

public class PgBackRestOptions
{
    public string Stanza { get; init; } = "qalitrack";
    public string PostgresContainerName { get; init; } = "qalitrack-postgres-prod";
    public string PostgresImage { get; init; } = "qalitrack-postgres-prod:latest";
    public string PostgresDataVolume { get; init; } = "postgres_prod_data";
    public string PgBackRestRepoVolume { get; init; } = "pgbackrest_repo";
    public int BackupTimeoutMinutes { get; init; } = 60;
    public int RestoreTimeoutMinutes { get; init; } = 60;
    public int ContainerStopStartTimeoutSeconds { get; init; } = 120;
}
