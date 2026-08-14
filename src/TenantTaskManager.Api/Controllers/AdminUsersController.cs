using Microsoft.AspNetCore.Mvc;
using TenantTaskManager.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using TenantTaskManager.Application.Users.GetUsers;

namespace TenantTaskManager.Api.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[Route("api/admin/users")]
public sealed class AdminUsersController(GetUsersHandler getUsersHandler) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<UserDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var users = await getUsersHandler.HandleAsync(cancellationToken);
        return Ok(users);
    }
}