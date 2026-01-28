using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NadMatcher.Application.Workflows;
using NadMatcher.Application.Workflows.Base;
using NadMatcher.Domain.Entities;
using NadMatcher.Domain.Events;
using NadMatcher.Domain.Interfaces;

namespace NadMatcher.UI.ViewModels;

public partial class CountrySelectionViewModel : ViewModelBase
{
    private readonly ICountryRepository _countryRepository;
    private readonly CountriesToNadWorkflow _workflow;

    [ObservableProperty]
    private ObservableCollection<CountryItem> _countries = [];

    [ObservableProperty]
    private ObservableCollection<string> _regions = [];

    [ObservableProperty]
    private string? _selectedRegion;

    [ObservableProperty]
    private ObservableCollection<NadRecommendationItem> _recommendations = [];

    [ObservableProperty]
    private ObservableCollection<NadCombinationRecommendation> _combinationRecommendations = [];

    [ObservableProperty]
    private string _statusMessage = "Select countries to find compatible NAD modules.";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _summary = string.Empty;

    [ObservableProperty]
    private bool _hasPerfectMatch;

    [ObservableProperty]
    private int _selectedCount;

    // Technology filters
    [ObservableProperty]
    private bool _includeGsm = true;

    [ObservableProperty]
    private bool _includeUmts = true;

    [ObservableProperty]
    private bool _includeLte = true;

    [ObservableProperty]
    private bool _include5G = true;

    public CountrySelectionViewModel(
        ICountryRepository countryRepository,
        CountriesToNadWorkflow workflow)
    {
        _countryRepository = countryRepository;
        _workflow = workflow;

        _workflow.OnStepExecuted += OnWorkflowStepExecuted;
        _workflow.OnCompleted += OnWorkflowCompleted;
    }

    public async Task LoadDataAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading countries...";

        try
        {
            var regions = await _countryRepository.GetRegionsAsync();
            Regions = new ObservableCollection<string>(["All Regions", .. regions]);

            var countries = await _countryRepository.GetAllAsync();
            Countries = new ObservableCollection<CountryItem>(
                countries.OrderBy(c => c.Name).Select(c => new CountryItem(c, OnCountrySelectionChanged)));

            StatusMessage = $"Loaded {countries.Count} countries. Select countries and click Analyze.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading data: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedRegionChanged(string? value)
    {
        _ = FilterByRegionAsync(value);
    }

    private async Task FilterByRegionAsync(string? region)
    {
        IReadOnlyList<Country> filtered;

        if (string.IsNullOrEmpty(region) || region == "All Regions")
        {
            filtered = await _countryRepository.GetAllAsync();
        }
        else
        {
            filtered = await _countryRepository.GetByRegionAsync(region);
        }

        // Preserve selection state when filtering
        var selectedNames = Countries.Where(c => c.IsSelected).Select(c => c.Country.Name).ToHashSet();

        Countries = new ObservableCollection<CountryItem>(
            filtered.OrderBy(c => c.Name).Select(c => new CountryItem(c, OnCountrySelectionChanged)
            {
                IsSelected = selectedNames.Contains(c.Name)
            }));

        UpdateSelectedCount();
    }

    private void OnCountrySelectionChanged()
    {
        UpdateSelectedCount();
    }

    private void UpdateSelectedCount()
    {
        SelectedCount = Countries.Count(c => c.IsSelected);
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var country in Countries)
        {
            country.IsSelected = true;
        }
        UpdateSelectedCount();
    }

    [RelayCommand]
    private void ClearSelection()
    {
        foreach (var country in Countries)
        {
            country.IsSelected = false;
        }
        UpdateSelectedCount();
        Recommendations.Clear();
        CombinationRecommendations.Clear();
        Summary = string.Empty;
    }

    [RelayCommand]
    private async Task AnalyzeAsync()
    {
        var selectedCountries = Countries.Where(c => c.IsSelected).Select(c => c.Country).ToList();

        if (selectedCountries.Count == 0)
        {
            StatusMessage = "Please select at least one country.";
            return;
        }

        if (!IncludeGsm && !IncludeUmts && !IncludeLte && !Include5G)
        {
            StatusMessage = "Please select at least one technology filter.";
            return;
        }

        IsLoading = true;
        Recommendations.Clear();
        CombinationRecommendations.Clear();
        Summary = string.Empty;

        try
        {
            var input = new CountriesToNadInput
            {
                SelectedCountries = selectedCountries,
                MaxRecommendations = 10,
                IncludeGsm = IncludeGsm,
                IncludeUmts = IncludeUmts,
                IncludeLte = IncludeLte,
                Include5G = Include5G
            };

            var output = await _workflow.ExecuteAsync(input);

            Recommendations = new ObservableCollection<NadRecommendationItem>(
                output.Recommendations.Select(r => new NadRecommendationItem(r)));
            CombinationRecommendations = new ObservableCollection<NadCombinationRecommendation>(output.CombinationRecommendations);
            HasPerfectMatch = output.HasPerfectMatch;
            Summary = output.Summary;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnWorkflowStepExecuted(object? sender, WorkflowStepEvent e)
    {
        StatusMessage = $"[{e.StepName}] {e.Message}";
    }

    private void OnWorkflowCompleted(object? sender, WorkflowCompletedEventArgs e)
    {
        if (e.Success)
        {
            StatusMessage = $"Analysis completed in {e.Duration.TotalMilliseconds:F0}ms";
        }
        else
        {
            StatusMessage = $"Analysis failed: {e.ErrorMessage}";
        }
    }
}

public partial class CountryItem : ObservableObject
{
    private readonly Action? _onSelectionChanged;

    public Country Country { get; }

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isExpanded;

    public string DisplayName => $"{Country.Name} ({Country.IsoCode})";
    public string Region => Country.Region;
    public bool Has5G => Country.Bands.Nr5G.Count > 0;
    public bool HasLte => Country.Bands.Lte.Count > 0;
    public bool HasUmts => Country.Bands.Umts.Count > 0;
    public bool HasGsm => Country.Bands.Gsm.Count > 0;
    public int BandCount => Country.GetAllBands().Count;

    // Band details for expanded view
    public string GsmBands => Country.Bands.Gsm.Count > 0
        ? string.Join(", ", Country.Bands.Gsm.Select(FormatCountryBand))
        : "None";

    public string UmtsBands => Country.Bands.Umts.Count > 0
        ? string.Join(", ", Country.Bands.Umts.Select(FormatCountryBand))
        : "None";

    public string LteBands => Country.Bands.Lte.Count > 0
        ? string.Join(", ", Country.Bands.Lte.Select(FormatCountryBand))
        : "None";

    public string Nr5GBands => Country.Bands.Nr5G.Count > 0
        ? string.Join(", ", Country.Bands.Nr5G.Select(FormatCountryBand))
        : "None";

    private static string FormatCountryBand(BandInfo b)
    {
        // Use the country's own frequency if available, otherwise use the global map
        if (b.FrequencyMhz.HasValue)
            return $"{b.Band} ({b.FrequencyMhz} MHz)";
        return BandFrequencyMap.FormatBandWithFrequency(b.Band);
    }

    public int GsmCount => Country.Bands.Gsm.Count;
    public int UmtsCount => Country.Bands.Umts.Count;
    public int LteCount => Country.Bands.Lte.Count;
    public int Nr5GCount => Country.Bands.Nr5G.Count;

    public CountryItem(Country country, Action? onSelectionChanged = null)
    {
        Country = country;
        _onSelectionChanged = onSelectionChanged;
    }

    partial void OnIsSelectedChanged(bool value)
    {
        _onSelectionChanged?.Invoke();
    }

    [RelayCommand]
    private void ToggleExpand()
    {
        IsExpanded = !IsExpanded;
    }
}

public partial class NadRecommendationItem : ObservableObject
{
    public NadRecommendation Recommendation { get; }

    [ObservableProperty]
    private bool _isExpanded;

    public NadModule Nad => Recommendation.Nad;
    public double CoveragePercentage => Recommendation.CoveragePercentage;
    public List<string> CoveredCountries => Recommendation.CoveredCountries;
    public List<string> PartiallyCoveredCountries => Recommendation.PartiallyCoveredCountries;
    public List<string> UncoveredCountries => Recommendation.UncoveredCountries;
    public string RecommendationReason => Recommendation.RecommendationReason;

    // NAD details for expanded view
    public string Category => Nad.Category;
    public string FormFactor => Nad.FormFactor;
    public string Chipset => Nad.Chipset;
    public int MaxDownlink => Nad.MaxDownlinkMbps;
    public int MaxUplink => Nad.MaxUplinkMbps;
    public string TargetRegion => Nad.TargetRegion;

    public string GsmBands => Nad.Bands.Gsm.Count > 0
        ? BandFrequencyMap.FormatBandsWithFrequencies(Nad.Bands.Gsm)
        : "None";

    public string UmtsBands => Nad.Bands.Umts.Count > 0
        ? BandFrequencyMap.FormatBandsWithFrequencies(Nad.Bands.Umts)
        : "None";

    public string LteBands => Nad.Bands.Lte.Count > 0
        ? BandFrequencyMap.FormatBandsWithFrequencies(Nad.Bands.Lte)
        : "None";

    public string Nr5GBands => Nad.Bands.Nr5G.Count > 0
        ? BandFrequencyMap.FormatBandsWithFrequencies(Nad.Bands.Nr5G)
        : "None";

    public bool Supports5G => Nad.Supports5G;
    public bool SupportsLte => Nad.SupportsLte;
    public bool SupportsUmts => Nad.SupportsUmts;
    public bool SupportsGsm => Nad.SupportsGsm;

    public string Technologies => string.Join(", ", Nad.Technology);
    public string Features => string.Join(", ", Nad.Features);
    public string Certifications => string.Join(", ", Nad.Certifications);

    // Missing bands info (for partial matches)
    public bool HasMissingInfo => CoveragePercentage < 100;
    public string MissingCountriesInfo => UncoveredCountries.Count > 0
        ? $"Not covered: {string.Join(", ", UncoveredCountries)}"
        : string.Empty;
    public string PartialCountriesInfo => PartiallyCoveredCountries.Count > 0
        ? $"Partially covered: {string.Join(", ", PartiallyCoveredCountries)}"
        : string.Empty;

    // Detailed missing bands per country
    public List<CountryMissingBandsInfo> CountryMissingBands => Recommendation.CountryDetails
        .Where(cd => cd.MatchResult.MissingBands.Count > 0)
        .Select(cd => new CountryMissingBandsInfo
        {
            CountryName = cd.Country.Name,
            MatchPercentage = cd.MatchResult.OverallMatchPercentage,
            MissingBands = BandFrequencyMap.FormatBandsWithFrequencies(cd.MatchResult.MissingBands),
            MissingLteBands = BandFrequencyMap.FormatBandsWithFrequencies(cd.MatchResult.LteMatch.MissingBands),
            MissingUmtsBands = BandFrequencyMap.FormatBandsWithFrequencies(cd.MatchResult.UmtsMatch.MissingBands),
            MissingGsmBands = BandFrequencyMap.FormatBandsWithFrequencies(cd.MatchResult.GsmMatch.MissingBands),
            Missing5GBands = BandFrequencyMap.FormatBandsWithFrequencies(cd.MatchResult.Nr5GMatch.MissingBands)
        })
        .ToList();

    public bool HasCountryDetails => CountryMissingBands.Count > 0;

    public NadRecommendationItem(NadRecommendation recommendation)
    {
        Recommendation = recommendation;
    }

    [RelayCommand]
    private void ToggleExpand()
    {
        IsExpanded = !IsExpanded;
    }
}

/// <summary>
/// Information about missing bands for a specific country.
/// </summary>
public class CountryMissingBandsInfo
{
    public string CountryName { get; init; } = string.Empty;
    public double MatchPercentage { get; init; }
    public string MissingBands { get; init; } = string.Empty;
    public string MissingLteBands { get; init; } = string.Empty;
    public string MissingUmtsBands { get; init; } = string.Empty;
    public string MissingGsmBands { get; init; } = string.Empty;
    public string Missing5GBands { get; init; } = string.Empty;

    public bool HasMissingLte => !string.IsNullOrEmpty(MissingLteBands);
    public bool HasMissingUmts => !string.IsNullOrEmpty(MissingUmtsBands);
    public bool HasMissingGsm => !string.IsNullOrEmpty(MissingGsmBands);
    public bool HasMissing5G => !string.IsNullOrEmpty(Missing5GBands);
    public bool IsPartial => MatchPercentage >= 50 && MatchPercentage < 100;
    public bool IsNotCovered => MatchPercentage < 50;
}
