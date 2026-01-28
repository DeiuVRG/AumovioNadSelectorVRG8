using System.Reflection;
using System.Text.Json;
using NadMatcher.Domain.Entities;
using NadMatcher.Domain.Interfaces;
using NadMatcher.Infrastructure.JsonModels;

namespace NadMatcher.Infrastructure.Repositories;

/// <summary>
/// JSON-based repository for countries.
/// Implements Dependency Inversion Principle - depends on abstraction (ICountryRepository).
/// </summary>
public class JsonCountryRepository : ICountryRepository
{
    private readonly Lazy<Task<List<Country>>> _countriesLoader;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public JsonCountryRepository()
    {
        _countriesLoader = new Lazy<Task<List<Country>>>(LoadCountriesAsync);
    }

    public async Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _countriesLoader.Value;
    }

    public async Task<Country?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var countries = await _countriesLoader.Value;
        return countries.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Country?> GetByIsoCodeAsync(string isoCode, CancellationToken cancellationToken = default)
    {
        var countries = await _countriesLoader.Value;
        return countries.FirstOrDefault(c => c.IsoCode.Equals(isoCode, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IReadOnlyList<Country>> GetByRegionAsync(string region, CancellationToken cancellationToken = default)
    {
        var countries = await _countriesLoader.Value;
        return countries
            .Where(c => c.Region.Equals(region, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<IReadOnlyList<Country>> GetBy5GSupportAsync(bool supports5G, CancellationToken cancellationToken = default)
    {
        var countries = await _countriesLoader.Value;
        return countries
            .Where(c => (c.Bands.Nr5G.Count > 0) == supports5G)
            .ToList();
    }

    public async Task<IReadOnlyList<string>> GetRegionsAsync(CancellationToken cancellationToken = default)
    {
        var countries = await _countriesLoader.Value;
        return countries
            .Select(c => c.Region)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(r => r)
            .ToList();
    }

    private async Task<List<Country>> LoadCountriesAsync()
    {
        var json = await LoadEmbeddedResourceAsync("NadMatcher.Infrastructure.Data.country_frequency_bands.json");
        var fileData = JsonSerializer.Deserialize<CountryBandsFileJson>(json, JsonOptions);

        if (fileData?.Countries == null)
            return [];

        return fileData.Countries.Select(MapToEntity).ToList();
    }

    private static async Task<string> LoadEmbeddedResourceAsync(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private static Country MapToEntity(CountryJson json)
    {
        return new Country
        {
            Name = json.Name,
            IsoCode = json.IsoCode,
            Region = json.Region,
            Bands = new CountryBands
            {
                Nr5G = json.Bands.Nr5G.Select(b => new BandInfo
                {
                    Band = b.Band,
                    FrequencyMhz = b.FrequencyMhz
                }).ToList(),
                Lte = json.Bands.Lte.Select(b => new BandInfo
                {
                    Band = b.Band,
                    FrequencyMhz = b.FrequencyMhz
                }).ToList(),
                Umts = json.Bands.Umts.Select(b => new BandInfo
                {
                    Band = b.Band,
                    FrequencyMhz = b.FrequencyMhz
                }).ToList(),
                Gsm = json.Bands.Gsm.Select(b => new BandInfo
                {
                    Band = b.Band,
                    FrequencyMhz = b.FrequencyMhz
                }).ToList()
            }
        };
    }
}
