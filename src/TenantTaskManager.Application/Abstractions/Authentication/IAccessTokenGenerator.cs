using TenantTaskManager.Domain.Entities;

namespace TenantTaskManager.Application.Abstractions.Authentication;

public interface IAccessTokenGenerator
{
    AccessToken Generate(UserAccount user);
}