using AsyncLearningApp.Api.Data;
using AsyncLearningApp.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsyncLearningApp.Api.Controllers;

/// <summary>
/// Controller demonstrating basic async CRUD operations.
/// 
/// Learning Objectives:
/// - Understand async/await patterns in API controllers
/// - Learn how to use Task<T> return types
/// - See proper async database operations with Entity Framework
/// - Understand when to use async vs sync operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<TasksController> _logger;

    public TasksController(AppDbContext context, ILogger<TasksController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/tasks - Retrieve all tasks asynchronously.
    /// 
    /// Key Learning Points:
    /// - ToListAsync() is the async version of ToList()
    /// - Database I/O is async, so the thread is freed while waiting for data
    /// - Task<ActionResult<List<TaskItem>>> indicates async method returning a list
    /// - The 'async' keyword allows use of 'await' inside the method
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<TaskItem>>> GetAllTasks()
    {
        _logger.LogInformation("Fetching all tasks asynchronously");

        // ToListAsync() performs I/O-bound database operation asynchronously
        // The calling thread is released while waiting for database response
        var tasks = await _context.Tasks.ToListAsync();

        _logger.LogInformation("Retrieved {Count} tasks", tasks.Count);
        return Ok(tasks);
    }

    /// <summary>
    /// GET /api/tasks/{id} - Retrieve a single task by ID.
    /// 
    /// Key Learning Points:
    /// - FirstOrDefaultAsync() is async version for finding single items
    /// - Null checking after async operations
    /// - Proper HTTP status codes (404 Not Found, 200 OK)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItem>> GetTaskById(int id)
    {
        _logger.LogInformation("Fetching task with ID {TaskId}", id);

        // Find task asynchronously
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            _logger.LogWarning("Task with ID {TaskId} not found", id);
            return NotFound(new { message = $"Task with ID {id} not found" });
        }

        return Ok(task);
    }

    /// <summary>
    /// POST /api/tasks - Create a new task.
    /// 
    /// Key Learning Points:
    /// - AddAsync() adds entity to context (minimal performance benefit vs Add)
    /// - SaveChangesAsync() commits changes to database asynchronously
    /// - This is where async really matters - database write is I/O-bound
    /// - CreatedAtAction returns 201 Created with location header
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TaskItem>> CreateTask([FromBody] CreateTaskDto dto)
    {
        _logger.LogInformation("Creating new task: {Title}", dto.Title);

        // Simulate some processing delay to demonstrate async behavior
        await Task.Delay(100); // Simulates validation or other async operations

        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        // Add to context (async mainly for consistency, not much async benefit here)
        await _context.Tasks.AddAsync(task);

        // Save changes to database - THIS is the important async operation
        // Database write is I/O-bound, so async allows thread to do other work while waiting
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created task with ID {TaskId}", task.Id);

        // Return 201 Created with the new resource and location header
        return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
    }

    /// <summary>
    /// PUT /api/tasks/{id} - Update an existing task.
    /// 
    /// Key Learning Points:
    /// - Combining read and write async operations
    /// - Partial updates using DTO pattern
    /// - SaveChangesAsync() for committing changes
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TaskItem>> UpdateTask(int id, [FromBody] UpdateTaskDto dto)
    {
        _logger.LogInformation("Updating task with ID {TaskId}", id);

        // First, fetch the existing task asynchronously
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            _logger.LogWarning("Task with ID {TaskId} not found for update", id);
            return NotFound(new { message = $"Task with ID {id} not found" });
        }

        // Update only the provided fields (partial update pattern)
        if (dto.Title != null)
            task.Title = dto.Title;

        if (dto.Description != null)
            task.Description = dto.Description;

        if (dto.IsCompleted.HasValue)
        {
            task.IsCompleted = dto.IsCompleted.Value;
            if (task.IsCompleted && !task.CompletedAt.HasValue)
            {
                task.CompletedAt = DateTime.UtcNow;
            }
            else if (!task.IsCompleted)
            {
                task.CompletedAt = null;
            }
        }

        // Commit changes asynchronously
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated task with ID {TaskId}", id);
        return Ok(task);
    }

    /// <summary>
    /// DELETE /api/tasks/{id} - Delete a task.
    /// 
    /// Key Learning Points:
    /// - Remove is synchronous, but SaveChangesAsync is where async matters
    /// - 204 No Content is standard response for successful deletion
    /// - Checking existence before deletion
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        _logger.LogInformation("Deleting task with ID {TaskId}", id);

        // Find task asynchronously
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            _logger.LogWarning("Task with ID {TaskId} not found for deletion", id);
            return NotFound(new { message = $"Task with ID {id} not found" });
        }

        // Remove from context (synchronous operation)
        _context.Tasks.Remove(task);

        // Save changes asynchronously - the I/O-bound database operation
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted task with ID {TaskId}", id);

        // 204 No Content - successful deletion
        return NoContent();
    }

    /// <summary>
    /// GET /api/tasks/completed - Get all completed tasks.
    /// 
    /// Demonstrates filtering with async operations.
    /// </summary>
    [HttpGet("completed")]
    public async Task<ActionResult<List<TaskItem>>> GetCompletedTasks()
    {
        _logger.LogInformation("Fetching completed tasks");

        var completedTasks = await _context.Tasks
            .Where(t => t.IsCompleted)
            .OrderByDescending(t => t.CompletedAt)
            .ToListAsync();

        return Ok(completedTasks);
    }

    /// <summary>
    /// GET /api/tasks/pending - Get all pending tasks.
    /// 
    /// Demonstrates another filtering example.
    /// </summary>
    [HttpGet("pending")]
    public async Task<ActionResult<List<TaskItem>>> GetPendingTasks()
    {
        _logger.LogInformation("Fetching pending tasks");

        var pendingTasks = await _context.Tasks
            .Where(t => !t.IsCompleted)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();

        return Ok(pendingTasks);
    }
}
