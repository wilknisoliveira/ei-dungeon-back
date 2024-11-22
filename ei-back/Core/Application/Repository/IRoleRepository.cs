using ei_back.Core.Domain.Entity;

namespace ei_back.Core.Application.Repository
{
    public interface IRoleRepository : IRepository<Role>
    {
        Task<List<Role>> FindRolesAndUsersAsync(CancellationToken cancellationToken = default);
    }
}
