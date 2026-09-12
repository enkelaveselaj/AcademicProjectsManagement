using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Features.Documents.DTOs;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Documents.Commands;

public sealed class CreateDocumentCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess,
    ProjectNotificationService notifier,
    IFileStorageService fileStorage)
    : IRequestHandler<CreateDocumentCommand, DocumentDto>
{
    public async Task<DocumentDto> Handle(
        CreateDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var projectTitle = await context.Projects
            .Where(project => project.Id == request.ProjectId)
            .Select(project => project.Title)
            .FirstOrDefaultAsync(cancellationToken);

        if (projectTitle is null)
        {
            throw new NotFoundException("Project", request.ProjectId);
        }

        var userId = currentUser.GetUserId();

        if (!currentUser.IsAdministrator()
            && !await projectAccess.IsMemberAsync(request.ProjectId, userId, cancellationToken))
        {
            throw new ForbiddenAccessException("Only members of this project can upload documents to it.");
        }

        var storedFileName = await fileStorage.SaveAsync(
            request.Content,
            Path.GetExtension(request.FileName),
            cancellationToken);

        var document = new Document
        {
            FileName = request.FileName.Trim(),
            StoredFileName = storedFileName,
            ContentType = request.ContentType,
            FileSizeBytes = request.FileSizeBytes,
            UploadedById = userId,
            ProjectId = request.ProjectId
        };

        context.Documents.Add(document);

        await notifier.NotifyMembersAsync(
            request.ProjectId,
            userId,
            $"New document uploaded to project '{projectTitle}': {document.FileName}.",
            NotificationType.Information,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return new DocumentDto(
            document.Id,
            document.FileName,
            document.ContentType,
            document.FileSizeBytes,
            document.UploadedById,
            document.ProjectId,
            document.CreatedAt,
            document.UpdatedAt);
    }
}
