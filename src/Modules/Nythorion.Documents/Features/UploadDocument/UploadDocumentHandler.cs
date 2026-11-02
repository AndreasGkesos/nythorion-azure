using MediatR;
using Microsoft.EntityFrameworkCore;
using Nythorion.Documents.Data;
using Nythorion.Documents.Domain;
using Nythorion.Documents.Parsing;
using Nythorion.Documents.Processing;

namespace Nythorion.Documents.Features.UploadDocument;

public class UploadDocumentHandler(
    IDocumentsDbContext db,
    IEnumerable<IDocumentParser> parsers,
    IDocumentProcessingQueue queue)
    : IRequestHandler<UploadDocumentCommand, UploadDocumentResult>
{
    public async Task<UploadDocumentResult> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        var parser = parsers.FirstOrDefault(p => p.CanParse(request.ContentType))
            ?? throw new NotSupportedException($"No parser found for content type '{request.ContentType}'.");

        var text = parser.ExtractText(request.FileStream);
        var chunks = TextChunker.Chunk(text);

        var displayName = await ResolveDisplayNameAsync(request.UserId, request.DisplayName, cancellationToken);
        var document = Document.Create(request.UserId, displayName, request.FileName, request.ContentType, request.FileSizeBytes);

        foreach (var (chunk, index) in chunks.Select((c, i) => (c, i)))
            document.Chunks.Add(DocumentChunk.Create(document.Id, index, chunk));

        db.Documents.Add(document);
        await db.SaveChangesAsync(cancellationToken);

        await queue.EnqueueAsync(document.Id, cancellationToken);

        return new UploadDocumentResult(document.Id, chunks.Count);
    }

    private async Task<string> ResolveDisplayNameAsync(string userId, string desiredName, CancellationToken ct)
    {
        var existingNames = await db.Documents
            .Where(d => d.UserId == userId)
            .Select(d => d.DisplayName)
            .ToListAsync(ct);

        if (!existingNames.Contains(desiredName))
            return desiredName;

        var counter = 1;
        string candidate;
        do { candidate = $"{desiredName} ({counter++})"; }
        while (existingNames.Contains(candidate));

        return candidate;
    }
}
