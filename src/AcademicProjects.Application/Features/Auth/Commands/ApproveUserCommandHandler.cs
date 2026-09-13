using AcademicProjects.Application.Authentication;
using MediatR;

namespace AcademicProjects.Application.Features.Auth.Commands;

public sealed class ApproveUserCommandHandler(IUserManagementService userManagementService)
    : IRequestHandler<ApproveUserCommand, ServiceResult<UserSummary>>
{
    public Task<ServiceResult<UserSummary>> Handle(
        ApproveUserCommand request,
        CancellationToken cancellationToken) =>
        userManagementService.ApproveUserAsync(request.UserId, cancellationToken);
}
