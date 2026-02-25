using TravelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelBooking.Infrastructure.Configurations;

/// <summary>
/// Entity Framework configuration for the Testimonial entity.
/// </summary>
public class TestimonialConfiguration : IEntityTypeConfiguration<Testimonial>
{
    public void Configure(EntityTypeBuilder<Testimonial> builder)
    {
        builder.ToTable("Testimonials");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.CustomerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Location)
            .HasMaxLength(200);

        builder.Property(t => t.Comment)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(t => t.Rating)
            .IsRequired();

        builder.Property(t => t.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(t => t.IsApproved)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.ApprovedDate);

        builder.Property(t => t.ApprovedBy)
            .HasMaxLength(100);

        builder.Property(t => t.RejectionReason)
            .HasMaxLength(1000);

        builder.Property(t => t.CreatedDate)
            .IsRequired();

        builder.Property(t => t.UpdatedDate);

        // Index for filtering approved testimonials
        builder.HasIndex(t => t.IsApproved);
        
        // Index for sorting by creation date
        builder.HasIndex(t => t.CreatedDate);
    }
}
