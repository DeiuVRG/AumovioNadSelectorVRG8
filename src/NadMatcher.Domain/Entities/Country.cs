namespace NadMatcher.Domain.Entities;

/// <summary>
/// Represents a country with its supported frequency bands.
/// </summary>
public class Country
{
    public string Name { get; init; } = string.Empty;
    public string IsoCode { get; init; } = string.Empty;
    public string Region { get; init; } = string.Empty;
    public CountryBands Bands { get; init; } = new();

    public HashSet<string> GetAllBands()
    {
        var allBands = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var band in Bands.Nr5G) allBands.Add(band.Band);
        foreach (var band in Bands.Lte) allBands.Add(band.Band);
        foreach (var band in Bands.Umts) allBands.Add(band.Band);
        foreach (var band in Bands.Gsm) allBands.Add(band.Band);

        return allBands;
    }

    public HashSet<string> GetLteBands() => Bands.Lte.Select(b => b.Band).ToHashSet(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> Get5GBands() => Bands.Nr5G.Select(b => b.Band).ToHashSet(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> GetUmtsBands() => Bands.Umts.Select(b => b.Band).ToHashSet(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> GetGsmBands() => Bands.Gsm.Select(b => b.Band).ToHashSet(StringComparer.OrdinalIgnoreCase);
}

public class CountryBands
{
    public List<BandInfo> Gsm { get; init; } = [];
    public List<BandInfo> Umts { get; init; } = [];
    public List<BandInfo> Lte { get; init; } = [];
    public List<BandInfo> Nr5G { get; init; } = [];
}

public class BandInfo
{
    public string Band { get; init; } = string.Empty;
    public int? FrequencyMhz { get; init; }
}
