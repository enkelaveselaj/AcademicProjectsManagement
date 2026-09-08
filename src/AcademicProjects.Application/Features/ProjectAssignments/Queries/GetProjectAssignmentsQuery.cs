using AcademicProjects.Application.Features.ProjectAssignments.DTOs;
using MediatR;

namespace AcademicProjects.Application.Features.ProjectAssignments.Queries;

public sealed record GetProjectAssignmentsQuery
    : IRequest<IReadOnlyList<ProjectAssignmentDto>>;
