using ei_back.Core.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ei_back.Infrastructure.Context.Map
{
    public class GameMap : BaseMap<GameEntity>
    {
        public GameMap() : base("games") { }

        public override void Configure(EntityTypeBuilder<GameEntity> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Name).HasColumnName("name").IsRequired();
            builder.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").IsRequired();
            builder.Property(x => x.SystemGame).HasColumnName("system_game").IsRequired();

            builder.HasOne(x => x.OwnerUser)
                .WithMany(x => x.Games)
                .HasForeignKey(x => x.OwnerUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
