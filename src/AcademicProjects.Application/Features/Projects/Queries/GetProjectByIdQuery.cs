using AcademicProjects.Application.Features.Projects.DTOs;
using MediatR;

namespace AcademicProjects.Application.Features.Projects.Queries;

public sealed record GetProjectByIdQuery(Guid Id)
    : IRequest<ProjectDto?>;