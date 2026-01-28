using System.Diagnostics;
using NadMatcher.Application.Workflows.Base;
using NadMatcher.Domain.Entities;
using NadMatcher.Domain.Events;
using NadMatcher.Domain.Interfaces;

namespace NadMatcher.Application.Workflows;

/// <summary>
/// Input for NAD to Countries workflow.
/// </summary>
public class NadToCountriesInput
{
    public required NadModule SelectedNad { get; init; }
    public double MinimumMatchPercentage { get; init; } = 80.0;
}

/// <summary>
/// Output from NAD to Countries workflow.
/// </summary>
public class NadToCountriesOutput
{
    public required NadModule Nad { get; init; }
    public required IReadOnlyList<MatchResult> CompatibleCountries { get; init; }
    public required int TotalCountries { get; init; }
    public required int FullMatchCount { get; init; }
    public required int PartialMatchCount { get; init; }
}

/// <summary>
/// Workflow: Given a NAD, find all compatible countries.
/// Implements Transform and Event patterns.
/// </summary>
public class NadToCountriesWorkflow : IWorkflow<NadToCountriesInput, NadToCountriesOutput>
{
    private readonly IMatchingService _matchingService;
    private readonly ICountryRepository _countryRepository;

    public event EventHandler<WorkflowStepEvent>? OnStepExecuted;
    public event EventHandler<WorkflowCompletedEventArgs>? OnCompleted;

    public NadToCountriesWorkflow(
        IMatchingService matchingService,
        ICountryRepository countryRepository)
    {
        _matchingService = matchingService;
        _countryRepository = countryRepository;
    }

    public async Task<NadToCountriesOutput> ExecuteAsync(
        NadToCountriesInput input,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Step 1: Load all countries (Transform)
            RaiseStepEvent("LoadCountries", WorkflowStepStatus.Started, "Loading country data...");
            var countries = await _countryRepository.GetAllAsync(cancellationToken);
            RaiseStepEvent("LoadCountries", WorkflowStepStatus.Completed,
                $"Loaded {countries.Count} countries", countries);

            // Step 2: Match NAD against all countries (Transform)
            RaiseStepEvent("MatchBands", WorkflowStepStatus.Started,
                $"Matching {input.SelectedNad.Name} against countries...");

            var matches = new List<MatchResult>();
            foreach (var country in countries)
            {
                var match = _matchingService.MatchNadToCountry(input.SelectedNad, country);
                if (match.OverallMatchPercentage >= input.MinimumMatchPercentage)
                {
                    matches.Add(match);
                }
            }

            RaiseStepEvent("MatchBands", WorkflowStepStatus.Completed,
                $"Found {matches.Count} compatible countries", matches);

            // Step 3: Sort and transform results (Transform)
            RaiseStepEvent("TransformResults", WorkflowStepStatus.Started, "Sorting results...");
            var sortedMatches = matches
                .OrderByDescending(m => m.OverallMatchPercentage)
                .ToList();

            var output = new NadToCountriesOutput
            {
                Nad = input.SelectedNad,
                CompatibleCountries = sortedMatches,
                TotalCountries = countries.Count,
                FullMatchCount = sortedMatches.Count(m => m.IsFullMatch),
                PartialMatchCount = sortedMatches.Count(m => m.IsPartialMatch)
            };

            RaiseStepEvent("TransformResults", WorkflowStepStatus.Completed,
                "Results transformed", output);

            stopwatch.Stop();

            // Raise completion event
            OnCompleted?.Invoke(this, new WorkflowCompletedEventArgs
            {
                Success = true,
                Duration = stopwatch.Elapsed,
                Result = output
            });

            return output;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            OnCompleted?.Invoke(this, new WorkflowCompletedEventArgs
            {
                Success = false,
                Duration = stopwatch.Elapsed,
                ErrorMessage = ex.Message
            });

            throw;
        }
    }

    private void RaiseStepEvent(string stepName, WorkflowStepStatus status, string? message = null, object? data = null)
    {
        OnStepExecuted?.Invoke(this, new WorkflowStepEvent
        {
            StepName = stepName,
            Status = status,
            Message = message,
            Data = data
        });
    }
}
