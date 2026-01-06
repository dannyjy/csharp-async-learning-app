# C# Async/Await Learning Guide

This guide provides detailed explanations of asynchronous programming concepts demonstrated in this application.

## Table of Contents

1. [Introduction to Async/Await](#introduction-to-asyncawait)
2. [Module 1: Task Manager - Basic Async CRUD](#module-1-task-manager)
3. [Module 2: File Processing - Long-Running Operations](#module-2-file-processing)
4. [Module 3: External API - Async I/O Patterns](#module-3-external-api)
5. [Module 4: Notifications - Real-Time Events](#module-4-notifications)
6. [Module 5: Parallel Processing - Concurrency](#module-5-parallel-processing)
7. [Common Pitfalls and Best Practices](#common-pitfalls)
8. [Exercises](#exercises)

---

## Introduction to Async/Await

### What is Asynchronous Programming?

Asynchronous programming allows a program to perform operations without blocking the execution thread. This is especially important for:
- **I/O-bound operations**: Database queries, file operations, network requests
- **Long-running computations**: Complex calculations that take time
- **Responsive applications**: Keeping UIs responsive while work happens in background

### The Problem with Synchronous Code

```csharp
// Synchronous - BLOCKS the thread
public List<Task> GetAllTasks()
{
    return _context.Tasks.ToList(); // Thread waits here
}
```

When this executes, the thread is blocked until the database responds. In a web server handling many requests, this wastes resources.

### The Async/Await Solution

```csharp
// Asynchronous - RELEASES the thread
public async Task<List<TaskItem>> GetAllTasks()
{
    return await _context.Tasks.ToListAsync(); // Thread is freed
}
```

With `async`/`await`, the thread is released back to the thread pool while waiting, allowing it to handle other requests.

### Key Concepts

- **`async`**: Modifier that enables the use of `await` in a method
- **`await`**: Operator that asynchronously waits for a Task to complete
- **`Task`**: Represents an asynchronous operation that returns no value
- **`Task<T>`**: Represents an asynchronous operation that returns a value of type T

---

## Module 1: Task Manager

### Concepts Demonstrated

1. **Async CRUD Operations**
2. **Entity Framework Core Async Methods**
3. **HTTP Status Codes**
4. **Error Handling with try/catch**

### Example: Creating a Task

```csharp
[HttpPost]
public async Task<ActionResult<TaskItem>> CreateTask([FromBody] CreateTaskDto dto)
{
    // Simulate processing delay
    await Task.Delay(100);

    var task = new TaskItem
    {
        Title = dto.Title,
        Description = dto.Description,
        IsCompleted = false,
        CreatedAt = DateTime.UtcNow
    };

    await _context.Tasks.AddAsync(task);
    await _context.SaveChangesAsync(); // I/O-bound operation

    return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
}
```

### Why This Matters

- **AddAsync**: While Add() is synchronous in EF Core, AddAsync exists for special value generators
- **SaveChangesAsync**: This is WHERE async really matters - database writes are I/O-bound
- The thread is freed while waiting for the database, improving scalability

### React Side: Async in Event Handlers

```javascript
const handleCreateTask = async (e) => {
  e.preventDefault();
  try {
    setLoading(true);
    // API call returns a Promise
    await taskService.createTask(newTask);
    await loadTasks(); // Reload tasks
  } catch (err) {
    setError(err.message);
  } finally {
    setLoading(false);
  }
};
```

**Key Points:**
- Event handlers can be `async` functions
- Use try/catch for error handling
- Update loading states for better UX
- Chain multiple async operations with `await`

---

## Module 2: File Processing

### Concepts Demonstrated

1. **Task.Run for CPU-bound Work**
2. **Task.Delay for Simulation**
3. **CancellationToken**
4. **Progress Tracking**
5. **Fire-and-Forget Pattern**

### Example: Background Processing

```csharp
public async Task<FileProcessingJob> StartProcessingAsync(
    FileUploadDto uploadDto, 
    CancellationToken cancellationToken = default)
{
    var job = new FileProcessingJob
    {
        Id = Guid.NewGuid().ToString(),
        FileName = uploadDto.FileName,
        Status = ProcessingStatus.Pending
    };

    _jobs.TryAdd(job.Id, job);

    // Start background processing without waiting
    _ = Task.Run(async () => 
        await ProcessFileAsync(job, uploadDto.FileSizeKb, cancellationToken), 
        cancellationToken);

    return job; // Return immediately
}
```

### Task.Run vs await

- **`await`**: Use for I/O-bound operations (database, network)
- **`Task.Run`**: Use for CPU-bound operations (calculations, processing)

```csharp
// CPU-bound work
await Task.Run(() => {
    for (int i = 0; i < 1000000; i++)
    {
        // Complex calculation
    }
});

// I/O-bound work
await httpClient.GetAsync(url);
```

### Cancellation Pattern

```csharp
for (int i = 0; i < totalSteps; i++)
{
    // Check if cancellation was requested
    if (cancellationToken.IsCancellationRequested)
    {
        job.Status = ProcessingStatus.Cancelled;
        return;
    }

    await Task.Delay(delayMs, cancellationToken);
}
```

**Best Practice:** Always check cancellation tokens in long-running operations.

### React: Polling Pattern

```javascript
const pollStatus = async (id) => {
  const interval = setInterval(async () => {
    try {
      const statusData = await fileProcessingService.getStatus(id);
      setStatus(statusData);
      
      // Stop polling when complete
      if (statusData.status === 'Completed' || statusData.status === 'Failed') {
        clearInterval(interval);
      }
    } catch (err) {
      clearInterval(interval);
    }
  }, 1000); // Poll every second
};
```

---

## Module 3: External API

### Concepts Demonstrated

1. **Task.WhenAll - Parallel Execution**
2. **Task.WhenAny - Race Conditions**
3. **Sequential vs Parallel**
4. **Retry Logic with Exponential Backoff**

### Task.WhenAll: Run Multiple Operations in Parallel

```csharp
public async Task<ParallelOperationResult> FetchParallelAsync(
    List<string> sources, 
    CancellationToken cancellationToken = default)
{
    var startTime = DateTime.UtcNow;

    // Create all tasks but don't await them yet
    var tasks = sources.Select(source => 
        FetchFromApiAsync(source, Random.Shared.Next(500, 2000), cancellationToken)
    ).ToList();

    // Wait for ALL tasks to complete in parallel
    var results = await Task.WhenAll(tasks);

    var totalTime = DateTime.UtcNow - startTime;

    return new ParallelOperationResult
    {
        Results = results.ToList(),
        TotalTime = totalTime,
        OperationType = "Parallel (Task.WhenAll)"
    };
}
```

**Performance:**
- Sequential: If each operation takes 1s, 4 operations = 4s total
- Parallel: All 4 operations run simultaneously = ~1s total (the slowest one)

### Task.WhenAny: Return First Completed

```csharp
public async Task<RaceOperationResult> FetchRaceAsync(
    List<string> sources, 
    CancellationToken cancellationToken = default)
{
    var tasks = sources.Select(source => 
        FetchFromApiAsync(source, Random.Shared.Next(500, 3000), cancellationToken)
    ).ToList();

    // Wait for the FIRST task to complete
    var completedTask = await Task.WhenAny(tasks);
    var winner = await completedTask;

    return new RaceOperationResult
    {
        Winner = winner,
        TotalTime = totalTime
    };
}
```

**Use Cases for Task.WhenAny:**
- Timeout patterns
- Redundant requests to multiple servers
- Getting the fastest response

### Retry Logic with Exponential Backoff

```csharp
public async Task<ExternalApiData> FetchWithRetryAsync(
    string source, 
    int maxRetries = 3, 
    CancellationToken cancellationToken = default)
{
    int attempt = 0;
    int delayMs = 500;

    while (attempt < maxRetries)
    {
        try
        {
            return await FetchFromApiAsync(source, 1000, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            attempt++;
            if (attempt >= maxRetries) throw;

            // Exponential backoff: 500ms, 1000ms, 2000ms
            await Task.Delay(delayMs, cancellationToken);
            delayMs *= 2;
        }
    }
}
```

**Why Exponential Backoff?**
- Prevents overwhelming a failing service
- Gives the service time to recover
- Standard practice for handling transient failures

---

## Module 4: Notifications

### Concepts Demonstrated

1. **Server-Sent Events (SSE)**
2. **Long Polling**
3. **Async Event Handlers**
4. **EventSource API**

### Server-Sent Events Pattern

```csharp
[HttpGet("stream")]
public async Task StreamNotifications(CancellationToken cancellationToken)
{
    // Set SSE headers
    Response.Headers.Append("Content-Type", "text/event-stream");
    Response.Headers.Append("Cache-Control", "no-cache");

    // Subscribe to notification events
    async Task OnNotificationCreated(Notification notification)
    {
        var message = JsonSerializer.Serialize(notification);
        await SendSseMessage(message, "notification");
    }

    _notificationService.NotificationCreated += OnNotificationCreated;

    try
    {
        // Keep connection alive
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            await SendSseMessage("keepalive", "ping");
            await Response.Body.FlushAsync(cancellationToken);
        }
    }
    finally
    {
        _notificationService.NotificationCreated -= OnNotificationCreated;
    }
}
```

### React: EventSource API

```javascript
useEffect(() => {
  const eventSource = new EventSource('http://localhost:5000/api/notifications/stream');
  
  eventSource.addEventListener('notification', (event) => {
    const notification = JSON.parse(event.data);
    setNotifications(prev => [notification, ...prev]);
  });

  eventSource.onerror = (err) => {
    console.error('SSE Error:', err);
  };

  return () => {
    eventSource.close(); // Cleanup
  };
}, []);
```

---

## Module 5: Parallel Processing

### Concepts Demonstrated

1. **Parallel.ForEachAsync**
2. **MaxDegreeOfParallelism**
3. **Sync vs Async Comparison**
4. **Thread-Safe Collections**

### Parallel.ForEachAsync (New in .NET 6)

```csharp
await Parallel.ForEachAsync(
    request.Items,
    new ParallelOptions
    {
        MaxDegreeOfParallelism = request.MaxDegreeOfParallelism,
        CancellationToken = cancellationToken
    },
    async (item, ct) =>
    {
        // Simulate async processing
        await Task.Delay(Random.Shared.Next(100, 500), ct);

        var result = await Task.Run(() => {
            // CPU-bound work
            return CalculateResult(item);
        }, ct);

        // Thread-safe collection update
        lock (lockObject)
        {
            processedItems.Add(result);
        }
    });
```

### Why MaxDegreeOfParallelism?

Controls how many operations run concurrently:
- **Too High**: May overwhelm resources (memory, connections)
- **Too Low**: Underutilizes available resources
- **Just Right**: Balances throughput and resource usage

### Sync vs Async Performance

**Synchronous:**
```csharp
foreach (var item in items)
{
    Thread.Sleep(500); // Blocks thread
    ProcessItem(item);
}
// 10 items × 500ms = 5 seconds
```

**Asynchronous:**
```csharp
await Parallel.ForEachAsync(items, 
    new ParallelOptions { MaxDegreeOfParallelism = 4 },
    async (item, ct) => {
        await Task.Delay(500, ct); // Doesn't block
        await ProcessItemAsync(item);
    });
// ~1.25 seconds with 4 parallel operations
```

---

## Common Pitfalls

### 1. Async Void

❌ **DON'T:**
```csharp
public async void ProcessData() // WRONG!
{
    await _service.ProcessAsync();
}
```

✅ **DO:**
```csharp
public async Task ProcessData()
{
    await _service.ProcessAsync();
}
```

**Why?** Async void methods:
- Can't be awaited
- Exceptions can't be caught
- Only use for event handlers

### 2. Blocking on Async Code

❌ **DON'T:**
```csharp
var result = _service.GetDataAsync().Result; // Deadlock risk!
```

✅ **DO:**
```csharp
var result = await _service.GetDataAsync();
```

### 3. Unnecessary Task.Run

❌ **DON'T:**
```csharp
public async Task<string> GetDataAsync()
{
    return await Task.Run(async () => 
        await httpClient.GetStringAsync(url)); // Unnecessary!
}
```

✅ **DO:**
```csharp
public async Task<string> GetDataAsync()
{
    return await httpClient.GetStringAsync(url);
}
```

### 4. Not Using ConfigureAwait(false) in Libraries

In library code (not ASP.NET Core controllers):
```csharp
var result = await httpClient.GetAsync(url).ConfigureAwait(false);
```

This avoids capturing the synchronization context, improving performance.

---

## Best Practices

1. **Use async all the way down**: Don't mix sync and async
2. **Return Task, not async Task** when possible
3. **Use CancellationToken** for cancellable operations
4. **Avoid async void** except for event handlers
5. **Don't block on async code** with .Result or .Wait()
6. **Use Task.WhenAll** for parallel operations
7. **Handle exceptions** properly with try/catch
8. **Use async suffix** for async method names
9. **Consider ConfigureAwait** in libraries
10. **Test async code** thoroughly

---

## Exercises

### Exercise 1: Add Search to Task Manager

Implement an async search endpoint:
```csharp
[HttpGet("search")]
public async Task<ActionResult<List<TaskItem>>> SearchTasks([FromQuery] string query)
{
    // Your code here
    // Use LINQ and ToListAsync
}
```

### Exercise 2: Implement Timeout Pattern

Add a timeout to external API calls:
```csharp
var fetchTask = FetchFromApiAsync(source);
var timeoutTask = Task.Delay(5000);
var completedTask = await Task.WhenAny(fetchTask, timeoutTask);

if (completedTask == timeoutTask)
{
    throw new TimeoutException("Operation timed out");
}

return await fetchTask;
```

### Exercise 3: Batch Operations

Implement a batch delete for tasks:
```csharp
[HttpPost("batch-delete")]
public async Task<IActionResult> DeleteTasks([FromBody] List<int> taskIds)
{
    // Delete multiple tasks efficiently
    // Use Task.WhenAll for parallel deletes
}
```

### Exercise 4: Progress Reporting

Add progress reporting to parallel processing:
```csharp
var progress = new Progress<int>(percent => {
    // Report progress to client
});

await ProcessWithProgress(items, progress);
```

---

## Conclusion

This guide covered the fundamental patterns of asynchronous programming in C#. The key takeaways are:

1. Use async/await for I/O-bound operations
2. Understand Task.WhenAll vs Task.WhenAny
3. Implement cancellation properly
4. Handle errors with try/catch
5. Consider performance implications
6. Follow best practices consistently

Continue exploring the application and experimenting with the code to deepen your understanding!

## Additional Resources

- [Microsoft Async/Await Best Practices](https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)
- [Stephen Cleary's Blog](https://blog.stephencleary.com/)
- [David Fowler's Async Guidance](https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md)
