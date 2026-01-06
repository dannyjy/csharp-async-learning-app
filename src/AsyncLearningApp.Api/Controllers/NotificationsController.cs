using AsyncLearningApp.Api.Models;
using AsyncLearningApp.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace AsyncLearningApp.Api.Controllers;

/// <summary>
/// Controller demonstrating real-time notifications and async event handling.
/// 
/// Learning Objectives:
/// - Understand async event patterns
/// - Learn Server-Sent Events (SSE) for real-time communication
/// - Implement long-polling as an alternative
/// - Handle background async operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/notifications/trigger - Create and trigger a notification.
    /// 
    /// Key Learning Points:
    /// - Async event handling
    /// - Creating notifications that trigger events
    /// - Fire-and-forget pattern for notifications
    /// </summary>
    [HttpPost("trigger")]
    public async Task<ActionResult<Notification>> TriggerNotification([FromBody] CreateNotificationDto dto)
    {
        _logger.LogInformation("Triggering notification: {Title}", dto.Title);

        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return BadRequest(new { message = "Title is required" });
        }

        // Create notification asynchronously
        // This will trigger any subscribed event handlers
        var notification = await _notificationService.CreateNotificationAsync(dto);

        _logger.LogInformation("Notification {NotificationId} created and triggered", notification.Id);

        return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
    }

    /// <summary>
    /// GET /api/notifications/{id} - Get a specific notification.
    /// 
    /// This is a placeholder for retrieving individual notifications.
    /// In a real application, you'd store and retrieve from a database.
    /// </summary>
    [HttpGet("{id}")]
    public ActionResult<object> GetNotification(string id)
    {
        // This is a simplified endpoint for demonstration
        return Ok(new { id, message = "Notification endpoint" });
    }

    /// <summary>
    /// GET /api/notifications/recent - Get recent notifications.
    /// 
    /// Key Learning Points:
    /// - Retrieving historical notifications
    /// - Async data access patterns
    /// </summary>
    [HttpGet("recent")]
    public async Task<ActionResult<List<Notification>>> GetRecentNotifications([FromQuery] int count = 10)
    {
        _logger.LogInformation("Fetching recent {Count} notifications", count);

        if (count <= 0 || count > 100)
        {
            return BadRequest(new { message = "Count must be between 1 and 100" });
        }

        var notifications = await _notificationService.GetRecentNotificationsAsync(count);

        return Ok(notifications);
    }

    /// <summary>
    /// GET /api/notifications/stream - Server-Sent Events stream for real-time notifications.
    /// 
    /// Key Learning Points:
    /// - Server-Sent Events (SSE) for real-time push to clients
    /// - Long-running async operations that keep connection alive
    /// - Streaming data to clients
    /// - Proper cleanup with CancellationToken
    /// 
    /// Note: SSE is one-way (server to client). For bidirectional, use SignalR/WebSockets.
    /// </summary>
    [HttpGet("stream")]
    public async Task StreamNotifications(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Client connected to notification stream");

        // Set headers for Server-Sent Events
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        // Subscribe to notification events
        var notificationQueue = new List<Notification>();
        var semaphore = new SemaphoreSlim(0);

        // Event handler for new notifications
        async Task OnNotificationCreated(Notification notification)
        {
            notificationQueue.Add(notification);
            semaphore.Release();
            await Task.CompletedTask;
        }

        _notificationService.NotificationCreated += OnNotificationCreated;

        try
        {
            // Send initial connection message
            await SendSseMessage("Connected to notification stream", "connection");

            // Keep connection alive and send notifications
            while (!cancellationToken.IsCancellationRequested)
            {
                // Wait for notification or timeout
                var hasNotification = await semaphore.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);

                if (hasNotification && notificationQueue.Count > 0)
                {
                    var notification = notificationQueue[0];
                    notificationQueue.RemoveAt(0);

                    // Send notification as SSE
                    var message = $"{{\"id\":\"{notification.Id}\",\"title\":\"{notification.Title}\"," +
                                $"\"message\":\"{notification.Message}\",\"type\":\"{notification.Type}\"," +
                                $"\"createdAt\":\"{notification.CreatedAt:o}\"}}";

                    await SendSseMessage(message, "notification");
                }
                else
                {
                    // Send keepalive
                    await SendSseMessage("keepalive", "ping");
                }

                // Flush to ensure data is sent immediately
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Client disconnected from notification stream");
        }
        finally
        {
            // Cleanup: unsubscribe from events
            _notificationService.NotificationCreated -= OnNotificationCreated;
        }
    }

    /// <summary>
    /// GET /api/notifications/poll - Long polling endpoint for notifications.
    /// 
    /// Key Learning Points:
    /// - Long polling as alternative to SSE
    /// - Request waits until notification arrives or timeout
    /// - Simpler than SSE but requires client to repeatedly reconnect
    /// - Good fallback for clients that don't support SSE
    /// </summary>
    [HttpGet("poll")]
    public async Task<ActionResult<Notification>> PollForNotification(
        [FromQuery] int timeoutSeconds = 30,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Client polling for notifications with {Timeout}s timeout", timeoutSeconds);

        var semaphore = new SemaphoreSlim(0);
        Notification? receivedNotification = null;

        // Subscribe to notification events
        async Task OnNotificationCreated(Notification notification)
        {
            receivedNotification = notification;
            semaphore.Release();
            await Task.CompletedTask;
        }

        _notificationService.NotificationCreated += OnNotificationCreated;

        try
        {
            // Wait for notification or timeout
            var received = await semaphore.WaitAsync(
                TimeSpan.FromSeconds(Math.Min(timeoutSeconds, 60)),
                cancellationToken);

            if (received && receivedNotification != null)
            {
                return Ok(receivedNotification);
            }

            // Timeout - no notification received
            return NoContent();
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, new { message = "Request was cancelled" });
        }
        finally
        {
            _notificationService.NotificationCreated -= OnNotificationCreated;
        }
    }

    /// <summary>
    /// Helper method to send Server-Sent Event messages.
    /// </summary>
    private async Task SendSseMessage(string data, string eventType = "message")
    {
        var message = $"event: {eventType}\ndata: {data}\n\n";
        var bytes = Encoding.UTF8.GetBytes(message);
        await Response.Body.WriteAsync(bytes);
    }
}
