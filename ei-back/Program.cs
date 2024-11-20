using ei_back.Infrastructure.Context;
using ei_back.Infrastructure.Context.Interfaces;
using ei_back.Infrastructure.Exceptions;
using ei_back.Infrastructure.Mappings;
using ei_back.Infrastructure.ExternalAPIs;
using ei_back.Infrastructure.ExternalAPIs.Client.GenerativeAIApiClient;
using ei_back.Infrastructure.ExternalAPIs.Interfaces;
using ei_back.Infrastructure.Swagger;
using ei_back.Infrastructure.Token;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Globalization;
using System.Reflection;
using System.Text;
using ei_back.UserInterface.Hubs;
using ei_back.Core.Application.UseCase.Role;
using ei_back.Core.Application.UseCase.User;
using ei_back.Core.Application.UseCase.Role.Interfaces;
using ei_back.Core.Application.UseCase.User.Interfaces;
using ei_back.Core.Application.UseCase.Play;
using ei_back.Core.Application.UseCase.Game;
using ei_back.Core.Application.UseCase.Play.Interfaces;
using ei_back.Core.Application.UseCase.Game.Interfaces;
using ei_back.Core.Application.Repository;
using ei_back.Infrastructure.Context.Repository;
using ei_back.Core.Application.Service.User;
using ei_back.Core.Application.Service.Game;
using ei_back.Core.Application.Service.Play;
using ei_back.Core.Application.Service.Player;
using ei_back.Core.Application.Service.Role;
using ei_back.Core.Application.Service.User.Interfaces;
using ei_back.Core.Application.Service.Game.Interfaces;
using ei_back.Core.Application.Service.Play.Interfaces;
using ei_back.Core.Application.Service.Player.Interfaces;
using ei_back.Core.Application.Service.Role.Interfaces;

var builder = WebApplication.CreateBuilder(args);

//Logs
builder.Logging.ClearProviders();
var logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Infrastructure/Logs/logs.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

//Enviroment
builder.Configuration.AddEnvironmentVariables()
    .AddUserSecrets(Assembly.GetExecutingAssembly(), true);

//Deploy
//var port = Environment.GetEnvironmentVariable("PORT") ?? "8081";
//builder.WebHost.UseUrls($"http://*:{port}");

//Exception Handler
builder.Services.AddExceptionHandler<AppExceptionHandler>();

//Token Configurations
var tokenConfigurations = new TokenConfiguration();

new ConfigureFromConfigurationOptions<TokenConfiguration>(
    builder.Configuration.GetSection("TokenConfigurations")
    ).Configure(tokenConfigurations);

builder.Services.AddSingleton(tokenConfigurations);

//Token Configurations -> Define the authentication parameters
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = tokenConfigurations.Issuer,
        ValidAudience = tokenConfigurations.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenConfigurations.Secret))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
                //context.HttpContext.Request.Headers.Add("Authorization", "Bearer " + accessToken);
            }
            return Task.CompletedTask;
        }
    };
});

//Token Configurations -> Authorizate
builder.Services.AddAuthorization(auth =>
{
    auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser().Build()
        );
});

//Cors
builder.Services.AddCors( options => options.AddDefaultPolicy(builder =>
{
    builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
}));

//Native
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddInfrastructureSwagger();

//Database
var connection = builder.Configuration["PostgresConnection:PostgresConnectionString"];
builder.Services.AddDbContext<EIContext>(options => options.UseNpgsql(
    connection, 
    assembly => assembly.MigrationsAssembly(typeof(EIContext).Assembly.FullName))
);

//HealthChecks
builder.Services.AddHealthChecks()
    .AddNpgSql(connection, name: "Postgres Check", tags: new string[] { "db", "data" });

builder.Services.AddHealthChecksUI()
    .AddInMemoryStorage();

//AutoMapper
builder.Services.AddAutoMapper(typeof(MappingsProfile));

//SignalR
builder.Services.AddSignalR();

//Resources
builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Infrastructure/Resources";
});

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en-US")
    };

    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
});


//Apply the Dependecy Injection here!
//ExternalApi
builder.Services.AddScoped<IGenerativeAIApiHttpService, GenerativeAIApiHttpService>();
builder.Services.AddScoped<IGenerativeAIApiClient, OpenAIApiClient>();
//Base
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
//User
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICreateUserUseCase,  CreateUserUseCase>();
builder.Services.AddScoped<IGetUserUseCase, GetUserUseCase>();
builder.Services.AddScoped<ISignInUseCase, SigninUseCase>();
builder.Services.AddScoped<IChangePasswordUseCase, ChangePasswordUseCase>();
builder.Services.AddScoped<IGetUserNameUseCase, GetUserNameUseCase>();
//Role
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IApplyRolesUseCase, ApplyRolesUseCase>();
builder.Services.AddScoped<ICreateRoleUseCase, CreateRoleUseCase>();
builder.Services.AddScoped<IGetAllRoleUseCase, GetAllRoleUseCase>();
//Player
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IPlayerFactory, PlayerFactory>();
//Game
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<ICreateGameUseCase, CreateGameUseCase>();
builder.Services.AddScoped<IGetGamesUseCase, GetGamesUseCase>();

//Play
builder.Services.AddScoped<IPlayRepository, PlayRepository>();
builder.Services.AddScoped<IPlayService, PlayService>();
builder.Services.AddScoped<IGetPlaysUseCase, GetPlaysUseCase>();
builder.Services.AddScoped<INewUserPlayUseCase, NewUserPlayUseCase>();
builder.Services.AddScoped<IInitialMasterPlayService, InitialMasterPlayService>();
builder.Services.AddScoped<IGeneratePlaysResumeService, GeneratePlaysResumeService>();

//Prompt
//builder.Services.AddScoped<IAiPrompt, AiPrompt>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Health Check
app.UseHealthChecks("/health", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseHealthChecksUI(options =>
{
    options.UIPath = "/healthDashboard";
});

//Map Web Socket
app.MapHub<ExampleHub>("/hubs");

//Exception Handler
app.UseExceptionHandler(_ => { });

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseRequestLocalization();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EIContext>();
    dbContext.Database.Migrate();
}

app.Run();
