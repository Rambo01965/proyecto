using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HospitalAppointmentSystem.Domain.Entities;

namespace HospitalAppointmentSystem.Infrastructure.Configurations;

public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Rating)
            .IsRequired()
            .HasAnnotation("Range", new[] { 1, 5 });

        builder.Property(f => f.Comment)
            .HasMaxLength(1000);

        builder.Property(f => f.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(f => f.AppointmentId)
            .IsUnique();
    }
}
