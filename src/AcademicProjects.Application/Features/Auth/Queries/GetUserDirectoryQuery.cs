using AcademicProjects.Application.Authentication;
using MediatR;

namespace AcademicProjects.Application.Features.Auth.Queries;

public sealed record GetUserDirectoryQuery : IRequest<IReadOnlyList<UserDirectoryEntry>>;
