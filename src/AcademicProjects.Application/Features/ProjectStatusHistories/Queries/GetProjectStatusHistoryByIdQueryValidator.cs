using FluentValidation;

namespace AcademicProjects.Application.Features.ProjectStatusHistories.Queries;

public sealed class GetProjectStatusHistoryByIdQueryValidator
    : AbstractValidator<GetProjectStatusHistoryByIdQuery>
{
    public GetProjectStatusHistoryByIdQueryValidator()
    {
        RuleFor(history => history.Id)
            .NotEmpty();
    }
}
