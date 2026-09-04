using AcademicProjects.Application.Features.Categories.DTOs;
using MediatR;

namespace AcademicProjects.Application.Features.Categories.Commands;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Description) : IRequest<CategoryDto?>;