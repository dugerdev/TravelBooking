using System;

namespace TravelBooking.Domain.Events;

public class NewsArticlePublishedEvent : IDomainEvent
{
    public Guid NewsArticleId { get; }
    public string Title { get; }
    public string Category { get; }
    public DateTime PublishDate { get; }
    public DateTime DateOccurred { get; } = DateTime.UtcNow;

    public NewsArticlePublishedEvent(Guid newsArticleId, string title, string category, DateTime publishDate)
    {
        NewsArticleId = newsArticleId;
        Title = title;
        Category = category;
        PublishDate = publishDate;
    }
}
