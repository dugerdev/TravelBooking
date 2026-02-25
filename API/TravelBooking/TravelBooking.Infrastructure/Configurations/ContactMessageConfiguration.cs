using TravelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelBooking.Infrastructure.Configurations;

public class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.ToTable("ContactMessages");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(256);
        builder.Property(c => c.Phone).HasMaxLength(50);
        builder.Property(c => c.Subject).IsRequired().HasMaxLength(300);
        builder.Property(c => c.Message).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(c => c.IsRead).IsRequired();
        builder.Property(c => c.ReadDate);
        builder.Property(c => c.ReadBy).HasMaxLength(450);
        builder.Property(c => c.Response).HasColumnType("nvarchar(max)");
        builder.Property(c => c.ResponseDate);
        builder.Property(c => c.ResponseBy).HasMaxLength(450);

        builder.HasIndex(c => c.IsRead);
        builder.HasIndex(c => c.CreatedDate);
    }
}
