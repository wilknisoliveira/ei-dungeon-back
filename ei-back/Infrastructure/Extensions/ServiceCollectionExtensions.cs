using ei_back.Core.Application.Repository;
using ei_back.Infrastructure.Context.Interfaces;
using ei_back.Infrastructure.Context.Repository;
using ei_back.Infrastructure.Context;
using ei_back.Infrastructure.ExternalAPIs.Interfaces;
using ei_back.Infrastructure.ExternalAPIs;
using ei_back.Core.Application.Service.User.Interfaces;
using ei_back.Core.Application.Service.User;
using ei_back.Infrastructure.Token;
using ei_back.Core.Application.Service.Role.Interfaces;
using ei_back.Core.Application.Service.Role;
using ei_back.Core.Application.Service.Player.Interfaces;
using ei_back.Core.Application.Service.Player;
using ei_back.Core.Application.Service.Game.Interfaces;
using ei_back.Core.Application.Service.Game;
using ei_back.Core.Application.Service.Play.Interfaces;
using ei_back.Core.Application.Service.Play;
using ei_back.Infrastructure.ExternalAPIs.Client.GenerativeAIApiClient;
using ei_back.Core.Application.UseCase.User.Interfaces;
using ei_back.Core.Application.UseCase.User;
using ei_back.Core.Application.UseCase.Role.Interfaces;
using ei_back.Core.Application.UseCase.Role;
using ei_back.Core.Application.UseCase.Game.Interfaces;
using ei_back.Core.Application.UseCase.Game;
using ei_back.Core.Application.UseCase.Play.Interfaces;
using ei_back.Core.Application.UseCase.Play;

namespace ei_back.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IPlayerRepository, PlayerRepository>();
            services.AddScoped<IGameRepository, GameRepository>();
            services.AddScoped<IPlayRepository, PlayRepository>();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IGenerativeAIApiClient, OpenAIApiClient>();
            services.AddScoped<IGenerativeAIApiHttpService, GenerativeAIApiHttpService>();
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IPlayerService, PlayerService>();
            services.AddScoped<IGameService, GameService>();
            services.AddScoped<IPlayService, PlayService>();
            services.AddScoped<IGeneratePlaysResumeService, GeneratePlaysResumeService>();
            services.AddScoped<IInitialMasterPlayService, InitialMasterPlayService>();
            services.AddScoped<IPlayerFactory, PlayerFactory>();

            return services;
        }

        public static IServiceCollection AddUseCases(this IServiceCollection services)
        {
            services.AddScoped<ICreateUserUseCase, CreateUserUseCase>();
            services.AddScoped<IGetUserUseCase, GetUserUseCase>();
            services.AddScoped<ISignInUseCase, SigninUseCase>();
            services.AddScoped<IChangePasswordUseCase, ChangePasswordUseCase>();
            services.AddScoped<IGetUserNameUseCase, GetUserNameUseCase>();
            services.AddScoped<IApplyRolesUseCase, ApplyRolesUseCase>();
            services.AddScoped<ICreateRoleUseCase, CreateRoleUseCase>();
            services.AddScoped<IGetAllRoleUseCase, GetAllRoleUseCase>();
            services.AddScoped<ICreateGameUseCase, CreateGameUseCase>();
            services.AddScoped<IGetGamesUseCase, GetGamesUseCase>();
            services.AddScoped<IGetPlaysUseCase, GetPlaysUseCase>();
            services.AddScoped<INewUserPlayUseCase, NewUserPlayUseCase>();

            return services;
        }
    }
}
