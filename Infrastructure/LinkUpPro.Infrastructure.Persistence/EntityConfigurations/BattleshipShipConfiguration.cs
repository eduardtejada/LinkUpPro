using LinkUpPro.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.EntityConfigurations;

public class BattleshipShipConfiguration : IEntityTypeConfiguration<BattleshipShip>
{
    public void Configure(EntityTypeBuilder<BattleshipShip> builder)
    {
        builder.ToTable("BattleshipShips");

        builder.HasKey(s => s.Id);

        builder.HasOne(s => s.Game)
            .WithMany(g => g.Ships)
            .HasForeignKey(s => s.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Player)
            .WithMany()
            .HasForeignKey(s => s.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
