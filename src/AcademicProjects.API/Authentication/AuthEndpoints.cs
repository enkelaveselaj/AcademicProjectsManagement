using System.Security.Claims;
using AcademicProjects.Application.Authentication;
using AcademicProjects.Domain.Enums;
using ApplicationAuthenticationService = AcademicProjects.Application.Authentication.IAuthenticationService;

namespace AcademicProjects.API.Authentication;

public static class AuthEndpoints
{
public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
{
var group = endpoints.MapGroup("/api/auth");

    group.MapPost("/register", RegisterAsync);
    group.MapPost("/login", LoginAsync);
    group.MapGet("/me", GetCurrentUser).RequireAuthorization();

    group.MapGet("/users", GetUsersAsync)
        .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Administrator)));
    group.MapPut("/users/{id:guid}/role", ChangeUserRoleAsync)
        .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Administrator)));
    group.MapGet("/pending-users", GetPendingUsersAsync)
        .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Administrator)));
    group.MapPut("/users/{id:guid}/approve", ApproveUserAsync)
        .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Administrator)));
    group.MapDelete("/users/{id:guid}/reject", RejectUserAsync)
        .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Administrator)));

    return endpoints;
}

private static async Task<IResult> RegisterAsync(
    RegisterUserRequest request,
    ApplicationAuthenticationService authenticationService,
    CancellationToken cancellationToken)
{
    var result = await authenticationService.RegisterAsync(request, cancellationToken);

    return result.Succeeded
        ? Results.Created($"/api/auth/users/{result.Value!.Id}", result.Value)
        : Results.ValidationProblem(result.Errors);
}

private static async Task<IResult> LoginAsync(
    LoginRequest request,
    ApplicationAuthenticationService authenticationService,
    CancellationToken cancellationToken)
{
    var result = await authenticationService.LoginAsync(request, cancellationToken);

    if (result.Succeeded)
    {
        return Results.Ok(new
        {
            accessToken = result.Value!.Value,
            tokenType = "Bearer",
            expiresIn = result.Value.ExpiresInSeconds
        });
    }

    if (result.Errors.TryGetValue("approval", out var approvalErrors))
    {
        return Results.Json(new { title = approvalErrors[0] }, statusCode: StatusCodes.Status403Forbidden);
    }

    return Results.Unauthorized();
}

private static IResult GetCurrentUser(ClaimsPrincipal user) => Results.Ok(new
{
    id = user.FindFirstValue(ClaimTypes.NameIdentifier),
    email = user.FindFirstValue(ClaimTypes.Email),
    roles = user.FindAll(ClaimTypes.Role).Select(claim => claim.Value)
});

private static async Task<IResult> GetUsersAsync(
    IUserManagementService userManagementService,
    CancellationToken cancellationToken)
{
    var users = await userManagementService.GetUsersAsync(cancellationToken);

    return Results.Ok(users);
}

private static async Task<IResult> ChangeUserRoleAsync(
    Guid id,
    ChangeUserRoleRequest request,
    IUserManagementService userManagementService,
    CancellationToken cancellationToken)
{
    var result = await userManagementService.ChangeUserRoleAsync(id, request.Role, cancellationToken);

    return result.Succeeded
        ? Results.Ok(result.Value)
        : Results.ValidationProblem(result.Errors);
}

private static async Task<IResult> GetPendingUsersAsync(
    IUserManagementService userManagementService,
    CancellationToken cancellationToken)
{
    var pendingUsers = await userManagementService.GetPendingUsersAsync(cancellationToken);

    return Results.Ok(pendingUsers);
}

private static async Task<IResult> ApproveUserAsync(
    Guid id,
    IUserManagementService userManagementService,
    CancellationToken cancellationToken)
{
    var result = await userManagementService.ApproveUserAsync(id, cancellationToken);

    return result.Succeeded
        ? Results.Ok(result.Value)
        : Results.ValidationProblem(result.Errors);
}

private static async Task<IResult> RejectUserAsync(
    Guid id,
    IUserManagementService userManagementService,
    CancellationToken cancellationToken)
{
    var result = await userManagementService.RejectUserAsync(id, cancellationToken);

    return result.Succeeded
        ? Results.NoContent()
        : Results.ValidationProblem(result.Errors);
}
}
