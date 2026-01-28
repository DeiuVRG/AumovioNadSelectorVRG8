using NadMatcher.Domain.Entities;

namespace NadMatcher.Domain.Interfaces;

/// <summary>
/// Service for matching NADs to countries. (Single Responsibility Principle)
/// </summary>
public interface IMatchingService
{
    /// <summary>
    /// Matches a NAD module against a single country.
    /// </summary>
    MatchResult MatchNadToCountry(NadModule nad, Country country);

    /// <summary>
    /// Matches a NAD module against a single country with technology filters.
    /// </summary>
    MatchResult MatchNadToCountry(NadModule nad, Country country, TechnologyFilter filter);

    /// <summary>
    /// Matches a NAD module against multiple countries.
    /// </summary>
    AggregatedMatchResult MatchNadToCountries(NadModule nad, IEnumerable<Country> countries);

    /// <summary>
    /// Matches a NAD module against multiple countries with technology filters.
    /// </summary>
    AggregatedMatchResult MatchNadToCountries(NadModule nad, IEnumerable<Country> countries, TechnologyFilter filter);

    /// <summary>
    /// Finds all countries compatible with a NAD module.
    /// </summary>
    Task<IReadOnlyList<MatchResult>> FindCompatibleCountriesAsync(
        NadModule nad,
        double minimumMatchPercentage = 80.0,
        CancellationToken cancellationToken = default);
}
