using AutoMapper;
using ei_back.Core.Application.UseCase.Game.Dtos;
using ei_back.Core.Application.UseCase.Play.Dtos;
using ei_back.Core.Application.UseCase.Role.Dtos;
using ei_back.Core.Application.UseCase.User.Dtos;
using ei_back.Core.Domain.Entity;

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

            //Player
            CreateMap<PlayerEntity, PlayerDtoResponse>();
        }
    }
}
