using MediatR;

namespace AcademicProjects.Application.Features.Comments.Commands;

public sealed record DeleteCommentCommand(
    Guid Id) : IRequest;