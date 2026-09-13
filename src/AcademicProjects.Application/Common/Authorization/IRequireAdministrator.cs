namespace AcademicProjects.Application.Common.Authorization;

/// <summary>
/// A MediatR request that only an administrator may execute. Implementing this is enough to have
/// <see cref="AcademicProjects.Application.Common.Behaviors.AuthorizationBehavior{TRequest,TResponse}"/>
/// enforce the check before the handler runs - no per-handler authorization code needed.
/// </summary>
public interface IRequireAdministrator;
