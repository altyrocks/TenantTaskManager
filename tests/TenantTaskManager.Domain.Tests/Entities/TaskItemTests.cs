using TenantTaskManager.Domain.Entities;

namespace TenantTaskManager.Domain.Tests.Entities;

public sealed class TaskItemTests
{
    [Fact]
    public void Constructor_WithValidValues_CreatesTask()
    {
        var tenantId = Guid.NewGuid();
        var beforeCreation = DateTimeOffset.UtcNow;

        var task = new TaskItem(tenantId, "Prepare report");

        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal(tenantId, task.TenantId);
        Assert.Equal("Prepare report", task.Title);
        Assert.False(task.IsCompleted);
        Assert.Null(task.CompletedAtUtc);
        Assert.InRange(task.CreatedAtUtc, beforeCreation, DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Constructor_WithEmptyTenantId_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new TaskItem(Guid.Empty, "Prepare report"));

        Assert.Equal("tenantId", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithBlankTitle_ThrowsArgumentException(string title)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new TaskItem(Guid.NewGuid(), title));

        Assert.Equal("title", exception.ParamName);
    }

    [Fact]
    public void Constructor_TrimsTitle()
    {
        var task = new TaskItem(Guid.NewGuid(), "  Prepare report  ");

        Assert.Equal("Prepare report", task.Title);
    }

    [Fact]
    public void Complete_WhenIncomplete_MarksTaskComplete()
    {
        var task = new TaskItem(Guid.NewGuid(), "Prepare report");
        var beforeCompletion = DateTimeOffset.UtcNow;

        task.Complete();

        Assert.True(task.IsCompleted);
        Assert.NotNull(task.CompletedAtUtc);
        Assert.InRange(
            task.CompletedAtUtc.Value,
            beforeCompletion,
            DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Complete_WhenAlreadyComplete_DoesNotChangeCompletionTime()
    {
        var task = new TaskItem(Guid.NewGuid(), "Prepare report");
        task.Complete();
        var originalCompletionTime = task.CompletedAtUtc;

        task.Complete();

        Assert.Equal(originalCompletionTime, task.CompletedAtUtc);
    }
}