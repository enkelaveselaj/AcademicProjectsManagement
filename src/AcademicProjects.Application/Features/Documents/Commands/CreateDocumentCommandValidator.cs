using FluentValidation;

namespace AcademicProjects.Application.Features.Documents.Commands;

public sealed class CreateDocumentCommandValidator
    : AbstractValidator<CreateDocumentCommand>
{
    public CreateDocumentCommandValidator()
    {
        RuleFor(document => document.FileName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(document => document.FilePath)
            .NotEmpty()
            .MaximumLength(1_000);

        RuleFor(document => document.ProjectId)
            .NotEmpty();
    }
}