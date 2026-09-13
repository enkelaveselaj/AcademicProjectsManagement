using MediatR;
using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Features.Categories.DTOs;

namespace AcademicProjects.Application.Features.Categories.Commands;

public sealed record CreateCategoryCommand(
    string Name,
    string? Description) : IRequest<CategoryDto>, IRequireAdministrator;