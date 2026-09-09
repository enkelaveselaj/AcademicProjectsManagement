using System.Security.Claims;
using AcademicProjects.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AcademicProjects.Infrastructure.Authentication;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);

    public IReadOnlyCollection<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray()
            ?? [];

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
