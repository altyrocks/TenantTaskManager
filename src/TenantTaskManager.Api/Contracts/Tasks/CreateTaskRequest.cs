using System.ComponentModel.DataAnnotations;

namespace TenantTaskManager.Api.Contracts.Tasks;

public sealed class CreateTaskRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = string.Empty;
}