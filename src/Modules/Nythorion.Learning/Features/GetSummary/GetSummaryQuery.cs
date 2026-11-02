using MediatR;
using Nythorion.Learning.Features.GenerateSummary;

namespace Nythorion.Learning.Features.GetSummary;

public sealed record GetSummaryQuery(Guid DocumentId) : IRequest<GenerateSummaryResult?>;
