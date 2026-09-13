using AcademicProjects.Application.Authentication;
using AcademicProjects.Domain.Enums;
using MediatR;

namespace AcademicProjects.Application.Features.Auth.Commands;

public sealed record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    DateTime DateOfBirth,
    string PersonalIdNumber,
    UserRole RequestedRole,
    string? StudentId) : IRequest<ServiceResult<RegisteredUser>>;
