using AppointmentScheduler.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentScheduler.Infrastructure.Persistence.Configurations;

public sealed class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("Providers");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Specialty)
            .IsRequired()
            .HasMaxLength(200);

        // Domain events are transient - they live only until they're
        // dispatched after SaveChanges - so EF must never try to persist them.
        builder.Ignore(p => p.DomainEvents);

        // Provider.Appointments is a read-only projection over a private
        // backing field, populated by eager-loading. Map it as a standard
        // one-to-many relationship, owned (FK-wise) by Appointment.
        builder.HasMany(p => p.Appointments)
            .WithOne()
            .HasForeignKey(a => a.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Appointments has no public setter, so EF Core reads/writes it
        // through the backing field automatically once the field is named.
        builder.Navigation(p => p.Appointments)
            .HasField("_appointments");
    }
}
