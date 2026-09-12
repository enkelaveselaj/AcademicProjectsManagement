using AcademicProjects.Application.Features.ProjectMilestones.DTOs;
using AcademicProjects.Domain.Enums;
using MediatR;

namespace AcademicProjects.Application.Features.ProjectMilestones.Commands;

public sealed record UpdateProjectMilestoneCommand(
    Guid Id,
    string Title,
    string? Description,
    DateTime DueDate,
    MilestoneStatus Status,
    Guid ProjectId) : IRequest<ProjectMilestoneDto>;
