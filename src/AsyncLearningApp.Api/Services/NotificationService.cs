using AsyncLearningApp.Api.Models;
using System.Collections.Concurrent;

namespace AsyncLearningApp.Api.Services;

/// <summary>
/// Service for managing notifications and demonstrating async event handling.
/// 
/// Key Learning Points:
/// - Event-based async patterns using async event handlers
/// - Thread-safe collections (ConcurrentBag) for async operations
/// - Background notification processing
/// </summary>
public interface INotificationService
{
    event Func<Notification, Task>? NotificationCreated;
    Task<Notification> CreateNotificationAsync(CreateNotificationDto dto);
    Task<List<Notification>> GetRecentNotificationsAsync(int count = 10);
}

public class NotificationService : INotificationService
{
    // ConcurrentBag is thread-safe for storing notifications from async operations
    private readonly ConcurrentBag<Notification> _notifications = new();
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Event that fires when a notification is created.
    /// Subscribers can handle this asynchronously.
    /// </summary>
    public event Func<Notification, Task>? NotificationCreated;

    /// <summary>
    /// Creates a new notification and triggers async event handlers.
    /// 
    /// Key Learning Points:
    /// - Creating and storing data
    /// - Invoking async event handlers
    /// - Fire-and-forget pattern with event notification
    /// </summary>
    public async Task<Notification> CreateNotificationAsync(CreateNotificationDto dto)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid().ToString(),
            Title = dto.Title,
            Message = dto.Message,
            Type = dto.Type,
            CreatedAt = DateTime.UtcNow
        };

        _notifications.Add(notification);
        _logger.LogInformation("Created notification {NotificationId}: {Title}", notification.Id, notification.Title);

        // Trigger async event handlers
        // Note: In production, you might want to handle exceptions from event handlers
        if (NotificationCreated != null)
        {
            await NotificationCreated.Invoke(notification);
        }

        return notification;
    }

    /// <summary>
    /// Retrieves recent notifications.
    /// Demonstrates async data retrieval pattern even for in-memory data.
    /// </summary>
    public Task<List<Notification>> GetRecentNotificationsAsync(int count = 10)
    {
        var recent = _notifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .ToList();

        return Task.FromResult(recent);
    }
}
