using FluentValidation;

namespace AcademicProjects.Application.Features.Projects.Commands;

public sealed class CreateProjectCommandValidator
    : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(command => command.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.Description)
            .MaximumLength(4000);

        RuleFor(command => command.CategoryId)
            .NotEmpty();

        RuleFor(command => command.Status)
            .IsInEnum();
    }
}