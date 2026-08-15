using Microsoft.AspNetCore.Diagnostics;
using TenantTaskManager.Application.Authentication.Login;
using TenantTaskManager.Application.Tasks;

namespace TenantTaskManager.Api.ErrorHandling;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            InvalidCredentialsException => (
                StatusCodes.Status401Unauthorized,
                "Invalid credentials",
                exception.Message),
            TaskNotFoundException => (
                StatusCodes.Status404NotFound,
                "Task not found",
                exception.Message),
            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "Invalid request",
                exception.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                "The server could not complete the request.")
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled exception processing {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning(
                "Request failed with status {StatusCode} for {Method} {Path}: {ErrorMessage}",
                statusCode,
                httpContext.Request.Method,
                httpContext.Request.Path,
                exception.Message);
        }

        await Results.Problem(
            statusCode: statusCode,
            title: title,
            detail: detail,
            instance: httpContext.Request.Path,
            extensions: new Dictionary<string, object?>
            {
                ["traceId"] = httpContext.TraceIdentifier
            }).ExecuteAsync(httpContext);

        return true;
    }
}
