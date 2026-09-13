namespace AcademicProjects.API.Features.Categories;

public sealed record UpdateCategoryRequest(
    string Name,
    string Description);