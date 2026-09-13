using AcademicProjects.Application.Authentication;
using AcademicProjects.Application.Common.Authorization;
using MediatR;

namespace AcademicProjects.Application.Features.Auth.Commands;

public sealed record ApproveUserCommand(Guid UserId)
    : IRequest<ServiceResult<UserSummary>>, IRequireAdministrator;
