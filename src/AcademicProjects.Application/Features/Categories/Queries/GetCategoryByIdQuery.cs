using AcademicProjects.Application.Features.Categories.DTOs;
using MediatR;

namespace AcademicProjects.Application.Features.Categories.Queries;

public sealed record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto?>;