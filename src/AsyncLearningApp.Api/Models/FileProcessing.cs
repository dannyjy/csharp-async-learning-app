namespace AsyncLearningApp.Api.Models;

/// <summary>
/// Represents the status of a file processing operation.
/// Demonstrates tracking long-running async operations.
/// </summary>
public class FileProcessingJob
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string FileName { get; set; } = string.Empty;
    
    public ProcessingStatus Status { get; set; }
    
    public int Progress { get; set; }
    
    public DateTime StartedAt { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    public string? Result { get; set; }
    
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Status enumeration for file processing.
/// </summary>
public enum ProcessingStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// DTO for file upload request.
/// </summary>
public class FileUploadDto
{
    public string FileName { get; set; } = string.Empty;
    public int FileSizeKb { get; set; }
}

/// <summary>
/// DTO for processing status response.
/// </summary>
public class ProcessingStatusDto
{
    public string Id { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public string? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan ElapsedTime { get; set; }
}
