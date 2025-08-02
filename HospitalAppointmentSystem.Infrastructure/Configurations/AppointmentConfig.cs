// This file serves as an alias to AppointmentConfiguration.cs to match the exact naming requirement
// The actual configuration is implemented in AppointmentConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HospitalAppointmentSystem.Domain.Entities;

namespace HospitalAppointmentSystem.Infrastructure.Configurations;

public class AppointmentConfig : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        // Delegate to the main configuration
        var mainConfig = new AppointmentConfiguration();
        mainConfig.Configure(builder);
    }
}
