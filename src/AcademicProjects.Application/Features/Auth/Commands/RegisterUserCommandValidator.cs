using AcademicProjects.Domain.Enums;
using FluentValidation;

namespace AcademicProjects.Application.Features.Auth.Commands;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(command => command.FirstName)
            .NotEmpty()
            .WithMessage("First name is required.");

        RuleFor(command => command.LastName)
            .NotEmpty()
            .WithMessage("Last name is required.");

        RuleFor(command => command.Email)
            .NotEmpty()
            .WithMessage("Email is required.");

        RuleFor(command => command.Password)
            .NotEmpty()
            .WithMessage("Password is required.");

        RuleFor(command => command.PersonalIdNumber)
            .NotEmpty()
            .WithMessage("A personal ID number is required.");

        RuleFor(command => command.RequestedRole)
            .Must(role => role is UserRole.Student or UserRole.Mentor)
            .WithMessage("You can only request a Student or Mentor account.");

        RuleFor(command => command.StudentId)
            .NotEmpty()
            .When(command => command.RequestedRole == UserRole.Student)
            .WithMessage("A student ID is required for a Student account.");

        RuleFor(command => command.DateOfBirth)
            .Must(dateOfBirth => dateOfBirth.Date <= DateTime.UtcNow.Date)
            .WithMessage("Date of birth cannot be in the future.")
            .Must(dateOfBirth => dateOfBirth.Date <= DateTime.UtcNow.Date.AddYears(-16))
            .WithMessage("You must be at least 16 years old to register.");
    }
}
