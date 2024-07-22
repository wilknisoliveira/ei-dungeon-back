using AutoMapper;
using ei_back.Application.Api.Role.Dtos;
using ei_back.Application.Api.User.Dtos;
using ei_back.Domain.Role;
using ei_back.Domain.User;

namespace ei_back.Infrastructure.Mappings
{
    public class MappingsProfile : Profile
    {
        public MappingsProfile()
        {
            //User
            CreateMap<UserEntity, UserDtoResponse>();
            CreateMap<UserEntity, UserGetDtoResponse>();
            CreateMap<UserDtoRequest, UserEntity>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore());

            //Role
            CreateMap<RoleEntity, RoleDto>().ReverseMap();
            CreateMap<RoleEntity, RoleDtoResponse>()
                .ForMember(dest => dest.Users, opt => opt.Ignore());
            CreateMap<UserEntity, ApplyRoleDtoResponse>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore());
        }
    }
}
