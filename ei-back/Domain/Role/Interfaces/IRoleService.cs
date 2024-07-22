namespace ei_back.Domain.Role.Interfaces
{
    public interface IRoleService
    {
        Task<RoleEntity> CreateAsync(RoleEntity entity);
        Task<List<RoleEntity>> FindAllAsync();
        Task<List<RoleEntity>> FindSelectedRoles(List<string> rolesList);
    }
}
