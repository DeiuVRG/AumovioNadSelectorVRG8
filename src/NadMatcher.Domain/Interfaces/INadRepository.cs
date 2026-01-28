using NadMatcher.Domain.Entities;

namespace NadMatcher.Domain.Interfaces;

/// <summary>
/// Repository for NAD module data access. (Interface Segregation Principle)
/// </summary>
public interface INadRepository
{
    Task<IReadOnlyList<NadModule>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<NadModule?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NadModule>> GetByManufacturerAsync(string manufacturer, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NadModule>> GetByRegionAsync(string region, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NadModule>> GetBy5GSupportAsync(bool supports5G, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetManufacturersAsync(CancellationToken cancellationToken = default);
}
