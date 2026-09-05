using FluentValidation;

namespace AcademicProjects.Application.Features.Documents.Commands;

public sealed class UpdateDocumentCommandValidator
    : AbstractValidator<UpdateDocumentCommand>
{
    public UpdateDocumentCommandValidator()
    {
        RuleFor(document => document.Id)
            .NotEmpty();

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