using System.Text.Json.Serialization;

namespace NadMatcher.Infrastructure.JsonModels;

/// <summary>
/// JSON model for NAD module data file.
/// </summary>
public class NadModulesFileJson
{
    [JsonPropertyName("metadata")]
    public NadMetadataJson Metadata { get; set; } = new();

    [JsonPropertyName("manufacturers")]
    public List<string> Manufacturers { get; set; } = [];

    [JsonPropertyName("modules")]
    public List<NadModuleJson>? Modules { get; set; }

    [JsonPropertyName("modules_by_region")]
    public Dictionary<string, List<NadModuleJson>>? ModulesByRegion { get; set; }
}

public class NadMetadataJson
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("last_updated")]
    public string LastUpdated { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("total_modules")]
    public int TotalModules { get; set; }
}

public class NadModuleJson
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("manufacturer")]
    public string Manufacturer { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("technology")]
    public List<string> Technology { get; set; } = [];

    [JsonPropertyName("form_factor")]
    public string FormFactor { get; set; } = string.Empty;

    [JsonPropertyName("chipset")]
    public string Chipset { get; set; } = string.Empty;

    [JsonPropertyName("max_downlink_mbps")]
    public int MaxDownlinkMbps { get; set; }

    [JsonPropertyName("max_uplink_mbps")]
    public int MaxUplinkMbps { get; set; }

    [JsonPropertyName("bands")]
    public NadBandsJson Bands { get; set; } = new();

    [JsonPropertyName("features")]
    public List<string> Features { get; set; } = [];

    [JsonPropertyName("certifications")]
    public List<string> Certifications { get; set; } = [];

    [JsonPropertyName("target_region")]
    public string TargetRegion { get; set; } = string.Empty;

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;
}

public class NadBandsJson
{
    [JsonPropertyName("5G_NR")]
    public List<string> Nr5G { get; set; } = [];

    [JsonPropertyName("LTE")]
    public List<string> Lte { get; set; } = [];

    [JsonPropertyName("UMTS")]
    public List<string> Umts { get; set; } = [];

    [JsonPropertyName("GSM")]
    public List<string> Gsm { get; set; } = [];
}
