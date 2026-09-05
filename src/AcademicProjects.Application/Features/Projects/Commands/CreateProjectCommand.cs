using AcademicProjects.Application.Features.Projects.DTOs;
using AcademicProjects.Domain.Enums;
using MediatR;

namespace AcademicProjects.Application.Features.Projects.Commands;

public sealed record CreateProjectCommand(
    string Title,
    string? Description,
    ProjectStatus Status,
    Guid CategoryId) : IRequest<ProjectDto>;