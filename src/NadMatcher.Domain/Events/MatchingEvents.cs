using NadMatcher.Domain.Entities;

namespace NadMatcher.Domain.Events;

/// <summary>
/// Event raised when matching is completed.
/// </summary>
public class MatchingCompletedEvent : EventArgs
{
    public required string OperationType { get; init; }
    public required int TotalMatches { get; init; }
    public required int FullMatches { get; init; }
    public required int PartialMatches { get; init; }
    public required TimeSpan Duration { get; init; }
    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Event raised when recommendations are generated.
/// </summary>
public class RecommendationGeneratedEvent : EventArgs
{
    public required int RecommendationCount { get; init; }
    public required List<string> TopRecommendations { get; init; }
    public required double BestCoveragePercentage { get; init; }
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Event raised during workflow step execution (Transform event).
/// </summary>
public class WorkflowStepEvent : EventArgs
{
    public required string StepName { get; init; }
    public required WorkflowStepStatus Status { get; init; }
    public string? Message { get; init; }
    public object? Data { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public enum WorkflowStepStatus
{
    Started,
    InProgress,
    Completed,
    Failed
}
