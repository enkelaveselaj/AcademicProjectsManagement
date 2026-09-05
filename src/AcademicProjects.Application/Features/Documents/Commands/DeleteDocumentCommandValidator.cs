using FluentValidation;

namespace AcademicProjects.Application.Features.Documents.Commands;

public sealed class DeleteDocumentCommandValidator
    : AbstractValidator<DeleteDocumentCommand>
{
    public DeleteDocumentCommandValidator()
    {
        RuleFor(document => document.Id)
            .NotEmpty();
    }
}