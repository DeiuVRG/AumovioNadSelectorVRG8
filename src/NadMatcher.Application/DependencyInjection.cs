using Microsoft.Extensions.DependencyInjection;
using NadMatcher.Application.Services;
using NadMatcher.Application.Workflows;
using NadMatcher.Domain.Interfaces;

namespace NadMatcher.Application;

/// <summary>
/// Extension methods for registering Application layer services.
/// Follows Dependency Inversion Principle - registration of concrete implementations.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register services
        services.AddTransient<IMatchingService, MatchingService>();
        services.AddTransient<IRecommendationService, RecommendationService>();

        // Register workflows
        services.AddTransient<NadToCountriesWorkflow>();
        services.AddTransient<CountriesToNadWorkflow>();

        return services;
    }
}
