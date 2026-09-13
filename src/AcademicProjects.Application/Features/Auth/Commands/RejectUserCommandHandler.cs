using AcademicProjects.Application.Authentication;
using MediatR;

namespace AcademicProjects.Application.Features.Auth.Commands;

public sealed class RejectUserCommandHandler(IUserManagementService userManagementService)
    : IRequestHandler<RejectUserCommand, ServiceResult<bool>>
{
    public Task<ServiceResult<bool>> Handle(
        RejectUserCommand request,
        CancellationToken cancellationToken) =>
        userManagementService.RejectUserAsync(request.UserId, cancellationToken);
}
