using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HospitalAppointmentSystem.Domain.Entities;

namespace HospitalAppointmentSystem.Infrastructure.Configurations;

public class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
{
    public void Configure(EntityTypeBuilder<Schedule> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.DayOfWeek)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.StartTime)
            .IsRequired();

        builder.Property(s => s.EndTime)
            .IsRequired();

        builder.Property(s => s.IsAvailable)
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(s => new { s.DoctorId, s.DayOfWeek })
            .IsUnique();
    }
}
