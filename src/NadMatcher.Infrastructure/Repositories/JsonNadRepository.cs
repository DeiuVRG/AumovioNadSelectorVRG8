using System.Reflection;
using System.Text.Json;
using NadMatcher.Domain.Entities;
using NadMatcher.Domain.Interfaces;
using NadMatcher.Infrastructure.JsonModels;

namespace NadMatcher.Infrastructure.Repositories;

/// <summary>
/// JSON-based repository for NAD modules.
/// Implements Dependency Inversion Principle - depends on abstraction (INadRepository).
/// </summary>
public class JsonNadRepository : INadRepository
{
    private readonly Lazy<Task<List<NadModule>>> _modulesLoader;
    private readonly Lazy<Task<List<string>>> _manufacturersLoader;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public JsonNadRepository()
    {
        _modulesLoader = new Lazy<Task<List<NadModule>>>(LoadModulesAsync);
        _manufacturersLoader = new Lazy<Task<List<string>>>(LoadManufacturersAsync);
    }

    public async Task<IReadOnlyList<NadModule>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _modulesLoader.Value;
    }

    public async Task<NadModule?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var modules = await _modulesLoader.Value;
        return modules.FirstOrDefault(m => m.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IReadOnlyList<NadModule>> GetByManufacturerAsync(string manufacturer, CancellationToken cancellationToken = default)
    {
        var modules = await _modulesLoader.Value;
        return modules
            .Where(m => m.Manufacturer.Equals(manufacturer, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<IReadOnlyList<NadModule>> GetByRegionAsync(string region, CancellationToken cancellationToken = default)
    {
        var modules = await _modulesLoader.Value;
        return modules
            .Where(m => m.TargetRegion.Equals(region, StringComparison.OrdinalIgnoreCase) ||
                        m.TargetRegion.Equals("Global", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<IReadOnlyList<NadModule>> GetBy5GSupportAsync(bool supports5G, CancellationToken cancellationToken = default)
    {
        var modules = await _modulesLoader.Value;
        return modules
            .Where(m => m.Supports5G == supports5G)
            .ToList();
    }

    public async Task<IReadOnlyList<string>> GetManufacturersAsync(CancellationToken cancellationToken = default)
    {
        return await _manufacturersLoader.Value;
    }

    private async Task<List<NadModule>> LoadModulesAsync()
    {
        var json = await LoadEmbeddedResourceAsync("NadMatcher.Infrastructure.Data.nad_modules.json");
        var fileData = JsonSerializer.Deserialize<NadModulesFileJson>(json, JsonOptions);

        if (fileData == null)
            return [];

        // Support modules_by_region structure (new format)
        if (fileData.ModulesByRegion is { Count: > 0 })
        {
            var modules = new List<NadModule>();
            foreach (var (region, regionModules) in fileData.ModulesByRegion)
            {
                foreach (var moduleJson in regionModules)
                {
                    // Set TargetRegion from the dictionary key if not already set
                    if (string.IsNullOrEmpty(moduleJson.TargetRegion))
                        moduleJson.TargetRegion = region;
                    modules.Add(MapToEntity(moduleJson));
                }
            }
            return modules;
        }

        // Fallback to flat modules array (old format)
        if (fileData.Modules is { Count: > 0 })
            return fileData.Modules.Select(MapToEntity).ToList();

        return [];
    }

    private async Task<List<string>> LoadManufacturersAsync()
    {
        var json = await LoadEmbeddedResourceAsync("NadMatcher.Infrastructure.Data.nad_modules.json");
        var fileData = JsonSerializer.Deserialize<NadModulesFileJson>(json, JsonOptions);

        return fileData?.Manufacturers ?? [];
    }

    private static async Task<string> LoadEmbeddedResourceAsync(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private static NadModule MapToEntity(NadModuleJson json)
    {
        return new NadModule
        {
            Id = json.Id,
            Manufacturer = json.Manufacturer,
            Name = json.Name,
            Category = json.Category,
            Technology = json.Technology,
            FormFactor = json.FormFactor,
            Chipset = json.Chipset,
            MaxDownlinkMbps = json.MaxDownlinkMbps,
            MaxUplinkMbps = json.MaxUplinkMbps,
            Bands = new NadBands
            {
                Nr5G = json.Bands.Nr5G,
                Lte = json.Bands.Lte,
                Umts = json.Bands.Umts,
                Gsm = json.Bands.Gsm
            },
            Features = json.Features,
            Certifications = json.Certifications,
            TargetRegion = json.TargetRegion,
            Notes = json.Notes
        };
    }
}
