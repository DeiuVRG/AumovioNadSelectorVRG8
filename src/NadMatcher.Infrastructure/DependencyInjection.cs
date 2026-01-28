using Microsoft.Extensions.DependencyInjection;
using NadMatcher.Domain.Interfaces;
using NadMatcher.Infrastructure.Repositories;

namespace NadMatcher.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure layer services.
/// Follows Dependency Inversion Principle - registration of concrete implementations.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Register repositories as singletons (data is loaded once from embedded resources)
        services.AddSingleton<INadRepository, JsonNadRepository>();
        services.AddSingleton<ICountryRepository, JsonCountryRepository>();

        return services;
    }
}
