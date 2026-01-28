using System.Diagnostics;
using NadMatcher.Application.Workflows.Base;
using NadMatcher.Domain.Entities;
using NadMatcher.Domain.Events;
using NadMatcher.Domain.Interfaces;

namespace NadMatcher.Application.Workflows;

/// <summary>
/// Input for Countries to NAD workflow.
/// </summary>
public class CountriesToNadInput
{
    public required IReadOnlyList<Country> SelectedCountries { get; init; }
    public int MaxRecommendations { get; init; } = 5;

    // Technology filters
    public bool IncludeGsm { get; init; } = true;
    public bool IncludeUmts { get; init; } = true;
    public bool IncludeLte { get; init; } = true;
    public bool Include5G { get; init; } = true;
}

/// <summary>
/// Output from Countries to NAD workflow.
/// </summary>
public class CountriesToNadOutput
{
    public required IReadOnlyList<Country> SelectedCountries { get; init; }
    public required IReadOnlyList<NadRecommendation> Recommendations { get; init; }
    public required IReadOnlyList<NadCombinationRecommendation> CombinationRecommendations { get; init; }
    public required bool HasPerfectMatch { get; init; }
    public required string Summary { get; init; }
}

/// <summary>
/// Workflow: Given countries, find best NAD modules.
/// Implements Transform and Event patterns.
/// </summary>
public class CountriesToNadWorkflow : IWorkflow<CountriesToNadInput, CountriesToNadOutput>
{
    private readonly IRecommendationService _recommendationService;

    public event EventHandler<WorkflowStepEvent>? OnStepExecuted;
    public event EventHandler<WorkflowCompletedEventArgs>? OnCompleted;

    public CountriesToNadWorkflow(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    public async Task<CountriesToNadOutput> ExecuteAsync(
        CountriesToNadInput input,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Step 1: Validate input (Transform)
            RaiseStepEvent("ValidateInput", WorkflowStepStatus.Started,
                $"Validating {input.SelectedCountries.Count} selected countries...");

            if (input.SelectedCountries.Count == 0)
            {
                throw new ArgumentException("At least one country must be selected.");
            }

            RaiseStepEvent("ValidateInput", WorkflowStepStatus.Completed,
                $"Validated {input.SelectedCountries.Count} countries");

            // Create technology filter from input
            var filter = new TechnologyFilter
            {
                IncludeGsm = input.IncludeGsm,
                IncludeUmts = input.IncludeUmts,
                IncludeLte = input.IncludeLte,
                Include5G = input.Include5G
            };

            // Step 2: Get recommendations (Transform)
            var techs = new List<string>();
            if (input.IncludeGsm) techs.Add("GSM");
            if (input.IncludeUmts) techs.Add("UMTS");
            if (input.IncludeLte) techs.Add("LTE");
            if (input.Include5G) techs.Add("5G");

            RaiseStepEvent("GenerateRecommendations", WorkflowStepStatus.Started,
                $"Analyzing NAD compatibility for {string.Join(", ", techs)}...");

            var recommendations = await _recommendationService.GetRecommendationsForCountriesAsync(
                input.SelectedCountries,
                filter,
                input.MaxRecommendations,
                cancellationToken);

            RaiseStepEvent("GenerateRecommendations", WorkflowStepStatus.Completed,
                $"Generated {recommendations.Count} recommendations", recommendations);

            // Step 3: Check for perfect match or generate combinations (Transform)
            RaiseStepEvent("AnalyzeCoverage", WorkflowStepStatus.Started,
                "Analyzing coverage gaps...");

            var hasPerfectMatch = recommendations.Any(r =>
                r.CoveredCountries.Count == input.SelectedCountries.Count);

            var combinations = new List<NadCombinationRecommendation>();

            if (!hasPerfectMatch && input.SelectedCountries.Count > 1)
            {
                combinations = (await _recommendationService.GetNadCombinationsAsync(
                    input.SelectedCountries,
                    filter,
                    3,
                    cancellationToken)).ToList();
            }

            RaiseStepEvent("AnalyzeCoverage", WorkflowStepStatus.Completed,
                hasPerfectMatch ? "Found perfect match!" : $"Generated {combinations.Count} combination options");

            // Step 4: Generate summary (Transform)
            RaiseStepEvent("GenerateSummary", WorkflowStepStatus.Started,
                "Generating summary...");

            var summary = GenerateSummary(input.SelectedCountries, recommendations, hasPerfectMatch);

            var output = new CountriesToNadOutput
            {
                SelectedCountries = input.SelectedCountries,
                Recommendations = recommendations,
                CombinationRecommendations = combinations,
                HasPerfectMatch = hasPerfectMatch,
                Summary = summary
            };

            RaiseStepEvent("GenerateSummary", WorkflowStepStatus.Completed, summary);

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

    private static string GenerateSummary(
        IReadOnlyList<Country> countries,
        IReadOnlyList<NadRecommendation> recommendations,
        bool hasPerfectMatch)
    {
        if (hasPerfectMatch)
        {
            var best = recommendations.First();
            return $"Perfect match found! {best.Nad.Name} covers all {countries.Count} selected countries " +
                   $"with {best.CoveragePercentage:F1}% compatibility.";
        }

        if (recommendations.Count > 0)
        {
            var best = recommendations.First();
            return $"No single NAD covers all countries. Best option: {best.Nad.Name} " +
                   $"covers {best.CoveredCountries.Count}/{countries.Count} countries ({best.CoveragePercentage:F1}%). " +
                   $"Consider using NAD combinations for full coverage.";
        }

        return "No compatible NAD modules found for the selected countries.";
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
