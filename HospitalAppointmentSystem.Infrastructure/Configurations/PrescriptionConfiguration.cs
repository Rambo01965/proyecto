using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HospitalAppointmentSystem.Domain.Entities;

namespace HospitalAppointmentSystem.Infrastructure.Configurations;

public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Medication)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Dosage)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Instructions)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(p => p.IssueDate)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(p => p.ExpiryDate)
            .IsRequired();

        builder.Property(p => p.RenewalCount)
            .HasDefaultValue(0);
    }
}
