using AsyncLearningApp.Api.Models;
using System.Collections.Concurrent;

namespace AsyncLearningApp.Api.Services;

/// <summary>
/// Service for managing file processing jobs.
/// Demonstrates storing and tracking long-running async operations.
/// 
/// Uses ConcurrentDictionary for thread-safe access from multiple async operations.
/// </summary>
public interface IFileProcessingService
{
    Task<FileProcessingJob> StartProcessingAsync(FileUploadDto uploadDto, CancellationToken cancellationToken = default);
    Task<FileProcessingJob?> GetJobStatusAsync(string jobId);
    Task CancelJobAsync(string jobId);
}

public class FileProcessingService : IFileProcessingService
{
    // ConcurrentDictionary is thread-safe and can be accessed from multiple async operations
    private readonly ConcurrentDictionary<string, FileProcessingJob> _jobs = new();
    private readonly ILogger<FileProcessingService> _logger;

    public FileProcessingService(ILogger<FileProcessingService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Starts a new file processing job asynchronously.
    /// Demonstrates Task.Run for CPU-bound work and proper async/await usage.
    /// </summary>
    public async Task<FileProcessingJob> StartProcessingAsync(FileUploadDto uploadDto, CancellationToken cancellationToken = default)
    {
        var job = new FileProcessingJob
        {
            Id = Guid.NewGuid().ToString(),
            FileName = uploadDto.FileName,
            Status = ProcessingStatus.Pending,
            StartedAt = DateTime.UtcNow
        };

        _jobs.TryAdd(job.Id, job);
        _logger.LogInformation("Created file processing job {JobId} for file {FileName}", job.Id, job.FileName);

        // Start processing in background using Task.Run
        // This offloads the work to a thread pool thread
        _ = Task.Run(() => ProcessFileAsync(job, uploadDto.FileSizeKb, cancellationToken), cancellationToken);

        // Return immediately without waiting for processing to complete
        return job;
    }

    /// <summary>
    /// Retrieves the current status of a processing job.
    /// This is a pure data retrieval operation - no need for async in this simple case,
    /// but we use Task.FromResult to maintain async signature for consistency.
    /// </summary>
    public Task<FileProcessingJob?> GetJobStatusAsync(string jobId)
    {
        _jobs.TryGetValue(jobId, out var job);
        return Task.FromResult(job);
    }

    /// <summary>
    /// Cancels a running job.
    /// Demonstrates handling cancellation in async operations.
    /// </summary>
    public Task CancelJobAsync(string jobId)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            if (job.Status == ProcessingStatus.Processing || job.Status == ProcessingStatus.Pending)
            {
                job.Status = ProcessingStatus.Cancelled;
                job.CompletedAt = DateTime.UtcNow;
                _logger.LogInformation("Cancelled job {JobId}", jobId);
            }
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Simulates file processing with progress updates.
    /// 
    /// Key Learning Points:
    /// - Task.Delay simulates I/O-bound work (like reading/writing files)
    /// - Progress is updated incrementally to show intermediate states
    /// - CancellationToken is checked to allow operation cancellation
    /// - try-catch handles exceptions in async operations
    /// </summary>
    private async Task ProcessFileAsync(FileProcessingJob job, int fileSizeKb, CancellationToken cancellationToken)
    {
        try
        {
            job.Status = ProcessingStatus.Processing;
            _logger.LogInformation("Starting to process job {JobId}", job.Id);

            // Simulate processing in chunks with progress updates
            const int totalSteps = 10;
            for (int i = 0; i < totalSteps; i++)
            {
                // Check if cancellation was requested
                if (cancellationToken.IsCancellationRequested || job.Status == ProcessingStatus.Cancelled)
                {
                    job.Status = ProcessingStatus.Cancelled;
                    job.CompletedAt = DateTime.UtcNow;
                    _logger.LogInformation("Job {JobId} was cancelled", job.Id);
                    return;
                }

                // Simulate processing time based on file size
                var delayMs = fileSizeKb * 2; // Larger files take longer
                await Task.Delay(Math.Max(200, delayMs / totalSteps), cancellationToken);

                job.Progress = ((i + 1) * 100) / totalSteps;
                _logger.LogDebug("Job {JobId} progress: {Progress}%", job.Id, job.Progress);
            }

            // Mark as completed
            job.Status = ProcessingStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;
            job.Progress = 100;
            job.Result = $"Successfully processed {job.FileName}. File size: {fileSizeKb}KB. " +
                        $"Processing time: {(job.CompletedAt.Value - job.StartedAt).TotalSeconds:F2}s";

            _logger.LogInformation("Completed job {JobId}", job.Id);
        }
        catch (OperationCanceledException)
        {
            job.Status = ProcessingStatus.Cancelled;
            job.CompletedAt = DateTime.UtcNow;
            _logger.LogInformation("Job {JobId} was cancelled", job.Id);
        }
        catch (Exception ex)
        {
            job.Status = ProcessingStatus.Failed;
            job.CompletedAt = DateTime.UtcNow;
            job.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Job {JobId} failed", job.Id);
        }
    }
}
