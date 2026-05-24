using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistrationApp.Application.Common.Models;

namespace RegistrationApp.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Type).IsRequired().HasMaxLength(256);
        builder.Property(o => o.Content).IsRequired();
        builder.Property(o => o.OccurredOnUtc).IsRequired();
    }
}
