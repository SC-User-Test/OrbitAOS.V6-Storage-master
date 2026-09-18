using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrbitAOS.Domain.Interfaces;
using OrbitAOS.Infrastructure.Data;
using OrbitAOS.Infrastructure.Repositories;

namespace OrbitAOS.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure layer services with the DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Infrastructure layer services (EF Core, repositories) into the
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration (for connection strings).</param>
    /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register EF Core DbContext with SQL Server provider (EF Core 8)
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Register generic and specific repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();

        return services;
    }
}
