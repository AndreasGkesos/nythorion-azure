using MediatR;
using Microsoft.EntityFrameworkCore;
using Nythorion.Documents.Data;

namespace Nythorion.Documents.Features.RenameDocument;

internal sealed class RenameDocumentHandler(IDocumentsDbContext db) : IRequestHandler<RenameDocumentCommand>
{
    public async Task Handle(RenameDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await db.Documents
            .FirstOrDefaultAsync(d => d.Id == request.DocumentId && d.UserId == request.UserId, cancellationToken);

        if (document is null) throw new KeyNotFoundException($"Document {request.DocumentId} not found.");

        document.Rename(request.DisplayName);
        await db.SaveChangesAsync(cancellationToken);
    }
}
