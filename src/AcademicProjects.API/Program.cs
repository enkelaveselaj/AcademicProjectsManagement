using AcademicProjects.API.Authentication;
using AcademicProjects.Application;
using AcademicProjects.Infrastructure;
using AcademicProjects.Infrastructure.Identity;
using AcademicProjects.API.Features.Categories;
using AcademicProjects.API.Features.Projects;
using AcademicProjects.API.Features.Documents;
using AcademicProjects.API.Features.Comments;
using AcademicProjects.API.Features.Notifications;
using AcademicProjects.API.Features.ProjectMilestones;
using AcademicProjects.API.Features.ProjectAssignments;
using AcademicProjects.API.Features.ProjectStatusHistories;
using AcademicProjects.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

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
app.MapProjectEndpoints();
app.MapDocumentEndpoints();
app.MapCommentEndpoints();
app.MapNotificationEndpoints();
app.MapProjectMilestoneEndpoints();
app.MapProjectAssignmentEndpoints();
app.MapProjectStatusHistoryEndpoints();

await app.Services.SeedIdentityRolesAsync();

app.Run();
