using MediatR;

namespace AcademicProjects.Application.Features.ProjectMilestones.Commands;

public sealed record DeleteProjectMilestoneCommand(
    Guid Id) : IRequest;
