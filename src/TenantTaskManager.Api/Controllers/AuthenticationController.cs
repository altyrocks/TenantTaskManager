using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TenantTaskManager.Api.Contracts.Authentication;
using TenantTaskManager.Application.Authentication.Login;

namespace TenantTaskManager.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthenticationController(LoginHandler loginHandler) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = await loginHandler.HandleAsync(
                new LoginCommand(request.Email, request.Password),
                cancellationToken);

            return Ok(new LoginResponse(token.Value, token.ExpiresAtUtc));
        }
        catch (InvalidCredentialsException exception)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid credentials",
                Detail = exception.Message
            });
        }
    }
}