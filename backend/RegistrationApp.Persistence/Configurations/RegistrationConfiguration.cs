using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistrationApp.Domain.Entities;

namespace RegistrationApp.Persistence.Configurations;

public class RegistrationConfiguration : IEntityTypeConfiguration<Registration>
{
    public void Configure(EntityTypeBuilder<Registration> builder)
    {
        builder.ToTable("Registrations");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.FirstName).IsRequired().HasMaxLength(50);
        builder.Property(r => r.MiddleName).HasMaxLength(50);
        builder.Property(r => r.LastName).IsRequired().HasMaxLength(50);
        builder.Property(r => r.BirthDate).IsRequired();

        builder.Property(r => r.MobileNumber).IsRequired().HasMaxLength(20);
        builder.Property(r => r.Email).IsRequired().HasMaxLength(254);

        builder.Property(r => r.CreatedBy).IsRequired().HasMaxLength(50);
        builder.Property(r => r.CreatedAtUtc).IsRequired();
        builder.Property(r => r.UpdatedBy).HasMaxLength(50);

        builder.HasIndex(r => r.Email).IsUnique();
        builder.HasIndex(r => r.MobileNumber).IsUnique();

        builder.HasMany(r => r.Addresses)
            .WithOne()
            .HasForeignKey(a => a.RegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Registration.Addresses))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(r => r.DomainEvents);
    }
}
