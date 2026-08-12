using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TenantTaskManager.Api.Contracts.Tasks;
using TenantTaskManager.Application.Tasks.GetTasks;
using TenantTaskManager.Application.Tasks.CreateTask;
using TenantTaskManager.Application.Tasks.CompleteTask;

namespace TenantTaskManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
public sealed class TasksController(
    CreateTaskHandler createTaskHandler,
    GetTasksHandler getTasksHandler,
    CompleteTaskHandler completeTaskHandler) : ControllerBase
{
    [HttpPatch("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Complete(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            await completeTaskHandler.HandleAsync(id, cancellationToken);

            return NoContent();
        }
        catch (TaskNotFoundException exception)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Task not found",
                Detail = exception.Message
            });
        }
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TaskDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<TaskDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var tasks = await getTasksHandler.HandleAsync(cancellationToken);

        return Ok(tasks);
    }

    [HttpPost]
    [ProducesResponseType<CreateTaskResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CreateTaskResponse>> Create(
        CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var taskId = await createTaskHandler.HandleAsync(
            new CreateTaskCommand(request.Title),
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            new CreateTaskResponse(taskId));
    }
}