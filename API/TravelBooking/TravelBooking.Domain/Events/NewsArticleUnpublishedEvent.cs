using System;

namespace TravelBooking.Domain.Events;

public class NewsArticleUnpublishedEvent : IDomainEvent
{
    public Guid NewsArticleId { get; }
    public string Title { get; }
    public DateTime DateOccurred { get; } = DateTime.UtcNow;

    public NewsArticleUnpublishedEvent(Guid newsArticleId, string title)
    {
        NewsArticleId = newsArticleId;
        Title = title;
    }
}
