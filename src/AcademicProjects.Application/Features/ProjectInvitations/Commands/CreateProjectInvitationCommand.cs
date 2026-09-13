using AcademicProjects.Application.Features.ProjectInvitations.DTOs;
using MediatR;

namespace AcademicProjects.Application.Features.ProjectInvitations.Commands;

public sealed record CreateProjectInvitationCommand(
    Guid ProjectId,
    Guid InvitedUserId,
    string Role) : IRequest<ProjectInvitationDto>;
