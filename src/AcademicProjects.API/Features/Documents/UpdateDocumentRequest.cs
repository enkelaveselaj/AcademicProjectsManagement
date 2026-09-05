namespace AcademicProjects.API.Features.Documents;

public sealed record UpdateDocumentRequest(
    string FileName,
    string FilePath,
    Guid ProjectId);