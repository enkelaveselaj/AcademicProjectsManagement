using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Features.Comments.DTOs;
using MediatR;

namespace AcademicProjects.Application.Features.Comments.Commands;

public sealed record CreateCommentCommand(
    string Content,
    Guid ProjectId) : IRequest<CommentDto>, IProjectScopedRequest
{
    public ProjectAccessLevel RequiredAccess => ProjectAccessLevel.Member;
}