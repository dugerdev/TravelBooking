using TravelBooking.Domain.Common;
using TravelBooking.Domain.Events;
using System.Linq;

namespace TravelBooking.Domain.Entities;

public class NewsArticle : BaseEntity, IAggregateRoot
{
    public string Title { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public DateTime PublishDate { get; private set; }
    public string Author { get; private set; } = string.Empty;
    public string ImageUrl { get; private set; } = string.Empty;
    public int ViewCount { get; private set; }
    public bool IsPublished { get; private set; } = false;

    private readonly List<string> _tags = [];
    public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();

    protected NewsArticle() { }

    public NewsArticle(
        string title,
        string summary,
        string content,
        string category,
        DateTime publishDate,
        string author,
        string imageUrl,
        List<string> tags)
    {
        Title = title.Trim();
        Summary = summary.Trim();
        Content = content.Trim();
        Category = category.Trim();
        PublishDate = publishDate;
        Author = author.Trim();
        ImageUrl = imageUrl.Trim();
        ViewCount = 0;
        IsPublished = false;

        if (tags != null)
            _tags.AddRange(tags.Select(t => t.Trim()));
    }

    /// <summary>
    /// Updates the article information.
    /// </summary>
    /// <param name="title">The title of the article.</param>
    /// <param name="summary">The summary of the article.</param>
    /// <param name="content">The full content of the article.</param>
    /// <param name="category">The category of the article.</param>
    /// <param name="publishDate">The publication date.</param>
    /// <param name="author">The author of the article.</param>
    /// <param name="imageUrl">The URL of the article's image.</param>
    /// <param name="tags">The list of tags for the article.</param>
    /// <param name="isPublished">Whether the article is published.</param>
    public void Update(
        string title,
        string summary,
        string content,
        string category,
        DateTime publishDate,
        string author,
        string imageUrl,
        List<string> tags,
        bool isPublished)
    {
        Title = title.Trim();
        Summary = summary.Trim();
        Content = content.Trim();
        Category = category.Trim();
        PublishDate = publishDate;
        Author = author.Trim();
        ImageUrl = imageUrl.Trim();
        
        var wasPublished = IsPublished;
        IsPublished = isPublished;

        _tags.Clear();
        if (tags != null)
            _tags.AddRange(tags.Select(t => t.Trim()));

        if (!wasPublished && isPublished)
            AddDomainEvent(new NewsArticlePublishedEvent(this.Id, Title, Category, PublishDate));
        else if (wasPublished && !isPublished)
            AddDomainEvent(new NewsArticleUnpublishedEvent(this.Id, Title));
    }

    /// <summary>
    /// Publishes the article.
    /// Triggers a domain event when the article is published.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the article is already published.</exception>
    public void Publish()
    {
        if (IsPublished)
            throw new InvalidOperationException("Makale zaten yayinlanmis.");

        IsPublished = true;
        if (PublishDate == default || PublishDate > DateTime.Now)
            PublishDate = DateTime.Now;

        AddDomainEvent(new NewsArticlePublishedEvent(this.Id, Title, Category, PublishDate));
    }

    /// <summary>
    /// Unpublishes the article.
    /// Triggers a domain event when the article is unpublished.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the article is not published.</exception>
    public void Unpublish()
    {
        if (!IsPublished)
            throw new InvalidOperationException("Makale zaten yayinlanmamis.");

        IsPublished = false;

        AddDomainEvent(new NewsArticleUnpublishedEvent(this.Id, Title));
    }

    /// <summary>
    /// Updates the content of the article (title, summary, and content).
    /// </summary>
    /// <param name="title">The new title.</param>
    /// <param name="summary">The new summary.</param>
    /// <param name="content">The new content.</param>
    /// <exception cref="ArgumentException">Thrown when any parameter is null or whitespace.</exception>
    public void UpdateContent(string title, string summary, string content)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Baslik bos olamaz.", nameof(title));
        if (string.IsNullOrWhiteSpace(summary))
            throw new ArgumentException("Ozet bos olamaz.", nameof(summary));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Icerik bos olamaz.", nameof(content));

        Title = title.Trim();
        Summary = summary.Trim();
        Content = content.Trim();
    }

    /// <summary>
    /// Adds a tag to the article.
    /// </summary>
    /// <param name="tag">The tag to add.</param>
    /// <exception cref="ArgumentException">Thrown when tag is null or whitespace.</exception>
    public void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentException("Tag bos olamaz.", nameof(tag));

        var trimmed = tag.Trim();
        if (!_tags.Contains(trimmed))
            _tags.Add(trimmed);
    }

    /// <summary>
    /// Removes a tag from the article.
    /// </summary>
    /// <param name="tag">The tag to remove.</param>
    public void RemoveTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return;

        _tags.Remove(tag.Trim());
    }

    /// <summary>
    /// Increments the view count for the article.
    /// </summary>
    public void IncrementViewCount()
    {
        ViewCount++;
    }
}
