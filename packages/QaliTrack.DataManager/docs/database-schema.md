# Database Schema

The QaliTrack Data Manager Service uses a comprehensive database schema designed for multi-tenancy, scalability, and data integrity across seven operational modules.

## Schema Overview

The database contains 32+ tables organized by functional modules, with shared infrastructure tables supporting cross-module functionality.

## Base Entity Structure

All entities inherit from base classes that provide common functionality:

### BaseEntity
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }                    // Primary key
    public DateTime CreatedAt { get; set; }         // Creation timestamp
    public DateTime UpdatedAt { get; set; }         // Last modification timestamp
    public string CreatedBy { get; set; }           // Creator user ID
    public string UpdatedBy { get; set; }           // Last modifier user ID
    public bool IsDeleted { get; set; }             // Soft delete flag
}
```

### TenantBaseEntity
```csharp
public abstract class TenantBaseEntity : BaseEntity
{
    public Guid OrganizationId { get; set; }        // Tenant isolation
}
```

## Module Schemas

### 1. WeightData Module

#### WeightMeasurements
Primary table for weight measurement data.

| Column | Type | Description |
|--------|------|-------------|
| Id | Guid | Primary key |
| WeighbridgeId | Guid | Foreign key to Master Data |
| VehicleId | Guid | Foreign key to Master Data |
| MeasurementTime | DateTime | When measurement was taken |
| GrossWeight | decimal | Total weight including vehicle |
| TareWeight | decimal | Empty vehicle weight |
| NetWeight | decimal | Cargo weight (calculated) |
| WeightUnit | string | Unit of measurement (kg, lbs, tons) |
| Temperature | decimal? | Environmental temperature |
| Humidity | decimal? | Environmental humidity |
| CalibrationId | Guid? | Associated calibration record |
| QualityStatus | string | PASS/FAIL/WARNING |
| Notes | string? | Additional observations |
| OrganizationId | Guid | Tenant identifier |

#### WeighbridgeCalibrations
Calibration records for weighbridge equipment.

| Column | Type | Description |
|--------|------|-------------|
| WeighbridgeId | Guid | Equipment identifier |
| CalibratedBy | string | Technician identifier |
| CalibrationDate | DateTime | When calibration performed |
| NextCalibrationDue | DateTime | Scheduled next calibration |
| ReferenceWeight | decimal | Standard weight used |
| MeasuredWeight | decimal | Actual measurement |
| Deviation | decimal | Calculated difference |
| Status | string | VALID/EXPIRED/FAILED |
| CertificateNumber | string? | Certification reference |
| CalibrationMethod | string | Procedure used |

#### QualityChecks
Quality control and validation records.

| Column | Type | Description |
|--------|------|-------------|
| MeasurementId | Guid | Associated measurement |
| CheckType | string | Type of quality check |
| CheckResult | string | PASS/FAIL/WARNING |
| ExpectedRange | string | Expected value range |
| ActualValue | decimal | Measured value |
| Deviation | decimal | Difference from expected |
| AutomatedCheck | bool | System vs manual check |
| CheckedBy | string? | Inspector (if manual) |

### 2. Transactions Module

#### Transactions
Core business transaction records.

| Column | Type | Description |
|--------|------|-------------|
| TransactionNumber | string | Human-readable identifier |
| TransactionType | string | Inbound/Outbound/Transfer |
| VehicleId | Guid | Vehicle involved |
| DriverId | Guid | Driver involved |
| ProductId | Guid | Product being handled |
| SupplierId | Guid? | Supplier (for inbound) |
| CustomerId | Guid? | Customer (for outbound) |
| SourceLocationId | Guid? | Origin location |
| DestinationLocationId | Guid? | Target location |
| PlannedDate | DateTime | Scheduled date/time |
| ActualStartTime | DateTime? | When transaction began |
| ActualEndTime | DateTime? | When transaction completed |
| Status | string | Pending/InProgress/Completed/Cancelled |
| Priority | int | Processing priority (1-10) |
| EstimatedQuantity | decimal | Expected quantity |
| ActualQuantity | decimal? | Actual quantity processed |
| UnitOfMeasure | string | Quantity unit |
| ReferenceDocuments | string? | Related document references |
| SpecialInstructions | string? | Handling instructions |

#### TransactionWorkflows
Workflow state management for transactions.

| Column | Type | Description |
|--------|------|-------------|
| TransactionId | Guid | Associated transaction |
| WorkflowName | string | Workflow definition name |
| CurrentStep | string | Active workflow step |
| StepStatus | string | Pending/InProgress/Completed/Failed |
| StartedAt | DateTime | Workflow initiation time |
| CompletedAt | DateTime? | Workflow completion time |
| WorkflowData | string | JSON serialized state data |
| AssignedTo | string? | Current step assignee |
| DueDate | DateTime? | Step completion deadline |

#### TransactionApprovals
Approval process tracking.

| Column | Type | Description |
|--------|------|-------------|
| TransactionId | Guid | Transaction requiring approval |
| ApprovalType | string | Type of approval needed |
| RequiredBy | string | Policy/role requiring approval |
| RequestedBy | string | User requesting approval |
| RequestedAt | DateTime | When approval was requested |
| ApprovedBy | string? | User who approved |
| ApprovedAt | DateTime? | Approval timestamp |
| Status | string | Pending/Approved/Rejected |
| Reason | string? | Approval/rejection reason |
| ApprovalLevel | int | Multi-level approval stage |

### 3. Compliance Module

#### ComplianceViolations
Regulatory and policy violation records.

| Column | Type | Description |
|--------|------|-------------|
| ViolationType | string | Category of violation |
| Severity | string | Critical/High/Medium/Low |
| Description | string | Detailed violation description |
| EntityType | string | What entity violated (Transaction, etc.) |
| EntityId | Guid | Specific entity identifier |
| DetectedAt | DateTime | When violation was detected |
| DetectionMethod | string | Automated/Manual |
| RegulatoryStandardId | Guid? | Applicable standard |
| Status | string | Open/InProgress/Resolved/Dismissed |
| AssignedTo | string? | Responsible person |
| ResolvedAt | DateTime? | Resolution timestamp |
| ResolutionNotes | string? | How violation was resolved |
| RecurrenceCount | int | Number of similar violations |

#### RegulatoryStandards
Applicable regulations and standards.

| Column | Type | Description |
|--------|------|-------------|
| StandardName | string | Official standard name |
| StandardType | string | Regulation/Policy/Guideline |
| IssuingAuthority | string | Regulatory body |
| EffectiveDate | DateTime | When standard takes effect |
| ExpirationDate | DateTime? | When standard expires |
| Jurisdiction | string | Geographic applicability |
| ComplianceRequirements | string | JSON serialized requirements |
| PenaltyStructure | string? | Violation penalties |
| MonitoringFrequency | string | Required check frequency |

#### ComplianceReports
Generated compliance reports.

| Column | Type | Description |
|--------|------|-------------|
| ReportType | string | Daily/Weekly/Monthly/Annual |
| PeriodStart | DateTime | Reporting period start |
| PeriodEnd | DateTime | Reporting period end |
| GeneratedAt | DateTime | Report generation time |
| GeneratedBy | string | User who generated report |
| Status | string | Draft/Final/Submitted |
| ReportData | string | JSON serialized report content |
| ViolationCount | int | Number of violations in period |
| ComplianceRate | decimal | Compliance percentage |
| SubmittedTo | string? | Regulatory authority |
| SubmissionDate | DateTime? | When report was submitted |

### 4. Analytics Module

#### AnalyticsKPIs
Key Performance Indicator definitions and values.

| Column | Type | Description |
|--------|------|-------------|
| KPIName | string | Display name |
| KPIType | string | Revenue/Efficiency/Quality/Compliance |
| CalculationMethod | string | How value is calculated |
| DataSources | string | JSON array of data sources |
| TargetValue | decimal? | Goal or benchmark |
| CurrentValue | decimal | Latest calculated value |
| PreviousValue | decimal? | Previous period value |
| CalculatedAt | DateTime | Last calculation time |
| Trend | string | Improving/Declining/Stable |
| Unit | string | Value unit (%, $, count, etc.) |
| IsActive | bool | Whether KPI is tracked |

#### Dashboards
Dashboard configuration and layouts.

| Column | Type | Description |
|--------|------|-------------|
| DashboardName | string | Display name |
| DashboardType | string | Executive/Operational/Analytical |
| Layout | string | JSON serialized layout config |
| RefreshInterval | int | Auto-refresh seconds |
| IsPublic | bool | Available to all users |
| CreatedBy | string | Dashboard creator |
| SharedWith | string? | JSON array of user/role access |
| LastAccessed | DateTime? | Most recent view |
| AccessCount | int | Usage statistics |

#### TrendAnalysis
Historical trend analysis results.

| Column | Type | Description |
|--------|------|-------------|
| MetricName | string | What is being analyzed |
| AnalysisType | string | Linear/Seasonal/Exponential |
| PeriodStart | DateTime | Analysis period start |
| PeriodEnd | DateTime | Analysis period end |
| DataPoints | string | JSON array of time series data |
| TrendDirection | string | Upward/Downward/Flat |
| ConfidenceLevel | decimal | Statistical confidence (0-1) |
| ForecastData | string? | JSON predicted future values |
| Insights | string? | Generated analysis insights |

### 5. Operations Module

#### OperationalAlerts
System and operational alerts.

| Column | Type | Description |
|--------|------|-------------|
| AlertType | string | SystemError/MaintenanceDue/Threshold |
| Severity | string | Critical/High/Medium/Low |
| Message | string | Alert description |
| EntityType | string | Affected entity type |
| EntityId | Guid? | Specific entity identifier |
| AlertTime | DateTime | When alert was triggered |
| AcknowledgedBy | string? | User who acknowledged |
| AcknowledgedAt | DateTime? | Acknowledgment time |
| ResolvedBy | string? | User who resolved |
| ResolvedAt | DateTime? | Resolution time |
| Status | string | Open/Acknowledged/Resolved |
| AutoResolved | bool | System vs manual resolution |

#### MaintenanceSchedules
Equipment maintenance scheduling.

| Column | Type | Description |
|--------|------|-------------|
| WeighbridgeId | Guid? | Equipment identifier |
| VehicleId | Guid? | Vehicle identifier |
| MaintenanceType | string | Preventive/Corrective/Emergency |
| ScheduledDate | DateTime | Planned maintenance date |
| CompletedDate | DateTime? | Actual completion date |
| EstimatedDuration | int | Expected duration (hours) |
| ActualDuration | int? | Actual duration (hours) |
| MaintenanceProvider | string? | Service provider |
| EstimatedCost | decimal? | Budgeted cost |
| ActualCost | decimal? | Final cost |
| Currency | string? | Cost currency |
| Status | string | Scheduled/InProgress/Completed/Cancelled |
| Priority | int | Urgency level (1-10) |
| Notes | string? | Maintenance notes |

### 6. DataSync Module

#### SyncSessions
Multi-site synchronization sessions.

| Column | Type | Description |
|--------|------|-------------|
| SourceSiteId | string | Origin site identifier |
| TargetSiteId | string | Destination site identifier |
| SyncType | string | Full/Incremental/Delta |
| StartTime | DateTime | Session start time |
| EndTime | DateTime? | Session completion time |
| Status | string | InProgress/Completed/Failed/Cancelled |
| RecordsProcessed | int | Total records handled |
| RecordsSynced | int | Successfully synchronized |
| RecordsFailed | int | Failed synchronizations |
| SyncConfiguration | string? | JSON sync parameters |
| ErrorSummary | string? | Error details |
| TriggeredBy | string | Manual/Scheduled/Event |
| TriggerReason | string? | Why sync was initiated |

#### SiteConfigurations
Site-specific configuration for synchronization.

| Column | Type | Description |
|--------|------|-------------|
| SiteId | string | Unique site identifier |
| SiteName | string | Human-readable site name |
| SiteType | string | Remote/Hub/Standalone |
| ConnectionString | string | Database connection |
| ApiEndpoint | string | API endpoint URL |
| Status | string | Active/Inactive/Maintenance |
| LastHealthCheck | DateTime | Last connectivity check |
| IsHealthy | bool | Current health status |
| SyncIntervalMinutes | int | Auto-sync frequency |
| TimeZone | string | Site timezone |
| ConfigurationData | string? | JSON site-specific config |

#### SyncConflicts
Data synchronization conflicts.

| Column | Type | Description |
|--------|------|-------------|
| SyncSessionId | Guid | Associated sync session |
| EntityType | string | Conflicting entity type |
| EntityId | Guid | Conflicting entity ID |
| ConflictType | string | Insert/Update/Delete |
| SourceValue | string | JSON source data |
| TargetValue | string | JSON target data |
| DetectedAt | DateTime | Conflict detection time |
| Status | string | Unresolved/Resolved/Ignored |
| ResolutionStrategy | string? | How conflict was resolved |
| ResolvedAt | DateTime? | Resolution timestamp |
| ResolvedBy | string? | User who resolved |

### 7. Archive Module

#### ArchivePolicies
Data retention and archival policies.

| Column | Type | Description |
|--------|------|-------------|
| PolicyName | string | Descriptive policy name |
| EntityType | string | Target entity type |
| RetentionPeriodDays | int | Days before archival |
| ArchiveAfterDays | int? | Days before permanent archive |
| DeleteAfterDays | int? | Days before deletion |
| CompressionEnabled | bool | Whether to compress archived data |
| EncryptionRequired | bool | Whether to encrypt archived data |
| IsActive | bool | Policy enabled status |
| LastExecuted | DateTime? | Last policy execution |
| NextExecution | DateTime? | Scheduled next execution |
| ArchiveLocation | string? | Storage location |

#### ArchivedData
Archived record tracking.

| Column | Type | Description |
|--------|------|-------------|
| OriginalEntityType | string | Source entity type |
| OriginalEntityId | Guid | Source entity ID |
| ArchivedAt | DateTime | Archival timestamp |
| ArchivedBy | string | User/system that archived |
| ArchivePolicyId | Guid | Policy used for archival |
| ArchiveLocation | string | Storage path/identifier |
| DataSize | long | Archived data size (bytes) |
| CompressionRatio | decimal? | Compression achieved |
| IsEncrypted | bool | Whether data is encrypted |
| RestoreCount | int | Times data was restored |
| LastRestored | DateTime? | Most recent restoration |
| ExpiresAt | DateTime? | Permanent deletion date |

#### ArchiveJobs
Archive operation tracking.

| Column | Type | Description |
|--------|------|-------------|
| JobType | string | Archive/Restore/Delete |
| Status | string | Pending/Running/Completed/Failed |
| StartedAt | DateTime | Job start time |
| CompletedAt | DateTime? | Job completion time |
| RecordsProcessed | int | Total records handled |
| RecordsSucceeded | int | Successfully processed |
| RecordsFailed | int | Failed processing |
| ErrorSummary | string? | Error details |
| TriggeredBy | string | User or system trigger |
| JobConfiguration | string? | JSON job parameters |

## Indexes and Performance

### Primary Indexes
- All tables have clustered indexes on `Id` (Guid)
- All tenant tables have composite indexes on `(OrganizationId, Id)`
- Temporal queries use indexes on `CreatedAt`, `UpdatedAt`

### Foreign Key Indexes
- All foreign key columns have non-clustered indexes
- Composite indexes for common query patterns
- Covering indexes for frequently selected columns

### Query Optimization Indexes

#### WeightMeasurements
```sql
IX_WeightMeasurements_OrgId_WeighbridgeId_Time (OrganizationId, WeighbridgeId, MeasurementTime)
IX_WeightMeasurements_OrgId_VehicleId_Time (OrganizationId, VehicleId, MeasurementTime)
IX_WeightMeasurements_QualityStatus (QualityStatus) WHERE QualityStatus != 'PASS'
```

#### Transactions
```sql
IX_Transactions_OrgId_Type_Status (OrganizationId, TransactionType, Status)
IX_Transactions_OrgId_Vehicle_Date (OrganizationId, VehicleId, PlannedDate)
IX_Transactions_Number (TransactionNumber) -- Unique per organization
```

#### ComplianceViolations
```sql
IX_ComplianceViolations_OrgId_Status_Severity (OrganizationId, Status, Severity)
IX_ComplianceViolations_Entity (EntityType, EntityId)
IX_ComplianceViolations_DetectedAt (DetectedAt DESC)
```

## Constraints and Relationships

### Foreign Key Constraints
- Master Data references are enforced through application logic
- Cross-module references use Guid foreign keys
- Cascade delete policies vary by relationship type

### Check Constraints
- Enumerated values enforced via CHECK constraints
- Numeric ranges validated (weights > 0, percentages 0-100)
- Date logic constraints (end dates > start dates)

### Unique Constraints
- Transaction numbers unique per organization
- Calibration certificates globally unique
- Site IDs globally unique across all organizations

## Multi-Tenancy Implementation

### Data Isolation
- Global query filters automatically filter by `OrganizationId`
- Row-Level Security (RLS) policies for additional protection
- Connection string isolation for enterprise deployments

### Index Strategy
- All indexes include `OrganizationId` as first column
- Tenant-specific statistics for query optimization
- Partition schemes by organization for large deployments

## Backup and Recovery

### Backup Strategy
- Daily full backups with transaction log backups
- Point-in-time recovery capability
- Cross-region backup replication for disaster recovery

### Data Retention
- Archive policies automatically move old data
- Compliance-driven retention periods
- Secure deletion with audit trails