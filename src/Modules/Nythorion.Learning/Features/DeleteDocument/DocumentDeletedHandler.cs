using MediatR;
using Microsoft.EntityFrameworkCore;
using Nythorion.Documents.Features.DeleteDocument;
using Nythorion.Learning.Data;

namespace Nythorion.Learning.Features.DeleteDocument;

internal sealed class DocumentDeletedHandler(ILearningDbContext db)
    : INotificationHandler<DocumentDeletedNotification>
{
    public async Task Handle(DocumentDeletedNotification notification, CancellationToken cancellationToken)
    {
        await db.Summaries.Where(s => s.DocumentId == notification.DocumentId).ExecuteDeleteAsync(cancellationToken);
        await db.Flashcards.Where(f => f.DocumentId == notification.DocumentId).ExecuteDeleteAsync(cancellationToken);
        await db.QuizQuestions.Where(q => q.DocumentId == notification.DocumentId).ExecuteDeleteAsync(cancellationToken);
        await db.Notes.Where(n => n.DocumentId == notification.DocumentId).ExecuteDeleteAsync(cancellationToken);
    }
}
