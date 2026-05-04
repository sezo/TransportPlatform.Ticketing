using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransportPlatform.Ticketing.Domain.Entities;

namespace TransportPlatform.Ticketing.Infrastructure.Persistence.Configurations;

public class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.ToTable("routes");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Origin).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Destination).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(r => r.TotalSeats).IsRequired();
        builder.Property(r => r.IsActive).IsRequired();

        builder.HasIndex(r => r.IsActive);
    }
}
