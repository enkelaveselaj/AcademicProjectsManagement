using AcademicProjects.API.Authentication;
using AcademicProjects.Application;
using AcademicProjects.Infrastructure;
using AcademicProjects.Infrastructure.Identity;
using AcademicProjects.API.Features.Categories;
using AcademicProjects.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseMiddleware<ValidationExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { service = "Academic Projects API", status = "running" }));
app.MapHealthChecks("/health");
app.MapAuthEndpoints();
app.MapCategoryEndpoints();

await app.Services.SeedIdentityRolesAsync();

app.Run();
