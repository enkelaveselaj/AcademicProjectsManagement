using FluentValidation;
using System.Text.Json;

namespace AcademicProjects.API.Middleware;

public sealed class ValidationExceptionMiddleware(
    RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException exception)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";

            var errors = exception.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(error => error.ErrorMessage)
                        .ToArray());

            var response = new
            {
                title = "One or more validation errors occurred.",
                status = 400,
                errors
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}