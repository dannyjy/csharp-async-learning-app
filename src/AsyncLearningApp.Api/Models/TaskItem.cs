namespace AsyncLearningApp.Api.Models;

/// <summary>
/// Represents a task item in our learning application.
/// This simple model demonstrates async CRUD operations.
/// </summary>
public class TaskItem
{
    public int Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public bool IsCompleted { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// DTO for creating a new task.
/// DTOs help separate API contracts from internal models.
/// </summary>
public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating an existing task.
/// </summary>
public class UpdateTaskDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool? IsCompleted { get; set; }
}
