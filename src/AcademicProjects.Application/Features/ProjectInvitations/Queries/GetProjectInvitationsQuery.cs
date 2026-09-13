using AcademicProjects.Application.Features.ProjectInvitations.DTOs;
using MediatR;

namespace AcademicProjects.Application.Features.ProjectInvitations.Queries;

public sealed record GetProjectInvitationsQuery : IRequest<IReadOnlyList<ProjectInvitationDto>>;
