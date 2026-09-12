using FluentValidation;

namespace AcademicProjects.Application.Features.Documents.Queries;

public sealed class GetDocumentFileQueryValidator : AbstractValidator<GetDocumentFileQuery>
{
    public GetDocumentFileQueryValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}
