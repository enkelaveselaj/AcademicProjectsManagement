using AcademicProjects.Application.Features.Comments.DTOs;
using MediatR;

namespace AcademicProjects.Application.Features.Comments.Commands;

public sealed record UpdateCommentCommand(
    Guid Id,
    string Content,
    Guid ProjectId) : IRequest<CommentDto>;