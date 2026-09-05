using AcademicProjects.Application.Features.Comments.DTOs;
using MediatR;

namespace AcademicProjects.Application.Features.Comments.Queries;

public sealed record GetCommentByIdQuery(
    Guid Id) : IRequest<CommentDto?>;