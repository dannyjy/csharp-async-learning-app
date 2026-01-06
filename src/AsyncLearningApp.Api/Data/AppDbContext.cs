using AsyncLearningApp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AsyncLearningApp.Api.Data;

/// <summary>
/// Application database context using Entity Framework Core.
/// Configured to use InMemory database for this learning application.
/// 
/// Key Learning Points:
/// - DbContext implements async methods for all database operations
/// - SaveChangesAsync() is used instead of SaveChanges() for async operations
/// - Database operations are I/O-bound, making them ideal for async/await
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Tasks table - demonstrates basic async CRUD operations
    /// </summary>
    public DbSet<TaskItem> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure TaskItem
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
        });

        // Seed some initial data for demonstration
        modelBuilder.Entity<TaskItem>().HasData(
            new TaskItem
            {
                Id = 1,
                Title = "Learn async/await basics",
                Description = "Understand the fundamentals of asynchronous programming in C#",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new TaskItem
            {
                Id = 2,
                Title = "Explore Task.WhenAll",
                Description = "Learn how to run multiple async operations in parallel",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new TaskItem
            {
                Id = 3,
                Title = "Understand CancellationToken",
                Description = "Learn how to cancel long-running async operations",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
