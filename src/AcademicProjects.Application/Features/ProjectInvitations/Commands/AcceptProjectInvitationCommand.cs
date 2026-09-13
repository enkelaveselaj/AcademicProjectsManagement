using AcademicProjects.Application.Features.ProjectInvitations.DTOs;
using MediatR;

namespace AcademicProjects.Application.Features.ProjectInvitations.Commands;

public sealed record AcceptProjectInvitationCommand(
    Guid Id) : IRequest<ProjectInvitationDto>;
