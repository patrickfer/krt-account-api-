using FluentValidation;
using KRT.Application.Accounts.Commands.CreateAccount;
using KRT.Application.Accounts.Mappings;
using KRT.Application.Common.Interfaces;
using KRT.Domain.Interfaces.Repositories;
using KRT.Infrastructure.Caching;
using KRT.Infrastructure.Data.Context;
using KRT.Infrastructure.Data.Repositories;
using KRT.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace KRT.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        try
        {
            var redis = ConnectionMultiplexer.Connect(redisConnection);
            services.AddSingleton(redis);
            services.AddScoped<ICacheService, RedisCacheService>();
        }
        catch
        {
            services.AddScoped<ICacheService, NullCacheService>();
        }

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = typeof(AccountMapper).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }
}
