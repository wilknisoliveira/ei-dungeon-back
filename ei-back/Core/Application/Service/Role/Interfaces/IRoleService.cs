using ei_back.Core.Domain.Entity;

namespace ei_back.Core.Application.Service.Role.Interfaces
{
    public interface IRoleService
    {
        Task<Domain.Entity.Role> CreateAsync(Domain.Entity.Role entity);
        Task<List<Domain.Entity.Role>> FindAllAsync();
        Task<List<Domain.Entity.Role>> FindSelectedRoles(List<string> rolesList);
    }
}
