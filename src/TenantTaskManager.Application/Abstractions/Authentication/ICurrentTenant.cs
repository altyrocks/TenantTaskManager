namespace TenantTaskManager.Application.Abstractions.Authentication;

public interface ICurrentTenant
{
    Guid TenantId { get; }
}