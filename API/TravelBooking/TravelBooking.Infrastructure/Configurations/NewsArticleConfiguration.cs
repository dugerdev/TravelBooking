using TravelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace TravelBooking.Infrastructure.Configurations;

/// <summary>
/// NewsArticle entity icin Fluent API konfigurasyonlari
/// </summary>
public class NewsArticleConfiguration : IEntityTypeConfiguration<NewsArticle>
{
    public void Configure(EntityTypeBuilder<NewsArticle> builder)
    {
        builder.ToTable("News");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(300)
            .HasComment("Haber basligi");

        builder.Property(n => n.Summary)
            .IsRequired()
            .HasMaxLength(1000)
            .HasComment("Ozet");

        builder.Property(n => n.Content)
            .IsRequired()
            .HasColumnType("nvarchar(max)")
            .HasComment("Icerik");

        builder.Property(n => n.Category)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Kategori");

        builder.Property(n => n.PublishDate)
            .IsRequired()
            .HasComment("Yayin tarihi");

        builder.Property(n => n.Author)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Yazar");

        builder.Property(n => n.ImageUrl)
            .HasMaxLength(500)
            .HasComment("Gorsel URL");

        builder.Property(n => n.ViewCount)
            .HasComment("Goruntulenme sayisi");

        // JSON column for Tags list (using backing field)
        var listComparer = new ValueComparer<IReadOnlyCollection<string>>(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());
        builder.Property(n => n.Tags)
            .HasField("_tags")
            .HasConversion(
                v => JsonSerializer.Serialize(v.ToList(), (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)")
            .HasComment("Etiketler (JSON)")
            .Metadata.SetValueComparer(listComparer);

        builder.Property(n => n.IsPublished)
            .IsRequired()
            .HasComment("Yayinda mi?");

        // Index'ler
        builder.HasIndex(n => n.Category);
        builder.HasIndex(n => n.PublishDate);
        builder.HasIndex(n => n.IsPublished);
    }
}
