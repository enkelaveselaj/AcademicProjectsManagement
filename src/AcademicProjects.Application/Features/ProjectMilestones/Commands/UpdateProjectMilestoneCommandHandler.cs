using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.ProjectMilestones.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectMilestones.Commands;

public sealed class UpdateProjectMilestoneCommandHandler(
    IApplicationDbContext context)
    : IRequestHandler<UpdateProjectMilestoneCommand, ProjectMilestoneDto>
{
    public async Task<ProjectMilestoneDto> Handle(
        UpdateProjectMilestoneCommand request,
        CancellationToken cancellationToken)
    {
        var milestone = await context.ProjectMilestones
            .FirstOrDefaultAsync(
                milestone => milestone.Id == request.Id,
                cancellationToken);

        if (milestone is null)
        {
            throw new NotFoundException("ProjectMilestone", request.Id);
        }

        var projectExists = await context.Projects
            .AnyAsync(
                project => project.Id == request.ProjectId,
                cancellationToken);

        if (!projectExists)
        {
            throw new NotFoundException("Project", request.ProjectId);
        }

        milestone.Title = request.Title.Trim();
        milestone.ProjectId = request.ProjectId;

        await context.SaveChangesAsync(cancellationToken);

        return new ProjectMilestoneDto(
            milestone.Id,
            milestone.Title,
            milestone.ProjectId,
            milestone.CreatedAt,
            milestone.UpdatedAt);
    }
}
