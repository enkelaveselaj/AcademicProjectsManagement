using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Documents.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Documents.Commands;

public sealed class UpdateDocumentCommandHandler(
    IApplicationDbContext context)
    : IRequestHandler<UpdateDocumentCommand, DocumentDto>
{
    public async Task<DocumentDto> Handle(
        UpdateDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var document = await context.Documents
            .FirstOrDefaultAsync(
                document => document.Id == request.Id,
                cancellationToken);

        if (document is null)
        {
            throw new NotFoundException("Document", request.Id);
        }

        var projectExists = await context.Projects
            .AnyAsync(
                project => project.Id == request.ProjectId,
                cancellationToken);

        if (!projectExists)
        {
            throw new NotFoundException("Project", request.ProjectId);
        }

        document.FileName = request.FileName.Trim();
        document.FilePath = request.FilePath.Trim();
        document.ProjectId = request.ProjectId;

        await context.SaveChangesAsync(cancellationToken);

        return new DocumentDto(
            document.Id,
            document.FileName,
            document.FilePath,
            document.ProjectId,
            document.CreatedAt,
            document.UpdatedAt);
    }
}