using System.Text.Json.Serialization;

namespace NadMatcher.Infrastructure.JsonModels;

/// <summary>
/// JSON model for country frequency bands data file.
/// </summary>
public class CountryBandsFileJson
{
    [JsonPropertyName("metadata")]
    public CountryMetadataJson Metadata { get; set; } = new();

    [JsonPropertyName("band_definitions")]
    public BandDefinitionsJson BandDefinitions { get; set; } = new();

    [JsonPropertyName("countries")]
    public List<CountryJson> Countries { get; set; } = [];
}

public class CountryMetadataJson
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("last_updated")]
    public string LastUpdated { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    [JsonPropertyName("total_countries")]
    public int TotalCountries { get; set; }
}

public class BandDefinitionsJson
{
    [JsonPropertyName("GSM")]
    public Dictionary<string, BandDefinitionJson> Gsm { get; set; } = [];

    [JsonPropertyName("UMTS")]
    public Dictionary<string, BandDefinitionJson> Umts { get; set; } = [];

    [JsonPropertyName("LTE")]
    public Dictionary<string, BandDefinitionJson> Lte { get; set; } = [];

    [JsonPropertyName("5G_NR")]
    public Dictionary<string, BandDefinitionJson> Nr5G { get; set; } = [];
}

public class BandDefinitionJson
{
    [JsonPropertyName("frequency_mhz")]
    public int FrequencyMhz { get; set; }

    [JsonPropertyName("uplink")]
    public string? Uplink { get; set; }

    [JsonPropertyName("downlink")]
    public string? Downlink { get; set; }
}

public class CountryJson
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("iso_code")]
    public string IsoCode { get; set; } = string.Empty;

    [JsonPropertyName("region")]
    public string Region { get; set; } = string.Empty;

    [JsonPropertyName("bands")]
    public CountryBandsDataJson Bands { get; set; } = new();
}

public class CountryBandsDataJson
{
    [JsonPropertyName("GSM")]
    public List<BandInfoJson> Gsm { get; set; } = [];

    [JsonPropertyName("UMTS")]
    public List<BandInfoJson> Umts { get; set; } = [];

    [JsonPropertyName("LTE")]
    public List<BandInfoJson> Lte { get; set; } = [];

    [JsonPropertyName("5G_NR")]
    public List<BandInfoJson> Nr5G { get; set; } = [];
}

public class BandInfoJson
{
    [JsonPropertyName("band")]
    public string Band { get; set; } = string.Empty;

    [JsonPropertyName("frequency_mhz")]
    public int? FrequencyMhz { get; set; }
}
