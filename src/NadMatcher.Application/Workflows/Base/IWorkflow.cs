using NadMatcher.Domain.Events;

namespace NadMatcher.Application.Workflows.Base;

/// <summary>
/// Base interface for workflows. (Open/Closed Principle - extensible for new workflows)
/// </summary>
public interface IWorkflow<TInput, TOutput>
{
    /// <summary>
    /// Event raised when a workflow step transforms data.
    /// </summary>
    event EventHandler<WorkflowStepEvent>? OnStepExecuted;

    /// <summary>
    /// Event raised when the workflow completes.
    /// </summary>
    event EventHandler<WorkflowCompletedEventArgs>? OnCompleted;

    /// <summary>
    /// Executes the workflow with the given input.
    /// </summary>
    Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken = default);
}

public class WorkflowCompletedEventArgs : EventArgs
{
    public required bool Success { get; init; }
    public required TimeSpan Duration { get; init; }
    public string? ErrorMessage { get; init; }
    public object? Result { get; init; }
}
