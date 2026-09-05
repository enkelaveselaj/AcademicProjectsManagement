using MediatR;

namespace AcademicProjects.Application.Features.Projects.Commands;

public sealed record DeleteProjectCommand(Guid Id) : IRequest<bool>;