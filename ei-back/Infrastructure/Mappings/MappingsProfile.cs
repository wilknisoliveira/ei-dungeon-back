using AutoMapper;
using ei_back.Application.Api.Game.Dtos;
using ei_back.Application.Api.Play.Dtos;
using ei_back.Application.Api.Role.Dtos;
using ei_back.Application.Api.User.Dtos;
using ei_back.Domain.Game;
using ei_back.Domain.Play;
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
                .ForMember(dest => dest.Roles, opt => opt.Ignore())
                .ForMember(dest => dest.Games, opt => opt.Ignore());

            //Role
            CreateMap<RoleEntity, RoleDto>().ReverseMap();
            CreateMap<RoleEntity, RoleDtoResponse>()
                .ForMember(dest => dest.Users, opt => opt.Ignore());
            CreateMap<UserEntity, ApplyRoleDtoResponse>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore());

            //Game
            CreateMap<GameDtoRequest, GameEntity>()
                .ForMember(dest => dest.OwnerUser, opt => opt.Ignore())
                .ForMember(dest => dest.Players, opt => opt.Ignore())
                .ForMember(dest => dest.Plays, opt => opt.Ignore());
            CreateMap<GameEntity, GameDtoResponse>();

            //Play
            CreateMap<PlayEntity, PlayDtoResponse>();
        }
    }
}
