using FluentValidation;

namespace AcademicProjects.Application.Features.Notifications.Queries;

public sealed class GetNotificationByIdQueryValidator
    : AbstractValidator<GetNotificationByIdQuery>
{
    public GetNotificationByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}