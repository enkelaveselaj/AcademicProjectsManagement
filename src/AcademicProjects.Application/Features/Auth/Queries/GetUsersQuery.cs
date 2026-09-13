using AcademicProjects.Application.Authentication;
using AcademicProjects.Application.Common.Authorization;
using MediatR;

namespace AcademicProjects.Application.Features.Auth.Queries;

public sealed record GetUsersQuery : IRequest<IReadOnlyList<UserSummary>>, IRequireAdministrator;
