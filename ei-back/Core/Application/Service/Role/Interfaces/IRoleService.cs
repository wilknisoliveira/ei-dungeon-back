using ei_back.Core.Domain.Entity;

namespace ei_back.Core.Application.Service.Role.Interfaces
{
    public interface IRoleService
    {
        Task<RoleEntity> CreateAsync(RoleEntity entity);
        Task<List<RoleEntity>> FindAllAsync();
        Task<List<RoleEntity>> FindSelectedRoles(List<string> rolesList);
    }
}
