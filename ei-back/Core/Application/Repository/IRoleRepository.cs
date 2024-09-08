using ei_back.Core.Domain.Entity;

namespace ei_back.Core.Application.Repository
{
    public interface IRoleRepository : IRepository<RoleEntity>
    {
        Task<List<RoleEntity>> FindRolesAndUsersAsync(CancellationToken cancellationToken = default);
    }
}
