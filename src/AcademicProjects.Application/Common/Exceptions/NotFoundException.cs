namespace AcademicProjects.Application.Common.Exceptions;

public sealed class NotFoundException(string entityName, object key)
    : Exception($"{entityName} with ID '{key}' was not found.")
{
    public string EntityName { get; } = entityName;

    public object Key { get; } = key;
}
