using FluentValidation;

namespace AcademicProjects.Application.Features.Comments.Commands;

public sealed class CreateCommentCommandValidator
    : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(comment => comment.Content)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(comment => comment.ProjectId)
            .NotEmpty();
    }
}