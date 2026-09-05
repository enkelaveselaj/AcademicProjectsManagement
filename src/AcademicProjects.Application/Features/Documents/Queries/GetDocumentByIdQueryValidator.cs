using FluentValidation;

namespace AcademicProjects.Application.Features.Documents.Queries;

public sealed class GetDocumentByIdQueryValidator
    : AbstractValidator<GetDocumentByIdQuery>
{
    public GetDocumentByIdQueryValidator()
    {
        RuleFor(document => document.Id)
            .NotEmpty();
    }
}