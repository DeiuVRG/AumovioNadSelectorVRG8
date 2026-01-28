namespace NadMatcher.Domain.Entities;

/// <summary>
/// Represents a Network Access Device (NAD) module with its frequency band support.
/// </summary>
public class NadModule
{
    public string Id { get; init; } = string.Empty;
    public string Manufacturer { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public List<string> Technology { get; init; } = [];
    public string FormFactor { get; init; } = string.Empty;
    public string Chipset { get; init; } = string.Empty;
    public int MaxDownlinkMbps { get; init; }
    public int MaxUplinkMbps { get; init; }
    public NadBands Bands { get; init; } = new();
    public List<string> Features { get; init; } = [];
    public List<string> Certifications { get; init; } = [];
    public string TargetRegion { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;

    public bool Supports5G => Bands.Nr5G.Count > 0;
    public bool SupportsLte => Bands.Lte.Count > 0;
    public bool SupportsUmts => Bands.Umts.Count > 0;
    public bool SupportsGsm => Bands.Gsm.Count > 0;

    public HashSet<string> GetAllBands()
    {
        var allBands = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var band in Bands.Nr5G) allBands.Add(band);
        foreach (var band in Bands.Lte) allBands.Add(band);
        foreach (var band in Bands.Umts) allBands.Add(band);
        foreach (var band in Bands.Gsm) allBands.Add(band);

        return allBands;
    }
}

public class NadBands
{
    public List<string> Nr5G { get; init; } = [];
    public List<string> Lte { get; init; } = [];
    public List<string> Umts { get; init; } = [];
    public List<string> Gsm { get; init; } = [];
}
