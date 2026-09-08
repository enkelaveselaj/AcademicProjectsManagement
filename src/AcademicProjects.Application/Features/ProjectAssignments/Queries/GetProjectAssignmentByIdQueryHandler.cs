using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.ProjectAssignments.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectAssignments.Queries;

public sealed class GetProjectAssignmentByIdQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetProjectAssignmentByIdQuery, ProjectAssignmentDto>
{
    public async Task<ProjectAssignmentDto> Handle(
        GetProjectAssignmentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var assignment = await context.ProjectAssignments
            .AsNoTracking()
            .Where(assignment => assignment.Id == request.Id)
            .Select(assignment => new ProjectAssignmentDto(
                assignment.Id,
                assignment.ProjectId,
                assignment.UserId,
                assignment.Role,
                assignment.CreatedAt,
                assignment.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return assignment ?? throw new NotFoundException("ProjectAssignment", request.Id);
    }
}
