using NadMatcher.Domain.Entities;

namespace NadMatcher.Domain.Interfaces;

/// <summary>
/// Technology filter options for matching.
/// </summary>
public class TechnologyFilter
{
    public bool IncludeGsm { get; init; } = true;
    public bool IncludeUmts { get; init; } = true;
    public bool IncludeLte { get; init; } = true;
    public bool Include5G { get; init; } = true;

    public static TechnologyFilter All => new();
}

/// <summary>
/// Service for generating NAD recommendations. (Single Responsibility Principle)
/// </summary>
public interface IRecommendationService
{
    /// <summary>
    /// Finds the best NAD modules for a set of countries.
    /// </summary>
    Task<IReadOnlyList<NadRecommendation>> GetRecommendationsForCountriesAsync(
        IEnumerable<Country> countries,
        int maxRecommendations = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the best NAD modules for a set of countries with technology filters.
    /// </summary>
    Task<IReadOnlyList<NadRecommendation>> GetRecommendationsForCountriesAsync(
        IEnumerable<Country> countries,
        TechnologyFilter filter,
        int maxRecommendations = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds NAD combinations that cover all specified countries.
    /// </summary>
    Task<IReadOnlyList<NadCombinationRecommendation>> GetNadCombinationsAsync(
        IEnumerable<Country> countries,
        int maxCombinations = 3,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds NAD combinations with technology filters.
    /// </summary>
    Task<IReadOnlyList<NadCombinationRecommendation>> GetNadCombinationsAsync(
        IEnumerable<Country> countries,
        TechnologyFilter filter,
        int maxCombinations = 3,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Recommendation for a combination of NADs to cover multiple regions.
/// </summary>
public class NadCombinationRecommendation
{
    public List<NadModule> Nads { get; init; } = [];
    public double TotalCoveragePercentage { get; init; }
    public List<string> CoveredCountries { get; init; } = [];
    public List<string> UncoveredCountries { get; init; } = [];
    public string CombinationReason { get; init; } = string.Empty;
}
