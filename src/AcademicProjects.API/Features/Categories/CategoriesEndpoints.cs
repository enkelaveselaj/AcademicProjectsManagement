using AcademicProjects.Application.Features.Categories.Commands;
using AcademicProjects.Application.Features.Categories.Queries;
using MediatR;

namespace AcademicProjects.API.Features.Categories;

public static class CategoriesEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/categories")
            .RequireAuthorization();

        group.MapGet("/", GetCategoriesAsync);
        group.MapGet("/{id:guid}", GetCategoryByIdAsync);
        group.MapPost("/", CreateCategoryAsync);
        group.MapPut("/{id:guid}", UpdateCategoryAsync);
        group.MapDelete("/{id:guid}", DeleteCategoryAsync);

        return endpoints;
    }

    private static async Task<IResult> GetCategoriesAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var categories = await sender.Send(
            new GetCategoriesQuery(),
            cancellationToken);

        return Results.Ok(categories);
    }

    private static async Task<IResult> GetCategoryByIdAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var category = await sender.Send(
            new GetCategoryByIdQuery(id),
            cancellationToken);

        return category is null
            ? Results.NotFound()
            : Results.Ok(category);
    }

    private static async Task<IResult> CreateCategoryAsync(
        CreateCategoryCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var category = await sender.Send(
            command,
            cancellationToken);

        return Results.Created(
            $"/api/categories/{category.Id}",
            category);
    }

    private static async Task<IResult> UpdateCategoryAsync(
        Guid id,
        UpdateCategoryRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryCommand(
            id,
            request.Name,
            request.Description);

        var category = await sender.Send(
            command,
            cancellationToken);

        return category is null
            ? Results.NotFound()
            : Results.Ok(category);
    }

    private static async Task<IResult> DeleteCategoryAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var deleted = await sender.Send(
            new DeleteCategoryCommand(id),
            cancellationToken);

        return deleted
            ? Results.NoContent()
            : Results.NotFound();
    }
}