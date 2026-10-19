using LinkUpPro.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.EntityConfigurations;

public class BattleshipGameConfiguration : IEntityTypeConfiguration<BattleshipGame>
{
    public void Configure(EntityTypeBuilder<BattleshipGame> builder)
    {
        builder.ToTable("BattleshipGames");

        builder.HasKey(g => g.Id);

        builder.HasOne(g => g.Player1)
            .WithMany(u => u.BattleshipGamesAsPlayer1)
            .HasForeignKey(g => g.Player1Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(g => g.Player2)
            .WithMany(u => u.BattleshipGamesAsPlayer2)
            .HasForeignKey(g => g.Player2Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(g => g.CurrentTurnPlayer)
            .WithMany()
            .HasForeignKey(g => g.CurrentTurnPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(g => g.WinnerPlayer)
            .WithMany()
            .HasForeignKey(g => g.WinnerPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(g => new { g.Status, g.LastMoveAt });
    }
}
