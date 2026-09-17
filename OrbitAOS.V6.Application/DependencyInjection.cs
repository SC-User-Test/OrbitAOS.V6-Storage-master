using Microsoft.Extensions.DependencyInjection;
using OrbitAOS.V6.Application.Interfaces;
using OrbitAOS.V6.Application.Services;

namespace OrbitAOS.V6.Application;

/// <summary>
/// Extension methods for registering Application layer services with the DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Application layer services.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOrbitalComponentService, OrbitalComponentService>();
        return services;
    }
}
