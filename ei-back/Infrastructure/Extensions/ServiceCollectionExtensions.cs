using ei_back.Core.Application.Interfaces;
using ei_back.Core.Application.Repository;
using ei_back.Core.Application.Service.Encryption;
using ei_back.Core.Application.Service.Encryption.Interfaces;
using ei_back.Core.Application.Service.Play;
using ei_back.Core.Application.Service.Play.Interfaces;
using ei_back.Core.Application.UseCase.Game;
using ei_back.Core.Application.UseCase.Game.Interfaces;
using ei_back.Core.Application.UseCase.GameInfo;
using ei_back.Core.Application.UseCase.GameInfo.Interfaces;
using ei_back.Core.Application.UseCase.Play;
using ei_back.Core.Application.UseCase.Play.Interfaces;
using ei_back.Core.Application.UseCase.Role;
using ei_back.Core.Application.UseCase.Role.Interfaces;
using ei_back.Core.Application.UseCase.User;
using ei_back.Core.Application.UseCase.User.Interfaces;
using ei_back.Infrastructure.Context;
using ei_back.Infrastructure.Context.Interfaces;
using ei_back.Infrastructure.Context.Repository;
using ei_back.Infrastructure.GenAI;
using ei_back.Infrastructure.Token;
using Microsoft.Extensions.Configuration;

namespace ei_back.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IPlayerRepository, PlayerRepository>();
            services.AddScoped<IGameRepository, GameRepository>();
            services.AddScoped<IPlayRepository, PlayRepository>();
            services.AddScoped<IGameInfoRepository, GameInfoRepository>();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IEncryptionService, EncryptionService>();
            services.AddScoped<IGeneratePlaysSummaryService, GeneratePlaysSummaryService>();
            services.AddScoped<IInitialMasterPlayService, InitialMasterPlayService>();
            services.AddScoped<IUpsertWorldInfoService, UpsertWorldInfoService>();
            services.AddScoped<IPlayAnalyzerService, PlayAnalyzerService>();

            return services;
        }

        public static IServiceCollection AddUseCases(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ICreateUserUseCase, CreateUserUseCase>();
            services.AddScoped<IGetUserUseCase, GetUserUseCase>();
            services.AddScoped<ICheckUserInfoUseCase, CheckUserInfoUseCase>();
            services.AddScoped<ISignInUseCase, SigninUseCase>();
            services.AddScoped<IRefreshTokenUseCase, RefreshTokenUseCase>();
            services.AddScoped<ILogoutUseCase, LogoutUseCase>();
            services.AddScoped<IChangePasswordUseCase, ChangePasswordUseCase>();
            services.AddScoped<IGetUserNameUseCase, GetUserNameUseCase>();
            services.AddScoped<IApplyRolesUseCase, ApplyRolesUseCase>();
            services.AddScoped<IGetAllRoleUseCase, GetAllRoleUseCase>();
            services.AddScoped<ICreateGameUseCase, CreateGameUseCase>();
            services.AddScoped<IGetGamesUseCase, GetGamesUseCase>();
            services.AddScoped<IGetPlaysUseCase, GetPlaysUseCase>();

            var bypassLlm = configuration.GetValue<bool>("Features:BypassLlmGenerator");
            if (bypassLlm)
                services.AddScoped<INewUserPlayUseCase, BypassNewUserPlayUseCase>();
            else
                services.AddScoped<INewUserPlayUseCase, NewUserPlayUseCase>();

            services.AddScoped<ICreateGameInfoUseCase, CreateGameInfoUseCase>();
            services.AddScoped<IGetGameByIdAndUserUseCase, GetGameByIdAndUserUseCase>();
            services.AddScoped<IDeleteGameUseCase, DeleteGameUseCase>();

            return services;
        }

        public static IServiceCollection AddInfraHttpClients(this IServiceCollection services)
        {
            return services;
        }

        public static IServiceCollection AddGenAiClient(this IServiceCollection services)
        {
            services.AddScoped<IGenAi, GenAi>();
            return services;
        }
    }
}
