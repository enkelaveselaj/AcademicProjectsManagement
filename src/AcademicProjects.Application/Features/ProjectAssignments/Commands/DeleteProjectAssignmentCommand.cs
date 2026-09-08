using MediatR;

namespace AcademicProjects.Application.Features.ProjectAssignments.Commands;

public sealed record DeleteProjectAssignmentCommand(
    Guid Id) : IRequest<bool>;
