using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Interfaces;
using MediatR;

namespace AcademicProjects.Application.Common.Behaviors;

/// <summary>
/// Enforces the "admin, or project member/mentor" checks that used to be hand-written at the top
/// of most handlers. A request opts in by implementing <see cref="IRequireAdministrator"/> or
/// <see cref="IProjectScopedRequest"/> - handlers no longer need to (and no longer can forget to)
/// perform this check themselves. Requests that need to authorize against an entity only
/// resolvable by loading it first (e.g. "the author of this comment") still do that in the
/// handler, since the entity has to be loaded there anyway.
/// </summary>
public sealed class AuthorizationBehavior<TRequest, TResponse>(
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAdministrator())
        {
            if (request is IRequireAdministrator)
            {
                throw new ForbiddenAccessException("Only an administrator can perform this action.");
            }

            if (request is IProjectScopedRequest scopedRequest)
            {
                var userId = currentUser.GetUserId();

                var hasAccess = scopedRequest.RequiredAccess == ProjectAccessLevel.Mentor
                    ? await projectAccess.IsProjectMentorAsync(scopedRequest.ProjectId, userId, cancellationToken)
                    : await projectAccess.IsMemberAsync(scopedRequest.ProjectId, userId, cancellationToken);

                if (!hasAccess)
                {
                    var requirement = scopedRequest.RequiredAccess == ProjectAccessLevel.Mentor
                        ? "the project's mentor"
                        : "a project member";

                    throw new ForbiddenAccessException($"Only {requirement} or an administrator can perform this action.");
                }
            }
        }

        return await next();
    }
}
