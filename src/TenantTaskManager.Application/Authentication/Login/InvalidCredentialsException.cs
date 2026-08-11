namespace TenantTaskManager.Application.Authentication.Login;

public sealed class InvalidCredentialsException()
    : Exception("The email address or password is incorrect.");