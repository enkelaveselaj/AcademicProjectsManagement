using AcademicProjects.Application.Authentication;
using MediatR;

namespace AcademicProjects.Application.Features.Auth.Queries;

public sealed class GetUsersQueryHandler(IUserManagementService userManagementService)
    : IRequestHandler<GetUsersQuery, IReadOnlyList<UserSummary>>
{
    public Task<IReadOnlyList<UserSummary>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken) =>
        userManagementService.GetUsersAsync(cancellationToken);
}
