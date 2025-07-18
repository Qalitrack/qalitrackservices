using ReportService.Core.DTOs;
using ReportService.Core.Entities;

namespace ReportService.Core.Interfaces;

public interface ITemplateService
{
    // Basic CRUD Operations
    Task<IEnumerable<TemplateReadDto>> GetAllAsync();
    Task<TemplateReadDto?> GetByIdAsync(string id);
    Task<TemplateReadDto> CreateAsync(CreateTemplateDto dto);
    Task<TemplateReadDto?> UpdateAsync(string id, UpdateTemplateDto dto);
    Task<bool> DeleteAsync(string id);
    Task<bool> IsNameAvailableAsync(string name);
    
    // Template Management
    Task<List<TemplateReadDto>> GetByOrganizationAsync(string organizationId);
    Task<List<TemplateReadDto>> GetByTypeAsync(TemplateType templateType);
    Task<List<TemplateReadDto>> GetByStatusAsync(TemplateStatus status);
    Task<List<TemplateReadDto>> GetSystemTemplatesAsync();
    Task<List<TemplateReadDto>> GetDefaultTemplatesAsync();
    Task<List<TemplateReadDto>> GetActiveTemplatesAsync(string organizationId);
    Task<List<TemplateReadDto>> SearchTemplatesAsync(string searchTerm, string organizationId);
    
    // Template Categories and Classification
    Task<List<TemplateReadDto>> GetByIndustryAsync(string industry);
    Task<List<TemplateReadDto>> GetByCategoryAsync(string category);
    Task<List<TemplateReadDto>> GetByUseCaseAsync(string useCase);
    Task<List<TemplateReadDto>> GetTemplatesForReportTypeAsync(ReportType reportType);
    
    // Template Usage and Analytics
    Task<List<TemplateReadDto>> GetMostUsedTemplatesAsync(string organizationId, int limit = 10);
    Task<List<TemplateReadDto>> GetRecentlyUsedTemplatesAsync(string organizationId, int days = 30);
    Task<TemplateUsageDto> GetTemplateUsageAsync(string templateId);
    Task<List<TemplateUsageDto>> GetUsageAnalyticsAsync(string organizationId, DateTime fromDate, DateTime toDate);
    
    // Template Hierarchy and Inheritance
    Task<List<TemplateReadDto>> GetDerivedTemplatesAsync(string baseTemplateId);
    Task<TemplateReadDto?> GetBaseTemplateAsync(string templateId);
    Task<bool> CreateDerivedTemplateAsync(string baseTemplateId, CreateDerivedTemplateRequest request);
    Task<bool> UpdateTemplateInheritanceAsync(string templateId, string? baseTemplateId);
    
    // Template Validation and Quality
    Task<TemplateValidationDto> ValidateTemplateAsync(string templateId);
    Task<bool> ValidateTemplateStructureAsync(string templateId);
    Task<DataSourceCompatibilityDto> CheckDataSourceCompatibilityAsync(string templateId, List<string> dataSources);
    Task<TemplateQualityDto> GetTemplateQualityMetricsAsync(string templateId);
    
    // Template Approval and Lifecycle
    Task<bool> SubmitForApprovalAsync(string templateId, string submittedBy);
    Task<bool> ApproveTemplateAsync(string templateId, string approvedBy, string? comments = null);
    Task<bool> RejectTemplateAsync(string templateId, string reviewerId, string reason);
    Task<List<TemplateReadDto>> GetTemplatesAwaitingApprovalAsync();
    Task<List<TemplateReadDto>> GetApprovedTemplatesAsync();
    
    // Template Customization
    Task<TemplateCustomizationDto> GetCustomizationOptionsAsync(string templateId);
    Task<bool> UpdateCustomizationAsync(string templateId, UpdateCustomizationRequest request);
    Task<bool> ResetToDefaultsAsync(string templateId);
    Task<TemplatePreviewDto> PreviewTemplateAsync(string templateId, PreviewTemplateRequest? request = null);
    
    // Template Import and Export
    Task<TemplateReadDto> ImportTemplateAsync(ImportTemplateRequest request);
    Task<TemplateExportDto> ExportTemplateAsync(string templateId, ExportTemplateRequest? request = null);
    Task<bool> CloneTemplateAsync(string templateId, CloneTemplateRequest request);
    Task<TemplateConfigDto> GetTemplateConfigurationAsync(string templateId);
    
    // Template Library Management
    Task<List<TemplateReadDto>> GetTemplateLibraryAsync(TemplateLibraryFilter? filter = null);
    Task<bool> PublishToLibraryAsync(string templateId, PublishTemplateRequest request);
    Task<bool> UnpublishFromLibraryAsync(string templateId);
    Task<List<TemplateReadDto>> GetFeaturedTemplatesAsync();
    
    // Template Reports Integration
    Task<List<ReportReadDto>> GetReportsUsingTemplateAsync(string templateId);
    Task<TemplateImpactAnalysisDto> AnalyzeTemplateImpactAsync(string templateId);
    Task<bool> UpdateReportsWithTemplateChangesAsync(string templateId, bool applyToAll = false);
    
    // Template Performance and Optimization
    Task<TemplatePerformanceDto> GetTemplatePerformanceAsync(string templateId);
    Task<bool> OptimizeTemplateAsync(string templateId);
    Task<TemplateGenerationStatsDto> GetGenerationStatsAsync(string templateId);
    
    // Template Versioning
    Task<List<TemplateVersionDto>> GetTemplateVersionsAsync(string templateId);
    Task<TemplateReadDto> CreateTemplateVersionAsync(string templateId, CreateVersionRequest request);
    Task<bool> RollbackToVersionAsync(string templateId, string version);
    Task<TemplateComparisonDto> CompareTemplateVersionsAsync(string templateId, string version1, string version2);
    
    // Template Collaboration
    Task<bool> ShareTemplateAsync(string templateId, ShareTemplateRequest request);
    Task<bool> RevokeTemplateShareAsync(string shareToken);
    Task<TemplateReadDto?> GetSharedTemplateAsync(string shareToken);
    Task<List<TemplateAccessDto>> GetTemplateAccessListAsync(string templateId);
    
    // Template Maintenance
    Task<bool> ArchiveTemplateAsync(string templateId, string archivedBy);
    Task<bool> RestoreTemplateAsync(string templateId);
    Task<List<TemplateReadDto>> GetArchivedTemplatesAsync(string organizationId);
    Task<bool> DeprecateTemplateAsync(string templateId, string reason, string? replacementTemplateId = null);
}