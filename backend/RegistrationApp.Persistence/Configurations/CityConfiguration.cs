using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistrationApp.Domain.Entities;

namespace RegistrationApp.Persistence.Configurations;

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("Cities");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(c => c.NameEn).IsRequired().HasMaxLength(100);
        builder.Property(c => c.IsActive).HasDefaultValue(true);

        builder.HasOne(c => c.Governorate)
            .WithMany(g => g.Cities)
            .HasForeignKey(c => c.GovernorateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new City { Id = 1, GovernorateId = 1, NameAr = "مصر الجديدة", NameEn = "Heliopolis", IsActive = true },
            new City { Id = 2, GovernorateId = 1, NameAr = "مدينة نصر", NameEn = "Nasr City", IsActive = true },
            new City { Id = 3, GovernorateId = 1, NameAr = "التجمع الخامس", NameEn = "Fifth Settlement", IsActive = true },
            new City { Id = 4, GovernorateId = 1, NameAr = "المعادي", NameEn = "Maadi", IsActive = true },

            new City { Id = 5, GovernorateId = 2, NameAr = "سموحة", NameEn = "Smouha", IsActive = true },
            new City { Id = 6, GovernorateId = 2, NameAr = "المنتزة", NameEn = "Montaza", IsActive = true },
            new City { Id = 7, GovernorateId = 2, NameAr = "المعمورة", NameEn = "Maamoura", IsActive = true },

            new City { Id = 8, GovernorateId = 3, NameAr = "الدقي", NameEn = "Dokki", IsActive = true },
            new City { Id = 9, GovernorateId = 3, NameAr = "المهندسين", NameEn = "Mohandessin", IsActive = true },
            new City { Id = 10, GovernorateId = 3, NameAr = "6 أكتوبر", NameEn = "6th of October", IsActive = true },

            new City { Id = 11, GovernorateId = 4, NameAr = "بنها", NameEn = "Banha", IsActive = true },
            new City { Id = 12, GovernorateId = 4, NameAr = "شبرا الخيمة", NameEn = "Shubra El-Kheima", IsActive = true },

            new City { Id = 13, GovernorateId = 5, NameAr = "المنصورة", NameEn = "Mansoura", IsActive = true },
            new City { Id = 14, GovernorateId = 5, NameAr = "ميت غمر", NameEn = "Mit Ghamr", IsActive = true }
        );
    }
}
