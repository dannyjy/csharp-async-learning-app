namespace AsyncLearningApp.Api.Models;

/// <summary>
/// Represents data fetched from an external API.
/// Used to demonstrate various async I/O patterns.
/// </summary>
public class ExternalApiData
{
    public string Source { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public DateTime FetchedAt { get; set; }
    public int DelayMs { get; set; }
}

/// <summary>
/// Response for parallel operations showing performance metrics.
/// </summary>
public class ParallelOperationResult
{
    public List<ExternalApiData> Results { get; set; } = new();
    public TimeSpan TotalTime { get; set; }
    public string OperationType { get; set; } = string.Empty;
}

/// <summary>
/// Result of a race condition demonstration.
/// </summary>
public class RaceOperationResult
{
    public ExternalApiData Winner { get; set; } = new();
    public TimeSpan TotalTime { get; set; }
    public List<string> AllSources { get; set; } = new();
}
