using ei_back.Domain.Base.Interfaces;

namespace ei_back.Domain.Role.Interfaces
{
    public interface IRoleRepository : IRepository<RoleEntity> 
    {
        Task<List<RoleEntity>> FindRolesAndUsersAsync(CancellationToken cancellationToken = default);
    }
}
