using System;
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

public partial class NadSelectionViewModel : ViewModelBase
{
    private readonly INadRepository _nadRepository;
    private readonly NadToCountriesWorkflow _workflow;

    [ObservableProperty]
    private ObservableCollection<NadModule> _nadModules = [];

    [ObservableProperty]
    private ObservableCollection<string> _manufacturers = [];

    [ObservableProperty]
    private string? _selectedManufacturer;

    [ObservableProperty]
    private NadModule? _selectedNad;

    [ObservableProperty]
    private ObservableCollection<MatchResult> _matchResults = [];

    [ObservableProperty]
    private string _statusMessage = "Select a NAD module to see compatible countries.";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _summary = string.Empty;

    public NadSelectionViewModel(
        INadRepository nadRepository,
        NadToCountriesWorkflow workflow)
    {
        _nadRepository = nadRepository;
        _workflow = workflow;

        _workflow.OnStepExecuted += OnWorkflowStepExecuted;
        _workflow.OnCompleted += OnWorkflowCompleted;
    }

    public async Task LoadDataAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading NAD modules...";

        try
        {
            var manufacturers = await _nadRepository.GetManufacturersAsync();
            Manufacturers = new ObservableCollection<string>(manufacturers);

            var modules = await _nadRepository.GetAllAsync();
            NadModules = new ObservableCollection<NadModule>(modules);

            StatusMessage = $"Loaded {modules.Count} NAD modules. Select one to analyze.";
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

    partial void OnSelectedManufacturerChanged(string? value)
    {
        _ = FilterByManufacturerAsync(value);
    }

    private async Task FilterByManufacturerAsync(string? manufacturer)
    {
        if (string.IsNullOrEmpty(manufacturer))
        {
            var all = await _nadRepository.GetAllAsync();
            NadModules = new ObservableCollection<NadModule>(all);
        }
        else
        {
            var filtered = await _nadRepository.GetByManufacturerAsync(manufacturer);
            NadModules = new ObservableCollection<NadModule>(filtered);
        }
    }

    partial void OnSelectedNadChanged(NadModule? value)
    {
        if (value != null)
        {
            _ = AnalyzeNadAsync(value);
        }
    }

    [RelayCommand]
    private async Task AnalyzeNadAsync(NadModule nad)
    {
        IsLoading = true;
        MatchResults.Clear();
        Summary = string.Empty;

        try
        {
            var input = new NadToCountriesInput
            {
                SelectedNad = nad,
                MinimumMatchPercentage = 50.0
            };

            var output = await _workflow.ExecuteAsync(input);

            MatchResults = new ObservableCollection<MatchResult>(output.CompatibleCountries);
            Summary = $"Found {output.CompatibleCountries.Count} compatible countries out of {output.TotalCountries}. " +
                     $"Full matches: {output.FullMatchCount}, Partial matches: {output.PartialMatchCount}";
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
