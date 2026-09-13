using AcademicProjects.Application.Authentication;
using AcademicProjects.Application.Common.Authorization;
using MediatR;

namespace AcademicProjects.Application.Features.Auth.Queries;

public sealed record GetPendingUsersQuery : IRequest<IReadOnlyList<PendingUser>>, IRequireAdministrator;
