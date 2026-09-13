using AcademicProjects.Application.Authentication;
using MediatR;

namespace AcademicProjects.Application.Features.Auth.Queries;

public sealed class GetUserDirectoryQueryHandler(IUserManagementService userManagementService)
    : IRequestHandler<GetUserDirectoryQuery, IReadOnlyList<UserDirectoryEntry>>
{
    public Task<IReadOnlyList<UserDirectoryEntry>> Handle(
        GetUserDirectoryQuery request,
        CancellationToken cancellationToken) =>
        userManagementService.GetUserDirectoryAsync(cancellationToken);
}
