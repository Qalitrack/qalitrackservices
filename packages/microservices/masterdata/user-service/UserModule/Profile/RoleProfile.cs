using AutoMapper;
using UserModule.Dtos;
using UserModule.Dtos.Roles;
using UserModule.Models;

namespace UserModule.Profiles
{
    public class RoleProfiles : AutoMapper.Profile
    {
        public RoleProfiles()
        {
            CreateMap<Role, RoleCreateDto>().ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserRoles, opt => opt.Ignore());

            CreateMap<Role, RoleReadDto>();
        }
    }
}