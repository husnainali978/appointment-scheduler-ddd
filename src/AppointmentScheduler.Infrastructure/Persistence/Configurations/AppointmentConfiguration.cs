using AppointmentScheduler.Domain.Appointments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentScheduler.Infrastructure.Persistence.Configurations;

public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.ProviderId)
            .IsRequired();

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.Notes)
            .HasMaxLength(1000);

        builder.Property(a => a.CancellationReason)
            .HasMaxLength(500);

        builder.Property(a => a.CreatedAtUtc)
            .IsRequired();

        // TimeSlot and PatientInfo are value objects with no identity of
        // their own - they're owned by (and stored inline with) the
        // Appointment that holds them.
        builder.OwnsOne(a => a.Slot, slot =>
        {
            slot.Property(s => s.Start)
                .HasColumnName("StartTimeUtc")
                .IsRequired();

            slot.Property(s => s.End)
                .HasColumnName("EndTimeUtc")
                .IsRequired();
        });

        builder.OwnsOne(a => a.Patient, patient =>
        {
            patient.Property(p => p.Name)
                .HasColumnName("PatientName")
                .HasMaxLength(200)
                .IsRequired();

            patient.Property(p => p.Email)
                .HasColumnName("PatientEmail")
                .HasMaxLength(320)
                .IsRequired();
        });

        builder.Navigation(a => a.Slot).IsRequired();
        builder.Navigation(a => a.Patient).IsRequired();

        builder.Ignore(a => a.DomainEvents);

        builder.HasIndex(a => new { a.ProviderId, a.Status });
    }
}
