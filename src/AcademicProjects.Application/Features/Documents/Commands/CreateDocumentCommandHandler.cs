using AcademicProjects.Application.Features.Documents.DTOs;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Documents.Commands;

public sealed class CreateDocumentCommandHandler(
    IApplicationDbContext context)
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
            throw new KeyNotFoundException("Project not found.");
        }

        var document = new Document
        {
            FileName = request.FileName.Trim(),
            FilePath = request.FilePath.Trim(),
            ProjectId = request.ProjectId
        };

        context.Documents.Add(document);

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