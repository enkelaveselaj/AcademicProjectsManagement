using AcademicProjects.Application.Features.ProjectStatusHistories.DTOs;
using MediatR;

namespace AcademicProjects.Application.Features.ProjectStatusHistories.Queries;

public sealed record GetProjectStatusHistoryByIdQuery(
    Guid Id) : IRequest<ProjectStatusHistoryDto?>;
