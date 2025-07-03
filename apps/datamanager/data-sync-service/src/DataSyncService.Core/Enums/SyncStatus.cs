namespace DataSyncService.Core.Enums;

public enum SyncStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Cancelled,
    PartialSuccess
}

public enum SyncDirection
{
    Incoming,
    Outgoing,
    Bidirectional
}

public enum ConflictResolutionStrategy
{
    LastWriteWins,
    FirstWriteWins,
    MergeChanges,
    Manual,
    UserDefined
}

public enum ChangeOperation
{
    Insert,
    Update,
    Delete,
    Upsert
}

public enum SiteStatus
{
    Active,
    Inactive,
    Maintenance,
    Error,
    Disconnected
}

public enum ConflictStatus
{
    Detected,
    InResolution,
    Resolved,
    Escalated
}

public enum SyncMode
{
    Full,
    Incremental,
    Delta
}

public enum SyncPriority
{
    Low,
    Normal,
    High,
    Critical
}