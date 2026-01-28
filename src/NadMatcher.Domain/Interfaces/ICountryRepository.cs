using NadMatcher.Domain.Entities;

namespace NadMatcher.Domain.Interfaces;

/// <summary>
/// Repository for country frequency band data access. (Interface Segregation Principle)
/// </summary>
public interface ICountryRepository
{
    Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Country?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Country?> GetByIsoCodeAsync(string isoCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Country>> GetByRegionAsync(string region, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Country>> GetBy5GSupportAsync(bool supports5G, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetRegionsAsync(CancellationToken cancellationToken = default);
}
