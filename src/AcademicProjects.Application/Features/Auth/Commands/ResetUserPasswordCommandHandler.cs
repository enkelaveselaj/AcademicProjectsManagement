using AcademicProjects.Application.Authentication;
using MediatR;

namespace AcademicProjects.Application.Features.Auth.Commands;

public sealed class ResetUserPasswordCommandHandler(IUserManagementService userManagementService)
    : IRequestHandler<ResetUserPasswordCommand, ServiceResult<bool>>
{
    public Task<ServiceResult<bool>> Handle(
        ResetUserPasswordCommand request,
        CancellationToken cancellationToken) =>
        userManagementService.ResetUserPasswordAsync(request.UserId, request.NewPassword, cancellationToken);
}
