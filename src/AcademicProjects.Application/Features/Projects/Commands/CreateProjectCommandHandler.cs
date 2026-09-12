using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Projects.DTOs;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Projects.Commands;

public sealed class CreateProjectCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    public async Task<ProjectDto> Handle(
        CreateProjectCommand request,
        CancellationToken cancellationToken)
    {
        var category = await context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(
                category => category.Id == request.CategoryId,
                cancellationToken);

        if (category is null)
        {
            throw new NotFoundException("Category", request.CategoryId);
        }

        var userId = currentUser.GetUserId();

        var project = new Project
        {
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            Status = request.Status,
            CategoryId = request.CategoryId,
            CreatedById = userId
        };

        context.Projects.Add(project);

        // The creator becomes the first project member (student-led or mentor-led projects
        // both start this way). Administrators create projects on the platform's behalf and
        // aren't participants, so they aren't added to the roster.
        var creatorRole = currentUser.IsInRole(UserRole.Mentor)
            ? UserRole.Mentor
            : currentUser.IsInRole(UserRole.Student)
                ? UserRole.Student
                : (UserRole?)null;

        if (creatorRole is not null)
        {
            context.ProjectAssignments.Add(new ProjectAssignment
            {
                ProjectId = project.Id,
                UserId = userId,
                Role = creatorRole.Value.ToString()
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        return new ProjectDto(
            project.Id,
            project.Title,
            project.Description,
            project.Status,
            project.CategoryId,
            category.Name,
            project.CreatedById,
            project.CreatedAt,
            project.UpdatedAt);
    }
}
