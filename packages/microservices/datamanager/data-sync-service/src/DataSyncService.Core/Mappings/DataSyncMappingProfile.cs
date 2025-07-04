using AutoMapper;
using DataSyncService.Core.DTOs;
using DataSyncService.Core.Entities;

namespace DataSyncService.Core.Mappings;

public class DataSyncMappingProfile : Profile
{
    public DataSyncMappingProfile()
    {
        CreateMap<SyncSession, SyncSessionDto>()
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration))
            .ForMember(dest => dest.ProgressPercentage, opt => opt.MapFrom(src => src.ProgressPercentage));

        CreateMap<CreateSyncSessionRequest, SyncSession>()
            .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enums.SyncStatus.Pending))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<UpdateSyncSessionRequest, SyncSession>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<SyncSite, SyncSiteDto>();

        CreateMap<CreateSyncSiteRequest, SyncSite>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enums.SiteStatus.Active))
            .ForMember(dest => dest.LastHealthCheck, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<UpdateSyncSiteRequest, SyncSite>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<SyncConflict, SyncConflictDto>();

        CreateMap<ChangeRecord, ChangeRecordDto>();

        CreateMap<CreateChangeRecordRequest, ChangeRecord>()
            .ForMember(dest => dest.ChangeId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enums.SyncStatus.Pending))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<SiteHealthCheck, HealthCheckDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<ResolveConflictRequest, SyncConflict>()
            .ForMember(dest => dest.ResolvedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enums.ConflictStatus.Resolved))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}