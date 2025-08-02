// This file serves as an alias to DoctorConfiguration.cs to match the exact naming requirement
// The actual configuration is implemented in DoctorConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HospitalAppointmentSystem.Domain.Entities;

namespace HospitalAppointmentSystem.Infrastructure.Configurations;

public class DoctorConfig : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        // Delegate to the main configuration
        var mainConfig = new DoctorConfiguration();
        mainConfig.Configure(builder);
    }
}
