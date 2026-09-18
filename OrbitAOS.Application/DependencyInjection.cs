using Microsoft.Extensions.DependencyInjection;
using OrbitAOS.Application.Interfaces;
using OrbitAOS.Application.Services;

namespace OrbitAOS.Application;

/// <summary>
/// Extension methods for registering Application layer services with the DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Application layer services into the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserProfileService, UserProfileService>();
        return services;
    }
}
