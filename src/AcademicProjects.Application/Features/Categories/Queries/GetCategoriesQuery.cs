using AcademicProjects.Application.Features.Categories.DTOs;
using MediatR;

namespace AcademicProjects.Application.Features.Categories.Queries;

public sealed record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;