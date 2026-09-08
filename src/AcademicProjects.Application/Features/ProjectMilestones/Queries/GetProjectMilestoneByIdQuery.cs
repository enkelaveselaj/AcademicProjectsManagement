using AcademicProjects.Application.Features.ProjectMilestones.DTOs;
using MediatR;

namespace AcademicProjects.Application.Features.ProjectMilestones.Queries;

public sealed record GetProjectMilestoneByIdQuery(
    Guid Id) : IRequest<ProjectMilestoneDto?>;
