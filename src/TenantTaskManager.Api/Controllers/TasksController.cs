using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TenantTaskManager.Api.Contracts.Tasks;
using TenantTaskManager.Application.Tasks.GetTasks;
using TenantTaskManager.Application.Tasks.CreateTask;

namespace TenantTaskManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
public sealed class TasksController(
    CreateTaskHandler createTaskHandler,
    GetTasksHandler getTasksHandler) : ControllerBase
{
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