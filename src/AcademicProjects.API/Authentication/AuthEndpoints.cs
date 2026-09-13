using System.Security.Claims;
using AcademicProjects.Application.Features.Auth.Commands;
using AcademicProjects.Application.Features.Auth.Queries;
using AcademicProjects.Domain.Enums;
using MediatR;

namespace AcademicProjects.API.Authentication;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth");

        group.MapPost("/register", RegisterAsync);
        group.MapPost("/login", LoginAsync);
        group.MapGet("/me", GetCurrentUser).RequireAuthorization();
        group.MapPut("/change-password", ChangePasswordAsync).RequireAuthorization();
        group.MapGet("/directory", GetUserDirectoryAsync).RequireAuthorization();

        group.MapGet("/users", GetUsersAsync)
            .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Administrator)));
        group.MapPut("/users/{id:guid}/role", ChangeUserRoleAsync)
            .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Administrator)));
        group.MapPut("/users/{id:guid}/reset-password", ResetUserPasswordAsync)
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
        RegisterUserCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.Succeeded
            ? Results.Created($"/api/auth/users/{result.Value!.Id}", result.Value)
            : Results.ValidationProblem(result.Errors);
    }

    private static async Task<IResult> LoginAsync(
        LoginCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var token = await sender.Send(command, cancellationToken);

        return Results.Ok(new
        {
            accessToken = token.Value,
            tokenType = "Bearer",
            expiresIn = token.ExpiresInSeconds
        });
    }

    private static IResult GetCurrentUser(ClaimsPrincipal user) => Results.Ok(new
    {
        id = user.FindFirstValue(ClaimTypes.NameIdentifier),
        email = user.FindFirstValue(ClaimTypes.Email),
        roles = user.FindAll(ClaimTypes.Role).Select(claim => claim.Value)
    });

    private static async Task<IResult> ChangePasswordAsync(
        ChangePasswordRequest request,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await sender.Send(
            new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword),
            cancellationToken);

        return result.Succeeded
            ? Results.NoContent()
            : Results.ValidationProblem(result.Errors);
    }

    private static async Task<IResult> GetUserDirectoryAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var directory = await sender.Send(new GetUserDirectoryQuery(), cancellationToken);

        return Results.Ok(directory);
    }

    private static async Task<IResult> GetUsersAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var users = await sender.Send(new GetUsersQuery(), cancellationToken);

        return Results.Ok(users);
    }

    private static async Task<IResult> ChangeUserRoleAsync(
        Guid id,
        ChangeUserRoleRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ChangeUserRoleCommand(id, request.Role), cancellationToken);

        return result.Succeeded
            ? Results.Ok(result.Value)
            : Results.ValidationProblem(result.Errors);
    }

    private static async Task<IResult> ResetUserPasswordAsync(
        Guid id,
        ResetUserPasswordRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ResetUserPasswordCommand(id, request.NewPassword), cancellationToken);

        return result.Succeeded
            ? Results.NoContent()
            : Results.ValidationProblem(result.Errors);
    }

    private static async Task<IResult> GetPendingUsersAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var pendingUsers = await sender.Send(new GetPendingUsersQuery(), cancellationToken);

        return Results.Ok(pendingUsers);
    }

    private static async Task<IResult> ApproveUserAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ApproveUserCommand(id), cancellationToken);

        return result.Succeeded
            ? Results.Ok(result.Value)
            : Results.ValidationProblem(result.Errors);
    }

    private static async Task<IResult> RejectUserAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RejectUserCommand(id), cancellationToken);

        return result.Succeeded
            ? Results.NoContent()
            : Results.ValidationProblem(result.Errors);
    }
}
