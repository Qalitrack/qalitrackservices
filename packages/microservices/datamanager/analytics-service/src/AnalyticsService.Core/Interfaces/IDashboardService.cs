using AnalyticsService.Core.DTOs;
using AnalyticsService.Core.Entities;

namespace AnalyticsService.Core.Interfaces;

public interface IDashboardService
{
    // Dashboard Management
    Task<DashboardDto> CreateDashboardAsync(CreateDashboardRequest request);
    Task<DashboardDto?> GetDashboardAsync(string dashboardId);
    Task<DashboardDto> UpdateDashboardAsync(string dashboardId, UpdateDashboardRequest request);
    Task<bool> DeleteDashboardAsync(string dashboardId);
    Task<List<DashboardDto>> GetDashboardsByOrganizationAsync(string organizationId);
    Task<List<DashboardDto>> GetDashboardsByTypeAsync(DashboardType dashboardType);
    Task<List<DashboardDto>> GetDashboardsByUserAsync(string userId);
    
    // Dashboard Widgets
    Task<DashboardWidgetDto> AddWidgetToDashboardAsync(string dashboardId, CreateWidgetRequest request);
    Task<DashboardWidgetDto> UpdateWidgetAsync(string widgetId, UpdateWidgetRequest request);
    Task<bool> RemoveWidgetFromDashboardAsync(string dashboardId, string widgetId);
    Task<List<DashboardWidgetDto>> GetDashboardWidgetsAsync(string dashboardId);
    Task<bool> ReorderWidgetsAsync(string dashboardId, List<WidgetOrderRequest> widgetOrder);
    
    // Dashboard Layout
    Task<DashboardLayoutDto> GetDashboardLayoutAsync(string dashboardId);
    Task<bool> UpdateDashboardLayoutAsync(string dashboardId, UpdateLayoutRequest request);
    Task<bool> ApplyLayoutTemplateAsync(string dashboardId, string templateId);
    Task<List<DashboardTemplateDto>> GetLayoutTemplatesAsync();
    
    // Dashboard Data
    Task<DashboardDataDto> GetDashboardDataAsync(string dashboardId, DashboardFilterRequest? filters = null);
    Task<DashboardDataDto> RefreshDashboardDataAsync(string dashboardId);
    Task<WidgetDataDto> GetWidgetDataAsync(string widgetId, WidgetFilterRequest? filters = null);
    Task<bool> EnableRealTimeDashboardAsync(string dashboardId);
    Task<bool> DisableRealTimeDashboardAsync(string dashboardId);
    
    // Dashboard Sharing
    Task<ShareTokenDto> ShareDashboardAsync(string dashboardId, ShareDashboardRequest request);
    Task<bool> RevokeDashboardShareAsync(string shareToken);
    Task<DashboardDto?> GetSharedDashboardAsync(string shareToken);
    Task<EmbedCodeDto> GetDashboardEmbedCodeAsync(string dashboardId);
    
    // Dashboard Access Control
    Task<bool> GrantDashboardAccessAsync(string dashboardId, GrantAccessRequest request);
    Task<bool> RevokeDashboardAccessAsync(string dashboardId, string userId);
    Task<List<DashboardAccessDto>> GetDashboardAccessListAsync(string dashboardId);
    Task<bool> UpdateDashboardPermissionsAsync(string dashboardId, UpdatePermissionsRequest request);
    
    // Dashboard Analytics
    Task<DashboardUsageDto> GetDashboardUsageAsync(string dashboardId, DateTime fromDate, DateTime toDate);
    Task<List<DashboardUsageDto>> GetUsageAnalyticsAsync(string organizationId, DateTime fromDate, DateTime toDate);
    Task<DashboardPerformanceDto> GetDashboardPerformanceAsync(string dashboardId);
    Task<bool> OptimizeDashboardAsync(string dashboardId);
    
    // Dashboard Export
    Task<byte[]> ExportDashboardAsPdfAsync(string dashboardId);
    Task<byte[]> ExportDashboardAsImageAsync(string dashboardId, string format = "PNG");
    Task<DashboardConfigDto> ExportDashboardConfigAsync(string dashboardId);
    Task<DashboardDto> ImportDashboardConfigAsync(ImportDashboardRequest request);
    
    // Dashboard Templates
    Task<DashboardTemplateDto> CreateDashboardTemplateAsync(CreateTemplateRequest request);
    Task<List<DashboardTemplateDto>> GetDashboardTemplatesAsync(DashboardType? dashboardType = null);
    Task<DashboardDto> CreateDashboardFromTemplateAsync(string templateId, CreateFromTemplateRequest request);
    
    // Interactive Features
    Task<DrillDownResultDto> ExecuteDrillDownAsync(string widgetId, DrillDownRequest request);
    Task<FilterOptionsDto> GetAvailableFiltersAsync(string dashboardId);
    Task<DashboardDataDto> ApplyFiltersAsync(string dashboardId, DashboardFilterRequest filters);
    Task<List<DashboardAlertDto>> GetDashboardAlertsAsync(string dashboardId);
}