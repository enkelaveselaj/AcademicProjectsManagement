using AcademicProjects.Application.Features.ProjectMilestones.DTOs;
using MediatR;

namespace AcademicProjects.Application.Features.ProjectMilestones.Commands;

public sealed record CreateProjectMilestoneCommand(
    string Title,
    string? Description,
    DateTime DueDate,
    Guid ProjectId) : IRequest<ProjectMilestoneDto>;
