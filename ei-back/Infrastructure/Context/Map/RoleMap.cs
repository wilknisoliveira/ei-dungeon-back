using ei_back.Core.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ei_back.Infrastructure.Context.Map
{
    public class RoleMap : BaseMap<Role>
    {
        public RoleMap() : base("roles") { }

        public override void Configure(EntityTypeBuilder<Role> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Name).HasColumnName("name");
            builder.HasIndex(x => x.Name).IsUnique();
            builder.Property(x => x.Description).HasColumnName("description");
        }
    }
}
