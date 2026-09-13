using AcademicProjects.Application.Features.ProjectInvitations.DTOs;
using MediatR;

namespace AcademicProjects.Application.Features.ProjectInvitations.Commands;

public sealed record DeclineProjectInvitationCommand(
    Guid Id) : IRequest<ProjectInvitationDto>;
