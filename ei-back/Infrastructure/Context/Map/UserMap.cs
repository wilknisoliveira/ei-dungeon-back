using ei_back.Domain.Base;
using ei_back.Domain.Role;
using ei_back.Domain.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ei_back.Infrastructure.Context.Map
{
    public class UserMap : BaseMap<UserEntity>
    {
        public UserMap() : base("users") { }

        public override void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.UserName).HasColumnName("user_name");
            builder.Property(x => x.FullName).HasColumnName("full_name");
            builder.Property(x => x.Password).HasColumnName("password");
            builder.Property(x => x.Email).HasColumnName("email");
            builder.Property(x => x.RefreshToken).HasColumnName("refresh_token");
            builder.Property(x => x.RefreshTokenExpiryTime)
                .HasColumnName("refresh_token_expiry_time")
                .HasColumnType("timestamp without time zone");
        }
    }
}
