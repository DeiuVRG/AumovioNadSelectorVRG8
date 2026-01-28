namespace NadMatcher.Domain.Entities;

/// <summary>
/// Result of matching a NAD module against countries or vice versa.
/// </summary>
public class MatchResult
{
    public string EntityId { get; init; } = string.Empty;
    public string EntityName { get; init; } = string.Empty;
    public double OverallMatchPercentage { get; init; }
    public TechnologyMatch GsmMatch { get; init; } = new();
    public TechnologyMatch UmtsMatch { get; init; } = new();
    public TechnologyMatch LteMatch { get; init; } = new();
    public TechnologyMatch Nr5GMatch { get; init; } = new();
    public List<string> MissingBands { get; init; } = [];
    public List<string> MatchedBands { get; init; } = [];
    public bool IsFullMatch => OverallMatchPercentage >= 100.0;
    public bool IsPartialMatch => OverallMatchPercentage >= 50.0 && OverallMatchPercentage < 100.0;
}

public class TechnologyMatch
{
    public double MatchPercentage { get; init; }
    public List<string> MatchedBands { get; init; } = [];
    public List<string> MissingBands { get; init; } = [];
    public int TotalRequired { get; init; }
    public int TotalMatched { get; init; }
    public bool IsExcluded { get; init; }

    /// <summary>
    /// Empty match result for excluded technologies.
    /// </summary>
    public static TechnologyMatch Empty => new()
    {
        MatchPercentage = 100.0,
        MatchedBands = [],
        MissingBands = [],
        TotalRequired = 0,
        TotalMatched = 0,
        IsExcluded = true
    };
}

/// <summary>
/// Aggregated result when matching multiple countries.
/// </summary>
public class AggregatedMatchResult
{
    public NadModule? Nad { get; init; }
    public List<CountryMatchDetail> CountryMatches { get; init; } = [];
    public double AverageMatchPercentage { get; init; }
    public int FullMatchCount { get; init; }
    public int PartialMatchCount { get; init; }
    public int NoMatchCount { get; init; }
    public List<string> AllMissingBands { get; init; } = [];
}

public class CountryMatchDetail
{
    public Country Country { get; init; } = null!;
    public MatchResult MatchResult { get; init; } = null!;
}

/// <summary>
/// Recommendation for NAD selection.
/// </summary>
public class NadRecommendation
{
    public NadModule Nad { get; init; } = null!;
    public double CoveragePercentage { get; init; }
    public List<string> CoveredCountries { get; init; } = [];
    public List<string> PartiallyCoveredCountries { get; init; } = [];
    public List<string> UncoveredCountries { get; init; } = [];
    public List<string> MissingBands { get; init; } = [];
    public string RecommendationReason { get; init; } = string.Empty;

    /// <summary>
    /// Detailed match information per country, showing exactly what bands are missing.
    /// </summary>
    public List<CountryMatchDetail> CountryDetails { get; init; } = [];
}
