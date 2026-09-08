using AcademicProjects.Application.Features.ProjectAssignments.DTOs;
using MediatR;

namespace AcademicProjects.Application.Features.ProjectAssignments.Commands;

public sealed record CreateProjectAssignmentCommand(
    Guid ProjectId,
    Guid UserId,
    string Role) : IRequest<ProjectAssignmentDto>;
