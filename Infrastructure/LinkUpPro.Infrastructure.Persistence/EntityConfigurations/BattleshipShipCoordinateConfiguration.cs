using LinkUpPro.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.EntityConfigurations;

public class BattleshipShipCoordinateConfiguration : IEntityTypeConfiguration<BattleshipShipCoordinate>
{
    public void Configure(EntityTypeBuilder<BattleshipShipCoordinate> builder)
    {
        builder.ToTable("BattleshipShipCoordinates");

        builder.HasKey(c => c.Id);

        builder.HasOne(c => c.Ship)
            .WithMany(s => s.Coordinates)
            .HasForeignKey(c => c.ShipId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.ShipId, c.Row, c.Col })
            .IsUnique();
    }
}
