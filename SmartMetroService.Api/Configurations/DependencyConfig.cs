using Microsoft.EntityFrameworkCore;
using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Application.Managers;
using SmartMetroService.Application.Mapping;
using SmartMetroService.Storage.Repositories;
using SmartMetroService.Storage.Sql;
using System.Text.Json.Serialization;

namespace SmartMetroService.Api.Configurations;

public static class DependencyConfig
{
    public static void AddDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        // Service Registration
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IOTPService, OTPService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IStationService, StationService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IAdminService, AdminService>();


        // Repository Registration
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IUserOTPRepository, UserOTPRepository>();
        services.AddScoped<IStationRepository, StationRepository>();
        services.AddScoped<IStationDistanceRepository, StationDistanceRepository>();
        services.AddScoped<ITokenRepository, TokenRepository>();
        services.AddScoped<IAdminRepository, AdminRepository>();

        // AutoMapper
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter()
                );
            });


        var connectionString = configuration["ConnectionStrings:DefaultConnection"];
        services.AddDbContext<MyApplicationDbContext>(options =>
        options.UseSqlServer(connectionString)
        );

        // Add CORS policy
        services.AddCors(options =>
        {
            options.AddPolicy("AllowNgFrontend",
                builder =>
                {
                    builder
                        .WithOrigins("http://localhost:4200") // Angular app origin
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });
    }
}
