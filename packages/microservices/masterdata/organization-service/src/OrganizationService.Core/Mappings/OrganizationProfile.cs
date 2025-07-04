using AutoMapper;
using OrganizationService.Core.DTOs;
using OrganizationService.Core.Entities;

namespace OrganizationService.Core.Mappings;

public class OrganizationProfile : Profile
{
    public OrganizationProfile()
    {
        CreateMap<Organization, OrganizationDto>()
            .ForMember(dest => dest.ChildOrganizations, opt => opt.MapFrom(src => src.ChildOrganizations));
        
        CreateMap<CreateOrganizationRequest, Organization>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => OrganizationStatus.Active));
        
        CreateMap<UpdateOrganizationRequest, Organization>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Code, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<OrganizationUser, OrganizationUserDto>()
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null));
        
        CreateMap<CreateOrganizationUserRequest, OrganizationUser>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => UserStatus.Invited))
            .ForMember(dest => dest.InvitedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.InvitationToken, opt => opt.MapFrom(src => Guid.NewGuid().ToString()));
        
        CreateMap<UpdateOrganizationUserRequest, OrganizationUser>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<OrganizationSettings, OrganizationSettingsDto>();
        
        CreateMap<UpdateOrganizationSettingsRequest, OrganizationSettings>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<OrganizationDepartment, OrganizationDepartmentDto>();
        CreateMap<OrganizationLocation, OrganizationLocationDto>();
    }
}

public class OrganizationDepartmentDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ParentDepartmentId { get; set; }
    public string? ManagerId { get; set; }
    public DepartmentStatus Status { get; set; }
}

public class OrganizationLocationDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public LocationType Type { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public LocationStatus Status { get; set; }
    public bool IsHeadquarters { get; set; }
}