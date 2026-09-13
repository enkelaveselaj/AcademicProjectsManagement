using AcademicProjects.Application.Authentication;
using AcademicProjects.Application.Common.Authorization;
using MediatR;

namespace AcademicProjects.Application.Features.Auth.Commands;

public sealed record ResetUserPasswordCommand(Guid UserId, string NewPassword)
    : IRequest<ServiceResult<bool>>, IRequireAdministrator;
