using MediatR;
using Microsoft.EntityFrameworkCore;
using Nythorion.Learning.Data;
using Nythorion.Learning.Features.GenerateSummary;

namespace Nythorion.Learning.Features.GetSummary;

internal sealed class GetSummaryHandler(ILearningDbContext db)
    : IRequestHandler<GetSummaryQuery, GenerateSummaryResult?>
{
    public async Task<GenerateSummaryResult?> Handle(GetSummaryQuery request, CancellationToken cancellationToken)
    {
        var summary = await db.Summaries
            .FirstOrDefaultAsync(s => s.DocumentId == request.DocumentId, cancellationToken);

        return summary is null ? null : new GenerateSummaryResult(summary.Id, summary.Content);
    }
}
