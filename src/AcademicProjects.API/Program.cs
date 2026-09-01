using AcademicProjects.API.Authentication;
using AcademicProjects.Application;
using AcademicProjects.Infrastructure;
using AcademicProjects.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

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

await app.Services.SeedIdentityRolesAsync();

app.Run();
