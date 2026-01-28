namespace NadMatcher.Application.Workflows.Base;

/// <summary>
/// Context passed between workflow steps, containing shared data.
/// </summary>
public class WorkflowContext
{
    private readonly Dictionary<string, object> _data = new();

    public T? Get<T>(string key) where T : class
    {
        return _data.TryGetValue(key, out var value) ? value as T : null;
    }

    public void Set<T>(string key, T value) where T : class
    {
        _data[key] = value;
    }

    public bool Has(string key) => _data.ContainsKey(key);

    public void Remove(string key) => _data.Remove(key);

    public IReadOnlyDictionary<string, object> GetAll() => _data;
}

/// <summary>
/// Base interface for workflow steps. (Liskov Substitution Principle)
/// </summary>
public interface IWorkflowStep
{
    string Name { get; }
    int Order { get; }
    Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Base class for workflow steps with common functionality.
/// </summary>
public abstract class WorkflowStepBase : IWorkflowStep
{
    public abstract string Name { get; }
    public abstract int Order { get; }

    public abstract Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default);

    protected void ValidateContext(WorkflowContext context, params string[] requiredKeys)
    {
        foreach (var key in requiredKeys)
        {
            if (!context.Has(key))
            {
                throw new InvalidOperationException($"Required context key '{key}' not found.");
            }
        }
    }
}
