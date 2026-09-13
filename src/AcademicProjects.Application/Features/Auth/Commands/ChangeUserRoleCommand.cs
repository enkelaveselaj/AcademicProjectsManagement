using AcademicProjects.Application.Authentication;
using AcademicProjects.Application.Common.Authorization;
using MediatR;

namespace AcademicProjects.Application.Features.Auth.Commands;

public sealed record ChangeUserRoleCommand(Guid UserId, string Role)
    : IRequest<ServiceResult<UserSummary>>, IRequireAdministrator;
