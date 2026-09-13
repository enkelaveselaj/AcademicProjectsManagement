namespace AcademicProjects.Application.Common.Authorization;

public enum ProjectAccessLevel
{
    /// <summary>Any member of the project (or an administrator).</summary>
    Member,

    /// <summary>Only the project's mentor (or an administrator).</summary>
    Mentor
}

/// <summary>
/// A MediatR request that acts on a specific project and requires the caller to have at least
/// <see cref="RequiredAccess"/> on it. Implementing this is enough to have <see cref="AcademicProjects.Application.Common.Behaviors.AuthorizationBehavior{TRequest,TResponse}"/>
/// enforce the check before the handler runs - no per-handler authorization code needed.
/// </summary>
public interface IProjectScopedRequest
{
    Guid ProjectId { get; }

    ProjectAccessLevel RequiredAccess { get; }
}
