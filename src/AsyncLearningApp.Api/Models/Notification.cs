namespace AsyncLearningApp.Api.Models;

/// <summary>
/// Represents a notification in the system.
/// Used to demonstrate real-time async event handling.
/// </summary>
public class Notification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string Title { get; set; } = string.Empty;
    
    public string Message { get; set; } = string.Empty;
    
    public NotificationType Type { get; set; }
    
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Types of notifications.
/// </summary>
public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}

/// <summary>
/// DTO for creating a notification.
/// </summary>
public class CreateNotificationDto
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
}
