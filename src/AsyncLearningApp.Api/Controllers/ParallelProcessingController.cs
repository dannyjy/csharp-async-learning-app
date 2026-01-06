using AsyncLearningApp.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AsyncLearningApp.Api.Controllers;

/// <summary>
/// Controller demonstrating parallel and concurrent async operations.
/// 
/// Learning Objectives:
/// - Understand Parallel.ForEachAsync for concurrent processing
/// - Compare sync vs async performance
/// - Learn about concurrency limits and throttling
/// - Implement batch processing with parallelism control
/// - Measure and report performance metrics
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ParallelProcessingController : ControllerBase
{
    private readonly ILogger<ParallelProcessingController> _logger;

    public ParallelProcessingController(ILogger<ParallelProcessingController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// POST /api/parallel/batch - Process a batch of items in parallel.
    /// 
    /// Key Learning Points:
    /// - Parallel.ForEachAsync (introduced in .NET 6) for async parallel operations
    /// - MaxDegreeOfParallelism controls concurrency level
    /// - Useful for processing collections with async operations
    /// - Better than Task.WhenAll for large collections (memory efficient)
    /// </summary>
    [HttpPost("batch")]
    public async Task<ActionResult<BatchProcessingResult>> ProcessBatch(
        [FromBody] BatchProcessingRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing batch of {Count} items with parallelism {Parallelism}",
            request.Items.Count, request.MaxDegreeOfParallelism);

        if (request.Items.Count == 0 || request.Items.Count > 100)
        {
            return BadRequest(new { message = "Batch size must be between 1 and 100 items" });
        }

        if (request.MaxDegreeOfParallelism <= 0 || request.MaxDegreeOfParallelism > 10)
        {
            return BadRequest(new { message = "MaxDegreeOfParallelism must be between 1 and 10" });
        }

        var stopwatch = Stopwatch.StartNew();
        var processedItems = new List<ProcessedItem>();
        var lockObject = new object();

        // Parallel.ForEachAsync processes items concurrently with controlled parallelism
        await Parallel.ForEachAsync(
            request.Items,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = request.MaxDegreeOfParallelism,
                CancellationToken = cancellationToken
            },
            async (item, ct) =>
            {
                var itemStopwatch = Stopwatch.StartNew();

                // Simulate async processing (e.g., API call, database operation)
                await Task.Delay(Random.Shared.Next(100, 500), ct);

                // Simulate some CPU-bound work
                var result = await Task.Run(() =>
                {
                    var sum = 0;
                    for (int i = 0; i < item * 1000; i++)
                    {
                        sum += i;
                    }
                    return sum % 1000;
                }, ct);

                itemStopwatch.Stop();

                var processedItem = new ProcessedItem
                {
                    Input = item,
                    Result = result,
                    ProcessingTime = itemStopwatch.Elapsed
                };

                // Thread-safe collection update
                lock (lockObject)
                {
                    processedItems.Add(processedItem);
                }

                _logger.LogDebug("Processed item {Item} in {Ms}ms", item, itemStopwatch.ElapsedMilliseconds);
            });

        stopwatch.Stop();

        var result = new BatchProcessingResult
        {
            ProcessedItems = processedItems.OrderBy(p => p.Input).ToList(),
            ProcessingTime = stopwatch.Elapsed,
            ProcessingType = $"Parallel (MaxDegree: {request.MaxDegreeOfParallelism})",
            TotalItems = request.Items.Count
        };

        _logger.LogInformation("Batch processing completed in {TotalSeconds}s", result.ProcessingTime.TotalSeconds);

        return Ok(result);
    }

    /// <summary>
    /// GET /api/parallel/compare - Compare synchronous vs asynchronous processing.
    /// 
    /// Key Learning Points:
    /// - Performance comparison between sync and async approaches
    /// - When async provides benefits (I/O-bound operations)
    /// - Measuring and comparing execution times
    /// - Understanding speedup factor
    /// </summary>
    [HttpGet("compare")]
    public async Task<ActionResult<PerformanceComparisonResult>> ComparePerformance(
        [FromQuery] int itemCount = 10,
        [FromQuery] int parallelism = 4,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Comparing sync vs async performance with {ItemCount} items", itemCount);

        if (itemCount <= 0 || itemCount > 20)
        {
            return BadRequest(new { message = "Item count must be between 1 and 20" });
        }

        var items = Enumerable.Range(1, itemCount).ToList();

        // Run synchronous processing
        var syncResult = await ProcessSynchronously(items);

        // Run asynchronous processing
        var asyncResult = await ProcessAsynchronously(items, parallelism, cancellationToken);

        var comparison = new PerformanceComparisonResult
        {
            SyncResult = syncResult,
            AsyncResult = asyncResult,
            SpeedupFactor = syncResult.ProcessingTime.TotalSeconds / asyncResult.ProcessingTime.TotalSeconds,
            Recommendation = syncResult.ProcessingTime > asyncResult.ProcessingTime
                ? $"Async is {syncResult.ProcessingTime.TotalSeconds / asyncResult.ProcessingTime.TotalSeconds:F2}x faster for this workload"
                : "Sync might be sufficient for this simple workload, but async is better for I/O-bound operations"
        };

        _logger.LogInformation("Performance comparison complete. Speedup: {SpeedupFactor:F2}x",
            comparison.SpeedupFactor);

        return Ok(comparison);
    }

    /// <summary>
    /// POST /api/parallel/throttled - Process with rate limiting/throttling.
    /// 
    /// Key Learning Points:
    /// - Implementing throttling in async operations
    /// - SemaphoreSlim for controlling concurrency
    /// - Rate limiting patterns
    /// - Preventing resource exhaustion
    /// </summary>
    [HttpPost("throttled")]
    public async Task<ActionResult<BatchProcessingResult>> ProcessThrottled(
        [FromBody] BatchProcessingRequest request,
        [FromQuery] int requestsPerSecond = 5,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing {Count} items with throttling: {RPS} requests/second",
            request.Items.Count, requestsPerSecond);

        if (request.Items.Count == 0 || request.Items.Count > 100)
        {
            return BadRequest(new { message = "Batch size must be between 1 and 100 items" });
        }

        var stopwatch = Stopwatch.StartNew();
        var processedItems = new List<ProcessedItem>();
        var delayBetweenRequests = TimeSpan.FromSeconds(1.0 / requestsPerSecond);

        // Use SemaphoreSlim to control rate
        using var semaphore = new SemaphoreSlim(1, 1);

        var tasks = request.Items.Select(async item =>
        {
            // Wait for semaphore (throttling)
            await semaphore.WaitAsync(cancellationToken);

            try
            {
                var itemStopwatch = Stopwatch.StartNew();

                // Simulate processing
                await Task.Delay(Random.Shared.Next(50, 200), cancellationToken);

                var result = item * 2;

                itemStopwatch.Stop();

                return new ProcessedItem
                {
                    Input = item,
                    Result = result,
                    ProcessingTime = itemStopwatch.Elapsed
                };
            }
            finally
            {
                // Release after delay (implements rate limiting)
                _ = Task.Run(async () =>
                {
                    await Task.Delay(delayBetweenRequests, cancellationToken);
                    semaphore.Release();
                }, cancellationToken);
            }
        });

        processedItems = (await Task.WhenAll(tasks)).ToList();
        stopwatch.Stop();

        var result = new BatchProcessingResult
        {
            ProcessedItems = processedItems.OrderBy(p => p.Input).ToList(),
            ProcessingTime = stopwatch.Elapsed,
            ProcessingType = $"Throttled ({requestsPerSecond} req/s)",
            TotalItems = request.Items.Count
        };

        _logger.LogInformation("Throttled processing completed in {TotalSeconds}s",
            result.ProcessingTime.TotalSeconds);

        return Ok(result);
    }

    #region Helper Methods

    /// <summary>
    /// Process items synchronously (blocking).
    /// </summary>
    private async Task<BatchProcessingResult> ProcessSynchronously(List<int> items)
    {
        _logger.LogInformation("Starting synchronous processing of {Count} items", items.Count);
        var stopwatch = Stopwatch.StartNew();
        var processedItems = new List<ProcessedItem>();

        foreach (var item in items)
        {
            var itemStopwatch = Stopwatch.StartNew();

            // Simulate synchronous processing (blocking)
            Thread.Sleep(Random.Shared.Next(100, 500));

            var result = 0;
            for (int i = 0; i < item * 1000; i++)
            {
                result += i;
            }
            result = result % 1000;

            itemStopwatch.Stop();

            processedItems.Add(new ProcessedItem
            {
                Input = item,
                Result = result,
                ProcessingTime = itemStopwatch.Elapsed
            });
        }

        stopwatch.Stop();

        return await Task.FromResult(new BatchProcessingResult
        {
            ProcessedItems = processedItems,
            ProcessingTime = stopwatch.Elapsed,
            ProcessingType = "Synchronous (Sequential)",
            TotalItems = items.Count
        });
    }

    /// <summary>
    /// Process items asynchronously in parallel.
    /// </summary>
    private async Task<BatchProcessingResult> ProcessAsynchronously(
        List<int> items,
        int parallelism,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting asynchronous processing of {Count} items", items.Count);
        var stopwatch = Stopwatch.StartNew();
        var processedItems = new List<ProcessedItem>();
        var lockObject = new object();

        await Parallel.ForEachAsync(
            items,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = parallelism,
                CancellationToken = cancellationToken
            },
            async (item, ct) =>
            {
                var itemStopwatch = Stopwatch.StartNew();

                await Task.Delay(Random.Shared.Next(100, 500), ct);

                var result = await Task.Run(() =>
                {
                    var sum = 0;
                    for (int i = 0; i < item * 1000; i++)
                    {
                        sum += i;
                    }
                    return sum % 1000;
                }, ct);

                itemStopwatch.Stop();

                lock (lockObject)
                {
                    processedItems.Add(new ProcessedItem
                    {
                        Input = item,
                        Result = result,
                        ProcessingTime = itemStopwatch.Elapsed
                    });
                }
            });

        stopwatch.Stop();

        return new BatchProcessingResult
        {
            ProcessedItems = processedItems.OrderBy(p => p.Input).ToList(),
            ProcessingTime = stopwatch.Elapsed,
            ProcessingType = $"Asynchronous (Parallel, MaxDegree: {parallelism})",
            TotalItems = items.Count
        };
    }

    #endregion
}
