using LinkUpPro.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.EntityConfigurations;

public class BattleshipAttackConfiguration : IEntityTypeConfiguration<BattleshipAttack>
{
    public void Configure(EntityTypeBuilder<BattleshipAttack> builder)
    {
        builder.ToTable("BattleshipAttacks");

        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.Game)
            .WithMany(g => g.Attacks)
            .HasForeignKey(a => a.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Attacker)
            .WithMany()
            .HasForeignKey(a => a.AttackerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Regla: no permite que el mismo atacante dispare dos veces a la misma casilla en la misma partida
        builder.HasIndex(a => new { a.GameId, a.AttackerId, a.Row, a.Col })
            .IsUnique();
    }
}
