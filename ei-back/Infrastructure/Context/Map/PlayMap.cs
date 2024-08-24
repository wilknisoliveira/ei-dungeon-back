using ei_back.Domain.Play;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ei_back.Infrastructure.Context.Map
{
    public class PlayMap : BaseMap<PlayEntity>
    {
        public PlayMap() : base("plays") { }

        public override void Configure(EntityTypeBuilder<PlayEntity> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.GameId).HasColumnName("game_id").IsRequired();
            builder.Property(x => x.PlayerId).HasColumnName("player_id").IsRequired();
            builder.Property(x => x.Prompt).HasColumnName("promt").IsRequired();

            builder.HasOne(x=> x.Game)
                .WithMany(x => x.Plays)
                .HasForeignKey(x => x.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Player)
                .WithMany(x => x.Plays)
                .HasForeignKey(x => x.PlayerId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
