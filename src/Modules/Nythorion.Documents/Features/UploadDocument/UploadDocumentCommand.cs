using MediatR;

namespace Nythorion.Documents.Features.UploadDocument;

public record UploadDocumentCommand(
    string UserId,
    string DisplayName,
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSizeBytes) : IRequest<UploadDocumentResult>;

public record UploadDocumentResult(Guid DocumentId, int ChunkCount);
