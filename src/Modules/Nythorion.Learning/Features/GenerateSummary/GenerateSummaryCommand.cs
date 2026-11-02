using MediatR;

namespace Nythorion.Learning.Features.GenerateSummary;

public sealed record GenerateSummaryCommand(Guid DocumentId, string UserId) : IRequest<GenerateSummaryResult>;

public sealed record GenerateSummaryResult(Guid SummaryId, string Content);
