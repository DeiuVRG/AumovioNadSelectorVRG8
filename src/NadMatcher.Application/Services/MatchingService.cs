using NadMatcher.Domain.Entities;
using NadMatcher.Domain.Interfaces;

namespace NadMatcher.Application.Services;

/// <summary>
/// Implementation of matching service. (Single Responsibility Principle)
/// </summary>
public class MatchingService : IMatchingService
{
    private readonly ICountryRepository _countryRepository;

    public MatchingService(ICountryRepository countryRepository)
    {
        _countryRepository = countryRepository;
    }

    public MatchResult MatchNadToCountry(NadModule nad, Country country)
    {
        return MatchNadToCountry(nad, country, TechnologyFilter.All);
    }

    public MatchResult MatchNadToCountry(NadModule nad, Country country, TechnologyFilter filter)
    {
        var gsmMatch = filter.IncludeGsm
            ? CalculateTechnologyMatch(
                nad.Bands.Gsm.ToHashSet(StringComparer.OrdinalIgnoreCase),
                country.GetGsmBands())
            : TechnologyMatch.Empty;

        var umtsMatch = filter.IncludeUmts
            ? CalculateTechnologyMatch(
                nad.Bands.Umts.ToHashSet(StringComparer.OrdinalIgnoreCase),
                country.GetUmtsBands())
            : TechnologyMatch.Empty;

        var lteMatch = filter.IncludeLte
            ? CalculateTechnologyMatch(
                nad.Bands.Lte.ToHashSet(StringComparer.OrdinalIgnoreCase),
                country.GetLteBands())
            : TechnologyMatch.Empty;

        var nr5gMatch = filter.Include5G
            ? CalculateTechnologyMatch(
                nad.Bands.Nr5G.ToHashSet(StringComparer.OrdinalIgnoreCase),
                country.Get5GBands())
            : TechnologyMatch.Empty;

        var overallMatch = CalculateOverallMatchFiltered(gsmMatch, umtsMatch, lteMatch, nr5gMatch, country, filter);

        var allMissing = new List<string>();
        if (filter.IncludeGsm) allMissing.AddRange(gsmMatch.MissingBands);
        if (filter.IncludeUmts) allMissing.AddRange(umtsMatch.MissingBands);
        if (filter.IncludeLte) allMissing.AddRange(lteMatch.MissingBands);
        if (filter.Include5G) allMissing.AddRange(nr5gMatch.MissingBands);

        var allMatched = new List<string>();
        if (filter.IncludeGsm) allMatched.AddRange(gsmMatch.MatchedBands);
        if (filter.IncludeUmts) allMatched.AddRange(umtsMatch.MatchedBands);
        if (filter.IncludeLte) allMatched.AddRange(lteMatch.MatchedBands);
        if (filter.Include5G) allMatched.AddRange(nr5gMatch.MatchedBands);

        return new MatchResult
        {
            EntityId = country.IsoCode,
            EntityName = country.Name,
            OverallMatchPercentage = overallMatch,
            GsmMatch = gsmMatch,
            UmtsMatch = umtsMatch,
            LteMatch = lteMatch,
            Nr5GMatch = nr5gMatch,
            MissingBands = allMissing,
            MatchedBands = allMatched
        };
    }

    public AggregatedMatchResult MatchNadToCountries(NadModule nad, IEnumerable<Country> countries)
    {
        return MatchNadToCountries(nad, countries, TechnologyFilter.All);
    }

    public AggregatedMatchResult MatchNadToCountries(NadModule nad, IEnumerable<Country> countries, TechnologyFilter filter)
    {
        var countryList = countries.ToList();
        var matches = countryList.Select(c => new CountryMatchDetail
        {
            Country = c,
            MatchResult = MatchNadToCountry(nad, c, filter)
        }).ToList();

        var fullMatches = matches.Count(m => m.MatchResult.IsFullMatch);
        var partialMatches = matches.Count(m => m.MatchResult.IsPartialMatch);
        var noMatches = matches.Count - fullMatches - partialMatches;

        var allMissing = matches
            .SelectMany(m => m.MatchResult.MissingBands)
            .Distinct()
            .ToList();

        return new AggregatedMatchResult
        {
            Nad = nad,
            CountryMatches = matches,
            AverageMatchPercentage = matches.Count > 0 ? matches.Average(m => m.MatchResult.OverallMatchPercentage) : 0,
            FullMatchCount = fullMatches,
            PartialMatchCount = partialMatches,
            NoMatchCount = noMatches,
            AllMissingBands = allMissing
        };
    }

    public async Task<IReadOnlyList<MatchResult>> FindCompatibleCountriesAsync(
        NadModule nad,
        double minimumMatchPercentage = 80.0,
        CancellationToken cancellationToken = default)
    {
        var countries = await _countryRepository.GetAllAsync(cancellationToken);

        return countries
            .Select(c => MatchNadToCountry(nad, c))
            .Where(m => m.OverallMatchPercentage >= minimumMatchPercentage)
            .OrderByDescending(m => m.OverallMatchPercentage)
            .ToList();
    }

    private static TechnologyMatch CalculateTechnologyMatch(
        HashSet<string> nadBands,
        HashSet<string> countryBands)
    {
        if (countryBands.Count == 0)
        {
            return new TechnologyMatch
            {
                MatchPercentage = 100.0, // No requirements = full match
                MatchedBands = [],
                MissingBands = [],
                TotalRequired = 0,
                TotalMatched = 0
            };
        }

        var matched = countryBands.Where(b => nadBands.Contains(b)).ToList();
        var missing = countryBands.Where(b => !nadBands.Contains(b)).ToList();

        return new TechnologyMatch
        {
            MatchPercentage = (double)matched.Count / countryBands.Count * 100.0,
            MatchedBands = matched,
            MissingBands = missing,
            TotalRequired = countryBands.Count,
            TotalMatched = matched.Count
        };
    }

    private static double CalculateOverallMatch(
        TechnologyMatch gsm,
        TechnologyMatch umts,
        TechnologyMatch lte,
        TechnologyMatch nr5g,
        Country country)
    {
        return CalculateOverallMatchFiltered(gsm, umts, lte, nr5g, country, TechnologyFilter.All);
    }

    private static double CalculateOverallMatchFiltered(
        TechnologyMatch gsm,
        TechnologyMatch umts,
        TechnologyMatch lte,
        TechnologyMatch nr5g,
        Country country,
        TechnologyFilter filter)
    {
        // Weight by importance: 5G > LTE > UMTS > GSM
        var weights = new Dictionary<string, double>
        {
            { "5G", 0.35 },
            { "LTE", 0.40 },
            { "UMTS", 0.15 },
            { "GSM", 0.10 }
        };

        double totalWeight = 0;
        double weightedSum = 0;

        if (filter.Include5G && country.Bands.Nr5G.Count > 0 && !nr5g.IsExcluded)
        {
            weightedSum += nr5g.MatchPercentage * weights["5G"];
            totalWeight += weights["5G"];
        }

        if (filter.IncludeLte && country.Bands.Lte.Count > 0 && !lte.IsExcluded)
        {
            weightedSum += lte.MatchPercentage * weights["LTE"];
            totalWeight += weights["LTE"];
        }

        if (filter.IncludeUmts && country.Bands.Umts.Count > 0 && !umts.IsExcluded)
        {
            weightedSum += umts.MatchPercentage * weights["UMTS"];
            totalWeight += weights["UMTS"];
        }

        if (filter.IncludeGsm && country.Bands.Gsm.Count > 0 && !gsm.IsExcluded)
        {
            weightedSum += gsm.MatchPercentage * weights["GSM"];
            totalWeight += weights["GSM"];
        }

        return totalWeight > 0 ? weightedSum / totalWeight : 0;
    }
}
