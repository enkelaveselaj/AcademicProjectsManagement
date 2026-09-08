using AcademicProjects.Application.Features.ProjectMilestones.DTOs;
using MediatR;

namespace AcademicProjects.Application.Features.ProjectMilestones.Queries;

public sealed record GetProjectMilestonesQuery
    : IRequest<IReadOnlyList<ProjectMilestoneDto>>;
