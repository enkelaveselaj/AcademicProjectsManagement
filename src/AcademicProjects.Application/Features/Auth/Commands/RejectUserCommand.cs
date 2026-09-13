using AcademicProjects.Application.Authentication;
using AcademicProjects.Application.Common.Authorization;
using MediatR;

namespace AcademicProjects.Application.Features.Auth.Commands;

public sealed record RejectUserCommand(Guid UserId)
    : IRequest<ServiceResult<bool>>, IRequireAdministrator;
