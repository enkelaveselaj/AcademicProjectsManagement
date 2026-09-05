using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Documents.Commands;

public sealed class DeleteDocumentCommandHandler(
    IApplicationDbContext context)
    : IRequestHandler<DeleteDocumentCommand, bool>
{
    public async Task<bool> Handle(
        DeleteDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var document = await context.Documents
            .FirstOrDefaultAsync(
                document => document.Id == request.Id,
                cancellationToken);

        if (document is null)
        {
            return false;
        }

        context.Documents.Remove(document);

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}