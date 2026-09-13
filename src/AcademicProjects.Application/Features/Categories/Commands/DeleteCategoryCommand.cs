using AcademicProjects.Application.Common.Authorization;
using MediatR;

namespace AcademicProjects.Application.Features.Categories.Commands;

public sealed record DeleteCategoryCommand(Guid Id) : IRequest, IRequireAdministrator;