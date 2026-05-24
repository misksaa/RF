using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistrationApp.Domain.Entities;

namespace RegistrationApp.Persistence.Configurations;

public class GovernorateConfiguration : IEntityTypeConfiguration<Governorate>
{
    public void Configure(EntityTypeBuilder<Governorate> builder)
    {
        builder.ToTable("Governorates");

        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).ValueGeneratedNever();

        builder.Property(g => g.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(g => g.NameEn).IsRequired().HasMaxLength(100);
        builder.Property(g => g.IsActive).HasDefaultValue(true);

        builder.HasData(
            new Governorate { Id = 1, NameAr = "القاهرة", NameEn = "Cairo", IsActive = true },
            new Governorate { Id = 2, NameAr = "الإسكندرية", NameEn = "Alexandria", IsActive = true },
            new Governorate { Id = 3, NameAr = "الجيزة", NameEn = "Giza", IsActive = true },
            new Governorate { Id = 4, NameAr = "القليوبية", NameEn = "Qalyubia", IsActive = true },
            new Governorate { Id = 5, NameAr = "الدقهلية", NameEn = "Dakahlia", IsActive = true }
        );
    }
}
