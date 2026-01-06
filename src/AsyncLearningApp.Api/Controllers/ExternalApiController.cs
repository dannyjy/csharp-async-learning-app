using AsyncLearningApp.Api.Models;
using AsyncLearningApp.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsyncLearningApp.Api.Controllers;

/// <summary>
/// Controller demonstrating various async I/O patterns.
/// 
/// Learning Objectives:
/// - Understand Task.WhenAll for parallel async operations
/// - Learn Task.WhenAny for racing operations
/// - Compare sequential vs parallel execution
/// - Implement retry logic with exponential backoff
/// - Handle cancellation in async operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExternalApiController : ControllerBase
{
    private readonly IExternalApiService _apiService;
    private readonly ILogger<ExternalApiController> _logger;

    public ExternalApiController(
        IExternalApiService apiService,
        ILogger<ExternalApiController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/externalapi/fetch - Fetch data from a single external API.
    /// 
    /// Key Learning Points:
    /// - Basic async I/O operation
    /// - Task.Delay simulates network latency
    /// - CancellationToken support for aborting requests
    /// </summary>
    [HttpGet("fetch")]
    public async Task<ActionResult<ExternalApiData>> FetchSingleSource(
        [FromQuery] string source = "API-1",
        [FromQuery] int delayMs = 1000,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching from single source: {Source}", source);

        try
        {
            var result = await _apiService.FetchFromApiAsync(source, delayMs, cancellationToken);
            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Request was cancelled");
            return StatusCode(499, new { message = "Request was cancelled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching from {Source}", source);
            return StatusCode(500, new { message = "Error fetching data", error = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/externalapi/parallel - Fetch from multiple APIs in parallel.
    /// 
    /// Key Learning Points:
    /// - Task.WhenAll executes multiple async operations concurrently
    /// - All operations run simultaneously (not sequentially)
    /// - Total time is roughly equal to the slowest operation
    /// - Much faster than sequential for I/O-bound operations
    /// - All tasks must complete before returning results
    /// </summary>
    [HttpGet("parallel")]
    public async Task<ActionResult<ParallelOperationResult>> FetchParallel(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting parallel fetch operation");

        var sources = new List<string> { "API-1", "API-2", "API-3", "API-4" };

        try
        {
            // This will fetch from all sources simultaneously
            var result = await _apiService.FetchParallelAsync(sources, cancellationToken);

            _logger.LogInformation("Parallel fetch completed in {TotalSeconds}s with {Count} results",
                result.TotalTime.TotalSeconds, result.Results.Count);

            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, new { message = "Request was cancelled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in parallel fetch");
            return StatusCode(500, new { message = "Error fetching data", error = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/externalapi/sequential - Fetch from multiple APIs sequentially.
    /// 
    /// Key Learning Points:
    /// - Sequential execution: one operation completes before the next starts
    /// - Total time is the sum of all individual operation times
    /// - Slower than parallel but sometimes necessary:
    ///   * Rate limiting requirements
    ///   * Dependencies between operations
    ///   * Resource constraints
    /// - Demonstrates the performance difference compared to parallel
    /// </summary>
    [HttpGet("sequential")]
    public async Task<ActionResult<ParallelOperationResult>> FetchSequential(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting sequential fetch operation");

        var sources = new List<string> { "API-1", "API-2", "API-3", "API-4" };

        try
        {
            // This will fetch from sources one at a time
            var result = await _apiService.FetchSequentialAsync(sources, cancellationToken);

            _logger.LogInformation("Sequential fetch completed in {TotalSeconds}s with {Count} results",
                result.TotalTime.TotalSeconds, result.Results.Count);

            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, new { message = "Request was cancelled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in sequential fetch");
            return StatusCode(500, new { message = "Error fetching data", error = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/externalapi/race - Race multiple API calls and return the fastest.
    /// 
    /// Key Learning Points:
    /// - Task.WhenAny returns as soon as ANY task completes
    /// - Useful patterns:
    ///   * Timeout implementations
    ///   * Redundant requests to multiple sources
    ///   * Getting the fastest response
    /// - Other tasks continue running unless explicitly cancelled
    /// - Only the first completed result is returned
    /// </summary>
    [HttpGet("race")]
    public async Task<ActionResult<RaceOperationResult>> FetchRace(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting race fetch operation");

        var sources = new List<string> { "API-1", "API-2", "API-3", "API-4", "API-5" };

        try
        {
            // Returns as soon as the first source responds
            var result = await _apiService.FetchRaceAsync(sources, cancellationToken);

            _logger.LogInformation("Race completed in {TotalSeconds}s, winner: {Source}",
                result.TotalTime.TotalSeconds, result.Winner.Source);

            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, new { message = "Request was cancelled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in race fetch");
            return StatusCode(500, new { message = "Error fetching data", error = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/externalapi/retry - Fetch with automatic retry logic.
    /// 
    /// Key Learning Points:
    /// - Retry pattern for handling transient failures
    /// - Exponential backoff: increasing delay between retries
    /// - Common pattern for unreliable external services
    /// - Exception handling across multiple retry attempts
    /// </summary>
    [HttpGet("retry")]
    public async Task<ActionResult<ExternalApiData>> FetchWithRetry(
        [FromQuery] string source = "Unreliable-API",
        [FromQuery] int maxRetries = 3,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching from {Source} with retry (max {MaxRetries} attempts)", source, maxRetries);

        try
        {
            var result = await _apiService.FetchWithRetryAsync(source, maxRetries, cancellationToken);
            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch after {MaxRetries} retries", maxRetries);
            return StatusCode(503, new
            {
                message = $"Service unavailable after {maxRetries} retries",
                error = ex.Message
            });
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, new { message = "Request was cancelled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in retry fetch");
            return StatusCode(500, new { message = "Error fetching data", error = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/externalapi/compare - Compare sequential vs parallel performance.
    /// 
    /// Key Learning Points:
    /// - Direct performance comparison between patterns
    /// - Shows the benefit of parallelization for I/O-bound operations
    /// - Demonstrates when to use each pattern
    /// </summary>
    [HttpGet("compare")]
    public async Task<ActionResult<object>> ComparePatterns(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Comparing sequential vs parallel fetch patterns");

        var sources = new List<string> { "API-1", "API-2", "API-3" };

        try
        {
            // Run both patterns and compare
            var sequentialResult = await _apiService.FetchSequentialAsync(sources, cancellationToken);
            var parallelResult = await _apiService.FetchParallelAsync(sources, cancellationToken);

            var comparison = new
            {
                sequential = new
                {
                    totalTimeSeconds = sequentialResult.TotalTime.TotalSeconds,
                    operationType = sequentialResult.OperationType,
                    resultCount = sequentialResult.Results.Count
                },
                parallel = new
                {
                    totalTimeSeconds = parallelResult.TotalTime.TotalSeconds,
                    operationType = parallelResult.OperationType,
                    resultCount = parallelResult.Results.Count
                },
                speedupFactor = sequentialResult.TotalTime.TotalSeconds / parallelResult.TotalTime.TotalSeconds,
                recommendation = parallelResult.TotalTime < sequentialResult.TotalTime
                    ? "Use parallel for I/O-bound operations when operations are independent"
                    : "Use sequential when operations have dependencies or rate limiting is required"
            };

            _logger.LogInformation("Comparison complete. Speedup factor: {SpeedupFactor:F2}x", comparison.speedupFactor);

            return Ok(comparison);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in pattern comparison");
            return StatusCode(500, new { message = "Error comparing patterns", error = ex.Message });
        }
    }
}
