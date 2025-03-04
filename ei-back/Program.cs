using ei_back.Infrastructure.Context;
using ei_back.Infrastructure.Exceptions;
using ei_back.Infrastructure.Mappings;
using ei_back.Infrastructure.ExternalAPIs.Client.GenerativeAIApiClient;
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
using ei_back.Infrastructure.Extensions;

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

builder.Services.AddRepositories();
builder.Services.AddServices();
builder.Services.AddUseCases();
builder.Services.AddInfraHttpClients();

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
