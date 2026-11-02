using MediatR;

namespace Nythorion.Documents.Features.GetDocuments;

public sealed record GetDocumentsQuery(string UserId) : IRequest<List<DocumentDto>>;

public sealed record DocumentDto(Guid Id, string DisplayName, string FileName, string ContentType, long FileSizeBytes, string Status, DateTime UploadedAt);
