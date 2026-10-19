using LinkUpPro.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.EntityConfigurations;

public class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
{
    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        builder.ToTable("Friendships");

        builder.HasKey(f => f.Id);

        builder.HasOne(f => f.UserA)
            .WithMany(u => u.FriendshipsAsUserA)
            .HasForeignKey(f => f.UserAId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.UserB)
            .WithMany(u => u.FriendshipsAsUserB)
            .HasForeignKey(f => f.UserBId)
            .OnDelete(DeleteBehavior.Restrict);

        // Garantiza a nivel de base de datos una única relación por pareja
        builder.HasIndex(f => new { f.UserAId, f.UserBId })
            .IsUnique();
    }
}
