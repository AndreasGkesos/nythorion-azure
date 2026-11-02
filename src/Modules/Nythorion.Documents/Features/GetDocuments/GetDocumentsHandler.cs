using MediatR;
using Microsoft.EntityFrameworkCore;
using Nythorion.Documents.Data;

namespace Nythorion.Documents.Features.GetDocuments;

internal sealed class GetDocumentsHandler(IDocumentsDbContext db)
    : IRequestHandler<GetDocumentsQuery, List<DocumentDto>>
{
    public async Task<List<DocumentDto>> Handle(GetDocumentsQuery request, CancellationToken cancellationToken)
    {
        return await db.Documents
            .Where(d => d.UserId == request.UserId)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => new DocumentDto(d.Id, d.DisplayName, d.FileName, d.ContentType, d.FileSizeBytes, d.Status.ToString(), d.UploadedAt))
            .ToListAsync(cancellationToken);
    }
}
