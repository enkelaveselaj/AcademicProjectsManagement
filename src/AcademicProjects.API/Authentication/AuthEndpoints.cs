using System.Security.Claims;
using AcademicProjects.Application.Authentication;
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

    return result.Succeeded
        ? Results.Ok(new
        {
            accessToken = result.Value!.Value,
            tokenType = "Bearer",
            expiresIn = result.Value.ExpiresInSeconds
        })
        : Results.Unauthorized();
}

private static IResult GetCurrentUser(ClaimsPrincipal user) => Results.Ok(new
{
    id = user.FindFirstValue(ClaimTypes.NameIdentifier),
    email = user.FindFirstValue(ClaimTypes.Email),
    roles = user.FindAll(ClaimTypes.Role).Select(claim => claim.Value)
});
}
