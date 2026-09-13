using AcademicProjects.Domain.Enums;

namespace AcademicProjects.Application.Features.ProjectInvitations.DTOs;

public sealed record ProjectInvitationDto(
    Guid Id,
    Guid ProjectId,
    Guid InvitedUserId,
    Guid InvitedById,
    string Role,
    InvitationStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
