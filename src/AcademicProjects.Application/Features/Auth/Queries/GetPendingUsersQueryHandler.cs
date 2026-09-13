using AcademicProjects.Application.Authentication;
using MediatR;

namespace AcademicProjects.Application.Features.Auth.Queries;

public sealed class GetPendingUsersQueryHandler(IUserManagementService userManagementService)
    : IRequestHandler<GetPendingUsersQuery, IReadOnlyList<PendingUser>>
{
    public Task<IReadOnlyList<PendingUser>> Handle(
        GetPendingUsersQuery request,
        CancellationToken cancellationToken) =>
        userManagementService.GetPendingUsersAsync(cancellationToken);
}
