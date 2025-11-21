using ei_back.Core.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ei_back.Infrastructure.Context.Map
{
    public class PlayerMap : BaseMap<Player>
    {
        public PlayerMap() : base("players") { }

        public override void Configure(EntityTypeBuilder<Player> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Name).HasColumnName("name");
            builder.Property(x => x.Description).HasColumnName("description");
            builder.Property(x => x.Type).HasColumnName("type");
            builder.Property(x => x.GameId).HasColumnName("game_id");
            builder.Property(x => x.Race)
                .HasColumnName("race")
                .HasConversion<string>();
            builder.Property(x => x.Strength).HasColumnName("strength");
            builder.Property(x => x.Dexterity).HasColumnName("dexterity");
            builder.Property(x => x.Intelligence).HasColumnName("intelligence");
            builder.Property(x => x.Constitution).HasColumnName("constitution");
            builder.Property(x => x.Charisma).HasColumnName("charisma");
            builder.Property(x => x.Wisdom).HasColumnName("wisdom");
            

            builder.HasOne(x => x.Game)
                .WithMany(x => x.Players)
                .HasForeignKey(x => x.GameId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
