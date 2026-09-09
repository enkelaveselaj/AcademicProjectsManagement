using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Documents.DTOs;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Documents.Commands;

public sealed class CreateDocumentCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess)
    : IRequestHandler<CreateDocumentCommand, DocumentDto>
{
    public async Task<DocumentDto> Handle(
        CreateDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var projectExists = await context.Projects
            .AnyAsync(
                project => project.Id == request.ProjectId,
                cancellationToken);

        if (!projectExists)
        {
            throw new NotFoundException("Project", request.ProjectId);
        }

        var userId = currentUser.GetUserId();

        if (!currentUser.IsAdministrator()
            && !await projectAccess.IsMemberAsync(request.ProjectId, userId, cancellationToken))
        {
            throw new ForbiddenAccessException("Only members of this project can upload documents to it.");
        }

        var document = new Document
        {
            FileName = request.FileName.Trim(),
            FilePath = request.FilePath.Trim(),
            UploadedById = userId,
            ProjectId = request.ProjectId
        };

        context.Documents.Add(document);

        await context.SaveChangesAsync(cancellationToken);

        return new DocumentDto(
            document.Id,
            document.FileName,
            document.FilePath,
            document.UploadedById,
            document.ProjectId,
            document.CreatedAt,
            document.UpdatedAt);
    }
}
