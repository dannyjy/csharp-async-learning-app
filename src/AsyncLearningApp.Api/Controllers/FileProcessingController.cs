using AsyncLearningApp.Api.Models;
using AsyncLearningApp.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsyncLearningApp.Api.Controllers;

/// <summary>
/// Controller demonstrating long-running async operations.
/// 
/// Learning Objectives:
/// - Understand Task.Run for CPU-bound operations
/// - Learn how to track progress of long-running operations
/// - Implement cancellation with CancellationToken
/// - Use Task.Delay to simulate I/O operations
/// - Pattern for background processing with status checking
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FileProcessingController : ControllerBase
{
    private readonly IFileProcessingService _processingService;
    private readonly ILogger<FileProcessingController> _logger;

    public FileProcessingController(
        IFileProcessingService processingService,
        ILogger<FileProcessingController> logger)
    {
        _processingService = processingService;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/fileprocessing/upload - Simulates file upload and starts processing.
    /// 
    /// Key Learning Points:
    /// - Immediate response pattern: start job and return immediately
    /// - Client polls for status using the returned job ID
    /// - Task.Run offloads work to background thread
    /// - Returns 202 Accepted with location header for status checking
    /// </summary>
    [HttpPost("upload")]
    public async Task<ActionResult<FileProcessingJob>> UploadFile([FromBody] FileUploadDto uploadDto)
    {
        _logger.LogInformation("Received file upload request for {FileName}", uploadDto.FileName);

        // Validate input
        if (string.IsNullOrWhiteSpace(uploadDto.FileName))
        {
            return BadRequest(new { message = "File name is required" });
        }

        if (uploadDto.FileSizeKb <= 0 || uploadDto.FileSizeKb > 10000)
        {
            return BadRequest(new { message = "File size must be between 1KB and 10000KB" });
        }

        // Start processing asynchronously - this returns immediately
        var job = await _processingService.StartProcessingAsync(uploadDto);

        _logger.LogInformation("Started processing job {JobId}", job.Id);

        // Return 202 Accepted with job information
        // Client should poll /status/{id} to check progress
        return AcceptedAtAction(
            nameof(GetProcessingStatus),
            new { id = job.Id },
            job);
    }

    /// <summary>
    /// GET /api/fileprocessing/status/{id} - Check processing status.
    /// 
    /// Key Learning Points:
    /// - Polling pattern for checking long-running operation status
    /// - Returning different status codes based on job state
    /// - Client repeatedly calls this endpoint to track progress
    /// </summary>
    [HttpGet("status/{id}")]
    public async Task<ActionResult<ProcessingStatusDto>> GetProcessingStatus(string id)
    {
        _logger.LogInformation("Checking status for job {JobId}", id);

        var job = await _processingService.GetJobStatusAsync(id);

        if (job == null)
        {
            return NotFound(new { message = $"Job with ID {id} not found" });
        }

        var statusDto = new ProcessingStatusDto
        {
            Id = job.Id,
            FileName = job.FileName,
            Status = job.Status.ToString(),
            Progress = job.Progress,
            Result = job.Result,
            ErrorMessage = job.ErrorMessage,
            ElapsedTime = job.CompletedAt.HasValue
                ? job.CompletedAt.Value - job.StartedAt
                : DateTime.UtcNow - job.StartedAt
        };

        // Return different status codes based on job state
        return job.Status switch
        {
            ProcessingStatus.Completed => Ok(statusDto),
            ProcessingStatus.Failed => StatusCode(500, statusDto),
            ProcessingStatus.Cancelled => StatusCode(499, statusDto), // Client Closed Request
            _ => StatusCode(202, statusDto) // Still processing
        };
    }

    /// <summary>
    /// GET /api/fileprocessing/result/{id} - Get processing results.
    /// 
    /// Key Learning Points:
    /// - Only return results when processing is complete
    /// - Different responses based on job status
    /// </summary>
    [HttpGet("result/{id}")]
    public async Task<ActionResult<object>> GetProcessingResult(string id)
    {
        _logger.LogInformation("Retrieving result for job {JobId}", id);

        var job = await _processingService.GetJobStatusAsync(id);

        if (job == null)
        {
            return NotFound(new { message = $"Job with ID {id} not found" });
        }

        if (job.Status != ProcessingStatus.Completed)
        {
            return BadRequest(new
            {
                message = $"Job is not completed yet. Current status: {job.Status}",
                status = job.Status.ToString(),
                progress = job.Progress
            });
        }

        return Ok(new
        {
            jobId = job.Id,
            fileName = job.FileName,
            result = job.Result,
            processingTime = (job.CompletedAt!.Value - job.StartedAt).TotalSeconds
        });
    }

    /// <summary>
    /// POST /api/fileprocessing/cancel/{id} - Cancel a processing job.
    /// 
    /// Key Learning Points:
    /// - Implementing cancellation in async operations
    /// - CancellationToken propagation
    /// - Graceful handling of cancellation requests
    /// </summary>
    [HttpPost("cancel/{id}")]
    public async Task<IActionResult> CancelProcessing(string id)
    {
        _logger.LogInformation("Cancellation requested for job {JobId}", id);

        var job = await _processingService.GetJobStatusAsync(id);

        if (job == null)
        {
            return NotFound(new { message = $"Job with ID {id} not found" });
        }

        if (job.Status == ProcessingStatus.Completed || job.Status == ProcessingStatus.Failed)
        {
            return BadRequest(new
            {
                message = $"Cannot cancel job in {job.Status} status"
            });
        }

        await _processingService.CancelJobAsync(id);

        return Ok(new { message = $"Job {id} cancellation requested" });
    }

    /// <summary>
    /// POST /api/fileprocessing/batch - Process multiple files in parallel.
    /// 
    /// Key Learning Points:
    /// - Starting multiple async operations simultaneously
    /// - Task.WhenAll to wait for all operations
    /// - Parallel processing of independent operations
    /// </summary>
    [HttpPost("batch")]
    public async Task<ActionResult<List<FileProcessingJob>>> UploadBatch([FromBody] List<FileUploadDto> uploads)
    {
        _logger.LogInformation("Received batch upload request for {Count} files", uploads.Count);

        if (uploads.Count == 0 || uploads.Count > 10)
        {
            return BadRequest(new { message = "Batch size must be between 1 and 10 files" });
        }

        // Start all processing jobs in parallel
        var jobTasks = uploads.Select(dto => _processingService.StartProcessingAsync(dto)).ToList();

        // Wait for all jobs to be created (not completed, just started)
        var jobs = await Task.WhenAll(jobTasks);

        _logger.LogInformation("Started {Count} processing jobs", jobs.Length);

        return Accepted(jobs);
    }
}
