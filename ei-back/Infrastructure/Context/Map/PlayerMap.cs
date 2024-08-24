﻿using ei_back.Domain.Player;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ei_back.Infrastructure.Context.Map
{
    public class PlayerMap : BaseMap<PlayerEntity>
    {
        public PlayerMap() : base("players") { }

        public override void Configure(EntityTypeBuilder<PlayerEntity> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Name).HasColumnName("name");
            builder.Property(x => x.Description).HasColumnName("description");
            builder.Property(x => x.Type).HasColumnName("type");
        }
    }
}