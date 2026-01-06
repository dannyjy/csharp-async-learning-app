using AsyncLearningApp.Api.Models;

namespace AsyncLearningApp.Api.Services;

/// <summary>
/// Service for simulating external API calls.
/// Demonstrates various async I/O patterns and network operations.
/// 
/// Key Learning Points:
/// - Simulating network latency with Task.Delay
/// - Different async patterns: sequential, parallel, racing
/// - Error handling in async operations
/// - Retry logic with exponential backoff
/// </summary>
public interface IExternalApiService
{
    Task<ExternalApiData> FetchFromApiAsync(string source, int delayMs = 1000, CancellationToken cancellationToken = default);
    Task<ParallelOperationResult> FetchParallelAsync(List<string> sources, CancellationToken cancellationToken = default);
    Task<ParallelOperationResult> FetchSequentialAsync(List<string> sources, CancellationToken cancellationToken = default);
    Task<RaceOperationResult> FetchRaceAsync(List<string> sources, CancellationToken cancellationToken = default);
    Task<ExternalApiData> FetchWithRetryAsync(string source, int maxRetries = 3, CancellationToken cancellationToken = default);
}

public class ExternalApiService : IExternalApiService
{
    private readonly ILogger<ExternalApiService> _logger;
    private readonly Random _random = new();

    public ExternalApiService(ILogger<ExternalApiService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Simulates fetching data from an external API.
    /// 
    /// Key Learning Points:
    /// - Task.Delay simulates I/O-bound network operations
    /// - Async operations don't block the calling thread
    /// - CancellationToken allows operation cancellation
    /// </summary>
    public async Task<ExternalApiData> FetchFromApiAsync(string source, int delayMs = 1000, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching data from {Source} with {Delay}ms delay", source, delayMs);

        // Simulate network latency
        await Task.Delay(delayMs, cancellationToken);

        var data = new ExternalApiData
        {
            Source = source,
            Data = $"Data from {source} - Random value: {_random.Next(1000, 9999)}",
            FetchedAt = DateTime.UtcNow,
            DelayMs = delayMs
        };

        _logger.LogInformation("Successfully fetched data from {Source}", source);
        return data;
    }

    /// <summary>
    /// Fetches data from multiple sources in parallel using Task.WhenAll.
    /// 
    /// Key Learning Points:
    /// - Task.WhenAll runs multiple async operations concurrently
    /// - All tasks must complete before WhenAll returns
    /// - Much faster than sequential operations for I/O-bound work
    /// - If any task fails, WhenAll will throw, but other tasks continue
    /// </summary>
    public async Task<ParallelOperationResult> FetchParallelAsync(List<string> sources, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting parallel fetch from {Count} sources", sources.Count);
        var startTime = DateTime.UtcNow;

        // Create all tasks but don't await them yet
        var tasks = sources.Select(source => 
            FetchFromApiAsync(source, _random.Next(500, 2000), cancellationToken)
        ).ToList();

        // Wait for ALL tasks to complete in parallel
        var results = await Task.WhenAll(tasks);

        var totalTime = DateTime.UtcNow - startTime;
        _logger.LogInformation("Completed parallel fetch in {TotalSeconds}s", totalTime.TotalSeconds);

        return new ParallelOperationResult
        {
            Results = results.ToList(),
            TotalTime = totalTime,
            OperationType = "Parallel (Task.WhenAll)"
        };
    }

    /// <summary>
    /// Fetches data from multiple sources sequentially (one at a time).
    /// 
    /// Key Learning Points:
    /// - Sequential operations complete one after another
    /// - Each await blocks until the current operation completes
    /// - Slower than parallel but sometimes necessary (e.g., rate limiting, dependencies)
    /// - Good for demonstrating the performance difference vs parallel
    /// </summary>
    public async Task<ParallelOperationResult> FetchSequentialAsync(List<string> sources, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting sequential fetch from {Count} sources", sources.Count);
        var startTime = DateTime.UtcNow;
        var results = new List<ExternalApiData>();

        // Await each operation before starting the next
        foreach (var source in sources)
        {
            var result = await FetchFromApiAsync(source, _random.Next(500, 2000), cancellationToken);
            results.Add(result);
        }

        var totalTime = DateTime.UtcNow - startTime;
        _logger.LogInformation("Completed sequential fetch in {TotalSeconds}s", totalTime.TotalSeconds);

        return new ParallelOperationResult
        {
            Results = results,
            TotalTime = totalTime,
            OperationType = "Sequential (await in loop)"
        };
    }

    /// <summary>
    /// Demonstrates Task.WhenAny - returns as soon as ANY task completes.
    /// 
    /// Key Learning Points:
    /// - Task.WhenAny returns the first completed task
    /// - Useful for timeout patterns, racing operations, or redundant requests
    /// - Other tasks continue running unless explicitly cancelled
    /// - Good for scenarios where you only need the fastest response
    /// </summary>
    public async Task<RaceOperationResult> FetchRaceAsync(List<string> sources, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting race fetch from {Count} sources", sources.Count);
        var startTime = DateTime.UtcNow;

        // Create all tasks with random delays to simulate varying network conditions
        var tasks = sources.Select(source => 
            FetchFromApiAsync(source, _random.Next(500, 3000), cancellationToken)
        ).ToList();

        // Wait for the FIRST task to complete
        var completedTask = await Task.WhenAny(tasks);
        var winner = await completedTask;

        var totalTime = DateTime.UtcNow - startTime;
        _logger.LogInformation("Race completed in {TotalSeconds}s, winner: {Source}", 
            totalTime.TotalSeconds, winner.Source);

        return new RaceOperationResult
        {
            Winner = winner,
            TotalTime = totalTime,
            AllSources = sources
        };
    }

    /// <summary>
    /// Demonstrates retry logic with exponential backoff.
    /// 
    /// Key Learning Points:
    /// - Retry pattern for handling transient failures
    /// - Exponential backoff prevents overwhelming failing services
    /// - Exception handling in async methods
    /// - Pattern commonly used with external API calls
    /// </summary>
    public async Task<ExternalApiData> FetchWithRetryAsync(string source, int maxRetries = 3, CancellationToken cancellationToken = default)
    {
        int attempt = 0;
        int delayMs = 500;

        while (attempt < maxRetries)
        {
            try
            {
                _logger.LogInformation("Attempt {Attempt} to fetch from {Source}", attempt + 1, source);
                
                // Simulate occasional failures (30% chance)
                if (attempt < maxRetries - 1 && _random.Next(100) < 30)
                {
                    throw new HttpRequestException($"Simulated failure for {source}");
                }

                return await FetchFromApiAsync(source, 1000, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                attempt++;
                if (attempt >= maxRetries)
                {
                    _logger.LogError("All {MaxRetries} attempts failed for {Source}", maxRetries, source);
                    throw;
                }

                _logger.LogWarning("Attempt {Attempt} failed for {Source}: {Message}. Retrying in {Delay}ms...", 
                    attempt, source, ex.Message, delayMs);

                // Exponential backoff: wait longer after each failure
                await Task.Delay(delayMs, cancellationToken);
                delayMs *= 2; // Double the delay for next attempt
            }
        }

        throw new InvalidOperationException("This should never be reached");
    }
}
