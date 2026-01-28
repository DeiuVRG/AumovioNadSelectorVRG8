using NadMatcher.Domain.Entities;
using NadMatcher.Domain.Interfaces;

namespace NadMatcher.Application.Services;

/// <summary>
/// Service for generating NAD recommendations. (Single Responsibility Principle)
/// </summary>
public class RecommendationService : IRecommendationService
{
    private readonly INadRepository _nadRepository;
    private readonly IMatchingService _matchingService;

    public RecommendationService(INadRepository nadRepository, IMatchingService matchingService)
    {
        _nadRepository = nadRepository;
        _matchingService = matchingService;
    }

    public async Task<IReadOnlyList<NadRecommendation>> GetRecommendationsForCountriesAsync(
        IEnumerable<Country> countries,
        int maxRecommendations = 5,
        CancellationToken cancellationToken = default)
    {
        return await GetRecommendationsForCountriesAsync(countries, TechnologyFilter.All, maxRecommendations, cancellationToken);
    }

    public async Task<IReadOnlyList<NadRecommendation>> GetRecommendationsForCountriesAsync(
        IEnumerable<Country> countries,
        TechnologyFilter filter,
        int maxRecommendations = 5,
        CancellationToken cancellationToken = default)
    {
        var countryList = countries.ToList();
        var allNads = await _nadRepository.GetAllAsync(cancellationToken);

        var recommendations = new List<NadRecommendation>();

        foreach (var nad in allNads)
        {
            var aggregatedResult = _matchingService.MatchNadToCountries(nad, countryList, filter);

            var covered = aggregatedResult.CountryMatches
                .Where(m => m.MatchResult.IsFullMatch)
                .Select(m => m.Country.Name)
                .ToList();

            var partial = aggregatedResult.CountryMatches
                .Where(m => m.MatchResult.IsPartialMatch)
                .Select(m => m.Country.Name)
                .ToList();

            var uncovered = aggregatedResult.CountryMatches
                .Where(m => !m.MatchResult.IsFullMatch && !m.MatchResult.IsPartialMatch)
                .Select(m => m.Country.Name)
                .ToList();

            recommendations.Add(new NadRecommendation
            {
                Nad = nad,
                CoveragePercentage = aggregatedResult.AverageMatchPercentage,
                CoveredCountries = covered,
                PartiallyCoveredCountries = partial,
                UncoveredCountries = uncovered,
                MissingBands = aggregatedResult.AllMissingBands,
                RecommendationReason = GenerateRecommendationReason(nad, aggregatedResult, filter),
                CountryDetails = aggregatedResult.CountryMatches
            });
        }

        return recommendations
            .OrderByDescending(r => r.CoveragePercentage)
            .ThenByDescending(r => r.CoveredCountries.Count)
            .Take(maxRecommendations)
            .ToList();
    }

    public async Task<IReadOnlyList<NadCombinationRecommendation>> GetNadCombinationsAsync(
        IEnumerable<Country> countries,
        int maxCombinations = 3,
        CancellationToken cancellationToken = default)
    {
        return await GetNadCombinationsAsync(countries, TechnologyFilter.All, maxCombinations, cancellationToken);
    }

    public async Task<IReadOnlyList<NadCombinationRecommendation>> GetNadCombinationsAsync(
        IEnumerable<Country> countries,
        TechnologyFilter filter,
        int maxCombinations = 3,
        CancellationToken cancellationToken = default)
    {
        var countryList = countries.ToList();
        var allNads = await _nadRepository.GetAllAsync(cancellationToken);

        // Group NADs by target region
        var europeanNads = allNads.Where(n =>
            n.TargetRegion.Contains("Europe", StringComparison.OrdinalIgnoreCase) ||
            n.TargetRegion.Contains("EU", StringComparison.OrdinalIgnoreCase)).ToList();

        var northAmericanNads = allNads.Where(n =>
            n.TargetRegion.Contains("North America", StringComparison.OrdinalIgnoreCase) ||
            n.TargetRegion.Contains("NA", StringComparison.OrdinalIgnoreCase)).ToList();

        var globalNads = allNads.Where(n =>
            n.TargetRegion.Contains("Global", StringComparison.OrdinalIgnoreCase) ||
            n.TargetRegion.Contains("ROW", StringComparison.OrdinalIgnoreCase)).ToList();

        var combinations = new List<NadCombinationRecommendation>();

        // Try single global NAD first
        foreach (var nad in globalNads.Take(3))
        {
            var result = EvaluateCombination([nad], countryList, filter);
            if (result.TotalCoveragePercentage > 70)
            {
                combinations.Add(result);
            }
        }

        // Try EU + NA combinations
        foreach (var euNad in europeanNads.Take(2))
        {
            foreach (var naNad in northAmericanNads.Take(2))
            {
                var result = EvaluateCombination([euNad, naNad], countryList, filter);
                combinations.Add(result);
            }
        }

        return combinations
            .OrderByDescending(c => c.TotalCoveragePercentage)
            .ThenBy(c => c.Nads.Count)
            .Take(maxCombinations)
            .ToList();
    }

    private NadCombinationRecommendation EvaluateCombination(List<NadModule> nads, List<Country> countries, TechnologyFilter filter)
    {
        var covered = new HashSet<string>();
        var uncovered = new HashSet<string>();

        foreach (var country in countries)
        {
            bool isCovered = false;

            foreach (var nad in nads)
            {
                var match = _matchingService.MatchNadToCountry(nad, country, filter);
                if (match.OverallMatchPercentage >= 80)
                {
                    isCovered = true;
                    break;
                }
            }

            if (isCovered)
                covered.Add(country.Name);
            else
                uncovered.Add(country.Name);
        }

        var coveragePercentage = countries.Count > 0
            ? (double)covered.Count / countries.Count * 100
            : 0;

        return new NadCombinationRecommendation
        {
            Nads = nads,
            TotalCoveragePercentage = coveragePercentage,
            CoveredCountries = covered.ToList(),
            UncoveredCountries = uncovered.ToList(),
            CombinationReason = $"Combination of {string.Join(" + ", nads.Select(n => n.Name))} " +
                               $"covers {covered.Count}/{countries.Count} countries"
        };
    }

    private static string GenerateRecommendationReason(NadModule nad, AggregatedMatchResult result, TechnologyFilter filter)
    {
        var reasons = new List<string>();

        if (filter.Include5G && nad.Supports5G)
            reasons.Add("5G support");

        if (result.FullMatchCount == result.CountryMatches.Count)
            reasons.Add("full coverage");
        else if (result.AverageMatchPercentage >= 80)
            reasons.Add("high compatibility");

        if (nad.TargetRegion.Contains("Global", StringComparison.OrdinalIgnoreCase))
            reasons.Add("global bands");

        if (result.AllMissingBands.Count > 0 && result.AverageMatchPercentage >= 90)
            reasons.Add($"missing: {string.Join(", ", result.AllMissingBands.Take(3))}");

        return reasons.Count > 0
            ? string.Join(", ", reasons)
            : "standard compatibility";
    }
}
