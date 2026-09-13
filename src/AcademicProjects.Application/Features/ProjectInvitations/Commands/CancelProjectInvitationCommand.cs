using MediatR;

namespace AcademicProjects.Application.Features.ProjectInvitations.Commands;

public sealed record CancelProjectInvitationCommand(
    Guid Id) : IRequest;
