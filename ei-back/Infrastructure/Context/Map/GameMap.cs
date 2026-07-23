using ei_back.Core.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ei_back.Infrastructure.Context.Map
{
    public class GameMap : BaseMap<Game>
    {
        public GameMap() : base("games") { }

        public override void Configure(EntityTypeBuilder<Game> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Name).HasColumnName("name").IsRequired();
            builder.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").IsRequired();
            builder.Property(x => x.WorldInfo).HasColumnName("world_info").IsRequired();
            builder.Property(x => x.GameStatus).HasColumnName("game_status").IsRequired();
            builder.Property(x => x.LastPlayedAt)
                .HasColumnName("last_played_at")
                .HasColumnType("timestamp with time zone");

            builder.HasOne(x => x.OwnerUser)
                .WithMany(x => x.Games)
                .HasForeignKey(x => x.OwnerUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
