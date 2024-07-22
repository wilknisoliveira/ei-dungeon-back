using ei_back.Domain.Role;
using ei_back.Domain.User;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ei_back.Infrastructure.Context
{
    public class EIContext : DbContext
    {
        public EIContext() { }

        public EIContext(DbContextOptions<EIContext> options) : base(options) { }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<RoleEntity> Roles { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

            /*
            // Role seeding
            List<RoleEntity> roles =
            [
                new RoleEntity("Admin", ""),
                new RoleEntity("CommonUser", "")
            ];

            roles.ForEach(role => {
                if (role.Name == "Admin")
                    role.Id = Guid.Parse("440e090b-1245-4cfe-bb62-b22a676ab441"); //Tem que colocar um id para cada
                else
                    role.Id = new Guid();

                role.CreatedAt = DateTime.Parse("2024-03-30 23:43:03.919095");
            });
            modelBuilder.Entity<RoleEntity>().HasData(roles);
            */

            /*
            //User admin seeding
            var adminUser = new UserEntity(
                "admin",
                "admin",
                "admin@gmail.com",
                "24-0B-E5-18-FA-BD-27-24-DD-B6-F0-4E-EB-1D-A5-96-74-48-D7-E8-31-C0-8C-8F-A8-22-80-9F-74-C7-20-A9")
            {
                Id = Guid.Parse("7d9ff283-6174-40a6-a317-f32a4a0620d0"),
                CreatedAt = DateTime.Parse("2024-07-14 22:41:06.874402")
            };

            modelBuilder.Entity<UserEntity>().HasData(adminUser);
            */
        }
    }
}
