using AcademicProjects.Application.Authentication;
using MediatR;

namespace AcademicProjects.Application.Features.Auth.Commands;

public sealed class ChangeUserRoleCommandHandler(IUserManagementService userManagementService)
    : IRequestHandler<ChangeUserRoleCommand, ServiceResult<UserSummary>>
{
    public Task<ServiceResult<UserSummary>> Handle(
        ChangeUserRoleCommand request,
        CancellationToken cancellationToken) =>
        userManagementService.ChangeUserRoleAsync(request.UserId, request.Role, cancellationToken);
}
