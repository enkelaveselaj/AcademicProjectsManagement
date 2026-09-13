using AcademicProjects.Application.Authentication;
using MediatR;

namespace AcademicProjects.Application.Features.Auth.Commands;

public sealed record LoginCommand(string Email, string Password) : IRequest<AccessToken>;
