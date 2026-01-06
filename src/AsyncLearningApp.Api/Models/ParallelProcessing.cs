namespace AsyncLearningApp.Api.Models;

/// <summary>
/// Request for batch processing operation.
/// </summary>
public class BatchProcessingRequest
{
    public List<int> Items { get; set; } = new();
    public int MaxDegreeOfParallelism { get; set; } = 4;
}

/// <summary>
/// Result of a batch processing operation.
/// Shows performance comparison between sync and async.
/// </summary>
public class BatchProcessingResult
{
    public List<ProcessedItem> ProcessedItems { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
    public string ProcessingType { get; set; } = string.Empty;
    public int TotalItems { get; set; }
}

/// <summary>
/// Represents a single processed item.
/// </summary>
public class ProcessedItem
{
    public int Input { get; set; }
    public int Result { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}

/// <summary>
/// Comparison result between sync and async processing.
/// </summary>
public class PerformanceComparisonResult
{
    public BatchProcessingResult SyncResult { get; set; } = new();
    public BatchProcessingResult AsyncResult { get; set; } = new();
    public double SpeedupFactor { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}
